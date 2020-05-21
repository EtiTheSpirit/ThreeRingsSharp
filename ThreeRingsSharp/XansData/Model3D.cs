using com.google.inject;
using com.sun.org.apache.xalan.@internal.xsltc.cmdline;
using com.threerings.math;
using java.nio.channels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.IO;
using ThreeRingsSharp.XansData.IO.GLTF;
using ThreeRingsSharp.XansData.Structs;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// A unified representation of a model. Since various implementations from Clyde may store data differently, this provides a common interface.
	/// </summary>
	public class Model3D : IDisposable {

		// TODO: How do I make it so that I can have multiple model3Ds, but I don't write the mesh data twice?
		// I could create mesh data, then have this reference mesh data packets.

		/// <summary>
		/// A list of bindings from <see cref="ModelFormat"/>s to a singleton of their applicable <see cref="AbstractModelExporter"/>.
		/// </summary>
		private static readonly IReadOnlyDictionary<ModelFormat, dynamic> ExporterBindings = new Dictionary<ModelFormat, dynamic>() {
			[ModelFormat.OBJ] = new ModelExporterFactory<OBJExporter>(),
			[ModelFormat.glTF] = new ModelExporterFactory<GLTFExporter>()
		};

		/// <summary>
		/// The axis that represents the vertical component. This should be set depending on the program the model will be imported to.
		/// </summary>
		public static Axis TargetUpAxis { get; set; } = Axis.PositiveY;

		/// <summary>
		/// Multiplies the scale of exported models by 100. This is really handy for a lot of models but may cause others to be huge.<para/>
		/// This is <see langword="true"/> by default since it's used more than it isn't.
		/// </summary>
		public static bool MultiplyScaleByHundred { get; set; } = true;

		/// <summary>
		/// If <see langword="true"/>, any models that have a scale of zero will have their scale corrected. Cases where models are dubbed incorrect are:<para/>
		/// - The matrix's scale (a <see cref="Vector3f"/>) only has its X value populated, Y and Z are zero. Y and Z will be set to X (this happens for cases where uniform scale is stored as a Vector3f)<para/>
		/// - The matrix's scale has a magnitude of zero.<para/>
		/// - The <see cref="Transform3D.getScale()"/> method returns zero, in which case, if the matrix has a non-zero uniform scale it will be set to this, or it will be directly set to 1.
		/// </summary>
		public static bool ProtectAgainstZeroScale { get; set; } = true;

		/// <summary>
		/// The display name for this model, used in exporting (i.e. this is the name that will show up in Blender or any other modelling software.)
		/// </summary>
		public string Name { get; set; } = null;

		/// <summary>
		/// The current up axis for this model.<para/>
		/// Default value: <see cref="Axis.PositiveY"/> (in the context that +Y is up, and -Z is forward)
		/// </summary>
		public Axis Up { get; set; } = Axis.PositiveY;

		/// <summary>
		/// A reference to the file that the model here came from. This is used to reference textures and other path-dependent extra data.
		/// </summary>
		public FileInfo Source { get; set; }

		/// <summary>
		/// The transformation to apply to the model data. By default, this is the identity transformation (so no transform).
		/// </summary>
		public Transform3D Transform { get; set; } = new Transform3D();

		/// <summary>
		/// A reference to the geometry stored in this <see cref="Model3D"/>, since multiple models may share the same mesh data.
		/// </summary>
		public MeshData Mesh {
			get => _Mesh;
			set {
				if (value == _Mesh) return; // Skip if it's being set to the same thing.

				if (_Mesh != null) {
					// Was existing, now it's being set to something else.
					// This means we were using it, and now we aren't. Unregister.
					_Mesh.Users.Remove(this);
					_Mesh.DisposeIfNoUsersExist();
				}
				if (value != null) {
					value.Users.Add(this);
				}
				_Mesh = value;
			}
		}
		private MeshData _Mesh = null;

		/// <summary>
		/// If true, then <see cref="Transform"/> has been applied to the mesh data, and <see cref="ApplyTransformations"/> cannot be called again.
		/// </summary>
		public bool HasDoneTransformation { get; private set; } = false;

		/// <summary>
		/// If true, <see cref="ApplyScaling"/> has already been called and cannot be called again. This applies transformations like the zero-scale check and the x100 scale prefs.
		/// </summary>
		public bool HasAppliedScaleCorrections { get; private set; } = false;

		/// <summary>
		/// The textures tied to this model by full filepath.
		/// </summary>
		public readonly List<string> Textures = new List<string>();

		/// <summary>
		/// The texture that this model uses. This is by filename, not full path.
		/// </summary>
		public string ActiveTexture { get; set; } = null;

		/// <summary>
		/// The attached models on this <see cref="Model3D"/>.
		/// </summary>
		[Obsolete("Feature isn't ready, does nothing.")] public List<Model3D> Attachments { get; set; } = new List<Model3D>();

		public Model3D() { }

		/// <summary>
		/// Exports this model in a given format, writing the data to the target <see cref="FileInfo"/>
		/// </summary>
		/// <param name="targetFile">The file that will be written to.</param>
		/// <param name="targetFormat">The file format to use for the 3D model.</param>
		public void Export(FileInfo targetFile, ModelFormat targetFormat = ModelFormat.glTF) {
			var factory = ExporterBindings[targetFormat];
			AbstractModelExporter exporter = factory.NewInstance();
			exporter.Export(new Model3D[] { this }, targetFile);
		}


		/// <summary>
		/// This should only be used in contexts where the exported model format does not have abstractions between objects and mesh data (e.g. formats like OBJ)!<para/>
		/// This duplicates <see cref="Mesh"/> into a new instance made just for this <see cref="Model3D"/> and then applies <see cref="Transform"/> to its contents.<para/>
		/// This can only be called once (subsequent calls will do nothing.)
		/// </summary>
		public void ApplyTransformations() {
			if (HasDoneTransformation) return;

			ApplyScaling();
			// Now dupe the mesh data since we no longer will be sharing refs.
			// We need to unregister the old mesh.
			Mesh = Mesh.Clone();
			Mesh.ApplyTransform(Transform);

			HasDoneTransformation = true;
		}

		/// <summary>
		/// Applies the scale modifiers to this <see cref="Transform"/> as defined by user prefs. This can only be called once (subsequent calls will do nothing.)
		/// </summary>
		public void ApplyScaling() {
			if (HasAppliedScaleCorrections) return;
			// TODO: Is this necessary? Probably.
			if (ProtectAgainstZeroScale) {
				float fScale = Transform.getScale();
				Vector3f vScale = Transform.extractScale();
				float vScaleLength = vScale.distance(Vector3f.ZERO);
				if (fScale == 0) {
					if (vScaleLength != 0) {
						Transform.setScale(vScaleLength);
					} else {
						vScaleLength = 1;
						Transform.setScale(1);
					}
					XanLogger.WriteLine($"A MeshData had a uniform scale of 0. Protection was enabled, and it has been changed to {vScaleLength}.", true);
				} else if (vScaleLength == 0) {
					// fScale won't be 0 here since if it is, it'll go to the condition above instead.
					Transform.getMatrix().setToScale(fScale);
					XanLogger.WriteLine($"A MeshData had a matrix scale of 0. Protection was enabled, and it has been changed to {fScale} (on all axes).", true);
				}
			}

			// This was removed because new transformations created when loading files already performs a 100x scale.
			/*
			if (MultiplyScaleByHundred) {
				Transform.setScale(Transform.getScale() * 100f);
			}
			*/
			HasAppliedScaleCorrections = true;
		}

		/// <summary>
		/// Alters <see cref="Transform"/> by rotating it so that the given axis is treated as the vertical axis, then returns the modified <see cref="Transform3D"/> (this will not edit the internal model's <see cref="Transform"/>).<para/>
		/// <see cref="Up"/> will be set to <see cref="TargetUpAxis"/> when this is called.<para/>
		/// All axis are treated as left-handed.<para/>
		/// This should only be called after all transformations have been applied! It can only be called once. If this is called again, an <see cref="InvalidOperationException"/> will be thrown.
		/// </summary>
		[Obsolete("Overcomplicated.", true)] public Transform3D ApplyUpAxis() => ApplyUpAxis(TargetUpAxis);

		/// <summary>
		/// Alters <see cref="Transform"/> by rotating it so that the given axis is treated as the vertical axis, then returns the modified <see cref="Transform3D"/> (this will not edit the internal model's <see cref="Transform"/>).<para/>
		/// /// <see cref="Up"/> will be set to <paramref name="newUpAxis"/> when this is called.<para/>
		/// All axis are treated as left-handed.<para/>
		/// This should only be called after all transformations have been applied! It can only be called once. If this is called again, an <see cref="InvalidOperationException"/> will be thrown.
		/// </summary>
		/// <param name="newUpAxis">The new <see cref="Axis"/> that should be treated as the verical axis.</param>
		[Obsolete("Overcomplicated.", true)] public Transform3D ApplyUpAxis(Axis newUpAxis) {
			// Dupe the old transform.
			Transform3D newTrs = new Transform3D(Transform);
			if (Up == newUpAxis) return newTrs; // Skip any math since there's no change.
			
			// First off: The easy stuff.
			int currentDir = (int)Up;
			bool isOpposite = currentDir == (int)newUpAxis * -1;
			// The enum is programmed to have X,Y,Z as 1,2,3, and -X,-Y-,Z as -1,-2,-3.
			if (isOpposite) {
				// Rotate 180 on an axis.

				// In all cases, -Z is forward.
				int absCurrentDir = Math.Abs(currentDir);
				if (absCurrentDir == 1) {
					// X is up.
					// Rotate on Y by 180 degrees.
					Rotate180OnAxis(newTrs, Axis.PositiveY);
				} else if (absCurrentDir == 2) {
					// Y is up.
					// Rotate on Z by 180 degrees.
					Rotate180OnAxis(newTrs, Axis.PositiveZ);
				} else if (absCurrentDir == 3) {
					// Z is up.
					// Rotate on X by 180 degrees.
					Rotate180OnAxis(newTrs, Axis.PositiveX);
				}
			} else {
				// And now the not easy stuff.
				// When visualizing, imagine a block with its six faces colored for each axis.
				// You want to rotate this block. How do you do it *relative to the BLOCK, not the existing axis?*

				if (Up == Axis.PositiveX) {
					if (newUpAxis == Axis.PositiveY) {
						// Facing forward, rotate clockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, -90);
					} else if (newUpAxis == Axis.NegativeY) {
						// Facing forward, rotate counterclockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, 90);
					} else if (newUpAxis == Axis.PositiveZ) {
						// Facing right, rotate counterclockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, 90);
					} else if (newUpAxis == Axis.NegativeZ) {
						// Facing right, rotate clockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, -90);
					}
				} else if (Up == Axis.PositiveY) {
					if (newUpAxis == Axis.PositiveX) {
						// Facing forward, rotate counterclockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, 90);
					} else if (newUpAxis == Axis.NegativeX) {
						// Facing forward, rotate clockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, -90);
					} else if (newUpAxis == Axis.PositiveZ) {
						// Facing right, rotate clockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, -90);
					} else if (newUpAxis == Axis.NegativeZ) {
						// Facing right, rotate counterclockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, 90);
					}
				} else if (Up == Axis.PositiveZ) {
					if (newUpAxis == Axis.PositiveX) {
						// Facing forward, rotate counterclockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, 90);
					} else if (newUpAxis == Axis.NegativeX) {
						// Facing forward, rotate clockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, -90);
					} else if (newUpAxis == Axis.PositiveY) {
						// Facing right, rotate clockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, -90);
					} else if (newUpAxis == Axis.NegativeY) {
						// Facing right, rotate counterclockwise by 90 degrees.
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, 90);
					}

				// All of these are identical to ^ but opposite
				} else if (Up == Axis.NegativeX) {
					if (newUpAxis == Axis.PositiveY) {
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, -90);
					} else if (newUpAxis == Axis.NegativeY) {
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, 90);
					} else if (newUpAxis == Axis.PositiveZ) {
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, 90);
					} else if (newUpAxis == Axis.NegativeZ) {
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, -90);
					}
				} else if (Up == Axis.NegativeY) {
					if (newUpAxis == Axis.PositiveX) {
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, -90);
					} else if (newUpAxis == Axis.NegativeX) {
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, 90);
					} else if (newUpAxis == Axis.PositiveZ) {
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, 90);
					} else if (newUpAxis == Axis.NegativeZ) {
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, -90);
					}
				} else if (Up == Axis.NegativeZ) {
					if (newUpAxis == Axis.PositiveX) {
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, -90);
					} else if (newUpAxis == Axis.NegativeX) {
						newTrs.RotateOnAxisDegrees(Axis.NegativeZ, 90);
					} else if (newUpAxis == Axis.PositiveY) {
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, 90);
					} else if (newUpAxis == Axis.NegativeY) {
						newTrs.RotateOnAxisDegrees(Axis.PositiveX, -90);
					}
				}
			}

			Up = newUpAxis;
			return newTrs;
		}

		/// <summary>
		/// Rotates the given <see cref="Transform3D"/> by 180 degrees on the given axis.
		/// </summary>
		/// <param name="trs"></param>
		/// <param name="axis"></param>
		[Obsolete("Overcomplicated.", true)] private static void Rotate180OnAxis(Transform3D trs, Axis axis) => trs.RotateOnAxis(axis, (float)Math.PI);
		

		/// <summary>
		/// Frees all information used by this <see cref="Model3D"/>.
		/// </summary>
		public void Dispose() {
			Source = null;
			Name = null;
			Transform = null;
			Textures.Clear();
			Mesh.Dispose();
			Mesh = null;
		}

		/// <summary>
		/// Exports the given <see cref="Model3D"/> instances into a single file.
		/// </summary>
		/// <param name="targetFile"></param>
		/// <param name="targetFormat"></param>
		/// <param name="models"></param>
		public static void ExportIntoOne(FileInfo targetFile, ModelFormat targetFormat = ModelFormat.glTF, params Model3D[] models) {
			var factory = ExporterBindings[targetFormat];
			AbstractModelExporter exporter = factory.NewInstance();
			exporter.Export(models, targetFile);
		}
	}
}

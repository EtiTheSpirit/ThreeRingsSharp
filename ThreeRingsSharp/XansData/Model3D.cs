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
using ThreeRingsSharp.XansData.Extensions;

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
			// [ModelFormat.OBJ] = new ModelExporterFactory<OBJExporter>(),
			[ModelFormat.GLTF] = new ModelExporterFactory<GLTFExporter>()
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
					_Mesh._Users.Remove(this);
					_Mesh.DisposeIfNoUsersExist();
				}
				if (value != null) {
					value._Users.Add(this);
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
		/// The textures tied to this model relative to the rsrc directory.
		/// </summary>
		public List<string> Textures { get; } = new List<string>();

		/// <summary>
		/// The texture that this model uses. This is by filename, not full path.
		/// </summary>
		public string ActiveTexture { get; set; } = null;

		/// <summary>
		/// The <see cref="Armature"/> that this <see cref="Model3D"/> is attached to.
		/// </summary>
		public Armature AttachmentNode { get; set; } = null;

		/// <summary>
		/// Any extra information attached to this <see cref="Model3D"/> that serves as arbitrary data.
		/// </summary>
		public Dictionary<string, object> ExtraData { get; } = new Dictionary<string, object>();

		public Model3D() { }

		/// <summary>
		/// Exports this model in a given format, writing the data to the target <see cref="FileInfo"/>
		/// </summary>
		/// <param name="targetFile">The file that will be written to.</param>
		/// <param name="targetFormat">The file format to use for the 3D model.</param>
		public void Export(FileInfo targetFile, ModelFormat targetFormat = ModelFormat.GLTF) {
			var factory = ExporterBindings[targetFormat];
			AbstractModelExporter exporter = factory.NewInstance();
			exporter.Export(new Model3D[] { this }, targetFile);
		}


		/// <summary>
		/// This should only be used in contexts where the exported model format does not have abstractions between objects and mesh data (e.g. formats like OBJ)!<para/>
		/// This duplicates <see cref="Mesh"/> into a new instance made just for this <see cref="Model3D"/> and then applies <see cref="Transform"/> to its contents.<para/>
		/// This can only be called once (subsequent calls will do nothing.)
		/// </summary>
		[Obsolete]
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
			// TODO: Is zero-scale protection even necessary? Probably not!
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
					XanLogger.WriteLine($"A MeshData had a uniform scale of 0. Protection was enabled, and it has been changed to {vScaleLength}.", XanLogger.DEBUG);
				} else if (vScaleLength == 0) {
					// fScale won't be 0 here since if it is, it'll go to the condition above instead.
					Transform.getMatrix().setToScale(fScale);
					XanLogger.WriteLine($"A MeshData had a matrix scale of 0. Protection was enabled, and it has been changed to {fScale} (on all axes).", XanLogger.DEBUG);
				}
			}

			HasAppliedScaleCorrections = true;
		}

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
		public static void ExportIntoOne(FileInfo targetFile, ModelFormat targetFormat = ModelFormat.GLTF, params Model3D[] models) {
			var factory = ExporterBindings[targetFormat];
			AbstractModelExporter exporter = factory.NewInstance();
			exporter.Export(models, targetFile);
		}
	}
}

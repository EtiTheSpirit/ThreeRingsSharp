using com.samskivert.velocity;
using com.threerings.math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.Structs;

namespace ThreeRingsSharp.XansData.IO {

	/// <summary>
	/// Exports model data in the Wavefront OBJ format.
	/// </summary>
	public class OBJExporter : AbstractModelExporter {

		public override void Export(Model3D[] models, FileInfo toFile) {
			StringBuilder objBuilder = new StringBuilder("# " + TOOL);

			int modelIndex = 0;
			int indexOffset = 0;

			int numModelsSkipped = 0;

			foreach (Model3D model in models) {
				bool skip = (bool)model.ExtraData.GetOrDefault("SkipExport", false);
				if (skip) {
					numModelsSkipped++;
					continue; // Go to the next iteration.
				}

				objBuilder.Append("\n\no ");
				objBuilder.AppendLine(model.Name ?? "ExportedModel" + modelIndex);

				model.ApplyTransformations();
				
				//Transform3D newTrs = model.ApplyUpAxis();

				foreach (Vector3 vtx in model.Mesh.Vertices) {
					Vector3 vertex = vtx.RotateToAxis(Model3D.TargetUpAxis);
					WriteVertex(objBuilder, vertex);
				}

				foreach (Vector3 nrm in model.Mesh.Normals) {
					Vector3 normal = nrm.RotateToAxis(Model3D.TargetUpAxis);
					WriteNormal(objBuilder, normal);
				}

				foreach (Vector2 uv in model.Mesh.UVs) {
					WriteUVCoordinate(objBuilder, uv);
				}

				for (int idx = 0; idx < model.Mesh.Indices.Count / 3; idx++) {
					WriteIndexTriplet(objBuilder, model.Mesh.Indices, idx, indexOffset);
				}
				modelIndex++;
				indexOffset += model.Mesh.Vertices.Count;
			}

			XanLogger.WriteLine($"OBJ Exporter instantiated {modelIndex} models (skipped {numModelsSkipped} models).");

			// Write the file now.
			File.WriteAllText(toFile.FullName, objBuilder.ToString());
		}

		/// <summary>
		/// Writes a vertex to the given <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="objBuilder">A reference to the <see cref="StringBuilder"/> which is being used to construct the OBJ file.</param>
		/// <param name="vertex">The <see cref="Vector3"/> to write as a vertex.</param>
		private static void WriteVertex(StringBuilder objBuilder, Vector3 vertex) {
			objBuilder.Append("v ");
			objBuilder.AppendLine(vertex.ToString());
		}

		/// <summary>
		/// Writes a vertex normal to the given <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="objBuilder">A reference to the <see cref="StringBuilder"/> which is being used to construct the OBJ file.</param>
		/// <param name="normal">The <see cref="Vector3"/> to write as a normal.</param>
		private static void WriteNormal(StringBuilder objBuilder, Vector3 normal) {
			objBuilder.Append("vn ");
			objBuilder.AppendLine(normal.ToString());
		}

		/// <summary>
		/// Writes a UV Coordinate to the given <see cref="StringBuilder"/>.
		/// </summary>
		/// <param name="objBuilder">A reference to the <see cref="StringBuilder"/> which is being used to construct the OBJ file.</param>
		/// <param name="uv">The <see cref="Vector2"/> to write as a UV Coordinate</param>
		private static void WriteUVCoordinate(StringBuilder objBuilder, Vector2 uv) {
			objBuilder.Append("vt ");
			objBuilder.AppendLine(uv.ToString());
		}

		/// <summary>
		/// Writes the given indices to the given <see cref="StringBuilder"/>. This adds 1 to all of the indices so that they are compliant with OBJ standards.
		/// </summary>
		/// <param name="objBuilder">A reference to the <see cref="StringBuilder"/> which is being used to construct the OBJ file.</param>
		/// <param name="indices">The list of indices for this 3D model.</param>
		/// <param name="tripletStartIndex">An index multiplied by 3 to represent where to get a triplet from (so an index of 0 would be indices[0], [1], and [2], and an index of 1 would be [3], [4], and [5])</param>
		/// <param name="indexOffset">The value to offset geometry indices by.</param>
		private static void WriteIndexTriplet(StringBuilder objBuilder, List<ushort> indices, int tripletStartIndex, int indexOffset) {
			IEnumerable<ushort> skippedIndices = indices.Skip(tripletStartIndex * 3);
			if (skippedIndices.Count() < 3) {
				XanLogger.WriteLine("WARNING: Index count is not a multiple of 3!");
				return;
			}
			skippedIndices = skippedIndices.Take(3);
			int alpha   = skippedIndices.ElementAt(0) + 1 + indexOffset;
			int bravo   = skippedIndices.ElementAt(1) + 1 + indexOffset;
			int charlie = skippedIndices.ElementAt(2) + 1 + indexOffset;

			objBuilder.Append("f ");
			objBuilder.Append($"{alpha}/{alpha}/{alpha} ");
			objBuilder.Append($"{bravo}/{bravo}/{bravo} ");
			objBuilder.AppendLine($"{charlie}/{charlie}/{charlie}");
		}
	}
}

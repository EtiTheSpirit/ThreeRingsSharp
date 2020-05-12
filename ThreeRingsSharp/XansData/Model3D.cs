using com.google.inject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.IO;
using ThreeRingsSharp.XansData.Structs;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// A unified representation of a model. Since various implementations from Clyde may store data differently, this provides a common interface.
	/// </summary>
	public class Model3D : IDisposable {

		/// <summary>
		/// A list of bindings from <see cref="ModelFormat"/>s to a singleton of their applicable <see cref="AbstractModelExporter"/>.
		/// </summary>
		internal static readonly IReadOnlyDictionary<ModelFormat, dynamic> ExporterBindings = new Dictionary<ModelFormat, dynamic>() {
			[ModelFormat.FBX] = null,
			[ModelFormat.OBJ] = new ModelExporterFactory<OBJExporter>(),
		};

		/// <summary>
		/// The display name for this model.
		/// </summary>
		public string Name { get; set; } = null;

		/// <summary>
		/// A reference to the file that the model here came from. This is used to reference textures and other path-dependent extra data.
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// The vertices that make up this 3D model.
		/// </summary>
		public readonly List<Vector3> Vertices = new List<Vector3>();

		/// <summary>
		/// The normals that make up this 3D model.
		/// </summary>
		public readonly List<Vector3> Normals = new List<Vector3>();

		/// <summary>
		/// The UV coordinates.
		/// </summary>
		public readonly List<Vector2> UVs = new List<Vector2>();

		/// <summary>
		/// The indices that define triangles.
		/// </summary>
		public readonly List<short> Indices = new List<short>();

		/// <summary>
		/// Exports this model in a given format, writing the data to the target <see cref="FileInfo"/>
		/// </summary>
		/// <param name="targetFile">The file that will be written to.</param>
		/// <param name="targetFormat">The file format to use for the 3D model.</param>
		public void Export(FileInfo targetFile, ModelFormat targetFormat = ModelFormat.FBX) {
			var factory = ExporterBindings[targetFormat];
			AbstractModelExporter exporter = factory.NewInstance();
			exporter.Export(new Model3D[] { this }, targetFile);
		}

		/// <summary>
		/// Frees all information used by this <see cref="Model3D"/>.
		/// </summary>
		public void Dispose() {
			Vertices.Clear();
			Normals.Clear();
			UVs.Clear();
			Indices.Clear();
			Source = null;
			Name = null;
		}

		/// <summary>
		/// Exports the given <see cref="Model3D"/> instances into a single file.
		/// </summary>
		/// <param name="targetFile"></param>
		/// <param name="targetFormat"></param>
		/// <param name="models"></param>
		public static void ExportIntoOne(FileInfo targetFile, ModelFormat targetFormat = ModelFormat.FBX, params Model3D[] models) {
			var factory = ExporterBindings[targetFormat];
			AbstractModelExporter exporter = factory.NewInstance();
			exporter.Export(models, targetFile);
		}
	}
}

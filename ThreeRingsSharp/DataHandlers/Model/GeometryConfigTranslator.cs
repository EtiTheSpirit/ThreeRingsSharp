using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.threerings.opengl.geometry.config;
using static com.threerings.opengl.geometry.config.GeometryConfig;
using ThreeRingsSharp.XansData;
using System.Windows.Forms;
using java.nio;
using ThreeRingsSharp.XansData.Structs;

namespace ThreeRingsSharp.DataHandlers.Model {

	/// <summary>
	/// A specialized class designed to handle <see cref="GeometryConfig"/> types and pull the necessary information out, placing it into a <see cref="Model3D"/>
	/// </summary>
	class GeometryConfigTranslator {

		/// <summary>
		/// Translates the given <see cref="GeometryConfig"/> into a <see cref="Model3D"/>.
		/// </summary>
		/// <param name="geometry">The <see cref="GeometryConfig"/> storing the applicable data.</param>
		/// <returns></returns>
		public static Model3D GetGeometryInformation(GeometryConfig geometry) {
			// stored
			// indexed stored
			// skinned indexed stored

			Model3D model = new Model3D();
			// Go from the most complex to the least complex, since (presumably) simpler classes can nest.
			// Let's pull the basic arrays from the geometry data.
			float[] vertices;
			float[] uvs;
			float[] normals;
			short[] indices;
			if (geometry is SkinnedIndexedStored skinnedIndexedStored) {
				vertices = skinnedIndexedStored.getFloatArray(false, skinnedIndexedStored.vertexArray);
				uvs = skinnedIndexedStored.getFloatArray(false, skinnedIndexedStored.texCoordArrays);
				normals = skinnedIndexedStored.getFloatArray(false, skinnedIndexedStored.normalArray);
				indices = GetFromShortBuffer(skinnedIndexedStored.indices);
			} else if (geometry is IndexedStored indexedStored) {
				vertices = indexedStored.getFloatArray(false, indexedStored.vertexArray);
				uvs = indexedStored.getFloatArray(false, indexedStored.texCoordArrays);
				normals = indexedStored.getFloatArray(false, indexedStored.normalArray);
				indices = GetFromShortBuffer(indexedStored.indices);
			} else if (geometry is Stored stored) {
				vertices = stored.getFloatArray(false, stored.vertexArray);
				uvs = stored.getFloatArray(false, stored.texCoordArrays);
				normals = stored.getFloatArray(false, stored.normalArray);
				indices = new short[vertices.Length];
				for (short i = 0; i < indices.Length; i++) {
					indices[i] = i;
				}
			} else {
				throw new InvalidOperationException("The GeometryConfig type is unknown! Type: " + geometry.getClass().getName());
			}

			model.Vertices.SetTo(Vector3.FromFloatArray(vertices));
			model.UVs.SetTo(Vector2.FromFloatArray(uvs));
			model.Normals.SetTo(Vector3.FromFloatArray(normals));
			model.Indices.SetTo(indices);

			return model;
		}

		/// <summary>
		/// Since the given buffer may not have an array, this will automatically perform necessary edits to get a short array.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		private static short[] GetFromShortBuffer(ShortBuffer buffer) {
			if (buffer.hasArray()) return buffer.array();
			List<short> data = new List<short>();
			while (buffer.hasRemaining()) {
				data.Add(buffer.get());
			}
			return data.ToArray();
		}

	}
}

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
using com.threerings.opengl.renderer.config;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.Exceptions;
using System.Runtime.CompilerServices;

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
			int[,] boneIndices = new int[0, 0];
			float[,] boneWeights = new float[0, 0];
			string[] boneNames = new string[0];

			if (geometry is SkinnedIndexedStored skinnedIndexedStored) {
				vertices = skinnedIndexedStored.getFloatArray(false, skinnedIndexedStored.vertexArray);
				uvs = skinnedIndexedStored.getFloatArray(false, skinnedIndexedStored.texCoordArrays);
				normals = skinnedIndexedStored.getFloatArray(false, skinnedIndexedStored.normalArray);
				indices = GetFromShortBuffer(skinnedIndexedStored.indices);

				// Also need to handle skinning.
				if (skinnedIndexedStored.mode != Mode.TRIANGLES) {
					XanLogger.WriteLine("WARNING: This Articulated model may not export properly! Its mode isn't TRIANGLES, and other behaviors (e.g. TRIANGLESTRIP) haven't been coded in yet! The method used for TRIANGLES will be applied anyway just to try.");
				}
				AttributeArrayConfig[] allArrays = skinnedIndexedStored.vertexAttribArrays;
				AttributeArrayConfig boneIndicesAttr = GetArrayByName(allArrays, "boneIndices");
				AttributeArrayConfig boneWeightsAttr = GetArrayByName(allArrays, "boneWeights");

				int[] boneIndicesI = skinnedIndexedStored.getFloatArray(false, boneIndicesAttr).ToIntArray();
				float[] boneWeightsF = skinnedIndexedStored.getFloatArray(false, boneWeightsAttr);
				// Now let's consider this literally: indices and weights for bones are vertex *attribute* arrays.
				// So presumably this means that we iterate through the indices.
				// The vertex at vertices[index] is part of bone boneIndices[index]. The index returned by boneIndices is the index of a name.
				// A vertex can be in up to four groups at once, hence why these are in groups of quads.
				// If the bone group is 0, it should be ignored.

				// Apparently, this concept went way over my head in SK Animator Tools and it was a disaster. Part of why the code was so horrifying.

				// Now for ease in indexing, I'm going to bump all of the elements in the bone name array forward by 1, then set index 0 to null.
				boneNames = new string[skinnedIndexedStored.bones.Length + 1];
				boneNames[0] = null;
				for (int idx = 0; idx < boneNames.Length - 1; idx++) {
					boneNames[idx + 1] = skinnedIndexedStored.bones[idx];
				}

				boneIndices = boneIndicesI.As2D(4);
				boneWeights = boneWeightsF.As2D(4);

				model.HasBoneData = true;
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

			model.Vertices.SetFrom(Vector3.FromFloatArray(vertices));
			model.UVs.SetFrom(Vector2.FromFloatArray(uvs));
			model.Normals.SetFrom(Vector3.FromFloatArray(normals));
			model.Indices.SetFrom(indices);
			model.BoneNames = boneNames;
			model.BoneIndices = boneIndices;
			model.BoneWeights = boneWeights;
			model.ConstructGroups();

			return model;
		}

		/// <summary>
		/// Since the given buffer may not have an array, this will automatically perform necessary edits to get a <see cref="short"/> array.
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

		/// <summary>
		/// Attempts to locate a <see cref="AttributeArrayConfig"/> that has the given name from <paramref name="vertexAttributeArrays"/>.
		/// </summary>
		/// <param name="vertexAttributeArrays">The list of arrays to search.</param>
		/// <param name="name">The name to locate.</param>
		/// <returns></returns>
		private static AttributeArrayConfig GetArrayByName(AttributeArrayConfig[] vertexAttributeArrays, string name) {
			foreach (AttributeArrayConfig cfg in vertexAttributeArrays) {
				if (cfg.name == name) {
					return cfg;
				}
			}
			return null;
		}


		/// <summary>
		/// A container class for bone indices and weights.
		/// </summary>
		[Obsolete] private class BoneDataContainer {

			/// <summary>
			/// Bone indices are an array of four <see cref="float"/> values. It traverses the chain of bones. An index of 0 means stop traversing (this is because bone 0 is always %ROOT%).
			/// </summary>
			public List<float[]> BoneIndices = new List<float[]>();

			/// <summary>
			/// Bone weights are an array of four <see cref="float"/> values. These values correspond to a bone index.
			/// </summary>
			public List<float[]> BoneWeights = new List<float[]>();

			public BoneDataContainer(float[] masterArray, AttributeArrayConfig bIndices, AttributeArrayConfig bWeights) {
				PopulateArray(masterArray, bIndices, BoneIndices);
				PopulateArray(masterArray, bWeights, BoneWeights);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static void PopulateArray(float[] masterArray, AttributeArrayConfig from, List<float[]> to) {
				if (from.stride == 0) throw new ClydeDataReadException("An AttributeArrayConfig has a stride of zero! This will result in an infinite loop.");

				for (int index = 0; index < from.offset; index += from.stride) {
					float[] data = masterArray.Skip(index).Take(4).ToArray();
					to.Add(data);
				}
			}

		}
	}
}

using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.Structs;
using static ThreeRingsSharp.XansData.Armature;

namespace ThreeRingsSharp.ConfigHandlers.Common {
	public static class GeometryConfigTranslator {

		public static Model3D ToModel3D(ReadFileContext ctx, ShadowClass geometryConfig, string geometryIndex, Node? subRootNodeForMesh) {
			geometryConfig.AssertIsInstanceOf("com.threerings.opengl.geometry.config.GeometryConfig");

			Model3D model = new Model3D(geometryIndex);
			MeshData? existingMeshData = MeshData.MeshDataBindings.GetOrDefault(geometryIndex);

			if (existingMeshData == null) {
				existingMeshData = new MeshData(geometryIndex);

				ShadowClass[] texCoordArrays = geometryConfig["texCoordArrays"]!;
				ShadowClass colorArray = geometryConfig["colorArray"]!;
				ShadowClass normalArray = geometryConfig["normalArray"]!;
				ShadowClass vertexArray = geometryConfig["vertexArray"]!;

				bool isIndexed = geometryConfig.IsA("com.threerings.opengl.geometry.config.GeometryConfig$IndexedStored");
				bool isSkinned = geometryConfig.IsA("com.threerings.opengl.geometry.config.GeometryConfig$SkinnedIndexedStored");

				short[]? indexArray = isIndexed ? geometryConfig["indices"] : null; // Will never be ShortBuffer - those are read as arrays in OOOReader, not java.whatever.ShortBuffer
				string[]? boneNameArray = isSkinned ? geometryConfig["bones"] : null;

				int vtxCount = vertexArray["floatArray"]!.Length * 4 / vertexArray.GetNumericField<int>("stride");

				float[] vertices;
				float[] uvs;
				float[] normals;
				ushort[] indices;
				if (isSkinned) {
					vertices = GetArray<float>(vtxCount, vertexArray);
					uvs = GetArray<float>(vtxCount, texCoordArrays);
					normals = GetArray<float>(vtxCount, normalArray);
					indices = indexArray!.Cast<ushort>().ToArray();

					ShadowClass[] attributeArrayConfigs = geometryConfig["vertexAttribArrays"]!;
					/*
					AttributeArrayConfig boneIndicesAttr = GetArrayByName(allArrays, "boneIndices");
					AttributeArrayConfig boneWeightsAttr = GetArrayByName(allArrays, "boneWeights");
					*/
					ShadowClass? boneIndicesAttr = GetArrayByName(attributeArrayConfigs, "boneIndices");
					ShadowClass? boneWeightsAttr = GetArrayByName(attributeArrayConfigs, "boneWeights");

					ushort[] boneIndexArray = GetArray<ushort>(vtxCount, boneIndicesAttr!);
					float[] boneWeightArray = GetArray<float>(vtxCount, boneWeightsAttr!);

					// Now let's consider this literally: indices and weights for bones are vertex *attribute* arrays.
					// So presumably this means that we iterate through the indices.
					// The vertex at vertices[index] is part of bone boneIndices[index]. The index returned by boneIndices is the index of a name.
					// A vertex can be in up to four groups at once, hence why these are in groups of quads.
					// If the bone group is 0, it should be ignored.

					// Apparently, this concept went way over my head in SK Animator Tools and it was a disaster. Part of why the code was so horrifying.

					// Now for ease in indexing, I'm going to bump all of the elements in the bone name array forward by 1, then set index 0 to null.
					string?[] boneNames = new string[boneNameArray!.Length + 1];
					boneNames[0] = null;
					for (int idx = 0; idx < boneNames.Length - 1; idx++) {
						boneNames[idx + 1] = boneNameArray[idx];
					}

					ushort[,] boneIndices = boneIndexArray.As2D(4);
					float[,] boneWeights = boneWeightArray.As2D(4);

					existingMeshData.BoneIndicesNative = boneIndexArray;
					existingMeshData.BoneWeightsNative = boneWeightArray;
					existingMeshData.BoneNames = boneNames;
					existingMeshData.BoneIndices = boneIndices;
					existingMeshData.BoneWeights = boneWeights;

					existingMeshData.HasBoneData = true;

					if (subRootNodeForMesh == null) {
						// The model supplied no root node. This can happen for implementations (e.g. StaticConfig) that has a GeometryConfig
						// that is skinned.
						// PRESUMABLY this means that it uses an external reference for its root node (e.g. the knight model has a common root)
						// But in my case, I really can't read that right now.
						existingMeshData.UsesExternalRoot = true;
					} else {
						existingMeshData.SetBones(subRootNodeForMesh);
					}

				} else {
					existingMeshData.HasBoneData = false;
					vertices = GetArray<float>(vtxCount, vertexArray);
					uvs = GetArray<float>(vtxCount, texCoordArrays);
					normals = GetArray<float>(vtxCount, normalArray);
					if (isIndexed) {
						indices = indexArray!.Cast<ushort>().ToArray();
					} else {
						indices = new ushort[vertices.Length];
						for (ushort i = 0; i < indices.Length; i++) {
							indices[i] = i;
						}
					}
				}

				existingMeshData.Vertices.SetFrom(Vector3.FromFloatArray(vertices));
				existingMeshData.UVs.SetFrom(Vector2.FromFloatArray(uvs));
				existingMeshData.Normals.SetFrom(Vector3.FromFloatArray(normals));
				existingMeshData.Indices.SetFrom(indices);
				existingMeshData.ConstructGroups();
			}
			if (existingMeshData != null) {
				model.Mesh = existingMeshData;
			} else {
				Debugger.Break();
			}
			return model;
		}

		private static T[] GetArray<T>(int vtxCount, params ShadowClass[] clientArrayConfigs) where T : struct {
			int offset = 0;
			int[] offsets = new int[clientArrayConfigs.Length];
			int stride = 0;
			foreach (ShadowClass sc in clientArrayConfigs) {
				sc.AssertIsInstanceOf("com.threerings.opengl.renderer.config.ClientArrayConfig");

				offsets[stride] = offset;
				offset += clientArrayConfigs[stride].GetNumericField<int>("size");
				stride++;
			}

			stride = offset;
			float[] array = new float[stride * vtxCount];
			for (int i = 0; i < clientArrayConfigs.Length; i++) {
				PopulateArray(clientArrayConfigs[i], array, offsets[i], stride);
			}
			return array.CastValue<float, T>().ToArray();
		}



		/// <summary>
		/// Using the given ClientArrayConfig, this populates the given destination array using the input offset and stride.
		/// </summary>
		/// <param name="clientArrayConfig"></param>
		/// <param name="array"></param>
		/// <param name="destinationOffset"></param>
		/// <param name="destinationStride"></param>
		private static void PopulateArray(ShadowClass clientArrayConfig, float[] array, int destinationOffset, int destinationStride) {
			clientArrayConfig.AssertIsInstanceOf("com.threerings.opengl.renderer.config.ClientArrayConfig");

			float[] source = clientArrayConfig["floatArray"]!; // Will never be FloatBuffer - those are read as arrays in OOOReader
			int sourceStride = clientArrayConfig.GetNumericField<int>("stride") / 4; // Stride/Offset are in bytes, floats are 4 bytes
			int sourceIndex = clientArrayConfig.GetNumericField<int>("offset") / 4;
			int destinationIndex = destinationOffset;
			for (int i = 0; i < source.Length / sourceStride; i++) {
				Array.ConstrainedCopy(source, sourceIndex, array, destinationIndex, clientArrayConfig.GetNumericField<int>("size"));
				sourceIndex += sourceStride;
				destinationIndex += destinationStride;
			}
		}

		/// <summary>
		/// Attempts to locate an AttributeArrayConfig that has the given name from <paramref name="vertexAttributeArrays"/>.
		/// </summary>
		/// <param name="vertexAttributeArrays">The list of arrays to search.</param>
		/// <param name="name">The name to locate.</param>
		/// <returns></returns>
		private static ShadowClass? GetArrayByName(ShadowClass[] vertexAttributeArrays, string name) {
			foreach (ShadowClass cfg in vertexAttributeArrays) {
				cfg.AssertIsInstanceOf("com.threerings.opengl.geometry.config.GeometryConfig$AttributeArrayConfig");
				if (cfg["name"] == name) {
					return cfg;
				}
			}
			return null;
		}

	}
}

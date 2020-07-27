using com.threerings.math;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.IO.GLTF.JSON;
using ThreeRingsSharp.XansData.Structs;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.Exceptions;

namespace ThreeRingsSharp.XansData.IO.GLTF {

	/// <summary>
	/// Represents a GLTF file and its associated data.
	/// </summary>
	public class GLTFExporter : AbstractModelExporter {

		/// <summary>
		/// The unique header ID describing glTF files. This is the ASCII string "glTF".
		/// </summary>
		public const uint MAGIC_NUMBER = 0x46546C67;

		/// <summary>
		/// The glTF spec version.
		/// </summary>
		public const uint VERSION = 2;

		/// <summary>
		/// If <see langword="true"/>, any textures referenced by models will be imported into the glTF file as raw binary data.
		/// </summary>
		public static bool EmbedTextures { get; set; } = false;

		/// <summary>
		/// The JSON data for this glTF file.
		/// </summary>
		private GLTFJSONRoot JSONData { get; set; }

		/// <summary>
		/// Converts <see cref="JSONData"/> into a JSON String using <see cref="JsonConvert"/>, and then ensures it's aligned to a four-byte boundary of spaces as mandated by glTF 2.0 standards.
		/// </summary>
		private string GetPaddedJSONData() {
			string jsonString = JsonConvert.SerializeObject(JSONData);
			for (int numSpacesRequired = jsonString.Length % 4; numSpacesRequired > 0; numSpacesRequired--) {
				jsonString += ' ';
			}
			return jsonString;
		}

		/// <summary>
		/// Reads an image file and returns its data + mime type
		/// </summary>
		/// <param name="imageFile"></param>
		/// <returns></returns>
		private (byte[], string) GetImageData(FileInfo imageFile) {
			Image img = Image.FromFile(imageFile.FullName);
			ImageConverter conv = new ImageConverter();
			string mime = null;
			if (imageFile.Extension.ToLower() == ".png") {
				mime = "image/png";
			} else if (imageFile.Extension.ToLower() == ".jpg" || imageFile.Extension.ToLower() == ".jpeg") {
				mime = "image/jpeg";
			}
			if (mime == null) {
				throw new InvalidTypeException("Attempted to process an image file for glTF inclusion that isn't a jpeg or png!");
			}
			return ((byte[])conv.ConvertTo(img, typeof(byte[])), mime);
		}

		/// <summary>
		/// Returns the fully-formatted JSON data buffer, ready to be written into the file.
		/// </summary>
		/// <returns></returns>
		private byte[] GetJSONBuffer() {
			string jsonStr = GetPaddedJSONData();
			byte[] jsonBuffer = new byte[8 + jsonStr.Length];
			// 8 stores the two chunk values.
			BitConverter.GetBytes(jsonStr.Length).CopyTo(jsonBuffer, 0);
			BitConverter.GetBytes(0x4E4F534A).CopyTo(jsonBuffer, 4);
			jsonStr.WriteASCIIToByteArray(ref jsonBuffer, 8);
			return jsonBuffer;
		}
		
		/// <summary>
		/// Returns the GLB portion of the file, which stores all of the necessary data. This also populates the json.
		/// </summary>
		/// <param name="models"></param>
		/// <returns></returns>
		private byte[] GetBinaryData(Model3D[] models) {
			List<byte> binBuffer = new List<byte>();
			binBuffer.AddRange(BitConverter.GetBytes(0));
			binBuffer.AddRange(BitConverter.GetBytes(0x004E4942)); // The text ".BIN" (. is null)

			#region Set Up Vars
			int currentAccessorIndex = 0; // This is identical to currentBufferViewIndex for now, it's just here in case I decide to change this behavior
			int currentBufferViewIndex = 0;
			int currentMeshIndex = 0;
			int currentModelIndex = 0;
			int currentOffset = 0;
			int totalImageCount = 0;

			int numModelsSkipped = 0;
			int numModelsCreated = 0;

			// A mapping from texture filepath to integer index for textures in the glTF data.
			Dictionary<string, int> texFileToIndexMap = new Dictionary<string, int>();

			// A mapping from MeshData to its associated GLTFMesh
			Dictionary<MeshData, GLTFMesh> meshesToGLTF = new Dictionary<MeshData, GLTFMesh>();

			// A list of node IDs that the scene SHOULD include.
			List<int> sceneNodes = new List<int>();

			// TODO: Is this tuple the best way to do this?
			var glTFToAccessors = new Dictionary<GLTFMesh, (GLTFAccessor, GLTFAccessor, GLTFAccessor, GLTFAccessor, GLTFAccessor, GLTFAccessor)>();
			#endregion

			#region Instantiate Mesh Data

			foreach (MeshData meshData in MeshData.MeshDataBindings.Values) {
				List<GLTFBufferView> bufferViews = new List<GLTFBufferView>();
				List<GLTFAccessor> accessors = new List<GLTFAccessor>();
				List<byte> buffer = new List<byte>();

				// Quick thing beforehand: Change the up axis!
				// Edit: This is broken, it causes malformed outputs.
				// meshData.ApplyAxialTransformationMod();

				if (meshData.HasBoneData && DevelopmentFlags.FLAG_DO_BONE_EXPORTS) {

					// So for bone data, the target is as follows:
					// Node that the scene references: The main armature object (a node in glTF is an object)
					// The main armature object references children.

					#region Create Buffer Accessors
					foreach (VertexGroup grp in meshData.VertexGroups) {
						#region Accessor No. 1: Vertices
						GLTFAccessor vertexAccessor = new GLTFAccessor {
							BufferView = currentBufferViewIndex,
							ThisIndex = currentAccessorIndex,
							// byteOffset = currentBufferViewSize,
							ComponentType = GLTFComponentType.FLOAT,
							Type = GLTFType.VEC3,
							Count = grp.Vertices.Count
						};
						vertexAccessor.Min.SetListCap(0f, 3);
						vertexAccessor.Max.SetListCap(0f, 3);
						foreach (Vertex vertex in grp.Vertices) {
							Vector3 point = vertex.Point;
							float x = point.X;
							float y = point.Y;
							float z = point.Z;
							buffer.AddRange(BitConverter.GetBytes(x));
							buffer.AddRange(BitConverter.GetBytes(y));
							buffer.AddRange(BitConverter.GetBytes(z));

							// This garbage is weird. But it's required by standards, so..
							if (x < vertexAccessor.Min[0]) vertexAccessor.Min[0] = x;
							if (y < vertexAccessor.Min[1]) vertexAccessor.Min[1] = y;
							if (z < vertexAccessor.Min[2]) vertexAccessor.Min[2] = z;

							if (x > vertexAccessor.Max[0]) vertexAccessor.Max[0] = x;
							if (y > vertexAccessor.Max[1]) vertexAccessor.Max[1] = y;
							if (z > vertexAccessor.Max[2]) vertexAccessor.Max[2] = z;
						}
						accessors.Add(vertexAccessor);
						int vertexSize = vertexAccessor.Count * 12; // 4 bytes per float * 3 floats
						bufferViews.Add(new GLTFBufferView {
							ThisIndex = currentBufferViewIndex,
							ByteLength = vertexSize,
							ByteOffset = currentOffset
						});
						currentOffset += vertexSize;
						currentBufferViewIndex++;
						currentAccessorIndex++;
						#endregion

						#region Accessor No. 2: Normals
						GLTFAccessor normalAccessor = new GLTFAccessor {
							BufferView = currentBufferViewIndex,
							ThisIndex = currentAccessorIndex,
							ComponentType = GLTFComponentType.FLOAT,
							Type = GLTFType.VEC3,
							Count = grp.Vertices.Count // Get the number of vertices. Yes, vertices. This is using vertex groups which uses the Vertex struct, which contains the data (normals, uvs, weights)
						};

						normalAccessor.Min.SetListCap(0f, 3);
						normalAccessor.Max.SetListCap(0f, 3);
						foreach (Vertex vertex in grp.Vertices) {
							Vector3 normal = vertex.Normal;
							float x = normal.X;
							float y = normal.Y;
							float z = normal.Z;
							buffer.AddRange(BitConverter.GetBytes(x));
							buffer.AddRange(BitConverter.GetBytes(y));
							buffer.AddRange(BitConverter.GetBytes(z));

							// This garbage is weird. But it's required by standards, so..
							if (x < normalAccessor.Min[0]) normalAccessor.Min[0] = x;
							if (y < normalAccessor.Min[1]) normalAccessor.Min[1] = y;
							if (z < normalAccessor.Min[2]) normalAccessor.Min[2] = z;

							if (x > normalAccessor.Max[0]) normalAccessor.Max[0] = x;
							if (y > normalAccessor.Max[1]) normalAccessor.Max[1] = y;
							if (z > normalAccessor.Max[2]) normalAccessor.Max[2] = z;
						}

						accessors.Add(normalAccessor);
						int normalSize = normalAccessor.Count * 12;
						bufferViews.Add(new GLTFBufferView {
							ThisIndex = currentBufferViewIndex,
							ByteLength = normalSize,
							ByteOffset = currentOffset
						});
						currentOffset += normalSize; // 4 bytes per float * 3 floats
						currentBufferViewIndex++;
						currentAccessorIndex++;
						#endregion

						#region Accessor No. 3: UVs
						GLTFAccessor uvAccessor = new GLTFAccessor {
							BufferView = currentBufferViewIndex,
							ThisIndex = currentAccessorIndex,
							// byteOffset = currentBufferViewSize,
							ComponentType = GLTFComponentType.FLOAT,
							Type = GLTFType.VEC2,
							Count = grp.Vertices.Count // Get the number of vertices. Yes, vertices. This is using vertex groups which uses the Vertex struct, which contains the data (normals, uvs, weights)
						};

						uvAccessor.Min.SetListCap(0f, 2);
						uvAccessor.Max.SetListCap(0f, 2);
						foreach (Vertex vertex in grp.Vertices) {
							Vector2 uv = vertex.UV;
							float x = uv.X;
							float y = 1 - uv.Y; // Do 1 - y because glTF coordinates have (0,0) in the top left rather than the bottom left.
							buffer.AddRange(BitConverter.GetBytes(x));
							buffer.AddRange(BitConverter.GetBytes(y));

							// This garbage is weird. But it's required by standards, so..
							if (x < uvAccessor.Min[0]) uvAccessor.Min[0] = x;
							if (y < uvAccessor.Min[1]) uvAccessor.Min[1] = y;

							if (x > uvAccessor.Max[0]) uvAccessor.Max[0] = x;
							if (y > uvAccessor.Max[1]) uvAccessor.Max[1] = y;
						}

						accessors.Add(uvAccessor);
						int uvSize = uvAccessor.Count * 8;
						bufferViews.Add(new GLTFBufferView {
							ThisIndex = currentBufferViewIndex,
							ByteLength = uvSize,
							ByteOffset = currentOffset
						});
						currentOffset += uvSize; // 4 bytes per float * 2 floats
						currentBufferViewIndex++;
						currentAccessorIndex++;
						#endregion

						#region Accessor No. 4: Indices
						GLTFAccessor indexAccessor = new GLTFAccessor {
							BufferView = currentBufferViewIndex,
							ThisIndex = currentAccessorIndex,
							// byteOffset = currentBufferViewSize,
							ComponentType = GLTFComponentType.UNSIGNED_SHORT, // OOO models use shorts for indices.
							Type = GLTFType.SCALAR,
							Count = grp.Indices.Count
						};

						indexAccessor.Min.SetListCap((ushort)0, 1);
						indexAccessor.Max.SetListCap((ushort)0, 1);
						foreach (ushort index in grp.Indices) {
							buffer.AddRange(BitConverter.GetBytes(index));
							if (index < indexAccessor.Min[0]) indexAccessor.Min[0] = index;
							if (index > indexAccessor.Max[0]) indexAccessor.Max[0] = index;
						}

						accessors.Add(indexAccessor);
						int indexSize = indexAccessor.Count * 2; // 2 bytes per ushort.
						bufferViews.Add(new GLTFBufferView {
							ThisIndex = currentBufferViewIndex,
							ByteLength = indexSize,
							ByteOffset = currentOffset
						});
						currentOffset += indexSize; // 4 bytes per float * 2 floats
						currentBufferViewIndex++;
						currentAccessorIndex++;
						#endregion

						#region Accessor No. 5: Joints
						GLTFAccessor jointAccessor = null;
						if (meshData.HasBoneData && DevelopmentFlags.FLAG_DO_BONE_EXPORTS) {
							jointAccessor = new GLTFAccessor {
								BufferView = currentBufferViewIndex,
								ThisIndex = currentAccessorIndex,
								ComponentType = GLTFComponentType.UNSIGNED_SHORT,
								Type = GLTFType.VEC4,
								Count = meshData.BoneIndices.GetLength(0)
							};
							// This can't use min/max.
							accessors.Add(jointAccessor);
							int jointSize = jointAccessor.Count * 4 * 2; // 4 elements per VEC4, 2 bytes per ushort.
							bufferViews.Add(new GLTFBufferView {
								ThisIndex = currentBufferViewIndex,
								ByteLength = jointSize,
								ByteOffset = currentOffset
							});
							currentOffset += jointSize;
							currentBufferViewIndex++;
							currentAccessorIndex++;
						}
						#endregion

						#region Accessor No. 6: Weights
						GLTFAccessor weightAccessor = null;
						if (meshData.HasBoneData && DevelopmentFlags.FLAG_DO_BONE_EXPORTS) {
							weightAccessor = new GLTFAccessor {
								BufferView = currentBufferViewIndex,
								ThisIndex = currentAccessorIndex,
								ComponentType = GLTFComponentType.FLOAT,
								Type = GLTFType.VEC4,
								Count = meshData.BoneWeights.GetLength(0)
							};
							accessors.Add(weightAccessor);
							int weightSize = weightAccessor.Count * 4 * 4; // 4 elements per VEC4, 4 bytes per float;
							bufferViews.Add(new GLTFBufferView {
								ThisIndex = currentBufferViewIndex,
								ByteLength = weightSize,
								ByteOffset = currentOffset
							});
							currentOffset += weightSize;
							currentBufferViewIndex++;
							currentAccessorIndex++;
						}
						#endregion
					}
					#endregion

				} else {

					#region Create Buffer Accessors

					#region Accessor No. 1: Vertices
					GLTFAccessor vertexAccessor = new GLTFAccessor {
						BufferView = currentBufferViewIndex,
						ThisIndex = currentAccessorIndex,
						// byteOffset = currentBufferViewSize,
						ComponentType = GLTFComponentType.FLOAT,
						Type = GLTFType.VEC3,
						Count = meshData.Vertices.Count
					};
					vertexAccessor.Min.SetListCap(0f, 3);
					vertexAccessor.Max.SetListCap(0f, 3);
					foreach (Vector3 vertex in meshData.Vertices) {
						float x = vertex.X - meshData.VertexOffset.X;
						float y = vertex.Y - meshData.VertexOffset.Y;
						float z = vertex.Z - meshData.VertexOffset.Z;
						buffer.AddRange(BitConverter.GetBytes(x));
						buffer.AddRange(BitConverter.GetBytes(y));
						buffer.AddRange(BitConverter.GetBytes(z));

						// This garbage is weird. But it's required by standards, so..
						if (x < vertexAccessor.Min[0]) vertexAccessor.Min[0] = x;
						if (y < vertexAccessor.Min[1]) vertexAccessor.Min[1] = y;
						if (z < vertexAccessor.Min[2]) vertexAccessor.Min[2] = z;

						if (x > vertexAccessor.Max[0]) vertexAccessor.Max[0] = x;
						if (y > vertexAccessor.Max[1]) vertexAccessor.Max[1] = y;
						if (z > vertexAccessor.Max[2]) vertexAccessor.Max[2] = z;
					}
					accessors.Add(vertexAccessor);
					int vertexSize = vertexAccessor.Count * 12;
					bufferViews.Add(new GLTFBufferView {
						ThisIndex = currentBufferViewIndex,
						ByteLength = vertexSize,
						ByteOffset = currentOffset
					});
					currentOffset += vertexSize; // 4 bytes per float * 3 floats
					currentBufferViewIndex++;
					currentAccessorIndex++;
					#endregion

					#region Accessor No. 2: Normals
					GLTFAccessor normalAccessor = new GLTFAccessor {
						BufferView = currentBufferViewIndex,
						ThisIndex = currentAccessorIndex,
						// byteOffset = currentBufferViewSize,
						ComponentType = GLTFComponentType.FLOAT,
						Type = GLTFType.VEC3,
						Count = meshData.Normals.Count
					};
					
					normalAccessor.Min.SetListCap(0f, 3);
					normalAccessor.Max.SetListCap(0f, 3);
					foreach (Vector3 normal in meshData.Normals) {
						float x = normal.X;
						float y = normal.Y;
						float z = normal.Z;
						buffer.AddRange(BitConverter.GetBytes(x));
						buffer.AddRange(BitConverter.GetBytes(y));
						buffer.AddRange(BitConverter.GetBytes(z));

						// This garbage is weird. But it's required by standards, so..
						if (x < normalAccessor.Min[0]) normalAccessor.Min[0] = x;
						if (y < normalAccessor.Min[1]) normalAccessor.Min[1] = y;
						if (z < normalAccessor.Min[2]) normalAccessor.Min[2] = z;

						if (x > normalAccessor.Max[0]) normalAccessor.Max[0] = x;
						if (y > normalAccessor.Max[1]) normalAccessor.Max[1] = y;
						if (z > normalAccessor.Max[2]) normalAccessor.Max[2] = z;
					}
					
					accessors.Add(normalAccessor);
					int normalSize = normalAccessor.Count * 12;
					bufferViews.Add(new GLTFBufferView {
						ThisIndex = currentBufferViewIndex,
						ByteLength = normalSize,
						ByteOffset = currentOffset
					});
					currentOffset += normalSize; // 4 bytes per float * 3 floats
					currentBufferViewIndex++;
					currentAccessorIndex++;
					#endregion

					#region Accessor No. 3: UVs
					GLTFAccessor uvAccessor = new GLTFAccessor {
						BufferView = currentBufferViewIndex,
						ThisIndex = currentAccessorIndex,
						// byteOffset = currentBufferViewSize,
						ComponentType = GLTFComponentType.FLOAT,
						Type = GLTFType.VEC2,
						Count = meshData.UVs.Count
					};
					
					uvAccessor.Min.SetListCap(0f, 2);
					uvAccessor.Max.SetListCap(0f, 2);
					foreach (Vector2 uv in meshData.UVs) {
						float x = uv.X;
						float y = 1 - uv.Y; // Do 1 - y because glTF coordinates have (0,0) in the top left rather than the bottom left.
						buffer.AddRange(BitConverter.GetBytes(x));
						buffer.AddRange(BitConverter.GetBytes(y));

						// This garbage is weird. But it's required by standards, so..
						if (x < uvAccessor.Min[0]) uvAccessor.Min[0] = x;
						if (y < uvAccessor.Min[1]) uvAccessor.Min[1] = y;

						if (x > uvAccessor.Max[0]) uvAccessor.Max[0] = x;
						if (y > uvAccessor.Max[1]) uvAccessor.Max[1] = y;
					}
					
					accessors.Add(uvAccessor);
					int uvSize = uvAccessor.Count * 8;
					bufferViews.Add(new GLTFBufferView {
						ThisIndex = currentBufferViewIndex,
						ByteLength = uvSize,
						ByteOffset = currentOffset
					});
					currentOffset += uvSize; // 4 bytes per float * 2 floats
					currentBufferViewIndex++;
					currentAccessorIndex++;
					#endregion

					#region Accessor No. 4: Indices
					GLTFAccessor indexAccessor = new GLTFAccessor {
						BufferView = currentBufferViewIndex,
						ThisIndex = currentAccessorIndex,
						// byteOffset = currentBufferViewSize,
						ComponentType = GLTFComponentType.UNSIGNED_SHORT, // OOO models use shorts for indices.
						Type = GLTFType.SCALAR,
						Count = meshData.Indices.Count
					};
					
					indexAccessor.Min.SetListCap((ushort)0, 1);
					indexAccessor.Max.SetListCap((ushort)0, 1);
					foreach (ushort index in meshData.Indices) {
						buffer.AddRange(BitConverter.GetBytes(index));
						if (index < indexAccessor.Min[0]) indexAccessor.Min[0] = index;
						if (index > indexAccessor.Max[0]) indexAccessor.Max[0] = index;
					}
					
					accessors.Add(indexAccessor);
					int indexSize = indexAccessor.Count * 2; // 2 bytes per ushort.
					bufferViews.Add(new GLTFBufferView {
						ThisIndex = currentBufferViewIndex,
						ByteLength = indexSize,
						ByteOffset = currentOffset
					});
					currentOffset += indexSize; // 4 bytes per float * 2 floats
					currentBufferViewIndex++;
					currentAccessorIndex++;
					#endregion

					#endregion

					#region Create Mesh
					GLTFPrimitive primitive = new GLTFPrimitive {
						Indices = indexAccessor.ThisIndex,
						Attributes = new GLTFPrimitiveAttribute {
							Normal = normalAccessor.ThisIndex,
							Position = vertexAccessor.ThisIndex,
							TexCoord0 = uvAccessor.ThisIndex
						}
					};

					GLTFMesh mesh = new GLTFMesh {
						// Primitives has a length of 1 by default so ima use that to my advantage.
						ThisIndex = currentMeshIndex,
						Name = meshData.Name,
						Primitives = new List<GLTFPrimitive>() { primitive }
					};
					currentMeshIndex++;
					#endregion

					#region Register Data
					// Register all data to the glTF JSON and Binary data.
					binBuffer.AddRange(buffer);
					foreach (GLTFBufferView bufferView in bufferViews) JSONData.BufferViews.Add(bufferView);
					foreach (GLTFAccessor accessor in accessors) JSONData.Accessors.Add(accessor);
					JSONData.Meshes.Add(mesh);
					meshesToGLTF[meshData] = mesh;
					glTFToAccessors[mesh] = (vertexAccessor, normalAccessor, uvAccessor, indexAccessor, null, null);
					#endregion

				}

			}

			#endregion

			#region Instantiate Textures
			// Now textures may have multiple users from different models. We need to try to create a conglomerate representation of textures.
			// This is kind of like MeshData compared to Model3D -- multiple Model3Ds may share MeshData.

			foreach (Model3D model in models) {
				foreach (string texPath in model.Textures) {
					if (texPath != null && texPath.Length > 0) {
						FileInfo texFile = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + texPath);
						if (texFile.Exists) {
							// Sometimes these reference stuff like photoshop files, so check if it actually exists.
							// TODO: Find out how these map out to actual images.
							if (!texFileToIndexMap.ContainsKey(texPath)) {
								// New texture. Append it.
								texFileToIndexMap[texPath] = totalImageCount;

								if (EmbedTextures) {
									XanLogger.WriteLine($"Embedding [{texFile.FullName}].", true);

									#region Assign Core Data
									(byte[], string) iData = GetImageData(texFile);

									#region Create Buffer View & Write Bytes
									binBuffer.AddRange(iData.Item1);
									GLTFBufferView imageView = new GLTFBufferView() {
										ThisIndex = currentBufferViewIndex,
										ByteLength = iData.Item1.Length,
										ByteOffset = currentOffset
									};
									currentOffset += iData.Item1.Length;
									currentBufferViewIndex++;
									#endregion

									#region Create Image & Texture
									GLTFImage image = new GLTFImage() {
										ThisIndex = totalImageCount,
										BufferView = imageView.ThisIndex,
										MimeType = iData.Item2
									};
									GLTFTexture tex = new GLTFTexture() {
										Source = image.ThisIndex
									};
									#endregion

									#region Create Material
									GLTFMaterial material = new GLTFMaterial() {
										Name = texFile.Name.Replace(texFile.Extension, "")
									};
									material.PbrMetallicRoughness.BaseColorTexture.Index = image.ThisIndex;
									material.PbrMetallicRoughness.BaseColorTexture.TexCoord = 0;
									#endregion

									#endregion

									#region Register Data
									JSONData.BufferViews.Add(imageView);
									JSONData.Images.Add(image);
									JSONData.Textures.Add(tex);
									JSONData.Materials.Add(material);
									#endregion

								} else {
									XanLogger.WriteLine($"Adding reference to [{texFile.FullName}].", true);

									#region Assign Core Data
									#region Create Image & Texture
									GLTFImage image = new GLTFImage() {
										ThisIndex = totalImageCount,
										URI = texFile.FullName.Replace('\\', '/')
									};
									GLTFTexture tex = new GLTFTexture() {
										Source = image.ThisIndex
									};
									#endregion

									#region Create Material
									GLTFMaterial material = new GLTFMaterial() {
										Name = texFile.Name.Replace(texFile.Extension, "")
									};
									material.PbrMetallicRoughness.BaseColorTexture.Index = image.ThisIndex;
									material.PbrMetallicRoughness.BaseColorTexture.TexCoord = 0;
									#endregion
									#endregion

									#region Register Data
									JSONData.Images.Add(image);
									JSONData.Textures.Add(tex);
									JSONData.Materials.Add(material);
									#endregion
								}
								totalImageCount++;
							}
						} else {
							XanLogger.WriteLine($"Attempt to create image [{texFile.FullName}] failed -- File does not exist.", true);
						}
					} else {
						XanLogger.WriteLine($"Attempt to create image failed -- Image is null!", true);
					}
				}
			}

			if (JSONData.Textures.Count > 0) JSONData.Samplers.Add(new GLTFSampler());

			#endregion

			#region Instantiate Models

			foreach (Model3D model in models) {
				bool skip = (bool)model.ExtraData.GetOrDefault("SkipExport", false);
				if (skip) {
					numModelsSkipped++;
					continue; // Go to the next iteration.
				}
				numModelsCreated++;
				// This can get a bit awkward.
				// For static models, we make one node to represent the model.
				// For skinned models, we make one node to represent each *bone*.
				if (!meshesToGLTF.ContainsKey(model.Mesh)) throw new InvalidOperationException("A Model3D referenced a mesh that was not in the registry! Model: " + model.Name);
				GLTFMesh glMesh = meshesToGLTF[model.Mesh];
				var accessors = glTFToAccessors[glMesh];
				// vertexAccessor, normalAccessor, uvAccessor, indexAccessor, jointAccessor, weightAccessor
				GLTFAccessor vertexAccessor = accessors.Item1;
				GLTFAccessor normalAccessor = accessors.Item2;
				GLTFAccessor uvAccessor = accessors.Item3;
				GLTFAccessor indexAccessor = accessors.Item4;

				// These two may be null.
				GLTFAccessor jointAccessor = accessors.Item5;
				GLTFAccessor weightAccessor = accessors.Item6;

				#region Append Rigging (WIP)
				if (model.Mesh.HasBoneData && DevelopmentFlags.FLAG_DO_BONE_EXPORTS) {

				}
				#endregion

				#region Append Animations (WIP)
				if (model.Mesh.HasBoneData && DevelopmentFlags.FLAG_DO_BONE_EXPORTS) {

				}
				#endregion

				#region Create Object
				if (model.Mesh.HasBoneData && DevelopmentFlags.FLAG_DO_BONE_EXPORTS) {
					#region Create Object (Skinned)

					#endregion
				} else {
					#region Create Object (Static)
					GLTFNode node = new GLTFNode {
						Name = model.Name,
						Mesh = glMesh.ThisIndex
					};
					if (JSONData.Materials.Count > 0) {
							string fullName = model.Textures.Where(texturePath => {
								if (texturePath != null) {
									return new FileInfo(texturePath).Name == model.ActiveTexture;
								} else {
									return false;
								}
							}).FirstOrDefault();
							if (fullName != null) glMesh.Primitives[0].Material = texFileToIndexMap[fullName];
						
					}
					model.ApplyScaling();
					node.SetTransform(model.Transform);
					JSONData.Nodes.Add(node);
					#endregion
				}
				#endregion

				currentModelIndex++;
			}

			#endregion

			#region Instantiate Scene & Buffer Refs

			GLTFScene mainScene = new GLTFScene {
				Name = "Scene",
				Nodes = new List<int>(currentModelIndex)
			};
			for (int idx = 0; idx < currentModelIndex; idx++) {
				mainScene.Nodes.Add(idx);
			}
			JSONData.Scenes.Add(mainScene);

			// Add the null spacing as mandated by gltf standards.
			int bytesToAdd = binBuffer.Count % 4;
			for (int idx = 0; idx < bytesToAdd; idx++) binBuffer.Add(0);

			// Add the length value. Subtract 8 because the length does not include the header (which is part of binBuffer's data, and needs to be removed)
			BitConverter.GetBytes(binBuffer.Count - 8).CopyToList(binBuffer, 0);
			JSONData.Buffers.Add(new GLTFBuffer {
				ByteLength = binBuffer.Count - 8
			});

			#endregion

			XanLogger.WriteLine($"glTF Exporter instantiated {numModelsCreated} models (skipped {numModelsSkipped} models).");

			return binBuffer.ToArray();
		}

		public override void Export(Model3D[] models, FileInfo toFile) {
			JSONData = new GLTFJSONRoot();
			byte[] binaryData = GetBinaryData(models);
			byte[] jsonBuffer = GetJSONBuffer();
			using (FileStream writeStr = toFile.OpenWriteNew()) {
				using (BinaryWriter writer = new BinaryWriter(writeStr)) {
					// Write asset data
					writer.Write(MAGIC_NUMBER);
					writer.Write(VERSION);
					writer.Write(jsonBuffer.Length + binaryData.Length + 12);
					writer.Write(jsonBuffer);
					writer.Write(binaryData);
				}
			}
		}
	}
}

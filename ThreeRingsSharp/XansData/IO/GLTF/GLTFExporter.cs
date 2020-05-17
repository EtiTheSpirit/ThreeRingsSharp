using java.nio.channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.IO.GLTF.JSON;
using ThreeRingsSharp.XansData.Structs;

namespace ThreeRingsSharp.XansData.IO.GLTF {

	/// <summary>
	/// Represents a GLTF file and its associated data.
	/// </summary>
	public class GLTFExporter : AbstractModelExporter {

		/*
		/// <summary>
		/// Initialize camel case serialization.
		/// </summary>
		static GLTFExporter() {
		// I *guess* this is the best place to put this.
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
		}
		*/

		/// <summary>
		/// The unique header ID describing glTF files. This is the ASCII string "glTF".
		/// </summary>
		public const uint MAGIC_NUMBER = 0x46546C67;

		/// <summary>
		/// The glTF spec version.
		/// </summary>
		public const uint VERSION = 2;

		/// <summary>
		/// The JSON data for this glTF file.
		/// </summary>
		private readonly GLTFJSONRoot JSONData = new GLTFJSONRoot();

		/// <summary>
		/// Converts <see cref="JSONData"/> into a JSON String using <see cref="JsonConvert"/>, and then ensures it's aligned to a four-byte boundary as mandated by glTF 2.0 standards.
		/// </summary>
		private string GetPaddedJSONData() {
			string jsonString = JsonConvert.SerializeObject(JSONData);
			for (int numSpacesRequired = jsonString.Length % 4; numSpacesRequired > 0; numSpacesRequired--) {
				jsonString += ' ';
			}
			return jsonString;
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

		private byte[] GetBinaryData(Model3D[] models) {
			List<byte> binBuffer = new List<byte>();
			binBuffer.AddRange(BitConverter.GetBytes(0));
			binBuffer.AddRange(BitConverter.GetBytes(0x004E4942));

			// int lastLength = 0;
			int currentBufferViewIndex = 0;
			int currentAccessorBaseIndex = 0;
			int currentModelIndex = 0;
			int currentOffset = 0;
			foreach (Model3D model in models) {
				model.ApplyTransformations();

				// Need to construct a new bufferview for the individual model.
				// We also need to construct accessors for each overall component type.
				List<GLTFBufferView> bufferViews = new List<GLTFBufferView>();
				List<GLTFAccessor> accessors = new List<GLTFAccessor>();
				List<byte> buffer = new List<byte>();

				#region Accessor No. 1: Vertices
				GLTFAccessor vertexAccessor = new GLTFAccessor {
					bufferView = currentBufferViewIndex,
					// byteOffset = currentBufferViewSize,
					componentType = GLTFComponentType.FLOAT,
					type = GLTFType.VEC3,
					count = model.Vertices.Count
				};
				vertexAccessor.min.SetListCap(0f, 3);
				vertexAccessor.max.SetListCap(0f, 3);
				foreach (Vector3 vertex in model.Vertices) {
					float x = vertex.X;
					float y = vertex.Y;
					float z = vertex.Z;
					buffer.AddRange(BitConverter.GetBytes(x));
					buffer.AddRange(BitConverter.GetBytes(y));
					buffer.AddRange(BitConverter.GetBytes(z));

					// This garbage is weird. But it's required by standards, so..
					if (x < vertexAccessor.min[0]) vertexAccessor.min[0] = x;
					if (y < vertexAccessor.min[1]) vertexAccessor.min[1] = y;
					if (z < vertexAccessor.min[2]) vertexAccessor.min[2] = z;

					if (x > vertexAccessor.max[0]) vertexAccessor.max[0] = x;
					if (y > vertexAccessor.max[1]) vertexAccessor.max[1] = y;
					if (z > vertexAccessor.max[2]) vertexAccessor.max[2] = z;
				}
				accessors.Add(vertexAccessor);
				int vertexSize = vertexAccessor.count * 12;
				bufferViews.Add(new GLTFBufferView {
					byteLength = vertexSize,
					byteOffset = currentOffset
				});
				currentOffset += vertexSize; // 4 bytes per float * 3 floats
				currentBufferViewIndex++;

				#endregion

				#region Accessor No. 2: Normals
				GLTFAccessor normalAccessor = new GLTFAccessor {
					bufferView = currentBufferViewIndex,
					// byteOffset = currentBufferViewSize,
					componentType = GLTFComponentType.FLOAT,
					type = GLTFType.VEC3,
					count = model.Normals.Count
				};
				normalAccessor.min.SetListCap(0f, 3);
				normalAccessor.max.SetListCap(0f, 3);
				foreach (Vector3 normal in model.Normals) {
					float x = normal.X;
					float y = normal.Y;
					float z = normal.Z;
					buffer.AddRange(BitConverter.GetBytes(x));
					buffer.AddRange(BitConverter.GetBytes(y));
					buffer.AddRange(BitConverter.GetBytes(z));

					// This garbage is weird. But it's required by standards, so..
					if (x < normalAccessor.min[0]) normalAccessor.min[0] = x;
					if (y < normalAccessor.min[1]) normalAccessor.min[1] = y;
					if (z < normalAccessor.min[2]) normalAccessor.min[2] = z;

					if (x > normalAccessor.max[0]) normalAccessor.max[0] = x;
					if (y > normalAccessor.max[1]) normalAccessor.max[1] = y;
					if (z > normalAccessor.max[2]) normalAccessor.max[2] = z;
				}
				accessors.Add(normalAccessor);
				int normalSize = normalAccessor.count * 12;
				bufferViews.Add(new GLTFBufferView {
					byteLength = normalSize,
					byteOffset = currentOffset
				});
				currentOffset += normalSize; // 4 bytes per float * 3 floats
				currentBufferViewIndex++;
				#endregion

				#region Accessor No. 3: UVs
				GLTFAccessor uvAccessor = new GLTFAccessor {
					bufferView = currentBufferViewIndex,
					// byteOffset = currentBufferViewSize,
					componentType = GLTFComponentType.FLOAT,
					type = GLTFType.VEC2,
					count = model.UVs.Count
				};
				uvAccessor.min.SetListCap(0f, 2);
				uvAccessor.max.SetListCap(0f, 2);
				foreach (Vector2 uv in model.UVs) {
					float x = uv.X;
					float y = uv.Y;
					buffer.AddRange(BitConverter.GetBytes(x));
					buffer.AddRange(BitConverter.GetBytes(y));

					// This garbage is weird. But it's required by standards, so..
					if (x < uvAccessor.min[0]) uvAccessor.min[0] = x;
					if (y < uvAccessor.min[1]) uvAccessor.min[1] = y;

					if (x > uvAccessor.max[0]) uvAccessor.max[0] = x;
					if (y > uvAccessor.max[1]) uvAccessor.max[1] = y;
				}
				accessors.Add(uvAccessor);
				int uvSize = uvAccessor.count * 8;
				bufferViews.Add(new GLTFBufferView {
					byteLength = uvSize,
					byteOffset = currentOffset
				});
				currentOffset += uvSize; // 4 bytes per float * 2 floats
				currentBufferViewIndex++;
				#endregion

				#region Accessor No. 4: Indices
				GLTFAccessor indexAccessor = new GLTFAccessor {
					bufferView = currentBufferViewIndex,
					// byteOffset = currentBufferViewSize,
					componentType = GLTFComponentType.SHORT, // OOO models use shorts for indices.
					type = GLTFType.SCALAR,
					count = model.Indices.Count
				};
				indexAccessor.min.SetListCap((short)0, 1);
				indexAccessor.max.SetListCap((short)0, 1);
				foreach (short index in model.Indices) {
					buffer.AddRange(BitConverter.GetBytes(index));
					if (index < indexAccessor.min[0]) indexAccessor.min[0] = index;
					if (index > indexAccessor.max[0]) indexAccessor.max[0] = index;
				}
				accessors.Add(indexAccessor);
				int indexSize = indexAccessor.count * 2; // 2 bytes per short.
				bufferViews.Add(new GLTFBufferView {
					byteLength = indexSize,
					byteOffset = currentOffset
				});
				currentOffset += indexSize; // 4 bytes per float * 2 floats
				currentBufferViewIndex++;
				#endregion

				// Now add this + all of the accessors to json.
				foreach (GLTFBufferView bufferView in bufferViews) JSONData.bufferViews.Add(bufferView);
				foreach (GLTFAccessor accessor in accessors) JSONData.accessors.Add(accessor);

				// Make the mesh representation.
				GLTFMesh mesh = new GLTFMesh {
					// Primitives has a length of 1 by default so ima use that to my advantage.
					primitives = new List<GLTFPrimitive>() {
						new GLTFPrimitive {
							indices = currentAccessorBaseIndex + 3,
							attributes = new GLTFPrimitiveAttribute {
								NORMAL = currentAccessorBaseIndex + 1,
								POSITION = currentAccessorBaseIndex,
								TEXCOORD_0 = currentAccessorBaseIndex + 2
							}
						}
					}
				};
				JSONData.meshes.Add(mesh);
				JSONData.nodes.Add(new GLTFNode {
					name = model.Name,
					mesh = currentModelIndex
				});
				
				binBuffer.AddRange(buffer);
				currentAccessorBaseIndex += accessors.Count;
				currentModelIndex++;
			}

			GLTFScene mainScene = new GLTFScene {
				name = "Scene",
				nodes = new List<int>(currentModelIndex)
			};
			for (int idx = 0; idx < currentModelIndex; idx++) {
				mainScene.nodes.Add(idx);
			}
			JSONData.scenes.Add(mainScene);

			int bytesToAdd = binBuffer.Count % 4;
			for (int idx = 0; idx < bytesToAdd; idx++) binBuffer.Add(0);

			BitConverter.GetBytes(binBuffer.Count - 8).CopyToList(binBuffer, 0);
			JSONData.buffers.Add(new GLTFBuffer {
				byteLength = binBuffer.Count - 8
			});
			return binBuffer.ToArray();
		}

		public override void Export(Model3D[] models, FileInfo toFile) {
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

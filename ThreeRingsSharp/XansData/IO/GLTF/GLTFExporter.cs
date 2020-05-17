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

namespace ThreeRingsSharp.XansData.IO.GLTF {

	/// <summary>
	/// Represents a GLTF file and its associated data.
	/// </summary>
	public class GLTFExporter : AbstractModelExporter {

		/// <summary>
		/// Initialize camel case serialization.
		/// </summary>
		static GLTFExporter() {
		// I *guess* this is the best place to put this.
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
		}

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
		private GLTFJSONRoot JSONData = new GLTFJSONRoot();

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

		private byte[] GetBinaryData() {
			return new byte[0];
		}

		public override void Export(Model3D[] models, FileInfo toFile) {
			byte[] jsonBuffer = GetJSONBuffer();
			byte[] binaryData = GetBinaryData();
			using (FileStream writeStr = toFile.OpenWriteNew()) {
				using (BinaryWriter writer = new BinaryWriter(writeStr)) {
					// Write asset data
					writer.Write(MAGIC_NUMBER);
					writer.Write(VERSION);
					writer.Write(jsonBuffer.Length + binaryData.Length + 8);
					writer.Write(jsonBuffer);
					writer.Write(binaryData);
				}
			}
		}
	}
}

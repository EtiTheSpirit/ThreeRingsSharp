using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents the baseColorTexture node in the pbrMetallicRoughness node of materials.
	/// </summary>
	public class GLTFPBRBaseColor : GLTFObject {

		/// <summary>
		/// The texture index
		/// </summary>
		[JsonProperty("index")] public int Index = 0;

		/// <summary>
		/// The UV index
		/// </summary>
		[JsonProperty("texCoord")] public int TexCoord = 0;

	}
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents a binary data buffer.
	/// </summary>
	public class GLTFBuffer {

		/// <summary>
		/// The amount of bytes in this buffer.
		/// </summary>
		[JsonProperty("byteLength")] public int ByteLength = 0;

	}
}

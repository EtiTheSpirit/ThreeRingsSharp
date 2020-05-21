using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an image, usually for a texture.
	/// </summary>
	public class GLTFImage {

		// <summary>
		// The path to the image.
		// </summary>
		//[JsonProperty("uri")] public string URI;

		/// <summary>
		/// Used as a tricky method of referencing this accessor in a node. This is the index of the accessor itself in the json data.
		/// </summary>
		[JsonIgnore] public int ThisIndex = 0;

		/// <summary>
		/// The location of this image in the buffer.
		/// </summary>
		[JsonProperty("bufferView")] public int BufferView;

		[JsonProperty("mimeType")] public string MimeType = "image/png";

	}
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Represents an animation object in a GLTF file.
	/// </summary>
	public class GLTFAnimation : GLTFObject {

		[JsonProperty("name")] public string Name = "NULL";

		[JsonProperty("channels")] public object[] Channels;

		[JsonProperty("samplers")] public object[] Samplers;

	}
}

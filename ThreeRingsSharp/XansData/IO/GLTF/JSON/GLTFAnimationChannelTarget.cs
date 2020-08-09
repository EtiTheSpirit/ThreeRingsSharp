using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFAnimationChannelTarget : GLTFObject {

		[JsonProperty("node")] public int Node;

		[JsonProperty("path")] public string Path;

	}
}

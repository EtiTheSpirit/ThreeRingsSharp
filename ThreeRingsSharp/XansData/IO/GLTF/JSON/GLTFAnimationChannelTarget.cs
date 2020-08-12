using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFAnimationChannelTarget : GLTFObject {

		[JsonProperty("node")] public int Node;

		[JsonProperty("path")] public string Path;

	}
}

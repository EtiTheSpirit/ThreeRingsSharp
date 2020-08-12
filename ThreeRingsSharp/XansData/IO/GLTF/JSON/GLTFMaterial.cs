using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFMaterial : GLTFObject {

		/// <summary>
		/// The name of this material.
		/// </summary>
		[JsonProperty("name")] public string Name = "Material";

		/// <summary>
		/// The metallic roughness determining factor. This also stores the base texture.
		/// </summary>
		[JsonProperty("pbrMetallicRoughness")] public GLTFPBRMetallicRoughness PbrMetallicRoughness = new GLTFPBRMetallicRoughness();

		/// <summary>
		/// The alpha mode of this material. OPAQUE means alpha is ignored, MASK means there is an alpha mask image attached, and BLEND uses image alpha.
		/// </summary>
		[JsonProperty("alphaMode")] public string AlphaMode = "BLEND";

	}
}

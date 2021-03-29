using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// The PBR Metallic Roughness material attribute.
	/// </summary>
	public class GLTFPBRMetallicRoughness : GLTFObject {

		/// <summary>
		/// The modifier to the base color of the material. Default is white (so no changes)
		/// </summary>
		[JsonProperty("baseColorFactor")] public float[] BaseColorFactor = new float[4] { 1, 1, 1, 1 };

		/// <summary>
		/// The base texture.
		/// </summary>
		[JsonProperty("baseColorTexture")] public GLTFPBRBaseColor BaseColorTexture = new GLTFPBRBaseColor();

		/// <summary>
		/// How metallic the material is. Default is 0%.
		/// </summary>
		[JsonProperty("metallicFactor")] public float MetallicFactor = 0f;

		/// <summary>
		/// How rough the material is (how un-shiny). Default is 100%.
		/// </summary>
		[JsonProperty("roughnessFactor")] public float RoughnessFactor = 1f;

	}
}

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
		[JsonProperty("pbrMetallicRoughness")] public GLTFPBRMetallicRoughness PBRMetallicRoughness = new GLTFPBRMetallicRoughness();

		/// <summary>
		/// The alpha mode of this material. OPAQUE, MASK, and BLEND are valid options. Default value is MASK.
		/// </summary>
		[JsonProperty("alphaMode")] public string AlphaMode = "MASK";

		/// <summary>
		/// The threshold from which a transparent pixel is dubbed transparent enough to discard. Only relevant if <see cref="AlphaMode"/> is MASK. Default value is 0.5f
		/// </summary>
		[JsonProperty("alphaCutoff ")] public float AlphaCutoff = 0.5f;

		#region Newtonsoft Field Write Conditions
		// These are referenced by newtonsoft during runtime.
		// Format: ShouldSerialize...
		// Replace ... with the name of the field.

		public bool ShouldSerializePbrMetallicRoughness() => PBRMetallicRoughness != null;

		#endregion

	}
}

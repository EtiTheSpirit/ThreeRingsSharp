using Newtonsoft.Json;
using System.Collections.Generic;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Provides access to a <see cref="GLTFBufferView"/>.
	/// </summary>
	public class GLTFAccessor {

		/// <summary>
		/// Used as a tricky method of referencing this accessor in a node. This is the index of the accessor itself in the json data.
		/// </summary>
		[JsonIgnore]
		public int ThisIndex = 0;

		/// <summary>
		/// The <see cref="GLTFBufferView"/> this points to.
		/// </summary>
		[JsonProperty("bufferView")] public int BufferView = 0;

		/// <summary>
		/// The data type of the value stored in this <see cref="GLTFAccessor"/>.
		/// </summary>
		[JsonProperty("componentType")] public int ComponentType = GLTFComponentType.BYTE;

		/// <summary>
		/// The amount of values stored within this object.
		/// </summary>
		[JsonProperty("count")] public int Count = 0;

		/// <summary>
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="ComponentType"/>.
		/// </summary>
		[JsonProperty("max")] public List<dynamic> Max = new List<dynamic>();

		/// <summary>
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="ComponentType"/>.
		/// </summary>
		[JsonProperty("min")] public List<dynamic> Min = new List<dynamic>();

		/// <summary>
		/// The type of model data this accessor represents, which determines is size.
		/// </summary>
		[JsonProperty("type")] public string Type = GLTFType.SCALAR;

	}
}

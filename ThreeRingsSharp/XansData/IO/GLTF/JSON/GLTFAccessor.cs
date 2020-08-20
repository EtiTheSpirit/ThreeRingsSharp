using Newtonsoft.Json;
using System.Collections.Generic;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// Provides access to a <see cref="GLTFBufferView"/>.
	/// </summary>
	public class GLTFAccessor : GLTFObject {

		#region Automatic Size

		/// <summary>
		/// The size of this <see cref="GLTFAccessor"/> in bytes. Useful for its associated <see cref="GLTFBufferView"/>.
		/// </summary>
		[JsonIgnore]
		public int Size {
			get {
				if (NeedsToCalculateSize) {
					CachedSize = Count * AttributeExtension.GetSize(typeof(GLTFComponentType), ComponentType) * AttributeExtension.GetSize(typeof(GLTFValueType), Type);
					LastComponentType = ComponentType;
					LastType = Type;
					HasCalculatedSize = true;
				}
				return CachedSize;
			}
		}

		#region Size Helpers
		[JsonIgnore] private int CachedSize = -1;
		[JsonIgnore] private bool HasCalculatedSize = false;
		[JsonIgnore] private int LastComponentType = GLTFComponentType.BYTE;
		[JsonIgnore] private string LastType = GLTFValueType.SCALAR;

		/// <summary>
		/// Will be <see langword="true"/> if <see cref="Size"/> needs to be recalculated.
		/// </summary>
		[JsonIgnore] private bool NeedsToCalculateSize => !HasCalculatedSize || (LastComponentType != ComponentType) || (LastType != Type);

		#endregion

		#endregion

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
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="ComponentType"/>.<para/>
		/// Set the count to zero to skip writing this data.
		/// </summary>
		[JsonProperty("max")] public List<dynamic> Max = new List<dynamic>();

		/// <summary>
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="ComponentType"/>.<para/>
		/// Set the count to zero to skip writing this data.
		/// </summary>
		[JsonProperty("min")] public List<dynamic> Min = new List<dynamic>();

		/// <summary>
		/// The type of model data this accessor represents, which determines is size.
		/// </summary>
		[JsonProperty("type")] public string Type = GLTFValueType.SCALAR;

		/// <summary>
		/// The offset in the referenced buffer that this accessor should start at.
		/// </summary>
		[JsonProperty("byteOffset")] public int ByteOffset = 0;

		#region Newtonsoft Field Write Conditions
		// These are referenced by newtonsoft during runtime.
		// Format: ShouldSerialize...
		// Replace ... with the name of the field.

		public bool ShouldSerializeMin() => Min.Count > 0;

		public bool ShouldSerializeMax() => Max.Count > 0;
		#endregion

	}
}

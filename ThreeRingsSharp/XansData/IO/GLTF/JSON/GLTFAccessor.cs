using Newtonsoft.Json;
using System;
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
		public int Size => Count * AttributeExtension.GetSize(typeof(GLTFComponentType), ComponentType) * AttributeExtension.GetSize(typeof(GLTFValueType), Type);
		/*{
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
		*/
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
		[JsonProperty("max")] public List<object> Max = new List<object>();

		/// <summary>
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="ComponentType"/>.<para/>
		/// Set the count to zero to skip writing this data.
		/// </summary>
		[JsonProperty("min")] public List<object> Min = new List<object>();

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

	/// <summary>
	/// A typed variant of <see cref="GLTFAccessor"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class GLTFAccessor<T> : GLTFAccessor where T : struct {

		/// <summary>
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="GLTFAccessor.ComponentType"/>.<para/>
		/// Set the count to zero to skip writing this data.
		/// </summary>
		[JsonProperty("max")] public new List<T> Max = new List<T>();

		/// <summary>
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="GLTFAccessor.ComponentType"/>.<para/>
		/// Set the count to zero to skip writing this data.
		/// </summary>
		[JsonProperty("min")] public new List<T> Min = new List<T>();

		/// <summary>
		/// Create a new typed <see cref="GLTFAccessor{T}"/> which automatically sets <see cref="GLTFAccessor.ComponentType"/> appropriately.<para/>
		/// If <typeparamref name="T"/> is not a valid glTF component type, this will throw <see cref="InvalidOperationException"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">If <typeparamref name="T"/> is not able to be turned into a component type.</exception>
		public GLTFAccessor() {
			Type type = typeof(T);
			if (type == typeof(byte)) {
				ComponentType = GLTFComponentType.UNSIGNED_BYTE;
			} else if (type == typeof(sbyte)) {
				ComponentType = GLTFComponentType.BYTE;
			} else if (type == typeof(ushort)) {
				ComponentType = GLTFComponentType.UNSIGNED_SHORT;
			} else if (type == typeof(short)) {
				ComponentType = GLTFComponentType.SHORT;
			} else if (type == typeof(uint)) {
				ComponentType = GLTFComponentType.UNSIGNED_INT;
			} else if (type == typeof(float)) {
				ComponentType = GLTFComponentType.FLOAT;
			} else {
				throw new InvalidOperationException($"Type {type.FullName} is not a valid glTF Component Type! Allowed types are: sbyte, byte, short, ushort, uint, float");
			}
		}

		#region Newtonsoft Field Write Conditions
		// These are referenced by newtonsoft during runtime.
		// Format: ShouldSerialize...
		// Replace ... with the name of the field.

		public new bool ShouldSerializeMin() => Min.Count > 0;

		public new bool ShouldSerializeMax() => Max.Count > 0;
		#endregion

	}
}

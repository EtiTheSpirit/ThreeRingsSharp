using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.IO.GLTF;
using ThreeRingsSharp.XansData.IO.GLTF.JSON;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class AttributeExtension {

		/// <summary>
		/// Assuming <see cref="Type"/> is a GLTF value type, this will get the size attribute of the given field.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="fieldValue"></param>
		public static int GetSize(Type type, object fieldValue) {
			if (type == null) throw new ArgumentNullException("type");
			if (fieldValue == null) throw new ArgumentNullException("fieldValue");

			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			FieldInfo field = null;
			foreach (FieldInfo f in fields) {
				if (f.GetValue(null).Equals(fieldValue)) {
					field = f;
					break;
				}
			}

			if (field == null) {
				throw new InvalidOperationException("The given field does not exist!");
			}

			SizeAttribute sizeAttr = (SizeAttribute)Attribute.GetCustomAttribute(field, typeof(SizeAttribute));
			if (sizeAttr == null) {
				throw new NullReferenceException("The given field does not have the Size attribute!");
			}

			return sizeAttr.Size;
		}

	}

	/// <summary>
	/// Allows an object to define its size. Size is arbitrary, but generally refers to bytes.<para/>
	/// This is used by the <see cref="GLTFAccessor"/> class in tandem with <see cref="GLTFComponentType"/> to automatically get the size of an accessor.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public class SizeAttribute : Attribute {
		public SizeAttribute(int size) {
			if (size < 0) throw new ArgumentException("Cannot have size less than 0.");
			_Size = size;
		}

		private readonly int _Size;

		/// <summary>
		/// The size of this element.
		/// </summary>
		public int Size => _Size;
	}
}

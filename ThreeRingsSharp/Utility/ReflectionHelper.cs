using com.threerings.config;
using com.threerings.opengl.material.config;
using System;
using System.Reflection;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.XansData.XML.ConfigReferences;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// Provides aliases to get fields or indices of objects via reflection.
	/// </summary>
	public static class ReflectionHelper {

		/// <summary>
		/// Returns the value of the given instance field for the specified object.<para/>
		/// </summary>
		/// <param name="obj">The object to get the data of.</param>
		/// <param name="field">The name of the field to access.</param>
		/// <returns></returns>
		public static object Get(object obj, string field) {
			if (int.TryParse(field.BetweenBrackets(), out int idx)) {
				return GetArray(obj, idx);
			}
			return obj.GetType().GetField(field).GetValue(obj);
		}

		/// <summary>
		/// Assuming <paramref name="arrayObj"/> is an array, this returns <paramref name="arrayObj"/>[<paramref name="idx"/>]
		/// </summary>
		/// <param name="arrayObj">The object to get the data of, which should be an array.</param>
		/// <param name="idx">The index within the given array object to access.</param>
		/// <returns></returns>
		public static object GetArray(object arrayObj, int idx) {
			return ((Array)arrayObj).GetValue(idx);
		}

		/// <summary>
		/// Sets the value of the given instance field for the specified object.
		/// </summary>
		/// <param name="obj">The object to get the data of.</param>
		/// <param name="field">The name of the field to access.</param>
		/// <param name="value">The new value to set this field to.</param>
		public static void Set(object obj, string field, object value) {
			if (int.TryParse(field.BetweenBrackets(), out int idx)) {
				SetArray(obj, idx, value);
				return;
			}
			FieldInfo fObj = obj.GetType().GetField(field);
			bool fieldIsJava = fObj.FieldType.FullName.Contains("java.lang");
			bool valueIsJava = value?.GetType().FullName.Contains("java.lang") ?? false;
			if (fieldIsJava != valueIsJava) {
				if (fieldIsJava) {
					value = JavaToCSPrimitive.AsJavaPrimitive(value);
				} else {
					value = JavaToCSPrimitive.AsCSPrimitive(value);
				}
			}
			fObj.SetValue(obj, value);
		}

		/// <summary>
		/// Sets the element at <paramref name="index"/> in <paramref name="arrayObj"/> to <paramref name="value"/>.
		/// </summary>
		/// <param name="arrayObj"></param>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public static void SetArray(object arrayObj, int index, object value) {
			((Array)arrayObj).SetValue(value, index);
		}

		/// <summary>
		/// Calls the given method for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="onMember"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static object Call(Type type, string methodName, object onMember = null, params object[] parameters) {
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			return method.Invoke(onMember, parameters);
		}

		/// <summary>
		/// Calls the given method on the given object.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static object CallMember(object member, string methodName, params object[] parameters) => Call(member.GetType(), methodName, member, parameters);

	}

}

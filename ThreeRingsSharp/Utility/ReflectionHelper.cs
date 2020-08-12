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
			obj.GetType().GetField(field).SetValue(obj, value);
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
		/// Returns the type of the field. Unlike GetType(), this will work even if the object is null.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		public static Type GetTypeOfField(object obj, string field) {
			return obj.GetType().GetField(field).FieldType;
		}

		/// <summary>
		/// An alias to call getParameter on an object that is presumably a <see cref="ParameterizedConfig"/>.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="parameterName"></param>
		/// <returns></returns>
		[Obsolete("Cast the object to ParameterizedConfig and just call the method itself.")]
		public static object GetParameter(object from, string parameterName) {
			return from.GetType().GetMethod("getParameter", new Type[] { typeof(string) }).Invoke(from, new object[] { parameterName });
		}

		/// <summary>
		/// An alias that returns the <c>paths</c> field as a <see langword="string"/>[], of course, intended to be called on a <see cref="Parameter.Direct"/>
		/// </summary>
		/// <param name="direct"></param>
		/// <returns></returns>
		[Obsolete]
		public static string[] GetPaths(object direct) {
			return (string[])Get(direct, "paths");
		}


		/// <summary>
		/// In the context of config maps, a <see cref="ConfigReference"/> returns a path relative to an OOO config file.<para/>
		/// This method will attempt to resolve that path when given a <see cref="ConfigReference"/> and a configuration index (which is the name of one of the xml files in the spiral knights/config directory, e.g. "material" or "tile")<para/>
		/// Assuming <paramref name="fromObj"/> is a <see cref="ConfigReference"/>, this will call its <c>getName()</c> method (which returns its path), resolve the config data via <see cref="ConfigReferenceBootstrapper"/>, and return the object with the given name.<para/>
		/// Given <paramref name="directName"/>, it will attempt to call the <c>getParameter()</c> method on the returned config with an argument of this name, which should return the <see cref="Parameter"/> it's pointing to.<para/>
		/// The first return value is the <see cref="ConfigReference"/> after resolution (which will be a specific reference type, like <see cref="MaterialConfig"/>), and the second value is the <see cref="Parameter"/> returned by ^.
		/// </summary>
		/// <param name="fromObj"></param>
		/// <param name="directName"></param>
		/// <param name="threeRingsConfigName"></param>
		/// <returns></returns>
		[Obsolete]
		public static (object, object) GetOOOConfigAndParameter(object fromObj, string directName, string threeRingsConfigName) {
			MethodInfo getNameMethod = fromObj.GetType().GetMethod("getName");
			if (getNameMethod != null) {
				string name = (string)getNameMethod.Invoke(fromObj, new object[0]);
				// Sanity check:
				if (System.IO.File.Exists(ResourceDirectoryGrabber.ResourceDirectoryPath + name)) {
					// This isn't a config reference, this is an actual direct model reference!
					return (null, null);
				}

				ManagedConfig[] refs = (ManagedConfig[])ConfigReferenceBootstrapper.ConfigReferences[threeRingsConfigName];
				object refObj = refs.GetEntryByName(name);
				return (refObj, refObj.GetType().GetMethod("getParameter", new Type[] { typeof(string) }).Invoke(refObj, new object[] { directName }));
			}
			return (null, null);
		}

		/// <summary>
		/// In the context of config maps, a <see cref="ConfigReference"/> returns a path relative to an OOO config file.<para/>
		/// This method will attempt to resolve that path when given a <see cref="ConfigReference"/> and a configuration index (which is the name of one of the xml files in the spiral knights/config directory, e.g. "material" or "tile")<para/>
		/// Assuming <paramref name="fromObj"/> is a <see cref="ConfigReference"/>, this will call its <c>getName()</c> method (which returns its path), resolve the config data via <see cref="ConfigReferenceBootstrapper"/>, and return the object with the given name.<para/>
		/// Given <paramref name="directName"/>, it will attempt to call the <c>getParameter()</c> method on the returned config with an argument of this name, which should return the <see cref="Parameter"/> it's pointing to.<para/>
		/// The first return value is the <see cref="ConfigReference"/> after resolution (which will be a specific reference type, like <see cref="MaterialConfig"/>), and the second value is the <see cref="Parameter"/> returned by ^.
		/// </summary>
		/// <param name="fromObj"></param>
		/// <param name="directName"></param>
		/// <param name="threeRingsConfigType"></param>
		/// <returns></returns>
		[Obsolete]
		public static (object, object) GetOOOConfigAndParameter(object fromObj, string directName, Type threeRingsConfigType) {
			MethodInfo getNameMethod = fromObj.GetType().GetMethod("getName");
			if (getNameMethod != null) {
				string name = (string)getNameMethod.Invoke(fromObj, new object[0]);
				// Sanity check:
				if (System.IO.File.Exists(ResourceDirectoryGrabber.ResourceDirectoryPath + name)) {
					// This isn't a config reference, this is an actual direct model reference!
					return (null, null);
				}

				ManagedConfig[] refs = (ManagedConfig[])ConfigReferenceBootstrapper.ConfigReferences[threeRingsConfigType];
				object refObj = refs.GetEntryByName(name);
				return (refObj, refObj.GetType().GetMethod("getParameter", new Type[] { typeof(string) }).Invoke(refObj, new object[] { directName }));
			}
			return (null, null);
		}

		/// <summary>
		/// Gets a <see cref="ConfigReference"/> of the given name. Unlike the alternatives to this method, this one will iterate through all config refs (and by extension, does not require a direct config group name).
		/// </summary>
		/// <param name="fromObj"></param>
		/// <param name="directName"></param>
		/// <returns></returns>
		[Obsolete]
		public static (object, object) GetOOOConfigAndParameter(object fromObj, string directName) {

			MethodInfo getNameMethod = fromObj.GetType().GetMethod("getName");
			if (getNameMethod != null) {
				string name = (string)getNameMethod.Invoke(fromObj, new object[0]);
				// Sanity check:
				if (System.IO.File.Exists(ResourceDirectoryGrabber.ResourceDirectoryPath + name)) {
					// This isn't a config reference, this is an actual direct model reference!
					return (null, null);
				}

				XanLogger.WriteLine("NOTICE: DIRECT [" + directName + "] WITH VALUE [" + name + "] DID NOT HAVE A BINDING! It will now search... (type of parent object: [" + fromObj.GetType().FullName + "])");
				object refObj = ConfigReferenceBootstrapper.ConfigReferences.TryGetReferenceFromName(name);
				return (refObj, refObj.GetType().GetMethod("getParameter", new Type[] { typeof(string) }).Invoke(refObj, new object[] { directName }));
			}
			return (null, null);
		}

	}

}

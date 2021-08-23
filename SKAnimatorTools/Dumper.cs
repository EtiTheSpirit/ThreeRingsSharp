using com.threerings.export;
using java.lang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClydeDumper {
	class Dumper {
		static void Main2(string[] args) {
			Assembly asm = Assembly.GetAssembly(typeof(Exportable));
			//Assembly asm = Assembly.GetExecutingAssembly();
			Type[] trTypes = GetTypesInNamespace(asm, "com.threerings");
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			System.Text.StringBuilder classesWithRF = new System.Text.StringBuilder();
			System.Text.StringBuilder classesWithRFHelp = new System.Text.StringBuilder();
			foreach (Type t in trTypes) {
				if (t.GetInterfaces().Contains(typeof(Exportable))) {
					result.Append("CLASS ");
					result.AppendLine(t.FullName.Replace('+', '$'));
					foreach (string field in GetExportableFields(t)) {
						result.AppendLine(field);
					}
					if (HasMethodWithName(t, "readFields", false)) {
						classesWithRF.AppendLine(t.FullName);
					}
					if (Inherits(t, typeof(com.threerings.config.ManagedConfig))) {
						if (HasMethodWithName(t, "readFields", true)) {
							classesWithRFHelp.AppendLine($"[ClassName(typeof(ManagedConfig), \"{t.FullName.Replace('+', '$')}\")]");
						}
					}
				}
			}

			File.WriteAllText("./DUMP.txt", result.ToString());
			File.WriteAllText("./WITHRF.txt", classesWithRF.ToString());
			File.WriteAllText("./WITHRFAUTO.txt", classesWithRFHelp.ToString());
		}

		public static bool HasMethodWithName(Type t, string name, bool inherited) {
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			if (!inherited) {
				flags |= BindingFlags.DeclaredOnly;
			}
			return t.GetMethods(flags).FirstOrDefault(mtd => mtd.Name == name) != null;
		}

		/// <summary>
		/// Returns true if <paramref name="t"/> inherits from <paramref name="other"/>, even indirectly.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool Inherits(Type t, Type other) {
			while (t.BaseType != null) {
				if (t.BaseType == other) return true;
				t = t.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Returns the name of all exportable fields of the given type. What is "exportable" is defined by <see cref="ObjectMarshaller"/>.
		/// </summary>
		/// <param name="t"></param>
		public static List<string> GetExportableFields(Type t) {
			List<string> names = new List<string>();
			
			try {
				Class cls = Class.forName(t.FullName.Replace('+', '$'));
				ObjectMarshaller marshaller = ObjectMarshaller.getObjectMarshaller(cls);
				object prototype = marshaller.getPrototype();

				java.util.ArrayList fields = new java.util.ArrayList();
				ObjectMarshaller.getExportableFields(cls, fields);
				foreach (object o in fields) {
					var f = (java.lang.reflect.Field)o;
					string fieldType = f.getType().getName();
					string fieldName = f.getName();

					object defValue = f.get(prototype);
					string defaultValue;
					if (defValue?.GetType().IsArray ?? false) {
						defaultValue = "[]";
					} else {
						defaultValue = defValue.ToString() ?? "null";
					}
					names.Add($"{fieldType} {fieldName} {defaultValue}");
				}
				
			} catch { }
			
			return names;
		}


		// https://stackoverflow.com/questions/949246/how-can-i-get-all-classes-within-a-namespace
		private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace) {
			return
			  assembly.GetTypes()
					  .Where(t => t.Namespace?.StartsWith(nameSpace, StringComparison.Ordinal) ?? false)
					  .ToArray();
		}
	}
}

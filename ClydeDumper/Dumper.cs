using com.threerings.export;
using java.lang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClydeDumper {
	class Dumper {
		static void Main(string[] args) {
			Assembly asm = Assembly.GetAssembly(typeof(Exportable));
			//Assembly asm = Assembly.GetExecutingAssembly();
			Type[] trTypes = GetTypesInNamespace(asm, "com.threerings");
			Console.WriteLine(trTypes.Length);
			System.Text.StringBuilder result = new System.Text.StringBuilder("CLYDE TYPE DUMP\n");
			foreach (Type t in trTypes) {
				//if (t.GetInterfaces().Contains(typeof(Exportable))) {
					result.Append("CLASS ");
					result.AppendLine(t.FullName);
					foreach (string field in GetExportableFields(t)) {
						result.AppendLine(field);
					}
				//}
			}

			File.WriteAllText("./DUMP.txt", result.ToString());
		}

		/// <summary>
		/// Returns the name of all exportable fields of the given type. What is "exportable" is defined by <see cref="ObjectMarshaller"/>.
		/// </summary>
		/// <param name="t"></param>
		public static List<string> GetExportableFields(Type t) {
			List<string> names = new List<string>();
			Class cls = Class.forName(t.FullName.Replace('+', '$'));
			java.util.ArrayList fields = new java.util.ArrayList();
			GETEXP.Invoke(null, new object[] { cls, fields });
			foreach (object o in fields) {
				string name = ((java.lang.reflect.Field)o).getName();
				names.Add(name);
			}
			return names;
		}

		static Type OBJECTMARSHALLER = typeof(ObjectMarshaller);
		static MethodInfo GETEXP = OBJECTMARSHALLER.GetMethod("getExportableFields", BindingFlags.Static | BindingFlags.NonPublic);

		// https://stackoverflow.com/questions/949246/how-can-i-get-all-classes-within-a-namespace
		private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace) {
			return
			  assembly.GetTypes()
					  .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
					  .ToArray();
		}
	}
}

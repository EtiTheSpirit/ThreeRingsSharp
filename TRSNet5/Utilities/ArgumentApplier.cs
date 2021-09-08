using OOOReader.Clyde;
using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.Utilities {

	/// <summary>
	/// This class is a companion to <see cref="ConfigReferenceResolver"/> which handles applying an ArgumentMap's data to a loaded <see cref="ClydeFile"/>.
	/// </summary>
	[Obsolete("This has been superseded by Direct/Choice Traversal.")]
	public static class ArgumentApplier {

		/// <summary>
		/// Applies the given <see cref="ShadowClass"/> argument map to the loaded <see cref="ShadowClass"/> config, which is expected to be loaded from <see cref="ConfigReferenceResolver.ResolveConfigReference(ShadowClass)"/>
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="argumentMap"></param>
		public static void ApplyTo(ShadowClass instance, Dictionary<object, object?> argumentMap) {
			throw new NotImplementedException();

			instance.AssertIsInstanceOf("com.threerings.config.ParameterizedConfig");
			ShadowClass[] parameters = instance["parameters"]!;
			foreach (KeyValuePair<object, object?> entry in argumentMap) {
				string key = entry.Key.ToString()!;
				ShadowClass? param = GetParameter(parameters, key);
				if (param == null) continue;

				//ShadowClass? property = null;
			}
		}

		private static ShadowClass? GetParameter(ShadowClass[] parameters, string name) => parameters.FirstOrDefault(param => param["name"] == name);

	}
}

using OOOReader.Clyde;
using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.Utilities {

	/// <summary>
	/// A wrapper-implementation of OOO's ConfigReference type, which is a way to reference another file without loading it until it's needed.
	/// </summary>
	public class ConfigReference {
		/// <summary>
		/// The name of this reference, which is either the file path to it (relative to rsrc) or a prenamed config object as seen within rsrc/config's
		/// many files.
		/// </summary>
		public string Name { get; protected set; } = string.Empty;

		/// <summary>
		/// The arguments that will be applied to the referenced model's parameters once instantiated.
		/// </summary>
		public Dictionary<string, object?> Arguments { get; } = new Dictionary<string, object?>();

		/// <summary>
		/// The original <see cref="ShadowClass"/> whose <see cref="ShadowClass.Name"/> is <c>com.threerings.config.ConfigReference</c>.
		/// </summary>
		private readonly ShadowClass Original;

		public ConfigReference(ShadowClass shadow) {
			shadow.AssertIsInstanceOf("com.threerings.config.ConfigReference");
			Name = shadow["_name"]!;
			Dictionary<object, object?> args = shadow["_arguments"]!;
			foreach (KeyValuePair<object, object?> data in args) {
				Arguments[args.Keys.ToString()!] = data.Value;
			}
			Original = shadow;
		}

		/// <summary>
		/// Resolve this <see cref="ConfigReference"/>, returning the <see cref="ShadowClass"/> from the file it referenced.
		/// </summary>
		/// <remarks>
		/// Under normal Clyde engine behavior, the returned <see cref="ShadowClass"/> should either directly or indierectly extend <c>com.threerings.config.ParameterizedConfig</c>.
		/// </remarks>
		/// <returns></returns>
		public ShadowClass? Resolve() => ConfigReferenceResolver.ResolveConfigReference(Original);

	}
}

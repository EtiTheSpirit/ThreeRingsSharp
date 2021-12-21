using OOOReader.Exceptions;
using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities.Parameters.Implementation;

namespace ThreeRingsSharp.Utilities.Parameters {

	/// <summary>
	/// A utility class that returns a ParameterizedConfig's parameters as a custom implementation made just for TRS, as to avoid the convoluted mess that is
	/// <see cref="ShadowClass"/> traversal (because this just makes it worse). This comes with the capabilities to edit objects based on the applicable
	/// parameters as well.
	/// </summary>
	public static class ParameterTool {

		/// <summary>
		/// Returns the parameters on the given ParameterizedConfig shadow. Naturally, this asserts that the
		/// <see cref="ShadowClass"/> <em>MUST</em> be an instance of <c>com.threerings.config.ParameterizedConfig</c>
		/// </summary>
		/// <param name="parameterizedConfig"></param>
		/// <param name="args">The arguments for these parameters, which is used in creation, or null if this is not applicable.</param>
		/// <returns></returns>
		/// <exception cref="ShadowTypeMismatchException">If the signature is wrong.</exception>
		public static Parameter[] GetParameters(this ShadowClass parameterizedConfig) {
			parameterizedConfig.AssertIsInstanceOf("com.threerings.config.ParameterizedConfig");
			Parameter.SetupParameters(parameterizedConfig);
			return parameterizedConfig["__RichParameters"]!;
		}

	}
}

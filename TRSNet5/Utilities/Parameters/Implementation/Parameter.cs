using OOOReader.Exceptions;
using OOOReader.Reader;
using SKAnimatorTools.PrimaryInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.Utilities.Parameters.Implementation {
	public abstract class Parameter {

		/// <summary>
		/// The name of this parameter.
		/// </summary>
		public string Name { get; init; } = string.Empty;

		/// <summary>
		/// The ParameterizedConfig that this parameter exists on (represented as its <see cref="ShadowClass"/>)
		/// </summary>
		public ShadowClass ParameterizedConfig { get; }

		public Parameter(ShadowClass parameterizedConfig, string name = "") {
			Name = name;
			ParameterizedConfig = parameterizedConfig;
		}

		/// <summary>
		/// Sets up the parameters of a given <see cref="ShadowClass"/> representing an instance of ParameterizedConfig. This will add a new
		/// field to the <see cref="ShadowClass"/> named <c>RichParameters</c> which will be an array of instances deriving this class.
		/// 
		/// This does nothing if the input <see cref="ShadowClass"/> already has a field named RichParameters, unless <paramref name="forceOverride"/> is
		/// set to <see langword="true"/>.
		/// </summary>
		/// <param name="parameterizedConfig"></param>
		/// <param name="forceOverride"></param>
		/// <exception cref="ShadowTypeMismatchException">If the signature is wrong.</exception>
		public static void SetupParameters(ShadowClass parameterizedConfig, bool forceOverride = false) {
			parameterizedConfig.AssertIsInstanceOf("com.threerings.config.ParameterizedConfig");

			if (parameterizedConfig.HasField("__RichParameters") && !forceOverride) {
				return;
			}

			object paramsObj = parameterizedConfig["parameters"]!;
			if (paramsObj is ShadowClass[] parameters) {
				List<Parameter> realParams = new List<Parameter>();
				foreach (ShadowClass parameter in parameters) {
					if (parameter.IsA("com.threerings.config.Parameter$Direct")) {
						realParams.Add(new Direct(parameterizedConfig, parameter));
					} else if (parameter.IsA("com.threerings.config.Parameter$Choice")) {
						realParams.Add(new Choice(parameterizedConfig, parameter));
					}
				}
				parameterizedConfig["__RichParameters"] = realParams.ToArray();
			} else {
				if (parameterizedConfig["parameters"] is ShadowClassArrayTemplate pendingArray) {
					parameterizedConfig["parameters"] = pendingArray.NewInstance();
				}
				parameterizedConfig["__RichParameters"] = Array.Empty<Parameter>();
			}
		}
	}
}

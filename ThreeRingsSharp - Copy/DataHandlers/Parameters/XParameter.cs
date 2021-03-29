using com.threerings.opengl.model.config;
using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Exceptions;

namespace ThreeRingsSharp.DataHandlers.Parameters {

	/// <summary>
	/// An implementation of model parameters in C#
	/// </summary>
	public abstract class XParameter {

		/// <summary>
		/// The name of this parameter.
		/// </summary>
		public string Name => Original.name;

		/// <summary>
		/// The original <see cref="Parameter"/> used to construct this <see cref="XParameter"/>.
		/// </summary>
		protected Parameter Original { get; }

		/// <summary>
		/// The <see cref="ParameterizedConfig"/> containing <see cref="Original"/> as one of its parameters.
		/// </summary>
		public ParameterizedConfig Parent { get; }

		protected XParameter(ParameterizedConfig parent, Parameter source) {
			Parent = parent;
			Original = source;
		}

	}
}

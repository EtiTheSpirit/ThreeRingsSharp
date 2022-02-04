using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON.Extension {

	/// <summary>
	/// Denotes a <see cref="GLTFObject"/> as an extension container.
	/// </summary>
	public interface IGLTFExtension {

		/// <summary>
		/// The name of this extension as defined by its specification.
		/// </summary>
		[JsonIgnore]
		public abstract string ExtensionName { get; }

	}

}

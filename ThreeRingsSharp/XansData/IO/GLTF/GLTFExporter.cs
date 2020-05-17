using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF {

	/// <summary>
	/// Represents a GLTF file and its associated data.
	/// </summary>
	public class GLTFExporter : AbstractModelExporter {

		/// <summary>
		/// Initialize camel case serialization.
		/// </summary>
		static GLTFExporter() {
			// I *guess* this is the best place to put this.
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
		}

		public override void Export(Model3D[] models, FileInfo toFile) {
			throw new NotImplementedException();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFAccessor {

		public int bufferView = 0;

		// public int byteOffset = 0;

		public int componentType = GLTFComponentType.BYTE;

		public int count = 0;

		/// <summary>
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="componentType"/>.
		/// </summary>
		public List<dynamic> max = new List<dynamic>();

		/// <summary>
		/// NOTE: Cast these values into their appropriate type as dictated by <see cref="componentType"/>.
		/// </summary>
		public List<dynamic> min = new List<dynamic>();

		public string type = GLTFType.SCALAR;

	}
}

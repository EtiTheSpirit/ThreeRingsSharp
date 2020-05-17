using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFBufferView {

		/// <summary>
		///  The ID of the buffer to browse.
		/// </summary>
		public int buffer = 0;

		/// <summary>
		/// The length of the view in bytes (how many bytes this view "contains")
		/// </summary>
		public int byteLength = 0;

		/// <summary>
		/// The offset of the first byte in this view
		/// </summary>
		public int byteOffset = 0;

		// <summary>
		// The spacing between the bytes in this view. For types larger than one byte, this represents the spacing between the first bytes (e.g. if I have a buffer of int32s, the stride here would be 4, NOT 1)
		// </summary>
		// public int? byteStride = null;

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFJSONRoot {

		public GLTFAsset Asset = new GLTFAsset();

		public GLTFBuffer[] Buffers = new GLTFBuffer[0];

		public GLTFBufferView[] BufferViews = new GLTFBufferView[0];

	}
}

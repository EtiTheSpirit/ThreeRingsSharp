using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFJSONRoot {

		public GLTFAsset asset = new GLTFAsset();

		public List<GLTFBuffer> buffers = new List<GLTFBuffer>();

		public List<GLTFBufferView> bufferViews = new List<GLTFBufferView>();

		public List<GLTFAccessor> accessors = new List<GLTFAccessor>();

		public List<GLTFMesh> meshes = new List<GLTFMesh>();

		public List<GLTFNode> nodes = new List<GLTFNode>();

		public List<GLTFImage> images = new List<GLTFImage>();

		public List<GLTFSampler> samplers = new List<GLTFSampler>();

		public List<GLTFTexture> textures = new List<GLTFTexture>();

		public int scene = 0;

		public List<GLTFScene> scenes = new List<GLTFScene>();

	}
}

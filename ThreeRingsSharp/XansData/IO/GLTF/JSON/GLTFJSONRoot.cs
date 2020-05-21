using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// The raw json data container.
	/// </summary>
	public class GLTFJSONRoot {

		/// <summary>
		/// Asset information for this glTF file.
		/// </summary>
		[JsonProperty("asset")] public GLTFAsset Asset = new GLTFAsset();

		/// <summary>
		/// A list of all buffers stored in this glTF file.
		/// </summary>
		[JsonProperty("buffers")] public List<GLTFBuffer> Buffers = new List<GLTFBuffer>();

		/// <summary>
		/// Scopes within the <see cref="Buffers"/> that point to a specific subsection of a given buffer.
		/// </summary>
		[JsonProperty("bufferViews")] public List<GLTFBufferView> BufferViews = new List<GLTFBufferView>();

		/// <summary>
		/// Objects that allow model systems to read from one of the <see cref="BufferViews"/>.
		/// </summary>
		[JsonProperty("accessors")] public List<GLTFAccessor> Accessors = new List<GLTFAccessor>();

		/// <summary>
		/// A mesh in this glTF file. Represents raw mesh data.
		/// </summary>
		[JsonProperty("meshes")] public List<GLTFMesh> Meshes = new List<GLTFMesh>();

		/// <summary>
		/// An object in this glTF file. Represents an object which can contain mesh data and other applicable information.
		/// </summary>
		[JsonProperty("nodes")] public List<GLTFNode> Nodes = new List<GLTFNode>();

		/// <summary>
		/// A list of references to images in this glTF file.
		/// </summary>
		[JsonProperty("images")] public List<GLTFImage> Images = new List<GLTFImage>();

		/// <summary>
		/// A list of samplers, which determine how images are applied to meshes.
		/// </summary>
		[JsonProperty("samplers")] public List<GLTFSampler> Samplers = new List<GLTFSampler>();

		/// <summary>
		/// A list of textures, which are used to apply images to meshes.
		/// </summary>
		[JsonProperty("textures")] public List<GLTFTexture> Textures = new List<GLTFTexture>();

		/// <summary>
		/// A list of materials.
		/// </summary>
		[JsonProperty("materials")] public List<GLTFMaterial> Materials = new List<GLTFMaterial>();

		/// <summary>
		/// The scene to use. Points to an entry in <see cref="Scenes"/>.
		/// </summary>
		[JsonProperty("scene")] public int Scene = 0;

		/// <summary>
		/// One or more scenes, which contains groups of geometry and other information.
		/// </summary>
		[JsonProperty("scenes")] public List<GLTFScene> Scenes = new List<GLTFScene>();

	}
}

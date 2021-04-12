using Newtonsoft.Json;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {

	/// <summary>
	/// The superclass of all json data in glTF files. This offers a field <see cref="ThisIndex"/> which is used to track objects when constructing glTF files.
	/// </summary>
	public abstract class GLTFObject {

		/// <summary>
		/// Used as a tricky method of referencing this accessor in a node. This is the index of the accessor itself in the json data.
		/// </summary>
		[JsonIgnore]
		public int ThisIndex = 0;

	}
}

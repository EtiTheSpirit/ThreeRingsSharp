using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ThreeRingsSharp.XansData.IO.GLTF.JSON.Extension;

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

		/// <summary>
		/// Any extensions to this object. The support of extensions in various applications is not guaranteed and so extensions should not be relied upon for core features.
		/// </summary>
		[JsonIgnore]
		public List<IGLTFExtension> Extensions { get; } = new List<IGLTFExtension>();

		#region Newtonsoft Serialization Garbage
		[JsonProperty("extensions")]
		private Dictionary<string, IGLTFExtension> _extensionsInternal { get; set; } = new Dictionary<string, IGLTFExtension>();

		public bool ShouldSerialize_extensionsInternal() {
			if (Extensions.Count == 0) return false;
			_extensionsInternal.Clear();
			bool useDefNotImpl = this is GLTFJSONRoot;
			foreach (IGLTFExtension ext in Extensions) {
				_extensionsInternal[ext.ExtensionName] = ext;
			}
			return true;
		}

		#endregion

	}
}

using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities;

namespace ThreeRingsSharp.ConfigHandlers.TudeyScenes.Entries {

	/// <summary>
	/// The base type for something that's in a TudeySceneModel.
	/// </summary>
	public abstract class SceneEntry {

		/// <summary>
		/// The key associated with this entry for use in its parent dictionary.
		/// </summary>
		public abstract object Key { get; }

		/// <summary>
		/// Sets the <see cref="ConfigReference"/> pointing to the model that this entry uses.
		/// </summary>
		/// <param name="reference"></param>
		public abstract void SetReference(ConfigReference reference);

		/// <summary>
		/// Returns a <see cref="ConfigReference"/> pointing to the model that this entry uses.
		/// </summary>
		/// <returns></returns>
		public abstract ConfigReference GetReference();

		/// <summary>
		/// The type of the returned <see cref="ShadowClass"/> from <see cref="GetReference"/>
		/// </summary>
		/// <returns></returns>
		public abstract string GetReferenceType();

		/// <summary>
		/// The elevation of this tile, or <see cref="int.MinValue"/> for none.
		/// </summary>
		public virtual int Elevation { get; }

		/// <summary>
		/// The bounds of this element.
		/// </summary>
		[Obsolete("This has not been implemented yet.", true)]
		public object Bounds => throw new NotImplementedException();

	}
}

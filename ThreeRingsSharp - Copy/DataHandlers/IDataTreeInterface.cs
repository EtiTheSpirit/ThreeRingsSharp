using com.threerings.export;
using com.threerings.opengl.model.config;
using ThreeRingsSharp.Utility.Interface;

namespace ThreeRingsSharp.DataHandlers {

	/// <summary>
	/// Represents a class that can interface with a data tree GUI element to display information about a <see cref="ModelConfig"/>.
	/// </summary>
	public interface IDataTreeInterface<T> where T : Exportable {

		/// <summary>
		/// Sets up the cosmetic data, or what's displayed in the GUI for the program.
		/// </summary>
		/// <param name="data">The object to pull data from.</param>
		/// <param name="dataTreeParent">This is the instance in the data tree that represents this object in the hierarchy.</param>
		void SetupCosmeticInformation(T data, DataTreeObject dataTreeParent);

	}

}

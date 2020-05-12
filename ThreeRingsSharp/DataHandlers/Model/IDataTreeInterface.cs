using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility.Interface;

namespace ThreeRingsSharp.DataHandlers.Model {

	/// <summary>
	/// Represents a class that can interface with a data tree GUI element to display information about a <see cref="ModelConfig"/>.
	/// </summary>
	public interface IDataTreeInterface<TModel> where TModel : ModelConfig.Implementation {

		/// <summary>
		/// Sets up the cosmetic data for this model, or, what's displayed in the GUI for the program.
		/// </summary>
		/// <param name="model">The <see cref="ModelConfig"/> to pull data from.</param>
		/// <param name="dataTreeParent">This is the instance in the data tree that represents this object in the hierarchy.</param>
		void SetupCosmeticInformation(TModel model, DataTreeObject dataTreeParent);

	}
}

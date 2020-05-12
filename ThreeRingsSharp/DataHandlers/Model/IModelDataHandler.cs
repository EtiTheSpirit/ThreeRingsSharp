using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.threerings.opengl.model.config;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;

namespace ThreeRingsSharp.DataHandlers.Model {

	/// <summary>
	/// Represents a class designed to handle the data within a <see cref="ModelConfig"/>.
	/// </summary>
	public interface IModelDataHandler {

		/// <summary>
		/// Handles the data from the given <see cref="ModelConfig"/>.
		/// </summary>
		/// <param name="sourceFile">The file that contains this data.</param>
		/// <param name="baseModel">The <see cref="ModelConfig"/> storing the data.</param>
		/// <param name="modelCollection">A reference to a list of models that will be written to.</param>
		/// <param name="dataTreeParent">An optional parameter for use in GUI Data Trees that will be populated with information about this model.</param>
		void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, ref List<Model3D> modelCollection, DataTreeObject dataTreeParent = null);

	}
}

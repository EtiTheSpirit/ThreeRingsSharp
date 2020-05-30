using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.threerings.math;
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
		/// <param name="dataTreeParent">An optional parameter for use in GUI Data Trees that will be populated with information about this model. This node will be one in the object hierarchy. Add properties to this to display these properties when it is selected, or add direct children to this.</param>
		/// <param name="globalTransform">Intended to be used by reference loaders, this specifies an offset for referenced models. All models loaded by this method in the given chain / hierarchy should have this transform applied to them.</param>
		/// <param name="extraData">Any extra data that should be included. This is mainly used by references (e.g. a reference is a <see cref="StaticSetConfig"/>, the target model in the set may be included as extra data)</param>
		void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null);
		// TODO: Deprecate extraData parameter?

	}
}

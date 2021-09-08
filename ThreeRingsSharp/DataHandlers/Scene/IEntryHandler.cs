using com.threerings.math;
using com.threerings.tudey.data;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.Logging.Interface;
using ThreeRingsSharp.XansData;
using static com.threerings.tudey.data.TudeySceneModel;

namespace ThreeRingsSharp.DataHandlers.Scene {
	public interface IEntryHandler {

		/// <summary>
		/// Handles a <see cref="Entry"/> of a specific type.
		/// </summary>
		/// <param name="sourceFile">The file that contains the <see cref="TudeySceneModel"/> with the given <see cref="Entry"/>.</param>
		/// <param name="entry">The <see cref="Entry"/> to handle.</param>
		/// <param name="modelCollection">A list of every model that has been loaded.</param>
		/// <param name="dataTreeParent">An optional parameter for use in GUI Data Trees that will be populated with information about this model. This node will be one in the object hierarchy. Add properties to this to display these properties when it is selected, or add direct children to this.</param>
		/// <param name="globalTransform">Intended to be used by reference loaders, this specifies an offset for referenced models. All models loaded by this method in the given chain / hierarchy should have this transform applied to them.</param>
		void HandleEntry(FileInfo sourceFile, Entry entry, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null);

	}
}

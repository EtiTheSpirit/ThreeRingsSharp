using com.threerings.config;
using com.threerings.math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using static com.threerings.opengl.model.config.CompoundConfig;

namespace ThreeRingsSharp.DataHandlers.Model.ConfigReferenceHandler {
	public class ConfigReferenceUtil {

		/// <summary>
		/// Takes in a <see cref="ConfigReference"/> and loads its data. It then returns the loaded model and all of its descendants as a list of <see cref="Model3D"/> instances.
		/// </summary>
		/// <param name="sourceFile">The original base-level file that contains the reference.</param>
		/// <param name="reference">The reference itself.</param>
		/// <param name="modelCollection">A list of every model that has been loaded recursively.</param>
		/// <param name="dataTreeParent">For cases where the GUI is used, this is the data tree representation.</param>
		/// <param name="globalTransform">The transformation to apply to all loaded models.</param>
		/// <param name="appendModelsToModelCollection">If true, the loaded models will be appended to <paramref name="modelCollection"/>.</param>
		public static List<Model3D> HandleConfigReference(FileInfo sourceFile, ConfigReference reference, List<Model3D> modelCollection, DataTreeObject dataTreeParent, Transform3D globalTransform, bool appendModelsToModelCollection = true) {
			if (reference == null) return null;
			string filePathRelativeToRsrc = reference.getName();
			if (filePathRelativeToRsrc.StartsWith("/")) filePathRelativeToRsrc = filePathRelativeToRsrc.Substring(1);
			FileInfo referencedModel = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + filePathRelativeToRsrc);
			if (!referencedModel.Exists) {
				throw new ClydeDataReadException($"ConfigReference within model at [{ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile, false)}] attempted to reference [{filePathRelativeToRsrc}], but this file could not be found!");
			}
			List<Model3D> referencedTree = new List<Model3D>();
			ClydeFileHandler.HandleClydeFile(referencedModel, referencedTree, false, dataTreeParent, false, globalTransform);
			if (appendModelsToModelCollection) modelCollection.AddRange(referencedTree);
			return referencedTree;
		}

		/// <summary>
		/// Takes in a <see cref="ComponentModel"/> and loads its data. It then returns the loaded model and all of its descendants as a list of <see cref="Model3D"/> instances.
		/// </summary>
		/// <param name="sourceFile">The original base-level file that contains the reference.</param>
		/// <param name="model">The reference itself, stored within a <see cref="ComponentModel"/>.</param>
		/// <param name="modelCollection">A list of every model that has been loaded recursively.</param>
		/// <param name="dataTreeParent">For cases where the GUI is used, this is the data tree representation.</param>
		/// <param name="globalTransform">The transformation to apply to all loaded models.</param>
		/// <param name="appendModelsToModelCollection">If true, the loaded models will be appended to <paramref name="modelCollection"/>.</param>
		public static List<Model3D> HandleComponentModel(FileInfo sourceFile, ComponentModel model, List<Model3D> modelCollection, DataTreeObject dataTreeParent, Transform3D globalTransform, bool appendModelsToModelCollection = true) {
			if (model == null) return null;
			string filePathRelativeToRsrc = model.model.getName();
			if (filePathRelativeToRsrc.StartsWith("/")) filePathRelativeToRsrc = filePathRelativeToRsrc.Substring(1);
			FileInfo referencedModel = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + filePathRelativeToRsrc);
			if (!referencedModel.Exists) {
				throw new ClydeDataReadException($"ConfigReference within model at [{ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile, false)}] attempted to reference [{filePathRelativeToRsrc}], but this file could not be found!");
			}
			List<Model3D> referencedTree = new List<Model3D>();
			Transform3D newTrs = model.transform;
			newTrs = globalTransform.compose(newTrs);
			ClydeFileHandler.HandleClydeFile(referencedModel, referencedTree, false, dataTreeParent, false, newTrs);
			if (appendModelsToModelCollection) modelCollection.AddRange(referencedTree);
			return referencedTree;
		}

	}
}

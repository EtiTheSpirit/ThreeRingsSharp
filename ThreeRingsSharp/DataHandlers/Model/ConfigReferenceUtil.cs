using com.threerings.config;
using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.Logging;
using ThreeRingsSharp.Logging.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using ThreeRingsSharp.XansData.Extensions;
using static com.threerings.opengl.model.config.CompoundConfig;

namespace ThreeRingsSharp.DataHandlers.Model {
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
		/// <param name="extraData">Any extra data that should be included. This is mainly used by references (e.g. a reference is a <see cref="StaticSetConfig"/>, the target model in the set may be included as extra data)</param>
		public static List<Model3D> HandleConfigReference(FileInfo sourceFile, ConfigReference reference, List<Model3D> modelCollection, DataTreeObject dataTreeParent, Transform3D globalTransform, bool appendModelsToModelCollection = true, Dictionary<string, dynamic> extraData = null) {
			if (reference == null) return null;
			string filePathRelativeToRsrc = reference.getName();
			if (extraData == null) {
				extraData = reference.ArgumentsToExtraData();
			} else {
				extraData = extraData.MergeWith(reference.ArgumentsToExtraData());
			}
			return HandleConfigReferenceFromLiteralPath(sourceFile, filePathRelativeToRsrc, modelCollection, dataTreeParent, globalTransform, appendModelsToModelCollection, extraData);
		}


		/// <summary>
		/// Takes in a <see cref="string"/> filepath and loads its data. It then returns the loaded model and all of its descendants as a list of <see cref="Model3D"/> instances.
		/// </summary>
		/// <param name="sourceFile">The original base-level file that contains the reference.</param>
		/// <param name="filePathRelativeToRsrc">The path of the referenced file, relative to the rsrc directory.</param>
		/// <param name="modelCollection">A list of every model that has been loaded recursively.</param>
		/// <param name="dataTreeParent">For cases where the GUI is used, this is the data tree representation.</param>
		/// <param name="globalTransform">The transformation to apply to all loaded models.</param>
		/// <param name="appendModelsToModelCollection">If true, the loaded models will be appended to <paramref name="modelCollection"/>.</param>
		/// <param name="extraData">Any extra data that should be included. This is mainly used by references (e.g. a reference is a <see cref="StaticSetConfig"/>, the target model in the set may be included as extra data)</param>
		private static List<Model3D> HandleConfigReferenceFromLiteralPath(FileInfo sourceFile, string filePathRelativeToRsrc, List<Model3D> modelCollection, DataTreeObject dataTreeParent, Transform3D globalTransform, bool appendModelsToModelCollection = true, Dictionary<string, dynamic> extraData = null) {
			if (filePathRelativeToRsrc == null) return null;
			if (filePathRelativeToRsrc.StartsWith("/")) filePathRelativeToRsrc = filePathRelativeToRsrc.Substring(1);
			FileInfo referencedModel = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + filePathRelativeToRsrc);
			if (!referencedModel.Exists) {
				throw new ClydeDataReadException($"ConfigReference within model at [{ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile, false, false, '/')}] attempted to reference [{filePathRelativeToRsrc}], but this file could not be found!");
			}

			List<Model3D> referencedTree = new List<Model3D>();
			ClydeFileHandler.HandleClydeFile(referencedModel, referencedTree, false, dataTreeParent, false, globalTransform, extraData);
			if (appendModelsToModelCollection) modelCollection.AddRange(referencedTree);
			return referencedTree;
		}

		/// <summary>
		/// Takes in a <see cref="ComponentModel"/> and loads its data. It then returns the loaded model and all of its descendants as a list of <see cref="Model3D"/> instances.<para/>
		/// WARNING: This will return <see langword="null"/> if the configreference does not reference anything!
		/// </summary>
		/// <param name="sourceFile">The original base-level file that contains the reference.</param>
		/// <param name="model">The reference itself, stored within a <see cref="ComponentModel"/>.</param>
		/// <param name="modelCollection">A list of every model that has been loaded recursively.</param>
		/// <param name="dataTreeParent">For cases where the GUI is used, this is the data tree representation.</param>
		/// <param name="globalTransform">The transformation to apply to all loaded models.</param>
		/// <param name="appendModelsToModelCollection">If true, the loaded models will be appended to <paramref name="modelCollection"/>.</param>
		/// <param name="extraData">Any extra data that should be included. This is mainly used by references (e.g. a reference is a <see cref="StaticSetConfig"/>, the target model in the set may be included as extra data)</param>
		public static List<Model3D> HandleComponentModel(FileInfo sourceFile, ComponentModel model, List<Model3D> modelCollection, DataTreeObject dataTreeParent, Transform3D globalTransform, bool appendModelsToModelCollection = true, Dictionary<string, dynamic> extraData = null) {
			if (model == null) return null;
			if (model.model?.getName() == null) return null;

			// This needs to be kept here since it has transform data.

			string filePathRelativeToRsrc = model.model.getName();
			if (filePathRelativeToRsrc.StartsWith("/")) filePathRelativeToRsrc = filePathRelativeToRsrc.Substring(1);
			FileInfo referencedModel = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + filePathRelativeToRsrc);
			if (!referencedModel.Exists) {
				throw new ClydeDataReadException($"ConfigReference within model at [{ResourceDirectoryGrabber.GetFormattedPathFromRsrc(sourceFile, false, false, '/')}] attempted to reference [{filePathRelativeToRsrc}], but this file could not be found!");
			}
			List<Model3D> referencedTree = new List<Model3D>();
			Transform3D newTrs = model.transform;
			newTrs = globalTransform.compose(newTrs);
			ClydeFileHandler.HandleClydeFile(referencedModel, referencedTree, false, dataTreeParent, false, newTrs, extraData);
			if (appendModelsToModelCollection) modelCollection.AddRange(referencedTree);
			return referencedTree;
		}

	}
}

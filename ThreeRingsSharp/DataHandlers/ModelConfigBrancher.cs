using com.threerings.math;
using com.threerings.opengl.model.config;
using com.threerings.opengl.scene.config;
using com.threerings.opengl.util;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ThreeRingsSharp.DataHandlers.Model;
using com.threerings.opengl.model;
using ThreeRingsSharp.Logging;
using ThreeRingsSharp.Logging.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;
using com.threerings.projectx.config;

namespace ThreeRingsSharp.DataHandlers {

	/// <summary>
	/// A class that takes in a <see cref="ModelConfig"/>, determines its subtype (e.g. <see cref="ArticulatedConfig"/>, <see cref="StaticConfig"/>, etc.), and translates the data into the TRS formats used as an intermediate step between OOO =&gt; glTF.
	/// </summary>
	public class ModelConfigBrancher {

		/// <summary>
		/// Sends an arbitrary <see cref="ModelConfig"/> into the data brancher and processes it. The data brancher is the component that attempts to locate the associated model type, and directs the data to the appropriate class. For all classes involved with converting model types, see the <see cref="ThreeRingsSharp.DataHandlers.Model"/> <see langword="namespace"/>.
		/// </summary>
		/// <param name="sourceFile">The file that the given <see cref="ModelConfig"/> came from. This is a .dat or .xml file.</param>
		/// <param name="model">The <see cref="ModelConfig"/> itself, which should be acquired via <see cref="com.threerings.export.BinaryImporter"/> or <see cref="com.threerings.export.XMLImporter"/>.</param>
		/// <param name="models">A list containing every processed model from the entire hierarchy. This is only useful on recursive calls to this method, and externally calling this method to begin conversion fresh out of the box should input an empty list of <see cref="Model3D"/></param>
		/// <param name="currentDataTreeObject">The current element in the data tree hierarchy to use. Similarly to <paramref name="models"/>, this is used mostly in recursion. This is only useful for the dedicated GUI that comes with TRS, and if you are implementing this library yourself and are either running it headless or running it without a data tree to visualize the hierarchy of the model, this can safely be <see langword="null"/>.</param>
		/// <param name="useImplementation">If <see langword="false"/>, the name of the implementation will be displayed instead of the file name in the GUI. On the contrary, if this is <see langword="true"/>, the data tree for this model will not display the implementation property as it would be redundant.</param>
		/// <param name="transform">Intended to be used by reference loaders (recursive calls), this specifies an offset for referenced models on this method call. All models loaded by this method in the given chain / hierarchy will have this transform applied to them. If the value passed in is <see langword="null"/>, it will be substituted with a new <see cref="Transform3D"/> located at the origin.</param>
		/// <param name="extraData">Any extra data that should be included. This is mainly used by references (e.g. a reference is a <see cref="StaticSetConfig"/>, the target model in the set may be included as extra data)</param>
		public static void HandleDataFrom(FileInfo sourceFile, ModelConfig model, List<Model3D> models, DataTreeObject currentDataTreeObject = null, bool useImplementation = false, Transform3D transform = null, Dictionary<string, dynamic> extraData = null) {
			transform = transform ?? new Transform3D();
			
			SKAnimatorToolsProxy.IncrementEnd(2);

			ModelConfig.Implementation implementation = model.implementation;
			if (implementation == null) {
				XanLogger.WriteLine("Implementation is null! Sending error.", XanLogger.DEBUG);
				if (currentDataTreeObject != null) {
					currentDataTreeObject.Text = "Unknown Implementation";
					currentDataTreeObject.ImageKey = SilkImage.Generic;
				}
				SKAnimatorToolsProxy.SetProgressState(ProgressBarState.Error);
				throw new ClydeDataReadException("This specific model does not have an implementation, which is the data for the model itself. This generally happens if the implementation is from another game that uses Clyde and has defined its own custom model types (e.g. Spiral Knights does this). As a result, the program cannot extract any information from this file. Sorry!", "Can't Read Model", MessageBoxIcon.Error);
			}

			string implName = (JavaClassNameStripper.GetWholeClassName(implementation.getClass()) ?? implementation.getClass().getTypeName()).Replace("$", "::");
			if (currentDataTreeObject != null) {
				if (useImplementation) {
					currentDataTreeObject.Text = implName;
				} else {
					currentDataTreeObject.Text = sourceFile.Name;
				}
			}

			ModelConfigHandler.SetupCosmeticInformation(model, currentDataTreeObject, useImplementation);

			SKAnimatorToolsProxy.IncrementProgress(); // Got model display data.
														 // Next one is to load the data.
			if (implementation is ArticulatedConfig) {
				XanLogger.WriteLine("Model is of the type 'ArticulatedConfig'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Articulated;

				// New special case: Is it a knight?
				//if (sourceFile.Directory.Name == "crew" && sourceFile.Name == "model.dat") {
				//	XanLogger.WriteLine("Model was a knight. Performing special behavior for knights...", XanLogger.DEBUG);
				//	NPCKnightHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);
				//} else {
					ArticulatedConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);
				//}
			} else if (implementation is StaticConfig) {
				XanLogger.WriteLine("Model is of the type 'StaticConfig'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Static;
				StaticConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is StaticSetConfig) {
				XanLogger.WriteLine("Model is of the type 'StaticSetConfig'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.MergedStatic;
				StaticSetConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is MergedStaticConfig) {
				XanLogger.WriteLine("Model is of type 'MergedStaticConfig'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.ModelSet;
				MergedStaticConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is CompoundConfig) {
				XanLogger.WriteLine("Model is of the type 'CompoundConfig'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.ModelSet;
				CompoundConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is ViewerAffecterConfig) {
				XanLogger.WriteLine("Model is of the type 'ViewerAffecterConfig'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.CameraShake;
				ViewerAffecterConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is ModelConfig.Derived) {
				XanLogger.WriteLine("Model is of the type 'ModelConfig::Derived'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Derived;
				ModelConfigHandler.DerivedHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is ModelConfig.Schemed) {
				XanLogger.WriteLine("Model is of the type 'ModelConfig::Schemed'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Schemed;
				ModelConfigHandler.SchemedHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is ConditionalConfig) {
				XanLogger.WriteLine("Model is of the type 'ConditionalConfig'. Accessing handlers...", XanLogger.DEBUG);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Conditional;
				ConditionalConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else {
				XanLogger.WriteLine($"\nERROR: A ModelConfig had an unknown implementation!\n=> Implementation: {implName}\n=> Referenced In: {sourceFile}\n", XanLogger.DEBUG);
				// AsyncMessageBox.ShowAsync("This specific implementation is valid, but it has no handler! (There's no code that can translate this data for you :c).\nImplementation: " + implementation.getClass().getTypeName(), "Can't Handle Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Generic;
			}
			SKAnimatorToolsProxy.IncrementProgress();
			// Handled model.
		}

	}
}

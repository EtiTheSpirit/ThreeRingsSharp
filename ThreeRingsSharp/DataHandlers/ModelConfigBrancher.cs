﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.threerings.math;
using com.threerings.opengl.model.config;
using com.threerings.opengl.scene.config;
using ThreeRingsSharp.DataHandlers.Model;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Exceptions;

namespace ThreeRingsSharp.DataHandlers {

	/// <summary>
	/// A class that takes in a <see cref="ModelConfig"/>, determines its subtype (e.g. <see cref="ArticulatedConfig"/>, <see cref="StaticConfig"/>, etc.), and handles the data appropriately.
	/// </summary>
	public class ModelConfigBrancher {

		/// <summary>
		/// Sends an arbitrary <see cref="ModelConfig"/> into the data brancher and processes it.
		/// </summary>
		/// <param name="sourceFile">The file that the given <see cref="ModelConfig"/> came from.</param>
		/// <param name="model">The <see cref="ModelConfig"/> itself.</param>
		/// <param name="models">A list containing every processed model from the entire hierarchy.</param>
		/// <param name="currentDataTreeObject">The current element in the data tree hierarchy to use.</param>
		/// <param name="useImplementation">If <see langword="false"/>, the name of the implementation will be displayed instead of the file name. Additionally, it will not have its implementation property.</param>
		/// <param name="transform">Intended to be used by reference loaders, this specifies an offset for referenced models. All models loaded by this method in the given chain / hierarchy will have this transform applied to them. If the value passed in is <see langword="null"/>, it will be substituted with a new <see cref="Transform3D"/>.</param>
		/// <param name="extraData">Any extra data that should be included. This is mainly used by references (e.g. a reference is a <see cref="StaticSetConfig"/>, the target model in the set may be included as extra data)</param>
		public static void HandleDataFrom(FileInfo sourceFile, ModelConfig model, List<Model3D> models, DataTreeObject currentDataTreeObject = null, bool useImplementation = false, Transform3D transform = null, Dictionary<string, dynamic> extraData = null) {
			transform = transform ?? new Transform3D();
			//transform.promote(Transform3D.GENERAL);

			ModelConfig.Implementation implementation = model.implementation;
			if (implementation == null) {
				XanLogger.WriteLine("Implementation is null! Sending error.", true);
				if (currentDataTreeObject != null) {
					currentDataTreeObject.Text = "Unknown Implementation";
					currentDataTreeObject.ImageKey = SilkImage.Generic;
				}
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

			if (implementation is ArticulatedConfig) {
				XanLogger.WriteLine("Model is of the type 'ArticulatedConfig'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Articulated;
				ArticulatedConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is StaticConfig) {
				XanLogger.WriteLine("Model is of the type 'StaticConfig'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Static;
				StaticConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is StaticSetConfig) {
				XanLogger.WriteLine("Model is of the type 'StaticSetConfig'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.MergedStatic;
				StaticSetConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is MergedStaticConfig) {
				XanLogger.WriteLine("Model is of type 'MergedStaticConfig'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.ModelSet;
				MergedStaticConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is CompoundConfig) {
				XanLogger.WriteLine("Model is of the type 'CompoundConfig'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.ModelSet;
				CompoundConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is ViewerAffecterConfig) {
				XanLogger.WriteLine("Model is of the type 'ViewerAffecterConfig'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.CameraShake;
				ViewerAffecterConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is ModelConfig.Derived) {
				XanLogger.WriteLine("Model is of the type 'ModelConfig::Derived'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Derived;
				ModelConfigHandler.DerivedHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is ModelConfig.Schemed) {
				XanLogger.WriteLine("Model is of the type 'ModelConfig::Schemed'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Schemed;
				ModelConfigHandler.SchemedHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else if (implementation is ConditionalConfig) {
				XanLogger.WriteLine("Model is of the type 'ConditionalConfig'. Accessing handlers...", true);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Conditional;
				ConditionalConfigHandler.Instance.HandleModelConfig(sourceFile, model, models, currentDataTreeObject, transform, extraData);

			} else {
				XanLogger.WriteLine($"\nERROR: A ModelConfig had an unknown implementation!\n=> Implementation: {implName}\n=> Referenced In: {sourceFile}\n", true);
				// AsyncMessageBox.ShowAsync("This specific implementation is valid, but it has no handler! (There's no code that can translate this data for you :c).\nImplementation: " + implementation.getClass().getTypeName(), "Can't Handle Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				if (currentDataTreeObject != null) currentDataTreeObject.ImageKey = SilkImage.Generic;
			}
		}

	}
}

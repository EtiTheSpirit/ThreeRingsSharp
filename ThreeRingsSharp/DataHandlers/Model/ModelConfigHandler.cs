using com.threerings.config;
using com.threerings.math;
using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.Logging;
using ThreeRingsSharp.Logging.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;
using static com.threerings.opengl.model.config.ModelConfig;

namespace ThreeRingsSharp.DataHandlers.Model {
	public class ModelConfigHandler {

		/// <summary>
		/// Sets up the cosmetic data for this model, or, what's displayed in the GUI for the program. This populates the lower data tree, visualizing configuration information.<para/>
		/// This specific method populates data that is common across all imported models.
		/// </summary>
		/// <param name="model">The model containing other data, such as the <see cref="Implementation"/>.</param>
		/// <param name="dataTreeParent">This is the instance in the data tree that represents this object in the hierarchy. If <see langword="null"/>, this method call is skipped.</param>
		/// <param name="dataTreeNameIsImplementation">If <see langword="true"/>, the name of <paramref name="dataTreeParent"/> is the implementation of the model, so the implementation property will not be added.</param>
		public static void SetupCosmeticInformation(ModelConfig model, DataTreeObject dataTreeParent, bool dataTreeNameIsImplementation) {
			if (dataTreeParent == null) return;
			Implementation impl = model.implementation;

			if (!dataTreeNameIsImplementation) {
				// If this is true, the name of the actual node is the implementation, rendering this property useless.

				string implementationName = JavaClassNameStripper.GetWholeClassName(impl.getClass().getName()) ?? "Unknown Implementation";
				dataTreeParent.AddSimpleProperty("Implementation", implementationName.Replace("$", "::"), SilkImage.Config, SilkImage.Config, true);
			}

			// It's imported!
			// ...Unless it's a CompoundConfig. (I mean given the properties below it makes sense, it's just a container, not actual model data.)
			if (impl is Imported imported) {
				List<object> influences = new List<object>(3);
				if (imported.influences.fog) influences.Add(new DataTreeObjectProperty("Fog", SilkImage.Shading));
				if (imported.influences.lights) influences.Add(new DataTreeObjectProperty("Lights", SilkImage.Light));
				if (imported.influences.projections) influences.Add(new DataTreeObjectProperty("Projections", SilkImage.Texture));
				if (influences.Count > 0) dataTreeParent.AddSimpleProperty("Influenced By...", influences.ToArray(), displaySinglePropertiesInline: false);
			}
			List<object> parameters = new List<object>();
			foreach (Parameter prop in model.parameters) {
				if (prop is Parameter.Direct direct) {
					DataTreeObject paths = new DataTreeObject {
						ImageKey = SilkImage.Tag,
						Text = "Direct: " + direct.name
					};
					int idx = 0;
					foreach (string path in direct.paths) {
						paths.AddSimpleProperty("Path " + idx, path);
						idx++;
					}
					parameters.Add(paths);
				} else if (prop is Parameter.Choice choice) {
					DataTreeObject choices = new DataTreeObject {
						ImageKey = SilkImage.Value,
						Text = "Choice: " + choice.name + " [Current: " + choice.choice + "]"
					};
					choices.ExtraData["ModelConfig"] = model;
					choices.ExtraData["RawOOOChoice"] = choice;

					List<DataTreeObject> choiceList = new List<DataTreeObject>();
					foreach (Parameter.Choice.Option option in choice.options) {
						// choiceList.Add(c);
						DataTreeObject choiceInfo = new DataTreeObject {
							ImageKey = SilkImage.Tag,
							Text = option.name
						};
						ArgumentMap args = option.arguments;
						object[] keys = args.keySet().toArray();
						foreach (object key in keys) {
							choiceInfo.AddSimpleProperty(key.ToString(), args.get(key));
						}
						
						choiceList.Add(choiceInfo);
					}
					choices.AddSimpleProperty("Choices", choiceList.ToArray(), SilkImage.Value, SilkImage.Tag, false);

					List<DataTreeObject> subDirects = new List<DataTreeObject>();
					foreach (Parameter.Direct dir in choice.directs) {
						DataTreeObject dirObj = new DataTreeObject {
							ImageKey = SilkImage.Tag,
							Text = "Direct: " + dir.name
						};
						int idx = 0;
						foreach (string path in dir.paths) {
							dirObj.AddSimpleProperty("Path " + idx, path);
							idx++;
						}
						subDirects.Add(dirObj);
					}
					choices.AddSimpleProperty("Choice Directs", subDirects.ToArray(), SilkImage.Value, SilkImage.Tag, false);
					parameters.Add(choices);
				} else {
					parameters.Add($"{prop.name} [{prop.GetType().FullName}]");
				}
			}
			dataTreeParent.AddSimpleProperty("Parameters", parameters.ToArray(), SilkImage.Value, SilkImage.Tag, false);
		}

		/// <summary>
		/// A class that handles the <see cref="Derived"/> subclass of <see cref="ModelConfig"/>.
		/// </summary>
		public class DerivedHandler : Singleton<DerivedHandler>, IModelDataHandler, IDataTreeInterface<Derived> {
			public void SetupCosmeticInformation(Derived model, DataTreeObject dataTreeParent) {
				if (dataTreeParent == null) return;
				dataTreeParent.AddSimpleProperty("Referenced Model", model.model.getName(), SilkImage.Reference, SilkImage.Reference, false);
			}

			public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
				Derived derived = (Derived)baseModel.implementation;
				// Derived implementations need to send the extra data over.
				ConfigReferenceUtil.HandleConfigReference(sourceFile, derived.model, modelCollection, dataTreeParent, globalTransform, extraData: extraData);
			}

		}

		/// <summary>
		/// A class that handles the <see cref="Schemed"/> subclass of <see cref="ModelConfig"/>.
		/// </summary>
		public class SchemedHandler : Singleton<SchemedHandler>, IModelDataHandler, IDataTreeInterface<Schemed> {
			public void SetupCosmeticInformation(Schemed model, DataTreeObject dataTreeParent) {
				if (dataTreeParent == null) return;

				SchemedModel[] models = model.models;
				List<object> refs = new List<object>();
				foreach (SchemedModel schemedModel in models) {
					refs.Add(schemedModel.model.getName());
				}
				dataTreeParent.AddSimpleProperty(models.Length + " Schemed References", refs.ToArray(), SilkImage.Reference, SilkImage.SchemedModel);
			}

			public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
				Schemed schemed = (Schemed)baseModel.implementation;
				SchemedModel[] models = schemed.models;
				SKAnimatorToolsProxy.IncrementEnd(models.Length);
				foreach (SchemedModel schemedModel in models) {
					// These shouldn't send the extra data over(?)
					ConfigReferenceUtil.HandleConfigReference(sourceFile, schemedModel.model, modelCollection, dataTreeParent, globalTransform);
					SKAnimatorToolsProxy.IncrementProgress();
				}
			}

		}

	}
}

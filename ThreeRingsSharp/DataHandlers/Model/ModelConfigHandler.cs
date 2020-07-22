using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility.Interface;
using static com.threerings.opengl.model.config.ModelConfig;
using static com.threerings.opengl.model.config.ModelConfig.Imported;
using static com.threerings.opengl.model.config.ModelConfig.Implementation;
using ThreeRingsSharp.Utility;
using com.google.inject;
using ThreeRingsSharp.XansData;
using System.IO;
using com.threerings.math;
using ThreeRingsSharp.XansData.Exceptions;
using System.Diagnostics;
using com.threerings.editor;
using com.threerings.config;
using ThreeRingsSharp.XansData.Extensions;
using com.threerings.opengl.material.config;
using ThreeRingsSharp.XansData.XML.ConfigReferences;
using com.threerings.opengl.renderer.config;
using ThreeRingsSharp.DataHandlers.Properties;

namespace ThreeRingsSharp.DataHandlers.Model {
	public class ModelConfigHandler {

		/// <summary>
		/// Sets up the cosmetic data for this model, or, what's displayed in the GUI for the program.<para/>
		/// This specific method populates data that is common across all imported models.
		/// </summary>
		/// <param name="model">The model containing other data, such as the <see cref="Implementation"/>.</param>
		/// <param name="dataTreeParent">This is the instance in the data tree that represents this object in the hierarchy. If null, this method call is skipped.</param>
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
				//RootDataTreeObject.AddSimpleProperty("Scale", model.scale);
				List<object> influences = new List<object>(3);
				if (imported.influences.fog) influences.Add(new DataTreeObjectProperty("Fog", SilkImage.Shading));
				if (imported.influences.lights) influences.Add(new DataTreeObjectProperty("Lights", SilkImage.Light));
				if (imported.influences.projections) influences.Add(new DataTreeObjectProperty("Projections", SilkImage.Texture));
				if (influences.Count > 0) dataTreeParent.AddSimpleProperty("Influenced By...", influences.ToArray(), displaySinglePropertiesInline: false);

				/*
				MaterialMapping[] matMaps = imported.materialMappings;
				object[] materialProperties = new object[matMaps.Length];
				for (int idx = 0; idx < materialProperties.Length; idx++) {
					ConfigReference mtlRef = matMaps[idx].material;
					ConfigReference texCfg = (ConfigReference)mtlRef.getArguments().getOrDefault("Texture", null);
					if (texCfg != null) {
						string texFile = (string)texCfg.getArguments().getOrDefault("File", "?");
						materialProperties[idx] = new DataTreeObjectProperty(texFile, SilkImage.Reference, false);
					} else {
						materialProperties[idx] = matMaps[idx].texture;
					}
				}
				dataTreeParent.AddSimpleProperty("Textures", materialProperties, SilkImage.Value, SilkImage.Texture, false);
				*/
			}
			List<object> parameters = new List<object>();
			foreach (Parameter prop in model.parameters) {
				if (prop is Parameter.Direct direct) {
					DataTreeObject paths = new DataTreeObject() {
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
					DataTreeObject choices = new DataTreeObject() {
						ImageKey = SilkImage.Value,
						Text = "Choice: " + choice.name + " [Default: " + choice.choice + "]"
					};
					List<string> choiceList = new List<string>();
					foreach (string c in choice.GetChoiceOptions()) {
						choiceList.Add(c);
					}
					choices.AddSimpleProperty("Choices", choiceList.ToArray(), SilkImage.Value, SilkImage.Tag, false);

					List<DataTreeObject> subDirects = new List<DataTreeObject>();
					foreach (Parameter.Direct dir in choice.directs) {
						DataTreeObject dirObj = new DataTreeObject() {
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
		/// Given an <see cref="Implementation"/>, this will extract all textures referenced by it. This will return <see langword="null"/> if the <see cref="Implementation"/> is not <see cref="Imported"/>.
		/// </summary>
		/// <param name="sourceFile">The file containing the <see cref="ModelConfig"/> that has defined <paramref name="implementation"/>.</param>
		/// <param name="implementation">The specific type of model that this is.</param>
		/// <returns></returns>
		[Obsolete("Use ModelPropertyUtility.FindTexturesFromDirects instead.", true)] public static List<string> GetTexturesFromModel(FileInfo sourceFile, Implementation implementation) {
			if (implementation is Imported /*imported*/) {
				//List<string> textures = new List<string>();
				// ModelPropertyUtility.TraverseDirectPath(implementation)
				/*
				MaterialMapping[] matMaps = imported.materialMappings;
				MaterialConfig[] matCfgs = (MaterialConfig[])ConfigReferenceBootstrapper.ConfigReferences["material"];
				TextureConfig[] texCfgs = (TextureConfig[])ConfigReferenceBootstrapper.ConfigReferences["texture"];
				for (int idx = 0; idx < matMaps.Length; idx++) {
					MaterialMapping mat = matMaps[idx];
					MaterialConfig mtl = matCfgs.GetEntryByName(mat.material.getName());
					if (mtl != null) {
						while (mtl.implementation is MaterialConfig.Derived derived) {
							mtl = matCfgs.GetEntryByName(derived.material.getName());
						}
						if (mtl.implementation is MaterialConfig.Original original) {
							foreach (TechniqueConfig technique in original.techniques) {
								if (technique.enqueuer is TechniqueConfig.NormalEnqueuer nrm) {
									foreach (PassConfig pass in nrm.passes) {
										if (pass.textureState != null) {
											TextureUnitConfig[] units = pass.textureState.units;
											foreach (TextureUnitConfig unit in units) {
												TextureConfig texCfg = texCfgs.GetEntryByName(unit.texture.getName());
												if (texCfg != null) {
													foreach (Parameter param in texCfg.parameters) {
														if (param is Parameter.Direct direct) {
															if (direct.paths.Contains("implementation.contents.file")) {
																// wow look its a file! finally.
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				*/

				/*
				MaterialMapping[] matMaps = imported.materialMappings;
				for (int idx = 0; idx < matMaps.Length; idx++) {
					MaterialMapping mat = matMaps[idx];
					//string tex = sourceFile.Directory.FullName.Replace("\\", "/") + "/" + matMaps[idx].texture;
					
					string tex = TextureGrabber.GetFullTexturePath(sourceFile, mat.texture);
					if (tex != null) textures.Add(tex);
				}
				return textures;
				*/
			}
			return null;
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
				foreach (SchemedModel schemedModel in models) {
					// These shouldn't send the extra data over(?)
					ConfigReferenceUtil.HandleConfigReference(sourceFile, schemedModel.model, modelCollection, dataTreeParent, globalTransform);
				}
			}

		}

	}
}

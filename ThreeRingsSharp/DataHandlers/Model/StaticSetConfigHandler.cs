using com.threerings.config;
using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThreeRingsSharp.DataHandlers.Properties;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;
using static com.threerings.opengl.model.config.ModelConfig;

namespace ThreeRingsSharp.DataHandlers.Model {
	class StaticSetConfigHandler : IModelDataHandler, IDataTreeInterface<StaticSetConfig> {

		/// <summary>
		/// A reference to the singleton instance of this handler.
		/// </summary>
		public static StaticSetConfigHandler Instance { get; } = new StaticSetConfigHandler();

		public void SetupCosmeticInformation(StaticSetConfig model, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			// So a note to self: Static sets have the 'meshes' container which is a map of other model files.
			// In SK Animator Tools V1 I was an idiot and thought the "model" property was the only key. This is false.
			// Instead, model represents the *default selection*. There may be more models. Iterate through the keys like damn lol.


			DataTreeObjectProperty targetProp = dataTreeParent.AddSimpleProperty("Target Set Model", model.model);
			targetProp.ExtraData["StaticSetConfig"] = model;
			// if (useOnlyTargetModel) dataTreeParent.AddSimpleProperty("Special Directive", "Only export target model", SilkImage.Scripted);

			List<object> objects = new List<object>();
			if (model.meshes != null) {
				object[] keys = model.meshes.keySet().toArray();
				int msIdx = 0;
				foreach (object key in keys) {
					MeshSet subModel = (MeshSet)model.meshes.get(key);
					VisibleMesh[] meshes = subModel.visible;
					DataTreeObject subModelRef = new DataTreeObject() {
						Text = key.ToString(),
						ImageKey = SilkImage.ModelSet
					};

					List<object> subModels = new List<object>();
					int idx = 0;
					foreach (VisibleMesh mesh in meshes) {
						subModels.Add(new DataTreeObjectProperty("Mesh " + idx, SilkImage.Triangle));
						idx++;
					}

					subModelRef.AddSimpleProperty("Geometry", subModels.ToArray(), SilkImage.Variant, SilkImage.Value, false);
					objects.Add(subModelRef);
					msIdx++;
				}
			}
			dataTreeParent.AddSimpleProperty("Contained Meshes", objects.ToArray(), SilkImage.Reference, SilkImage.Reference, false);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
			StaticSetConfig staticSet = (StaticSetConfig)baseModel.implementation;

			SetupCosmeticInformation(staticSet, dataTreeParent);

			string depth1Name = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile);
			string fullDepthName = ResourceDirectoryGrabber.GetDirectoryDepth(sourceFile, -1);

			if (staticSet.meshes != null) {
				SKAnimatorToolsProxy.IncrementEnd(staticSet.meshes.size());

				if (extraData != null && extraData.ContainsKey("DirectArgs")) {
					Dictionary<string, dynamic> directs = extraData["DirectArgs"];
					SKAnimatorToolsProxy.IncrementEnd(1);
					SKAnimatorToolsProxy.SetProgressState(ProgressBarState.ExtraWork);
					bool got = false;
					foreach (string key in directs.Keys) {
						Parameter param = baseModel.getParameter(key);
						if (param is Parameter.Direct direct) {
							if (direct.paths.Contains("implementation.model")) {
								staticSet.model = directs[key];
								XanLogger.WriteLine("Set model to " + staticSet.model, XanLogger.DEBUG);
								SKAnimatorToolsProxy.IncrementProgress();
								got = true;
								break;
							}
						} else if (param is Parameter.Choice choice) {
							foreach (Parameter.Direct dir in choice.directs) {
								if (dir.paths.Contains("implementation.model")) {
									staticSet.model = directs[key];
									XanLogger.WriteLine("Set model to " + staticSet.model, XanLogger.DEBUG);
									SKAnimatorToolsProxy.IncrementProgress();
									got = true;
									break;
								}
							}
						}
					}
					if (!got) SKAnimatorToolsProxy.IncrementProgress(); // Just inc anyway
				}

				// Export them all!
				object[] keys = staticSet.meshes.keySet().toArray();
				SKAnimatorToolsProxy.IncrementEnd(keys.Length);
				foreach (object key in keys) {
					MeshSet subModel = (MeshSet)staticSet.meshes.get(key);
					VisibleMesh[] meshes = subModel.visible;
					int idx = 0;

					SKAnimatorToolsProxy.IncrementEnd(meshes.Length);
					foreach (VisibleMesh mesh in meshes) {
						string meshTitle = "-MeshSets[" + key.ToString() + "].Mesh[" + idx + "]";

						Model3D meshToModel = GeometryConfigTranslator.GetGeometryInformation(mesh.geometry, fullDepthName + meshTitle);
						meshToModel.Name = depth1Name + meshTitle;
						meshToModel.ExtraData["StaticSetEntryName"] = key.ToString();
						meshToModel.ExtraData["StaticSetConfig"] = staticSet;
						if (globalTransform != null) meshToModel.Transform = globalTransform.compose(meshToModel.Transform);

						meshToModel.Textures.SetFrom(ModelPropertyUtility.FindTexturesFromDirects(baseModel));
						meshToModel.ActiveTexture = mesh.texture;

						modelCollection.Add(meshToModel);
						idx++;
						SKAnimatorToolsProxy.IncrementProgress();
					}

					SKAnimatorToolsProxy.IncrementProgress();
				}
			}
		}
	}
}

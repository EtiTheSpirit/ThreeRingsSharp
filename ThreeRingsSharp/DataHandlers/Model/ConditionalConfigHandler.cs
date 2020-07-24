using com.threerings.config;
using com.threerings.expr;
using com.threerings.math;
using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;

namespace ThreeRingsSharp.DataHandlers.Model {
	public class ConditionalConfigHandler : Singleton<ConditionalConfigHandler>, IModelDataHandler, IDataTreeInterface<ConditionalConfig> {

		private DataTreeObjectProperty[] MakeModelTransformPair(ConfigReference model, Transform3D transform, BooleanExpression expr = null) {
			string mdlName = model?.getName() ?? "null";
			string trs = transform?.toString() ?? "null";
			if (expr != null) {
				string state = "???";
				try {
					state = expr.createEvaluator(DummyScope.Instance).evaluate().ToString();
				} catch { }
				return new DataTreeObjectProperty[] {
					new DataTreeObjectProperty("Condition: " + expr.toString() + " = " + state, SilkImage.Conditional),
					new DataTreeObjectProperty("Model: " + mdlName, SilkImage.Reference),
					new DataTreeObjectProperty("Transform: " + trs, SilkImage.Matrix)
				};
			} else {
				return new DataTreeObjectProperty[] {
					new DataTreeObjectProperty("Model: " + mdlName, SilkImage.Reference),
					new DataTreeObjectProperty("Transform: " + trs, SilkImage.Matrix)
				};
			}
		}

		public void SetupCosmeticInformation(ConditionalConfig data, DataTreeObject dataTreeParent) {
			if (dataTreeParent == null) return;

			dataTreeParent.AddSimpleProperty("Default Data", MakeModelTransformPair(data.defaultModel, data.defaultTransform), SilkImage.Conditional);

			DataTreeObject optionContainer = new DataTreeObject() {
				ImageKey = SilkImage.Array
			};
			int idx = 0;
			foreach (ConditionalConfig.Case condition in data.cases) {
				optionContainer.AddSimpleProperty("Case " + idx, MakeModelTransformPair(condition.model, condition.transform, condition.condition), SilkImage.Conditional);
				idx++;
			}
			dataTreeParent.AddSimpleProperty("Cases", optionContainer, SilkImage.Array);
		}

		public void HandleModelConfig(FileInfo sourceFile, ModelConfig baseModel, List<Model3D> modelCollection, DataTreeObject dataTreeParent = null, Transform3D globalTransform = null, Dictionary<string, dynamic> extraData = null) {
			ConditionalConfig model = (ConditionalConfig)baseModel.implementation;
			SetupCosmeticInformation(model, dataTreeParent);


			if (model.defaultModel != null) {
				List<Model3D> mdls = ConfigReferenceUtil.HandleConfigReference(sourceFile, model.defaultModel, modelCollection, dataTreeParent, globalTransform, false, extraData);
				foreach (Model3D mdl in mdls) {
					mdl.ExtraData["ConditionalConfigFlag"] = true;
					mdl.ExtraData["ConditionalConfigDefault"] = true;
					if (model.defaultTransform != null) mdl.Transform = model.defaultTransform;
					modelCollection.Add(mdl);
				}
			}

			foreach (ConditionalConfig.Case condition in model.cases) {
				List<Model3D> mdls = ConfigReferenceUtil.HandleConfigReference(sourceFile, condition.model, modelCollection, dataTreeParent, globalTransform, false, extraData);
				foreach (Model3D mdl in mdls) {
					mdl.ExtraData["ConditionalConfigFlag"] = true;
					mdl.ExtraData["ConditionalConfigValue"] = true;
					try {
						bool state = condition.condition.createEvaluator(DummyScope.Instance).evaluate();
						mdl.ExtraData["ConditionalConfigValue"] = state;
					} catch { }
					if (condition.transform != null) mdl.Transform = condition.transform;
					modelCollection.Add(mdl);
				}
			}
		}
	}
}

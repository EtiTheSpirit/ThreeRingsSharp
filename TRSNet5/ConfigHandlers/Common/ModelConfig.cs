using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities.Parameters;
using ThreeRingsSharp.Utilities.Parameters.Implementation;
using ThreeRingsSharp.XansData.XDataTreeExtension;
using XDataTree;
using XDataTree.Data;
using XDataTree.TreeElements;
using static ThreeRingsSharp.Utilities.Parameters.Implementation.Direct;

namespace ThreeRingsSharp.ConfigHandlers.Common {
	public static class ModelConfig {

		/// <summary>
		/// Asserts that the the given <see cref="ShadowClass"/> is an instance of <c>com.threerings.opengl.model.config.ModelConfig</c>, and then if it is, it returns its implementation field, asserting that it is an instance of <paramref name="class"/>.
		/// </summary>
		/// <param name="modelConfig"></param>
		/// <param name="class"></param>
		/// <returns></returns>
		public static ShadowClass GetConfigFromFileSC(ShadowClass modelConfig, string @class) {
			modelConfig.AssertIsInstanceOf("com.threerings.opengl.model.config.ModelConfig");
			ShadowClass implementation = modelConfig["implementation"]!;
			implementation.AssertIsInstanceOf(@class);
			return implementation;
		}

		/// <summary>
		/// Given a ParameterizedConfig, this will observe its parameters and construct a new <see cref="KeyValueContainerElement"/> to put in the object's properties.
		/// </summary>
		/// <returns></returns>
		public static GenericElement SetupParametersForProperties(ShadowClass parameterizedConfig) {
			parameterizedConfig.AssertIsInstanceOf("com.threerings.config.ParameterizedConfig");
			Parameter[] parameters = parameterizedConfig.GetParameters();
			GenericElement paramElement = new GenericElement("Parameters", SilkImage.Config);
			for (int idx = 0; idx < parameters.Length; idx++) {
				Parameter param = parameters[idx];
				PopulateParameter(param, paramElement);
			}
			return paramElement;
		}

		private static void PopulateParameter(Parameter param, TreeElement parent) {
			if (param is Choice choice) {
				GenericElement choiceElement = new GenericElement("Choice: " + param.Name, SilkImage.Tag);
				ChoiceElement current = new ChoiceElement("Current", choice.CurrentName, true, SilkImage.Value) {
					ValueHolder = param.ParameterizedConfig,
					Choice = choice
				};
				GenericElement directs = new GenericElement("Directs", SilkImage.Tag);
				KeyValueContainerElement choices = new KeyValueContainerElement("Choices", SilkImage.Tag);

				foreach (Direct direct in choice.Directs) {
					PopulateParameter(direct, directs);
				}
				for (int optIdx = 0; optIdx < choice.OptionNames.Length; optIdx++) {
					choices.Add($"Choice #{optIdx + 1}", choice.OptionNames[optIdx], SilkImage.Value);
				}
				choiceElement.Add(current);
				choiceElement.Add(directs);
				choiceElement.Add(choices);
				parent.Add(choiceElement);

			} else if (param is Direct direct) {
				SilkImage icon = SilkImage.Tag;
				for (int pathIdx = 0; pathIdx < direct.Pointers.Length; pathIdx++) {
					if (direct.Pointers[pathIdx] is FaultyDirectPointer) {
						icon = SilkImage.RedTag;
						break;
					}
				}
				KeyValueContainerElement directElement = new KeyValueContainerElement("Direct: " + param.Name, icon);
				if (icon == SilkImage.RedTag) {
					directElement.Tooltip = "This direct is faulty because one or more of its paths couldn't be resolved.";
				}
				for (int pathIdx = 0; pathIdx < direct.Paths.Length; pathIdx++) {
					// TODO: Wouldn't it be cool if you could click on one of these, and then it'd change the data tree's selection to the object it affects?
					// Like both the main tree *and* the properties tree, e.g. selects applicable model in main tree, selects applicable property in props.
					directElement.Add($"Paths[{pathIdx}]", direct.Paths[pathIdx], SilkImage.Value);
				}
				parent.Add(directElement);
			}
		}

	}
}

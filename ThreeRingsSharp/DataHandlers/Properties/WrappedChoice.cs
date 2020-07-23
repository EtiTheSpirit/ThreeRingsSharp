using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.DataHandlers.Properties {
	public class WrappedChoice {

		/// <summary>
		/// The string choice that is currently selected. To get the <see cref="WrappedDirect"/> that is associated with this choice, consider calling <see cref="CurrentSelectedChoiceDirect"/>.<para/>
		/// This property's <see langword="setter"/> will throw an <see cref="ArgumentException"/> if the given <see cref="string"/> is not included in <see cref="PossibleChoices"/>. If the given <see cref="string"/> is <see langword="null"/>, it will be replaced with <see cref="DefaultChoice"/>.<para/>
		/// <para/>
		/// WARNING: Object not functional entirely.
		/// </summary>
		public string Choice {
			get => _Choice;
			set {
				if (value == null) {
					_Choice = DefaultChoice;
					return;
				}
				if (!PossibleChoices.Contains(value)) throw new ArgumentException("The given choice is not a valid option! See the PossibleChoices property of this WrappedChoice for a list of legal choices.");
				_Choice = value;
			}
		}
		private string _Choice = null;

		/// <summary>
		/// The stock choice that the underlying <see cref="Parameter.Choice"/> came with as its default option.
		/// </summary>
		public string DefaultChoice { get; }

		/// <summary>
		/// A reference to <see cref="BaseChoice"/><c>.getChoiceOptions</c> (see <see cref="Parameter.Choice.getChoiceOptions"/>) which returns a string array of all legal choices.
		/// </summary>
		public string[] PossibleChoices => BaseChoice.GetChoiceOptions();

		/// <summary>
		/// The reference to the Clyde object associated with this choice.
		/// </summary>
		public ParameterizedConfig Config { get; }

		/// <summary>
		/// The underlying <see cref="Parameter.Choice"/> that this <see cref="WrappedChoice"/> was created from.
		/// </summary>
		public Parameter.Choice BaseChoice { get; }

		/// <summary>
		/// The actual options in this <see cref="WrappedChoice"/>.
		/// </summary>
		public WrappedChoiceOption[] Options { get; }

		/// <summary>
		/// Construct a new <see cref="WrappedChoice"/> from the underlying Clyde object and the given <see cref="Parameter.Choice"/>.
		/// </summary>
		/// <param name="cfg"></param>
		/// <param name="choice"></param>
		/// <param name="args"></param>
		public WrappedChoice(ParameterizedConfig cfg, Parameter.Choice choice, ArgumentMap args = null) {
			_Choice = choice.choice;
			DefaultChoice = choice.GetDefaultOption().name;
			Config = cfg;
			BaseChoice = choice;
			
			List<WrappedChoiceOption> options = new List<WrappedChoiceOption>();
			foreach (Parameter.Choice.Option option in choice.options) {
				WrappedChoiceOption wrappedChoice = new WrappedChoiceOption(this, option, args);
				options.Add(wrappedChoice);
			}
			Options = options.ToArray();
		}

		/// <summary>
		/// Attempts to return the associated <see cref="WrappedChoiceOption"/> with this <see cref="WrappedChoice"/>'s current <see cref="Choice"/> property.
		/// </summary>
		/// <returns></returns>
		public WrappedChoiceOption CurrentSelectedChoice() => GetOptionFromChoiceName(Choice);

		/// <summary>
		/// Returns the <see cref="WrappedChoiceOption"/> associated with this <see cref="DefaultChoice"/>.
		/// </summary>
		/// <returns></returns>
		public WrappedChoiceOption GetDefaultChoice() => GetOptionFromChoiceName(DefaultChoice);

		/// <summary>
		/// Given a <see cref="string"/> <paramref name="choice"/>, this will attempt to return its associated <see cref="WrappedChoiceOption"/>, or null if it could not be found.
		/// </summary>
		/// <param name="choice"></param>
		/// <returns></returns>
		public WrappedChoiceOption GetOptionFromChoiceName(string choice) {
			foreach (WrappedChoiceOption option in Options) {
				if (option.Name == choice) {
					return option;
				}
			}
			return null;
		}

		/// <summary>
		/// Clones this <see cref="Config"/> for each entry in <see cref="Options"/>, and applies that option's data to the cloned <see cref="ParameterizedConfig"/>.<para/>
		/// This returns an array of these duplicated <see cref="ParameterizedConfig"/>s. The indices are parallel to <see cref="Options"/> (so indexing [1] on the return value of this method will return the data applied by Options[1])
		/// </summary>
		/// <returns></returns>
		public ParameterizedConfig[] CreateVariantsFromOptions() {
			ParameterizedConfig[] cfgArray = new ParameterizedConfig[Options.Length];
			for (int idx = 0; idx < cfgArray.Length; idx++) {
				WrappedChoiceOption option = Options[idx];
				ParameterizedConfig dupe = (ParameterizedConfig)Config.clone();
				dupe.ApplyArguments(option.Arguments, BaseChoice.name);
				cfgArray[idx] = dupe;
			}
			return cfgArray;
		}

	}
}

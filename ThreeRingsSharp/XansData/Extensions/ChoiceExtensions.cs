using com.threerings.config;
using System;
using System.Linq;

namespace ThreeRingsSharp.XansData.Extensions {

	/// <summary>
	/// Offers methods that make grabbing information from <see cref="Parameter.Choice"/>s easier.
	/// </summary>
	public static class ChoiceExtensions {

		/// <summary>
		/// A patched variant of <see cref="Parameter.Choice.getChoiceOptions()"/> that doesn't commit <see href="https://www.youtube.com/watch?v=Lebv2-ptzWY"/> when you call it.<para/>
		/// This returns a string array of every option name that displays in the dropdown menu of the ThreeRings Model Editor, should there be a multi-choice option on a model.
		/// </summary>
		/// <param name="choice">The choice to get the list of options from.</param>
		/// <returns></returns>
		public static string[] GetChoiceOptions(this Parameter.Choice choice) {
			string[] options = new string[choice.options.Length];
			for (int index = 0; index < choice.options.Length; index++) {
				options[index] = choice.options[index].name;
			}
			return options;
		}

		/// <summary>
		/// Returns the default option of this choice. Will return null if it couldn't be found for whatever reason.
		/// </summary>
		/// <param name="choice">The choice to get the default option from.</param>
		/// <returns></returns>
		public static Parameter.Choice.Option GetDefaultOption(this Parameter.Choice choice) {
			return choice.options.Where(opt => opt.name == choice.choice).FirstOrDefault();
		}

		/// <summary>
		/// Returns the arguments of this <see cref="Parameter.Choice.Option"/> that are applied to the directs within its parent choice.
		/// </summary>
		/// <param name="option">The option to get the arguments from.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">If choice is null</exception>
		public static ArgumentMap GetArguments(this Parameter.Choice.Option option) {
			if (option == null) throw new ArgumentNullException("option");
			try {
				// TODO: Go back into IL and hide _arguments again.
				// I did that as a lazy method of accessing it but I'd rather do it like this as odd as that might be.
				return option.GetType().GetField("_arguments").GetValue(option) as ArgumentMap;
			} catch {
				// So for some god-awful reason, there's SOME CASES where _arguments flat out doesn't exist. Like GetField returns null.
				// wat
				return new ArgumentMap();
			}
		}
	}
}

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
		/// Returns the default option of this choice. Will return <see langword="null"/> if it couldn't be found for whatever reason.<para/>
		/// All this does is iterate through <see cref="Parameter.Choice.options"/> and attempts to locate <see cref="Parameter.Choice.choice"/>. If it is found in the options array, it simply returns <see cref="Parameter.Choice.choice"/>, effectively validating the choice is proper or not.
		/// </summary>
		/// <param name="choice">The choice to get the default option from.</param>
		/// <returns></returns>
		[Obsolete("Way too expensive for such a simple thing. Just reference choice.choice")] public static Parameter.Choice.Option GetDefaultOption(this Parameter.Choice choice) {
			return choice.options.Where(opt => opt.name == choice.choice).FirstOrDefault();
		}

		/// <summary>
		/// Returns the arguments of this <see cref="Parameter.Choice.Option"/> that are applied to the directs within its parent choice.
		/// </summary>
		/// <param name="option">The option to get the arguments from.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">If choice is null</exception>
		[Obsolete("An IL edit to OOOLibAndDeps allows you to directly reference the arguments field. Please reference the arguments field instead of calling this extension.")]
		public static ArgumentMap GetArguments(this Parameter.Choice.Option option) {
			if (option == null) throw new ArgumentNullException("option");

			// To readers: This field was originally _arguments but I decided to expose it in IL.
			return option.arguments;
		}
	}
}

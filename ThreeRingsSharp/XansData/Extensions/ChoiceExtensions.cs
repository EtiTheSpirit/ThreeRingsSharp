using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.threerings.config;

namespace ThreeRingsSharp.XansData.Extensions {

	/// <summary>
	/// As mentioned in <see cref="Parameter.Choice.getChoiceOptions()"/>'s customized error, IKVM was unable to correctly convert this method.<para/>
	/// This static class houses an extension method that fixes this.
	/// </summary>
	public static class ChoiceExtensions {

		/// <summary>
		/// A patched variant of <see cref="Parameter.Choice.getChoiceOptions()"/> that doesn't commit <see href="https://www.youtube.com/watch?v=Lebv2-ptzWY"/> when you call it.<para/>
		/// This returns a string array of every option name that displays in the dropdown menu of the ThreeRings Model Editor, should there be a multi-choice option on a model.
		/// </summary>
		/// <param name="choice"></param>
		/// <returns></returns>
		public static string[] GetChoiceOptions(this Parameter.Choice choice) {
			string[] options = new string[choice.options.Length];
			for (int index = 0; index < choice.options.Length; index++) {
				options[index] = choice.options[index].name;
			}
			return options;
		}

		/// <summary>
		/// Returns the default option of this choice.
		/// </summary>
		/// <param name="choice"></param>
		/// <returns></returns>
		public static Parameter.Choice.Option GetDefaultOption(this Parameter.Choice choice) {
			return choice.options.Where(opt => opt.name == choice.choice).FirstOrDefault();
		}
	}
}

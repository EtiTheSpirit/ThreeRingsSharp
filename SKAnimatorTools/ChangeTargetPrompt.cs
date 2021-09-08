using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ThreeRingsSharp.Utilities.Parameters.Implementation;

namespace SKAnimatorTools {
	public partial class ChangeTargetPrompt : Form {
		public ChangeTargetPrompt() {
			InitializeComponent();
		}

		/// <summary>
		/// The available options.
		/// </summary>
		public string[] Options { get; private set; } = Array.Empty<string>();

		/// <summary>
		/// The current selected option for <see cref="Model"/> or <see cref="Choice"/> to use.
		/// </summary>
		public string? Option { get; private set; } = string.Empty;

		/// <summary>
		/// The <see cref="StaticSetConfig"/> that this is modifying, if applicable.
		/// </summary>
		public ShadowClass? Model { get; private set; }

		/// <summary>
		/// The <see cref="Parameter.Choice"/> that this is modifying, if applicable.
		/// </summary>
		public Choice? Choice { get; private set; }

		/// <summary>
		/// For cases where <see cref="Choice"/> is not <see langword="null"/>, this is the ParameterizedConfig that it is a part of that will be changed.
		/// </summary>
		public ShadowClass? ChoiceAffects { get; private set; }

		/// <summary>
		/// The node that is responsible for visualizing this option.
		/// </summary>
		public TreeNode? Node { get; set; }

		/// <summary>
		/// For StaticSetConfigs, this sets up the dialog to modify the target model of the set.
		/// </summary>
		/// <param name="set"></param>
		public void SetPossibleOptionsFrom(ShadowClass set) {
			set.AssertIsInstanceOf("com.threerings.opengl.model.config.StaticSetConfig");
			Model = set;

			Dictionary<object, object>.KeyCollection keys = set["meshes"]!.Keys;
			List<string> options = new List<string>();
			foreach (object o in keys) {
				options.Add(o.ToString() ?? "null");
			}
			Options = options.ToArray();
			Option_NewTarget.Text = set["model"]!;
			Option = set["model"]!;

			Option_NewTarget.Items.AddRange(Options);
			BtnSave.Enabled = true;
		}

		/// <summary>
		/// For any general ParameterizedConfig, this sets up the dialog to modify a Choice's parameter.
		/// </summary>
		/// <param name="cfg"></param>
		/// <param name="choice"></param>
		public void SetPossibleOptionsFrom(ShadowClass cfg, Choice choice) {
			cfg.AssertIsInstanceOf("com.threerings.config.ParameterizedConfig");
			Choice = choice;

			Options = choice.OptionNames;
			Option_NewTarget.Text = choice.CurrentName;
			Option = choice.CurrentName;

			ChoiceAffects = cfg;

			Option_NewTarget.Items.AddRange(Options);
			BtnSave.Enabled = true;
		}

		/// <summary>
		/// Given an option name, this searches <see cref="Options"/> to see if the given string is a valid option. It then returns the associated option with the proper casing.
		/// </summary>
		/// <param name="option"></param>
		/// <returns></returns>
		public string? GetOptionFromCaseless(string option) {
			IEnumerable<string> trimmed = Options.Where(opt => opt.ToLower() == option.ToLower());
			if (trimmed.Count() != 1) return null;
			return trimmed.First();
		}

		private void Option_NewTarget_SelectedIndexChanged(object sender, EventArgs e) {
			Option = GetOptionFromCaseless(Option_NewTarget.Text);
			if (Option != null) {
				BtnSave.Enabled = true;
				Option_NewTarget.ForeColor = Color.Black;
			} else {
				BtnSave.Enabled = false;
				Option_NewTarget.ForeColor = Color.Red;
			}
		}

		private void BtnSave_Click(object sender, EventArgs e) {
			if (Model != null) Model["model"] = Option;
			if (Choice != null && Option != null) Choice.CurrentName = Option;
			Close();
		}

		private void BtnCancel_Click(object sender, EventArgs e) {
			Close();
		}
	}
}

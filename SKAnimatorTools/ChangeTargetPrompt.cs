using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using com.threerings.config;
using ThreeRingsSharp.XansData.Extensions;

namespace SKAnimatorTools {
	public partial class ChangeTargetPrompt : Form {
		public ChangeTargetPrompt() {
			InitializeComponent();
		}

		/// <summary>
		/// The available options.
		/// </summary>
		public string[] Options { get; private set; } = new string[0];

		/// <summary>
		/// The current selected option for <see cref="Model"/> or <see cref="Choice"/> to use.
		/// </summary>
		public string Option { get; private set; } = null;

		/// <summary>
		/// The <see cref="StaticSetConfig"/> that this is modifying, if applicable.
		/// </summary>
		public StaticSetConfig Model { get; private set; } = null;

		/// <summary>
		/// The <see cref="Parameter.Choice"/> that this is modifying, if applicable.
		/// </summary>
		public Parameter.Choice Choice { get; private set; } = null;

		/// <summary>
		/// For cases where <see cref="Choice"/> is not <see langword="null"/>, this is the <see cref="ParameterizedConfig"/> that it is a part of that will be changed.
		/// </summary>
		public ParameterizedConfig ChoiceAffects { get; private set; } = null;

		public TreeNode Node { get; set; } = null;

		public void SetPossibleOptionsFrom(StaticSetConfig set) {
			Model = set;
			Options = set.getModelOptions();
			Option_NewTarget.Text = set.model;
			Option = set.model;

			Option_NewTarget.Items.AddRange(Options);
			BtnSave.Enabled = true;
		}

		public void SetPossibleOptionsFrom(ParameterizedConfig cfg, Parameter.Choice choice) {
			Choice = choice;
			Options = choice.GetChoiceOptions();
			Option_NewTarget.Text = choice.choice;
			Option = choice.choice;

			ChoiceAffects = cfg;

			Option_NewTarget.Items.AddRange(Options);
			BtnSave.Enabled = true;
		}

		/// <summary>
		/// Given an option name, this searches <see cref="Options"/> to see if the given string is a valid option. It then returns the associated option with the proper casing.
		/// </summary>
		/// <param name="option"></param>
		/// <returns></returns>
		public string GetOptionFromCaseless(string option) {
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
			if (Model != null) Model.model = Option;
			if (Choice != null)	Choice.choice = Option;
			Close();
		}

		private void BtnCancel_Click(object sender, EventArgs e) {
			Close();
		}
	}
}

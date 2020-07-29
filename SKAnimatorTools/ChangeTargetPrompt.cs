using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static com.threerings.opengl.model.config.ModelConfig;

namespace SKAnimatorTools {
	public partial class ChangeTargetPrompt : Form {
		public ChangeTargetPrompt() {
			InitializeComponent();
		}

		public string[] Options { get; private set; } = new string[0];

		public string Option { get; private set; } = null;

		public StaticSetConfig Model { get; private set; } = null;

		public TreeNode Node { get; set; } = null;

		public void SetPossibleOptionsFrom(StaticSetConfig set) {
			Model = set;
			Options = set.getModelOptions();
			Option_NewTarget.Text = set.model;
			Option = set.model;

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
			Model.model = Option;
			Close();
		}

		private void BtnCancel_Click(object sender, EventArgs e) {
			Close();
		}
	}
}

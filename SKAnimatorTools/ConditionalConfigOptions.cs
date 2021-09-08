using System;
using System.Windows.Forms;

namespace SKAnimatorTools {
	public partial class ConditionalConfigOptions : Form {
		public ConditionalConfigOptions() {
			InitializeComponent();
		}

		private void BtnExportAll_Click(object sender, EventArgs e) {
			DialogResult = (DialogResult)1;
			Close();
		}

		private void BtnOnlyEnabled_Click(object sender, EventArgs e) {
			DialogResult = (DialogResult)2;
			Close();
		}

		private void BtnOnlyDisabled_Click(object sender, EventArgs e) {
			DialogResult = (DialogResult)3;
			Close();
		}

		private void BtnOnlyDefault_Click(object sender, EventArgs e) {
			DialogResult = (DialogResult)4;
			Close();
		}

		private void BtnCancel_Click(object sender, EventArgs e) {
			DialogResult = (DialogResult)5; // lol
			// edit: 5 is not real
			Close();
		}

	}
}

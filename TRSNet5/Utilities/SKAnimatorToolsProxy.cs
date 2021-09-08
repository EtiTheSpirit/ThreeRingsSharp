#nullable disable // These will always be initialized before the program runs so we can ignore nulls.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SKAnimatorTools.PrimaryInterface {

	/// <summary>
	/// Provides a proxy to the primary GUI that allows outside tasks to communicate with the GUI.
	/// </summary>
	public static class SKAnimatorToolsProxy {

		public static BackgroundWorker ModelLoaderWorker { get; set; }

		/// <summary>
		/// Should be a ColoredProgressBar instance.
		/// </summary>
		public static object Progress { get; set; }

		public static SynchronizationContext UISyncContext { get; set; }

		public static Action<string, string, string, string> UpdateGUIAction { get; set; }

		/// <summary>
		/// Last parameter is a ProgressBarState enum.
		/// </summary>
		public static Action<int?, int?, object> ConfigsLoadingAction { get; set; }

		public static void ResetProgress() {
			dynamic cpb = (dynamic)Progress;
			cpb.Value = 0;
			cpb.Maximum = 1;
		}

	}
}

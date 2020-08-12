using System;
using System.Threading;
using System.Windows.Forms;
using ThreeRingsSharp.Utility;

namespace SKAnimatorTools {
	static class Program {

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			XanLogger.MainThreadId = Thread.CurrentThread.ManagedThreadId;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainWindow());
		}
	}
}

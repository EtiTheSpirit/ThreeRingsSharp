using com.threerings.export.tools;
using ikvm.runtime;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using ThreeRingsSharp.Utility;

namespace SKAnimatorTools {
	static class Program {

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			XanLogger.MainThreadId = Thread.CurrentThread.ManagedThreadId;
			Startup.addBootClassPathAssembly(Assembly.Load("OOOLibAndDeps"));

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainWindow(args));
		}
	}
}

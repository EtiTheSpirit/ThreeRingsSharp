using EtiLogger.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKAnimatorTools.PrimaryInterface {
	public static class XanLogger {

		//public static readonly XanLogger Instance = new XanLogger();

		//private XanLogger() { }

		#region Main Data

#if DEBUG
		public static readonly bool IsDebugMode = true;
#else
		public static readonly bool IsDebugMode = false;
#endif

		private static readonly Logger InternalLog = new Logger();

		public static void InitializeWith(RichTextBox rtb) {
			InternalLog.DefaultInfoColor = new EtiLogger.Data.Structs.Color(0, 0, 0);
			InternalLog.DefaultDebugColor = new EtiLogger.Data.Structs.Color(31, 31, 31);
			InternalLog.DefaultTraceColor = new EtiLogger.Data.Structs.Color(127, 127, 127);
			InternalLog.Target = new TRSLog(rtb);
		}

		#endregion

		#region Logging Levels

		public static int LoggingLevel = 0;

		public const int INFO = 0;

		public const int DEBUG = 1;

		public const int TRACE = 2;

		#endregion

		#region Writing

		public static void WriteLine(string text, int logLevel = 0, Color? color = null) {
			InternalLog.WriteLine(new LogMessage(new LogMessage.MessageComponent(text, (EtiLogger.Data.Structs.Color?)color)), (LogLevel)logLevel);
		}

		public static void LogException(Exception exc) {
			InternalLog.WriteException(exc);
		}

		#endregion

	}
}

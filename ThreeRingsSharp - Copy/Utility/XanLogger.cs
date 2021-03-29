using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData.XML.ConfigReferences;

namespace ThreeRingsSharp.Utility {
	public static class XanLogger {

		#region Log Levels

		/// <summary>
		/// Indicates a standard log message that is shown no matter what.
		/// </summary>
		public const int STANDARD = 0;

		/// <summary>
		/// Indicates a debug log message, which is generally more frequent and used to acutely guide the developer through code.
		/// </summary>
		public const int DEBUG = 1;

		/// <summary>
		/// Indicates a trace log message, which is vastly more frequent and used to relay very minuscule details.
		/// </summary>
		public const int TRACE = 2;

		#endregion

		/// <summary>
		/// The active log file.
		/// </summary>
		private static FileStream LogFileStream { get; }

		/// <summary>
		/// The log while <see cref="UpdateAutomatically"/> is false and it's written to (this is used to append all of the data when <see cref="UpdateLog"/> is called)
		/// </summary>
		private static List<(string, Color, int)> LogWhileNotUpdating { get; } = new List<(string, Color, int)>();

		/// <summary>
		/// The level of messages to display.
		/// </summary>
		public static int LoggingLevel { get; set; } = IsDebugMode ? DEBUG : STANDARD;

#if DEBUG
		/// <summary>
		/// Represents whether or not the program is running in Debug mode.<para/>
		/// Current State: <see langword="true"/>
		/// </summary>
		public static readonly bool IsDebugMode = true;
#else
		/// <summary>
		/// Represents whether or not the program is running in Debug mode.<para/>
		/// Current State: <see langword="false"/>
		/// </summary>
		public static readonly bool IsDebugMode = false;
#endif
		/// <summary>
		/// If <see langword="true"/>, the textbox will be updated to display new text the moment it is written. If <see langword="false"/>, <see cref="UpdateLog"/> can be called, which will copy the contents of <see cref="LogWhileNotUpdating"/> and append it to the textbox.<para/>
		/// Setting this to true will cause <see cref="UpdateLog"/> to run.
		/// </summary>
		public static bool UpdateAutomatically {
			get => _UpdateAutomatically;
			set {
				_UpdateAutomatically = value;
				if (!value) UpdateLog();
			}
		}
		private static bool _UpdateAutomatically = true;

		/// <summary>
		/// A reference to a textbox that should store the contents of the log in a GUI application.<para/>
		/// This should be an instance of <see cref="RichTextBox"/> for proper function. It can be <see langword="null"/> if there is no GUI.
		/// </summary>
		public static RichTextBox BoxReference { get; set; }

		/// <summary>
		/// If true, the box *was* at the bottom before text was appended to it (meaning it should autoscroll)
		/// </summary>
		private static bool WasAtBottom { get; set; }

		/// <summary>
		/// Intended to be set once in the main method of the program, this is the ID of the program's main thread.<para/>
		/// This is used to determine if it is safe to write to the GUI console or not.
		/// </summary>
		public static int MainThreadId { get; set; } = 0;

		/// <summary>
		/// This will be <see langword="true"/> if the program is running in its main thread (and by extension, it is safe to update the GUI).<para/>
		/// This will only be <see langword="false"/> in async calls, such as that in <see cref="ConfigReferenceBootstrapper"/>'s initialization method.
		/// </summary>
		public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == MainThreadId;

		private static bool IsUpdatingGUI = false;
		private static readonly ManualResetEventSlim UpdateComplete = new ManualResetEventSlim();

		/// <summary>
		/// Calls <see cref="ForceUpdateLog"/>, but respects user preferences that dictate to not write to the log (for instance, <see cref="SKAnimatorToolsProxy.PreferSpeedOverFeedback"/>.
		/// </summary>
		public static void UpdateLog() {
			if (SKAnimatorToolsProxy.PreferSpeedOverFeedback) return;
			ForceUpdateLog();
		}

		/// <summary>
		/// Manually update the contents of <see cref="BoxReference"/>. Only works if <see cref="UpdateAutomatically"/> is <see langword="false"/>, and of course, if <see cref="BoxReference"/> is not <see langword="null"/>.
		/// </summary>
		public static void ForceUpdateLog() {
			if (IsUpdatingGUI) return;
			if (!IsMainThread) return;
			if (BoxReference == null) return;

			IsUpdatingGUI = true;
			UpdateComplete.Reset();
			WasAtBottom = BoxReference.IsScrolledToBottom();
			foreach ((string, Color, int) logEntry in LogWhileNotUpdating) {
				BoxReference.AppendText(logEntry.Item1, logEntry.Item2);
			}

			if (WasAtBottom && !BoxReference.IsScrolledToBottom()) {
				BoxReference.SelectionStart = BoxReference.TextLength;
				BoxReference.ScrollToCaret();
			}

			LogWhileNotUpdating.Clear();
			BoxReference.Update();
			IsUpdatingGUI = false;
			UpdateComplete.Set();
		}

		/// <summary>
		/// Append a new line to the log.
		/// </summary>
		/// <param name="logLevel">The level to log. If this is greater than <see cref="LoggingLevel"/>, it will not be appended to the log.</param>
		/// <param name="color">The color of the text in the log.</param>
		public static void WriteLine(int logLevel = STANDARD, Color? color = null) => Write("\n", logLevel, color);

		/// <summary>
		/// Append the given text to the log and advance by one line.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="logLevel">The level to log. If this is greater than <see cref="LoggingLevel"/>, it will not be appended to the log.</param>
		/// <param name="color">The color of the text in the log.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <see langword="null"/>.</exception>
		public static void WriteLine(object obj, int logLevel = STANDARD, Color? color = null) {
			Write((obj?.ToString() ?? "null") + "\n", logLevel, color);
		}

		/// <summary>
		/// Append the given text to the log.
		/// </summary>
		/// <param name="obj">The text to write to the log.</param>
		/// <param name="logLevel">The level to log. If this is greater than <see cref="LoggingLevel"/>, it will not be appended to the log.</param>
		/// <param name="color">The color of the text in the log.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <see langword="null"/>.</exception>
		public static void Write(object obj, int logLevel = STANDARD, Color? color = null) {
			if (obj == null) throw new ArgumentNullException("obj");
			string text = obj?.ToString() ?? "null";
			string strippedText = VTConsole.StripColorFormattingCode(text);
			bool canLog = logLevel <= LoggingLevel;

			string prefix = "";
			if (logLevel == DEBUG) prefix = "[DEBUG] ";
			if (logLevel == TRACE) prefix = "[TRACE] ";
			byte[] fileWrite = Encoding.Unicode.GetBytes(prefix + strippedText);
			LogFileStream.Write(fileWrite, 0, fileWrite.Length);

			if (!canLog) return;
			VTConsole.ForegroundColor = color.HasValue ? ConsoleColorVT.FromColor(color.Value) : ConsoleColor.White;
			VTConsole.Write(text);

			if (BoxReference == null) return;

			Color defColor = BoxReference.ForeColor;
			if (logLevel == DEBUG) defColor = Color.Gray;
			if (logLevel == TRACE) defColor = Color.LightGray;
			Color writeColor = color.GetValueOrDefault(defColor);

			bool oldUpdateAutoValue = UpdateAutomatically;
			if (!IsMainThread) {
				_UpdateAutomatically = false;
			}

			if (IsUpdatingGUI) {
				UpdateComplete.Wait();
			}
			LogWhileNotUpdating.Add((strippedText, writeColor, logLevel));
			if (UpdateAutomatically) {
				UpdateLog();
			}
			/*
			if (UpdateAutomatically) {
				WasAtBottom = BoxReference.IsScrolledToBottom();
				BoxReference.AppendText(strippedText, writeColor);

				if (WasAtBottom && !BoxReference.IsScrolledToBottom()) {
					BoxReference.SelectionStart = BoxReference.TextLength;
					BoxReference.ScrollToCaret();
				}

				BoxReference.Update();
			} else {
				if (!UpdateComplete.IsSet) {
					//UpdateComplete.Wait(); // Wait until the latest update is done.
				} else {
					LogWhileNotUpdating.Add((strippedText, writeColor, logLevel));
				}
			}*/

			if (!IsMainThread) {
				// Yes, set the private member here.
				// I don't want it calling UpdateLog if it sets this to true
				_UpdateAutomatically = oldUpdateAutoValue;
			}
		}

		/// <summary>
		/// Writes an exception to the log file for review in bug reports.
		/// </summary>
		/// <param name="error"></param>
		public static void LogException(Exception error) {
			string errMsg = "[ERROR -- " + error.GetType().Name + "] " + error.Message ?? " NO MESSAGE" + "\n\n" + error.StackTrace.Replace("\r", "").Replace("\n", "\n\t");
			byte[] fileWrite = Encoding.ASCII.GetBytes(errMsg);
			LogFileStream.Write(fileWrite, 0, fileWrite.Length);
		}

		/// <summary>
		/// Clears all text from the log. This does not clear the VT console.
		/// </summary>
		public static void Clear() {
			BoxReference.Text = "";
		}

		static XanLogger() {
			if (File.Exists(@".\latest.log")) {
				File.Delete(@".\latest.log");
			}
			LogFileStream = File.OpenWrite(@".\latest.log");
		}
	}

	public static class RichTextBoxUtil {
		//private const int WM_VSCROLL = 0x115;
		//private const int WM_MOUSEWHEEL = 0x20A;
		private const int WM_USER = 0x400;
		private const int SB_VERT = 1;
		//private const int EM_SETSCROLLPOS = WM_USER + 222;
		private const int EM_GETSCROLLPOS = WM_USER + 221;

		[DllImport("user32.dll")]
		private static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

		/// <summary>
		/// Returns <see langword="true"/> if this <see cref="RichTextBox"/> is scrolled to the bottom, and <see langword="false"/> if it is not.
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public static bool IsScrolledToBottom(this RichTextBox box) {
			GetScrollRange(box.Handle, SB_VERT, out int _, out int maxScroll);
			Point rtfPoint = Point.Empty;
			SendMessage(box.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

			return rtfPoint.Y + box.ClientSize.Height >= maxScroll;
		}

		/// <summary>
		/// Appends colored text to the box.
		/// </summary>
		/// <param name="box"></param>
		/// <param name="text"></param>
		/// <param name="color"></param>
		public static void AppendText(this RichTextBox box, string text, Color color) {
			if (color == box.ForeColor) {
				// Skip if it has no actual color change.
				box.AppendText(text);
				return;
			}

			int oldSelStart = box.SelectionStart;
			int oldSelLength = box.SelectionLength;
			box.SelectionStart = box.TextLength;
			box.SelectionLength = 0;

			box.SelectionColor = color;
			box.AppendText(text);
			box.SelectionColor = box.ForeColor;
			box.SelectionStart = oldSelStart;
			box.SelectionLength = oldSelLength;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ThreeRingsSharp.Utility {
	public class XanLogger {

		/// <summary>
		/// Intended for exporting the log to a text file. This <see cref="StringBuilder"/> will contain the entire log.
		/// </summary>
		public static StringBuilder Log { get; } = new StringBuilder();

		/// <summary>
		/// The log while <see cref="UpdateAutomatically"/> is false and it's written to (this is used to append all of the data when <see cref="UpdateLog"/> is called)
		/// </summary>
		private static StringBuilder LogWhileNotUpdating { get; } = new StringBuilder();

		/// <summary>
		/// If <see langword="true"/>, verbose log entries will be posted in the log. Its default value is equal to <see cref="IsDebugMode"/>, but can be set at any time.
		/// </summary>
		public static bool VerboseLogging { get; set; } = IsDebugMode;

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
		/// If <see langword="true"/>, the textbox will be updated to display new text the moment it is written. If <see langword="false"/>, <see cref="UpdateLog"/> can be called, which will copy the contents of <see cref="Log"/> and append it to the textbox.<para/>
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
		/// Manually update the contents of <see cref="BoxReference"/>. Only works if <see cref="UpdateAutomatically"/> is <see langword="false"/>, and of course, if <see cref="BoxReference"/> is not <see langword="null"/>.
		/// </summary>
		public static void UpdateLog() {
			if (BoxReference == null) return;
			if (!UpdateAutomatically) {
				WasAtBottom = BoxReference.IsScrolledToBottom();
				BoxReference.AppendText(LogWhileNotUpdating.ToString());

				if (WasAtBottom && !BoxReference.IsScrolledToBottom()) {
					BoxReference.SelectionStart = BoxReference.TextLength;
					BoxReference.ScrollToCaret();
				}

				LogWhileNotUpdating.Clear();
			}
		}

		/// <summary>
		/// Append a new line to the log.
		/// </summary>
		/// <param name="isVerbose">If true, this is treated as a verbose log entry, which will not be appended to the log if <see cref="VerboseLogging"/> is false.</param>
		public static void WriteLine(bool isVerbose = false) => Write("\n", isVerbose);

		/// <summary>
		/// Append the given text to the log and advance by one line.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="isVerbose">If true, this is treated as a verbose log entry, which will not be appended to the log if <see cref="VerboseLogging"/> is false.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <see langword="null"/>.</exception>
		public static void WriteLine(object obj, bool isVerbose = false) {
			Write((obj?.ToString() ?? "null") + "\n", isVerbose);
		}

		/// <summary>
		/// Append the given text to the log.
		/// </summary>
		/// <param name="obj">The text to write to the log.</param>
		/// <param name="isVerbose">If true, this is treated as a verbose log entry, which will not be appended to the log if <see cref="VerboseLogging"/> is false.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <see langword="null"/>.</exception>
		public static void Write(object obj, bool isVerbose = false) {
			if (obj == null) throw new ArgumentNullException("obj");
			if (!VerboseLogging && isVerbose) return;

			string text = obj?.ToString() ?? "null";
			Log.Append(text);
			if (BoxReference == null) return;

			if (UpdateAutomatically) {
				WasAtBottom = BoxReference.IsScrolledToBottom();
				BoxReference.AppendText(text);

				if (WasAtBottom && !BoxReference.IsScrolledToBottom()) {
					BoxReference.SelectionStart = BoxReference.TextLength;
					BoxReference.ScrollToCaret();
				}
			} else {
				LogWhileNotUpdating.Append(text);
			}
		}

		/// <summary>
		/// Clears all text from the log. This also wipes <see cref="Log"/>.
		/// </summary>
		public static void Clear() {
			Log.Clear();
			BoxReference.Text = "";
		}
	}

	public static class RTBScrollUtil {
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
	}
}

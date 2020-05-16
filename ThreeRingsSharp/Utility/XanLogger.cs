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
		/// A reference to a textbox that should store the contents of the log in a GUI application.<para/>
		/// This should be an instance of <see cref="RTFScrolledBottom"/> for proper function. It can be <see langword="null"/> if there is no GUI.
		/// </summary>
		public static RTFScrolledBottom BoxReference { get; set; }

		/// <summary>
		/// If true, the box *was* at the bottom before text was appended to it (meaning it should autoscroll)
		/// </summary>
		private static bool WasAtBottom { get; set; }

		/// <summary>
		/// Append a new line to the log.
		/// </summary>
		public static void WriteLine() => Write("\n");

		/// <summary>
		/// Append the given text to the log and advance by one line.
		/// </summary>
		/// <param name="text"></param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is <see langword="null"/>.</exception>
		public static void WriteLine(string text) {
			if (text == null) text = "null";
			Write(text + "\n");
		}

		/// <summary>
		/// Append the given text to the log.
		/// </summary>
		/// <param name="text">The text to write to the log.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is <see langword="null"/>.</exception>
		public static void Write(string text) {
			if (text == null) throw new ArgumentNullException("text");
			Log.Append(text);
			if (BoxReference == null) return;

			WasAtBottom = BoxReference.IsScrolledToBottom;
			BoxReference.AppendText(text);
			
			if (WasAtBottom && !BoxReference.IsScrolledToBottom) {
				BoxReference.SelectionStart = BoxReference.TextLength;
				BoxReference.ScrollToCaret();
			}
		}

		/// <summary>
		/// Clears all text from the log. This also wipes <see cref="Log"/>.
		/// </summary>
		public static void Clear() {
			Log.Clear();
		}
	}


	/// <summary>
	/// A variant of <see cref="RichTextBox"/> that provides a means of testing if it is scrolled to the bottom.
	/// </summary>
	public class RTFScrolledBottom : RichTextBox {
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
		/// True if the component is scrolled as far down as possible.
		/// </summary>
		public bool IsScrolledToBottom => IsAtMaxScroll();

		private bool IsAtMaxScroll() {
			GetScrollRange(this.Handle, SB_VERT, out int _, out int maxScroll);
			Point rtfPoint = Point.Empty;
			SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

			return rtfPoint.Y + this.ClientSize.Height >= maxScroll;
		}
	}
}

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
		/// A reference to the text box storing the log.
		/// </summary>
		public static RTFScrolledBottom BoxReference { get; set; }

		private static bool WasAtBottom { get; set; }

		/// <summary>
		/// Append a new line to the log.
		/// </summary>
		public static void WriteLine() => Write("\n");

		/// <summary>
		/// Append the given text to the log and advance by one line.
		/// </summary>
		/// <param name="text"></param>
		public static void WriteLine(string text) {
			if (text == null) text = "null";
			Write(text + "\n");
		}

		/// <summary>
		/// Append the given text to the log.
		/// </summary>
		/// <param name="text"></param>
		/// <exception cref="NullReferenceException">If the RichTextBox reference has not been set.</exception>
		public static void Write(string text) {
			if (BoxReference == null) throw new NullReferenceException("A reference to the RichTextBox was not set!");

			WasAtBottom = BoxReference.IsScrolledToBottom;
			BoxReference.AppendText(text);
			if (WasAtBottom && !BoxReference.IsScrolledToBottom) {
				BoxReference.SelectionStart = BoxReference.TextLength;
				BoxReference.ScrollToCaret();
			}
		}
	}

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
			int minScroll;
			int maxScroll;
			GetScrollRange(this.Handle, SB_VERT, out minScroll, out maxScroll);
			Point rtfPoint = Point.Empty;
			SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref rtfPoint);

			return rtfPoint.Y + this.ClientSize.Height >= maxScroll;
		}
	}
}

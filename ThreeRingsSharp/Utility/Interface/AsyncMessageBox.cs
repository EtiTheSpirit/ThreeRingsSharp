using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThreeRingsSharp.Utility.Interface {

	/// <summary>
	/// Provides a method to asynchronously display a message box.
	/// </summary>
	public class AsyncMessageBox {

		/// <summary>
		/// Asynchronously show a message box (show a message box without causing the GUI to stop updating, allowing stuff to work in the background).<para/>
		/// This also allows the user to interact with the GUI before closing the message box, as a side effect.
		/// </summary>
		/// <param name="text">The text to display in the body of the message box.</param>
		/// <param name="title">The title displayed at the top of the message box.</param>
		/// <param name="buttons">The buttons used in the message box (e.g. OK)</param>
		/// <param name="icon">The icon to display on the left side of the message box.</param>
		/// <returns></returns>
		public static Task<DialogResult> Show(string text, string title = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None) {
			Task<DialogResult> display = Task.Run(() => {
				return MessageBox.Show(text, title, buttons, icon);
			});
			display.ConfigureAwait(false);
			return display;
		}

	}
}

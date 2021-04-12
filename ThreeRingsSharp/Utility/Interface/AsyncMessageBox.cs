using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThreeRingsSharp.Utility.Interface {

	/// <summary>
	/// Provides a method to asynchronously display a message box. Both methods will do nothing if this class's primary parameter, <see cref="IsInGUIContext"/>, is <see langword="false"/>.
	/// </summary>
	public class AsyncMessageBox {

		/// <summary>
		/// If <see langword="false"/>, <see cref="ShowAsync(string, string, MessageBoxButtons, MessageBoxIcon)"/> will not do anything.<para/>
		/// This should be set to <see langword="true"/> in contexts where there is a GUI accompanied with the program.
		/// </summary>
		public static bool IsInGUIContext { get; set; } = false;

		/// <summary>
		/// Asynchronously show a message box (show a message box without causing the GUI to stop updating, allowing stuff to work in the background).<para/>
		/// This also allows the user to interact with the GUI before closing the message box, as a side effect.<para/>
		/// This task will return <see langword="null"/> if <see cref="IsInGUIContext"/> is <see langword="false"/>!
		/// </summary>
		/// <param name="text">The text to display in the body of the message box.</param>
		/// <param name="title">The title displayed at the top of the message box.</param>
		/// <param name="buttons">The buttons used in the message box (e.g. OK)</param>
		/// <param name="icon">The icon to display on the left side of the message box.</param>
		/// <returns></returns>
		public static Task<DialogResult?> ShowAsync(string text, string title = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None) {
			if (!IsInGUIContext) return Task.FromResult<DialogResult?>(null);

			Task<DialogResult?> display = Task.Run<DialogResult?>(() => {
				return MessageBox.Show(text, title, buttons, icon);
			});
			display.ConfigureAwait(false);
			return display;
		}

		/// <summary>
		/// Synchronously show a message box. This is virtually identical to calling <see cref="MessageBox.Show(string, string, MessageBoxButtons, MessageBoxIcon)"/>, with the exception that it will return <see langword="null"/> if <see cref="IsInGUIContext"/> is <see langword="false"/>.<para/>
		/// Consider using <see cref="ShowAsync(string, string, MessageBoxButtons, MessageBoxIcon)"/> if you wish to allow the program to continue functioning and accept inputs while the dialog is open.
		/// </summary>
		/// <param name="text">The text to display in the body of the message box.</param>
		/// <param name="title">The title displayed at the top of the message box.</param>
		/// <param name="buttons">The buttons used in the message box (e.g. OK)</param>
		/// <param name="icon">The icon to display on the left side of the message box.</param>
		/// <returns></returns>
		public static DialogResult? Show(string text, string title = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None) {
			if (!IsInGUIContext) return null;
			return MessageBox.Show(text, title, buttons, icon);
		}

	}
}

using System;
using System.Windows.Forms;

namespace ThreeRingsSharp.XansData.Exceptions {

	/// <summary>
	/// An exception that is thrown when a critical read error occurs when trying to process files.
	/// </summary>
	public class ClydeDataReadException : Exception {

		/// <summary>
		/// The title that should be displayed in the <see cref="MessageBox"/> if this is sent to a GUI.
		/// </summary>
		public string ErrorWindowTitle { get; }

		/// <summary>
		/// The icon that should be displayed in the <see cref="MessageBox"/> if this is sent to a GUI.
		/// </summary>
		public MessageBoxIcon ErrorWindowIcon { get; }

		public ClydeDataReadException() : base() { }

		/// <summary>
		/// Construct a new <see cref="ClydeDataReadException"/> with the optional given title and icon, intended for use in GUI displays of this error.
		/// </summary>
		/// <param name="message">The message to display for this error.</param>
		/// <param name="title">The title to display for this error in a GUI.</param>
		/// <param name="icon">The icon to use in the GUI.</param>
		public ClydeDataReadException(string message, string title = "Oh no!", MessageBoxIcon icon = MessageBoxIcon.Error) : base(message) {
			ErrorWindowTitle = title;
			ErrorWindowIcon = icon;
		}
	}
}

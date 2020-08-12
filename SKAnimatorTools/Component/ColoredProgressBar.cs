using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreeRingsSharp;

namespace SKAnimatorTools.Component {

	/// <summary>
	/// A <see cref="ProgressBar"/> that supports custom colors, and unlike many online solutions, does not flicker.<para/>
	/// If you're coming here from reading TRS source code, by all means you are free to use this.
	/// Just note that the <see cref="ProgressBarState"/> enum has one-indexed values (1 = ok, 2 = error, 3 = whatever yellow is). This is because of the system colors.
	/// </summary>
	public class ColoredProgressBar : ProgressBar {

		/// <summary>
		/// In pixels, this is how much the actual colored bar is inset from its border.
		/// </summary>
		private const int BAR_INSET = 2;

		/// <summary>
		/// If <see langword="true"/>, this is no different than a standard <see cref="ProgressBar"/>.
		/// </summary>
		[
			Category("Bar Display"),
			Description("If true, all custom features of this object are ignored, causing this to behave identically to a stock ProgressBar.")
		]
		public bool UseSystemBar {
			get => _UseSystemBar;
			set {
				_UseSystemBar = value;
				if (Application.RenderWithVisualStyles) SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, !value);
			}
		}
		private bool _UseSystemBar = false;

		#region Colors
		/// <summary>
		/// The color to use when everything is going smoothly. This will only apply if <see cref="UseSystemBar"/> is false.
		/// </summary>
		[
			Category("Bar Display"),
			Description("The color to use when things are going as expected. This will not be applied if UseSystemBar is true.")
		]
		public Color OKColor { get; set; } = Color.FromArgb(31, 192, 31);

		/// <summary>
		/// The color to use if there's extra work to be done. This will only apply if <see cref="UseSystemBar"/> is false.
		/// </summary>
		[
			Category("Bar Display"),
			Description("The color to use when extra or unexpected work needs to be performed. This will not be applied if UseSystemBar is true.")
		]
		public Color ExtraWorkColor { get; set; } = Color.FromArgb(220, 180, 0);

		/// <summary>
		/// The color to use if something broke. This will only apply if <see cref="UseSystemBar"/> is false.
		/// </summary>
		[
			Category("Bar Display"),
			Description("The color to use when something has critically failed. This will not be applied if UseSystemBar is true.")
		]
		public Color ErrorColor { get; set; } = Color.FromArgb(220, 0, 0);

		[
			Category("Appearance"),
			Description("The foreground color of this component, which is used to determine the active color of the progress bar. This will not be applied if UseSystemBar is true.")
		]
		#endregion

		#region Overrides for cohesion
		public override Color ForeColor {
			get => base.ForeColor;
			set => base.ForeColor = value;
		}

		[
			Category("Appearance"),
			Description("The background color of this component, which is used to determine the color of the progress bar's background. This will not be applied if UseSystemBar is true.")
		]
		public override Color BackColor {
			get => base.BackColor;
			set => base.BackColor = value;
		}
		#endregion

		public ColoredProgressBar() {
			if (Application.RenderWithVisualStyles && !UseSystemBar) SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
		}

		protected override void OnPaint(PaintEventArgs e) {
			if (!Application.RenderWithVisualStyles || UseSystemBar) {
				base.OnPaint(e);
				return;
			}

			using (Image offscreenImage = new Bitmap(Width, Height)) {
				using (Graphics offscreen = Graphics.FromImage(offscreenImage)) {
					Rectangle rect = new Rectangle(0, 0, Width, Height);

					SolidBrush foreBrush = new SolidBrush(ForeColor);
					SolidBrush backBrush = new SolidBrush(BackColor);
					
					// Progress Bar Backing
					SolidBrush lineBrush = new SolidBrush(Color.FromArgb(192, 192, 192));
					Pen frame = new Pen(lineBrush) {
						Width = 2
					};
					e.Graphics.DrawRectangle(frame, rect);
					rect.Inflate(new Size(-1, -1));
					e.Graphics.FillRectangle(backBrush, rect);

					// Actual Bar
					rect.Inflate(new Size(-(BAR_INSET - 1), -(BAR_INSET - 1))); // Deflate inner rect.
					rect.Width = (int)(rect.Width * ((double)Value / Maximum));
					if (rect.Width == 0) {
						e.Graphics.DrawImage(offscreenImage, 0, 0);
						return;
					}

					offscreen.FillRectangle(foreBrush, BAR_INSET, BAR_INSET, rect.Width, rect.Height);
					e.Graphics.DrawImage(offscreenImage, 0, 0);
				}
			}
		}

		/// <summary>
		/// Sets the color of the bar based on the state.<para/>
		/// If this has <see cref="UseSystemBar"/> set to <see langword="true"/>, it will use the default windows green/yellow/red scheme.<para/>
		/// If this has <see cref="UseSystemBar"/> set to <see langword="false"/>, it will use the custom colors as defined by <see cref="OKColor"/>, <see cref="ExtraWorkColor"/>, and <see cref="ErrorColor"/>.
		/// </summary>
		/// <param name="state"></param>
		public void SetColorFromState(ProgressBarState state) {
			if (state == ProgressBarState.OK) {
				ForeColor = OKColor;
			} else if (state == ProgressBarState.ExtraWork) {
				ForeColor = ExtraWorkColor;
			} else if (state == ProgressBarState.Error) {
				ForeColor = ErrorColor;
			}
			this.SetState((int)state);
		}
	}

	// https://stackoverflow.com/questions/778678/how-to-change-the-color-of-progressbar-in-c-sharp-net-3-5
	public static class ModifyProgressBarColor {
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
		public static void SetState(this ProgressBar pBar, int state) {
			SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
		}
	}
}

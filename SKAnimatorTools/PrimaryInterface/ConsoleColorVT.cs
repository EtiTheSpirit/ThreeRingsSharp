using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using ThreeRingsSharp.XansData.Extensions;

namespace SKAnimatorTools.PrimaryInterface {

	/// <summary>
	/// A console color that uses VT sequences to employ custom RGB colors.
	/// </summary>
	public readonly struct ConsoleColorVT {

		#region Preset Data

		public const string COLOR_CODE_REGEX = @"\^#([0-9]|[a-f]|[A-F]){6};";

		#endregion

		#region Fields

		/// <summary>
		/// The R component of this console color.
		/// </summary>
		public readonly byte R;

		/// <summary>
		/// The G component of this console color.
		/// </summary>
		public readonly byte G;

		/// <summary>
		/// The B component of this console color.
		/// </summary>
		public readonly byte B;

		/// <summary>
		/// The <see cref="ConsoleColor"/> that is most similar to this <see cref="ConsoleColorVT"/> via treating the colors like coordinates in 3D space then testing distance.
		/// </summary>
		public readonly ConsoleColor NearestConsoleColor;

		#endregion

		#region Constructors & Helpers

		/// <summary>
		/// Construct a new <see cref="ConsoleColorVT"/> from the specified color value.
		/// </summary>
		/// <param name="r">The R component (0 - 255)</param>
		/// <param name="g">The G component (0 - 255)</param>
		/// <param name="b">The B component (0 - 255)</param>
		public ConsoleColorVT(byte r, byte g, byte b) {
			R = r;
			G = g;
			B = b;
			NearestConsoleColor = GetNearestConsoleColor_Ctor(r, g, b);
		}

		/// <summary>
		/// Returns a <see cref="ConsoleColorVT"/> created by the default color for the input <see cref="ConsoleColor"/>. This does NOT reflect changes made to the console's palette, and instead returns the MS-DOS default for the specific color.
		/// </summary>
		/// <param name="stockColor"></param>
		public static ConsoleColorVT FromConsoleColor(ConsoleColor stockColor) {
			return VTValues.Colors[stockColor];
		}

		/// <summary>
		/// Given a <see cref="Color"/>, it will be converted into an instance of <see cref="ConsoleColorVT"/>.
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static ConsoleColorVT FromColor(Color color) {
			return new ConsoleColorVT(color.R, color.G, color.B);
		}

		/// <summary>
		/// Translates a formatted piece of text into a <see cref="ConsoleColorVT"/>. The text should be formatted like: ^#FFFFFF; (where FFFFFF is a hex color code.)
		/// </summary>
		/// <param name="formatted">The formatted text.</param>
		/// <returns>A <see cref="ConsoleColorVT"/> using the specified hex code.</returns>
		/// <exception cref="ArgumentException"/>
		public static ConsoleColorVT FromFormattedString(string formatted) {
			if (formatted.Length != 9 || !Regex.IsMatch(formatted, COLOR_CODE_REGEX)) {
				// Length 9 enforces that it's *just* the code.
				throw new ArgumentException("Expected text formatted like ^#......; (where ...... translates into a hex color code).");
			}

			// Now treat it properly
			string hexClip = formatted.Substring(2, 6);
			int value = int.Parse(hexClip, System.Globalization.NumberStyles.HexNumber);
			byte r = (byte)((value & 0xFF0000) >> 16);
			byte g = (byte)((value & 0x00FF00) >> 8);
			byte b = (byte)(value & 0x0000FF);

			return new ConsoleColorVT(r, g, b);
		}

		#endregion

		#region Application Methods

		/// <summary>
		/// Applies this <see cref="ConsoleColorVT"/> to the foreground.
		/// </summary>
		public void ApplyToForeground() {
			Console.Write(ToString());
		}

		/// <summary>
		/// Applies this <see cref="ConsoleColorVT"/> to the background;
		/// </summary>
		public void ApplyToBackground() {
			Console.Write(ToStringBG());
		}

		#endregion

		#region ConsoleColor Conversions

		/// <summary>
		/// Attempts to return this <see cref="ConsoleColorVT"/> as a <see cref="ConsoleColor"/> based on its color.<para/>
		/// If this <see cref="ConsoleColorVT"/> does not match up with a <see cref="ConsoleColor"/>, this will return null.<para/>
		/// Consider referencing <see cref="NearestConsoleColor"/> if a non-null <see cref="ConsoleColor"/> is desired.
		/// </summary>
		/// <returns></returns>
		public ConsoleColor? AsConsoleColor() {
			if (VTValues.Colors.Values.Contains(this)) return VTValues.Colors.KeyOf(this);
			return null;
		}

		/// <summary>
		/// Returns the <see cref="ConsoleColor"/> whose color is most similar to this <see cref="ConsoleColorVT"/>. Similarity is tested via distance in 3D space, where RGB is mapped to XYZ.
		/// </summary>
		/// <returns></returns>
		private static ConsoleColor GetNearestConsoleColor_Ctor(byte r, byte g, byte b) {
			ConsoleColor minObj = ConsoleColor.White;
			double minDist = double.MaxValue;
			foreach ((byte, byte, byte) color in VTValues.ColorsAsValueTuple.Values) {
				double distance = GetColorDistance((r, g, b), color);
				if (distance < minDist) {
					minObj = VTValues.ColorsAsValueTuple.KeyOf(color);
					minDist = distance;
				}
			}
			return minObj;
		}

		#endregion

		#region ToStrings

		/// <summary>
		/// Returns the formatted code so that it can be applied to the foreground by writing the result of this function to the console.<para/>
		/// This observes <see cref="VTConsole.IsVTEnabled"/> to determine whether to return this as a full RGB VT Sequence, or as the nearest <see cref="ConsoleColor"/> that is most similar to this <see cref="ConsoleColorVT"/>'s color.<para/>
		/// Call <see cref="ToStringBG"/> to get the background format.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return ToString(VTConsole.IsVTEnabled);
		}


		/// <summary>
		/// Returns the formatted code so that it can be applied to the background by writing the result of this function to the console.<para/>
		/// This observes <see cref="VTConsole.IsVTEnabled"/> to determine whether to return this as a full RGB VT Sequence, or as the nearest <see cref="ConsoleColor"/> that is most similar to this <see cref="ConsoleColorVT"/>'s color.<para/>
		/// Call <see cref="ToString()"/> to get the foreground format.
		/// </summary>
		/// <returns></returns>
		public string ToStringBG() {
			return ToString(VTConsole.IsVTEnabled, true);
		}


		/// <summary>
		/// Returns the formatted code so that it can be applied to the foreground or background. The format is determined by <paramref name="asVT"/>
		/// </summary>
		/// <param name="asVT">If false, this will return the result of <see cref="ToStringNonVT"/> with its requireExactConsoleColor parameter set to false. Otherwise, this will return the result of <see cref="ToString()"/></param>
		/// <param name="background">If true, this string will use a VT sequence (if applicable) that applies to the background instead of the foreground.</param>
		/// <exception cref="NotSupportedException"/>
		public string ToString(bool asVT, bool background = false) {
			return asVT ? ToStringVT(background) : ToStringNonVT();
		}

		/// <summary>
		/// A tostring method that uses legacy coloring when translating this ConsoleColorVT to a string.<para/>
		/// If <paramref name="requireExactConsoleColor"/> is <see langword="true"/>, this will throw a <see cref="NotSupportedException"/> if this <see cref="ConsoleColorVT"/> is not using a color equal to one of the default <see cref="ConsoleColor"/>s<para/>
		/// If <paramref name="requireExactConsoleColor"/> is <see langword="false"/>, this will use <see cref="NearestConsoleColor"/> to apply.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"/>
		public string ToStringNonVT(bool requireExactConsoleColor = false) {
			if (VTValues.Colors.Values.Contains(this)) {
				return "§" + VTConsole.ConsoleColorMap.KeyOf(AsConsoleColor()!.Value).ToString("X");
			}
			if (!requireExactConsoleColor) {
				// We can try again.
				return "§" + VTConsole.ConsoleColorMap.KeyOf(NearestConsoleColor).ToString("X");
			}
			throw new NotSupportedException("This ConsoleColorVT does not have a color identical to a stock ConsoleColor.");
		}

		/// <summary>
		/// Converts this to a VT string.
		/// </summary>
		/// <param name="background">If true, the sequence will change the background color. If false, it will change the foreground color.</param>
		/// <returns></returns>
		public string ToStringVT(bool background = false) {
			if (background)
				return string.Format("\u001b[48;2;{0};{1};{2}m", R, G, B);

			return string.Format("\u001b[38;2;{0};{1};{2}m", R, G, B);
		}

		#endregion

		#region Casts & Comparisons

		public static implicit operator ConsoleColorVT(ConsoleColor src) {
			return FromConsoleColor(src);
		}

		public static explicit operator ConsoleColor(ConsoleColorVT src) {
			return src.NearestConsoleColor;
		}

		public static bool operator ==(ConsoleColorVT alpha, ConsoleColorVT bravo) {
			return alpha.Equals(bravo);
		}

		public static bool operator !=(ConsoleColorVT alpha, ConsoleColorVT bravo) {
			return !alpha.Equals(bravo);
		}

		public override bool Equals(object? obj) {
			if (obj is null) return false;
			if (obj is ConsoleColorVT other) {
				return (R == other.R) && (G == other.G) && (B == other.B);
			}
			return false;
		}

		public override int GetHashCode() {
			return HashCode.Combine(R, G, B);
		}

		/// <summary>
		/// Returns the distance between the two colors as if they were coordinates in 3D space.
		/// </summary>
		/// <param name="alpha"></param>
		/// <param name="bravo"></param>
		/// <returns></returns>
		private static double GetColorDistance(ConsoleColorVT alpha, ConsoleColorVT bravo) {
			double r = alpha.R - bravo.R;
			double g = alpha.G - bravo.G;
			double b = alpha.B - bravo.B;

			return Math.Sqrt(Math.Pow(r, 2) + Math.Pow(g, 2) + Math.Pow(b, 2));
		}

		/// <summary>
		/// Returns the distance between the two colors as if they were coordinates in 3D space.<para/>
		/// This variant is intended to be used in the constructor.
		/// </summary>
		/// <param name="alpha"></param>
		/// <param name="bravo"></param>
		/// <returns></returns>
		private static double GetColorDistance((byte, byte, byte) alpha, (byte, byte, byte) bravo) {
			double r = alpha.Item1 - bravo.Item1;
			double g = alpha.Item2 - bravo.Item2;
			double b = alpha.Item3 - bravo.Item3;

			return Math.Sqrt(Math.Pow(r, 2) + Math.Pow(g, 2) + Math.Pow(b, 2));
		}

		#endregion

	}

	public static class VTValues {

		public static IReadOnlyDictionary<ConsoleColor, (byte, byte, byte)> ColorsAsValueTuple = new Dictionary<ConsoleColor, (byte, byte, byte)> {
			[ConsoleColor.Black] = (0, 0, 0),
			[ConsoleColor.DarkBlue] = (0, 0, 128),
			[ConsoleColor.DarkGreen] = (0, 128, 0),
			[ConsoleColor.DarkCyan] = (0, 128, 128),
			[ConsoleColor.DarkRed] = (128, 0, 0),
			[ConsoleColor.DarkMagenta] = (128, 0, 128),
			[ConsoleColor.DarkYellow] = (128, 128, 0),
			[ConsoleColor.DarkGray] = (128, 128, 128),
			[ConsoleColor.Gray] = (192, 192, 192),
			[ConsoleColor.Blue] = (0, 0, 255),
			[ConsoleColor.Green] = (0, 255, 0),
			[ConsoleColor.Cyan] = (0, 255, 255),
			[ConsoleColor.Red] = (255, 0, 0),
			[ConsoleColor.Magenta] = (255, 0, 255),
			[ConsoleColor.Yellow] = (255, 255, 0),
			[ConsoleColor.White] = (255, 255, 255)
		};

		public static IReadOnlyDictionary<ConsoleColor, ConsoleColorVT> Colors {
			get {
				// Need to do a late instantiation or else it dies.
				if (_Colors == null) {
					_Colors = new Dictionary<ConsoleColor, ConsoleColorVT> {
						[ConsoleColor.Black] = new ConsoleColorVT(0, 0, 0),
						[ConsoleColor.DarkBlue] = new ConsoleColorVT(0, 0, 128),
						[ConsoleColor.DarkGreen] = new ConsoleColorVT(0, 128, 0),
						[ConsoleColor.DarkCyan] = new ConsoleColorVT(0, 128, 128),
						[ConsoleColor.DarkRed] = new ConsoleColorVT(128, 0, 0),
						[ConsoleColor.DarkMagenta] = new ConsoleColorVT(128, 0, 128),
						[ConsoleColor.DarkYellow] = new ConsoleColorVT(128, 128, 0),
						[ConsoleColor.DarkGray] = new ConsoleColorVT(128, 128, 128),
						[ConsoleColor.Gray] = new ConsoleColorVT(192, 192, 192),
						[ConsoleColor.Blue] = new ConsoleColorVT(0, 0, 255),
						[ConsoleColor.Green] = new ConsoleColorVT(0, 255, 0),
						[ConsoleColor.Cyan] = new ConsoleColorVT(0, 255, 255),
						[ConsoleColor.Red] = new ConsoleColorVT(255, 0, 0),
						[ConsoleColor.Magenta] = new ConsoleColorVT(255, 0, 255),
						[ConsoleColor.Yellow] = new ConsoleColorVT(255, 255, 0),
						[ConsoleColor.White] = new ConsoleColorVT(255, 255, 255)
					};
				}
				return _Colors;
			}
		}

		private static Dictionary<ConsoleColor, ConsoleColorVT>? _Colors = null;


	}
}

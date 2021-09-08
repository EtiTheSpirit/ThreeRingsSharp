using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SKAnimatorTools.PrimaryInterface {

	/// <summary>
	/// A console logging utility. Offers the ability to format messages with color codes.
	/// </summary>
	public static class VTConsole {

		/// <summary>
		/// The symbol recognized in messages for color codes. This is identical to Minecraft's color code system.<para/>
		/// See <a href="https://minecraft.gamepedia.com/Formatting_codes#Color_codes">https://minecraft.gamepedia.com/Formatting_codes#Color_codes</a> for more information.
		/// </summary>
		public const char COLOR_CODE_SYM = '§';

		/// <summary>
		/// A map of byte code values to ConsoleColors
		/// </summary>
		public static readonly IReadOnlyDictionary<byte, ConsoleColor> ConsoleColorMap = new Dictionary<byte, ConsoleColor> {
			[0] = ConsoleColor.Black,
			[1] = ConsoleColor.DarkBlue,
			[2] = ConsoleColor.DarkGreen,
			[3] = ConsoleColor.DarkCyan,
			[4] = ConsoleColor.DarkRed,
			[5] = ConsoleColor.DarkMagenta,
			[6] = ConsoleColor.DarkYellow,
			[7] = ConsoleColor.DarkGray,
			[8] = ConsoleColor.Gray,
			[9] = ConsoleColor.Blue,
			[10] = ConsoleColor.Green,
			[11] = ConsoleColor.Cyan,
			[12] = ConsoleColor.Red,
			[13] = ConsoleColor.Magenta,
			[14] = ConsoleColor.Yellow,
			[15] = ConsoleColor.White
		};

		#region Preset Data

		/// <summary>
		/// The default color used by the console for messages printed with no color. Default value is <see cref="ConsoleColor.White"/><para/>
		/// This can be set to a <see cref="ConsoleColorVT"/> or a <see cref="ConsoleColor"/>. Attempting to set it to a <see cref="ConsoleColorVT"/> when <see cref="IsVTEnabled"/> is false will throw a <see cref="NotSupportedException"/>.
		/// </summary>
		/// <exception cref="NotSupportedException"/>
		public static ConsoleColorVT DefaultColor {
			get => DefaultColorInternal;
			set {
				if (value.GetType() != typeof(ConsoleColor)) {
					if (!IsVTEnabled) throw new NotSupportedException("Cannot set DefaultColor to ConsoleColorVT -- VT is not enabled. Did you remember to call EnableVTSupport()?");
				}
				DefaultColorInternal = value;
			}
		}
		private static ConsoleColorVT DefaultColorInternal = ConsoleColor.White;

		#endregion

		#region Foreground & Background

		private static ConsoleColorVT FGColorInternal = ConsoleColor.White;
		private static ConsoleColorVT BGColorInternal = ConsoleColor.Black;

		/// <summary>
		/// The foreground color of this console.<para/>
		/// Setting this to null if VT is enabled will default to white.<para/>
		/// Attempting to set this value to a <see cref="ConsoleColorVT"/> if <see cref="IsVTEnabled"/> is false will round the color to the nearest <see cref="ConsoleColor"/>
		/// </summary>
		/// <exception cref="NotSupportedException"/>
		public static ConsoleColorVT ForegroundColor {
			get => FGColorInternal;
			set {
				if (!IsVTEnabled) {
					ConsoleColor asStock = value.NearestConsoleColor;
					Console.ForegroundColor = asStock;
					value = asStock;
				}
				FGColorInternal = value;
				value.ApplyToForeground();
			}
		}

		/// <summary>
		/// The background color of this console.<para/>
		/// Setting this to null if VT is enabled will default to black.<para/>
		/// Attempting to set this value to a <see cref="ConsoleColorVT"/> if <see cref="IsVTEnabled"/> is false will round the color to the nearest <see cref="ConsoleColor"/>
		/// </summary>
		/// <exception cref="NotSupportedException"/>
		public static ConsoleColorVT BackgroundColor {
			get => BGColorInternal;
			set {
				if (!IsVTEnabled) {
					ConsoleColor asStock = value.NearestConsoleColor;
					Console.ForegroundColor = asStock;
					value = asStock;
				}
				BGColorInternal = value;
				value.ApplyToBackground();
			}
		}

		#endregion

		#region Kernel32 API Imports
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

		#endregion

		#region VT Enabling

		/// <summary>
		/// Whether or not VT Sequences are enabled and should be used.
		/// </summary>
		public static bool IsVTEnabled { get; private set; } = false;

		/// <summary>
		/// When called, this attempts to enable VT Sequence support for the console. Whether or not this action will be successful depends on the platform this code is running on.<para/>
		/// This method will return <see langword="true"/> if VT sequences are supported and were enabled, and <see langword="false"/> if they are not supported on this machine.<para/>
		/// VT sequences allow low level control of the console's colors, including the allowance of full 16-million color RGB text and backgrounds, bold/underline formatting, and more.<para/>
		/// See <a href="https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences">https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences</a> for more information.
		/// </summary>
		/// <returns>True if VT sequences are supported and were successfully enabled, false if they are not.</returns>
		public static bool EnableVTSupport() {
			if (IsVTEnabled) return true;

			IntPtr hOut = GetStdHandle(-11);
			if (hOut != INVALID_HANDLE_VALUE) {
				if (GetConsoleMode(hOut, out uint mode)) {
					mode |= 0x4;
					if (SetConsoleMode(hOut, mode)) {
						IsVTEnabled = true;
						return true;
					}
				}
			}
			return false;
		}

		private static bool HasWarnedForLackOfVT = false;

		#endregion

		#region Utilities



		/// <summary>
		/// A proxy to Write that does some extra sanity checks.
		/// </summary>
		private static void WriteProxy(string message) {
			if (Regex.IsMatch(message, @"(\x1b)(\[[^m]+)m") && !IsVTEnabled) {
				if (!HasWarnedForLackOfVT) {
					Console.Beep();
					Console.WriteLine("\nWARNING: VT Sequence detected, but VT Sequences are not enabled! This warning will only show once. All VT sequences will be removed.\n");
					HasWarnedForLackOfVT = true;
				}
				Regex.Replace(message, @"(\x1b)(\[[^m]+)m", "");
			}
			Console.Write(message);
		}

		/// <summary>
		/// Returns a formatted timestamp: "[HH:MM:SS] "
		/// </summary>
		/// <returns>Returns a formatted timestamp: "[HH:MM:SS] "</returns>
		public static string GetFormattedTimestamp() {
			TimeSpan currentTime = DateTime.Now.TimeOfDay;
			return "[" + currentTime.Hours.ToString("D2") + ":" + currentTime.Minutes.ToString("D2") + ":" + currentTime.Seconds.ToString("D2") + "] ";
		}

		/// <summary>
		/// Returns <see langword="true"/> if the message contains the color code symbol and, by extension, a color code.
		/// </summary>
		public static bool MessageHasColors(string message) {
			if (IsVTEnabled) {
				return Regex.IsMatch(message, @"(\x1b)(\[[^m]+)m") || message.Contains(COLOR_CODE_SYM);
			}
			return message.Contains(COLOR_CODE_SYM);
		}

		/// <summary>
		/// Strips away all color formatting stuff from a message so that it's just plain text.
		/// </summary>
		public static string StripColorFormattingCode(string message) {
			string[] colorSegs = message.Split(COLOR_CODE_SYM);
			//colorSegs[0] will be empty if we have a color code at the start.
			string res = "";
			foreach (string coloredString in colorSegs) {
				if (coloredString.Length >= 1) {
					if (coloredString == colorSegs.First()) {
						if (message.Substring(0, 1) == COLOR_CODE_SYM.ToString()) {
							res += coloredString.Substring(1);
						} else {
							res += coloredString;
						}
					} else {
						res += coloredString.Substring(1);
					}
				} else {
					res += coloredString;
				}
			}
			return res;
		}

		/// <summary>
		/// Strips away all VT color formatting text.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string StripVTColorFormattingCode(string message) {
			// tfw the §# version is a huge block of text but this is just like "nah"
			string withoutx1bs = Regex.Replace(message, @"(\x1b)(\[[^m]+)m", "");
			return Regex.Replace(withoutx1bs, @"((\^#)([0-9]|[a-f]|[A-F]){6};)", "");
		}

		/// <summary>
		/// Calls both formatting cleanup methods to strip color codes.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string ClearColorFormattingCodeAll(string message) {
			message = StripVTColorFormattingCode(message);
			return StripColorFormattingCode(message);
		}

		#endregion

		#region Internal Writing Methods

		/// <summary>
		/// Writes a color coded console message in a similar manner to text in Minecraft, where colors are selected with § (e.g. §a will make all following text green)
		/// </summary>
		/// <param name="message">The message to write.</param>
		private static void WriteMessageFromColors(string message) {
			string[] colorSegs = message.Split(COLOR_CODE_SYM);
			// colorSegs[0] will be empty if we have a color code at the start.
			ConsoleColor defColor = Console.ForegroundColor;
			foreach (string coloredString in colorSegs) {
				if (coloredString.Length > 1) {
					byte code = 255;
					try { code = Convert.ToByte(coloredString.First().ToString(), 16); } catch (Exception) { }
					bool success = ConsoleColorMap.TryGetValue(code, out ConsoleColor color);
					if (success) {
						if (!IsVTEnabled) {
							Console.ForegroundColor = color;
						} else {
							ForegroundColor = color;
						}
						WriteProxy(coloredString.Substring(1));
					} else {
						WriteProxy(coloredString);
					}
				}
			}
			Console.ForegroundColor = defColor;
		}

		/// <summary>
		/// Writes a color coded console message via the custom VT formatting (^#......; where ...... is a hex color code)<para/>
		/// Has backwards compatibility for the Minecraft-esque color codes §. where . is a single hex value.
		/// </summary>
		/// <param name="message"></param>
		private static void WriteMessageFromColorsVT(string message) {
			MatchCollection colorMatches = Regex.Matches(message, ConsoleColorVT.COLOR_CODE_REGEX);
			message = Regex.Replace(message, ConsoleColorVT.COLOR_CODE_REGEX, SUPER_UNIQUE_SPLIT_THINGY[0]); // The behavior of Regex.split bricks everything so I have to use this disgusting hacky method.
			string[] colorSegs = message.Split(SUPER_UNIQUE_SPLIT_THINGY, StringSplitOptions.None);
			// colorSegs[0] will be empty if we have a color code at the start.

			ConsoleColorVT old = ForegroundColor;
			for (int idx = 0; idx < colorSegs.Length; idx++) {
				string coloredString = colorSegs[idx];
				if (coloredString.Contains(COLOR_CODE_SYM)) {
					// Backwards compatibility.
					// NEW CATCH CASE: Mixing VT and legacy codes causes a desync.
					// Is the color code the first character? If not, write the VT first, then run this code.
					if (coloredString.First() == COLOR_CODE_SYM) {
						WriteMessageFromColors(coloredString);
					} else {
						string[] subSegs = coloredString.Split(new char[] { COLOR_CODE_SYM }, 2);
						if (subSegs.Length == 2) {
							WriteProxy(subSegs[0]);
							WriteMessageFromColors(COLOR_CODE_SYM + subSegs[1]);
						} else {
							// Didn't split properly.
							WriteMessageFromColors(coloredString);
						}
					}
				} else {
					WriteProxy(coloredString);
				}

				if (idx < colorMatches.Count)
					ForegroundColor = ConsoleColorVT.FromFormattedString(colorMatches[idx].Value);
			}

			ForegroundColor = old;
		}
		private static readonly string[] SUPER_UNIQUE_SPLIT_THINGY = new string[] { "\x69\x42\x06\x66" };

		#endregion

		#region Writing Methods

		/// <summary>
		/// Logs information on a single line. This automatically decides whether to call <see cref="WriteMessageFromColors(string)"/> or <see cref="WriteMessageFromColorsVT(string)"/>.
		/// </summary>
		/// <param name="message">The text to log.</param>
		public static void Write(string message = "") {
			if (message == null) message = "null";
			if (IsVTEnabled) {
				WriteMessageFromColorsVT(ForegroundColor.ToString() + message);
			} else {
				message = StripVTColorFormattingCode(message); // Get rid of any VT sequences.
				WriteMessageFromColors(ForegroundColor.ToStringNonVT(true) + message);
			}
		}

		/// <summary>
		/// Logs information on a single line. This automatically decides whether to call <see cref="WriteMessageFromColors(string)"/> or <see cref="WriteMessageFromColorsVT(string)"/>.
		/// </summary>
		/// <param name="message">The text to log.</param>
		public static void WriteLine(string message = "") {
			Write(message ?? "null");
			Console.WriteLine();
		}

		#endregion
	}
}

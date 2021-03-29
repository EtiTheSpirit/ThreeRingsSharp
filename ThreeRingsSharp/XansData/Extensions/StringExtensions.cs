using System;
using System.Linq;
using System.Text;

namespace ThreeRingsSharp.XansData.Extensions {

	/// <summary>
	/// Provides some handy extension methods for <see cref="string"/>.
	/// </summary>
	public static class StringExtensions {
		/// <summary>
		/// Returns a <see langword="string"/> of everything after the first located instance of <paramref name="text"/>. This does not include the sequence. For instance, if the text <c>cool.beans.nice.meme</c> is input and the method is called with a parameter of <c>nice.</c> for <paramref name="text"/>, the return value will be <c>meme</c><para/>
		/// Returns <see langword="null"/> if <paramref name="text"/> could not be found.
		/// </summary>
		/// <param name="str">The string to search.</param>
		/// <param name="text">The text to locate.</param>
		/// <returns></returns>
		public static string AfterIndexOf(this string str, string text) {
			int idx = str.IndexOf(text);
			if (idx < 0) return null;
			if (idx + text.Length >= str.Length) return string.Empty;

			return str.Substring(idx + text.Length);
		}

		/// <summary>
		/// Returns a <see langword="string"/> of everything after the last located instance of <paramref name="text"/>. This does not include the sequence. For instance, if the text <c>cool.beans.nice.meme</c> is input and the method is called with a parameter of <c>nice.</c> for <paramref name="text"/>, the return value will be <c>meme</c><para/>
		/// Returns <see langword="null"/> if <paramref name="text"/> could not be found.
		/// </summary>
		/// <param name="str">The string to search.</param>
		/// <param name="text">The text to locate.</param>
		/// <returns></returns>
		public static string AfterLastIndexOf(this string str, string text) {
			int idx = str.LastIndexOf(text);
			if (idx < 0) return null;
			if (idx + text.Length >= str.Length) return string.Empty;

			return str.Substring(idx + text.Length);
		}

		/// <summary>
		/// Looks for the first instance of text between square brackets [like this] and returns that string, or <see langword="null"/> if it couldn't be found.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string BetweenBrackets(this string str) {
			int openBracket = str.IndexOf('[');
			if (openBracket == -1) return null;
			int closeBracket = str.IndexOf(']', openBracket + 1);
			if (closeBracket == -1) return null;

			return str.Substring(openBracket + 1, closeBracket - openBracket - 1);
		}


		/// <summary>
		/// Assuming <paramref name="str"/> is between quotation marks (that is, <paramref name="str"/> starts and ends with literal <c>"</c>s), this will strip them off of the end. Returns <see langword="null"/> if the string is not surrounded by quotes.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string BetweenQuotes(this string str) {
			int openQuote = str.IndexOf('"');
			if (openQuote == -1) return null;
			int closeQuote = str.IndexOf('"', openQuote + 1);
			if (closeQuote == -1) return null;

			return str.Substring(openQuote + 1, closeQuote - openQuote - 1);
		}

		/// <summary>
		/// Converts a <c>snake_case_string</c> to a <c>camelCaseString</c>.
		/// </summary>
		/// <param name="snakeCaseString"></param>
		/// <returns></returns>
		public static string SnakeToCamel(this string snakeCaseString) {
			if (snakeCaseString == null) throw new ArgumentNullException(nameof(snakeCaseString));
			if (!snakeCaseString.Contains("_")) return snakeCaseString;
			string[] parts = snakeCaseString.Split('_');
			if (parts.Length == 1) return parts[0];
			string retn = parts[0];
			foreach (string part in parts.Skip(1)) {
				// wtf lol
				retn += part.First().ToString().ToUpper();
				retn += part.Substring(1);
			}
			return retn;
		}

		/// <summary>
		/// Converts a <c>camelCaseString</c> to a <c>PascalCaseString</c>.
		/// </summary>
		/// <param name="camelCaseString"></param>
		/// <returns></returns>
		public static string CamelToPascal(this string camelCaseString) {
			if (camelCaseString == null) throw new ArgumentNullException(nameof(camelCaseString));
			string firstLetter = camelCaseString.Substring(0, 1);
			if (firstLetter.IsUppercase()) return camelCaseString;
			return firstLetter.ToUpper() + camelCaseString.Substring(1);
		}

		/// <summary>
		/// Returns true if the string is alphanumeric, or, if it is within the range of a-z, A-Z, and 0-9.
		/// </summary>
		/// <param name="inp"></param>
		/// <returns></returns>
		public static bool IsAlphanumeric(this string inp) {
			foreach (char c in inp) {
				bool isNumber = (c >= 48 && c <= 57);
				bool isCap = (c >= 65 && c <= 90);
				bool isLow = (c >= 97 && c <= 122);
				bool ok = isNumber || isCap || isLow;
				if (!ok) return false;
			}
			return true;
		}

		/// <summary>
		/// Returns <see langword="true"/> if all chars in <paramref name="text"/> are lowercase.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsLowercase(this string text) {
			foreach (char c in text) {
				bool isLow = c >= 97 && c <= 122;
				if (!isLow) return false;
			}
			return true;
		}

		/// <summary>
		/// Returns <see langword="true"/> if all chars in <paramref name="text"/> are uppercase.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsUppercase(this string text) {
			foreach (char c in text) {
				bool isCap = c >= 65 && c <= 90;
				if (!isCap) return false;
			}
			return true;
		}

		/// <summary>
		/// Returns <see langword="true"/> if all the chars in this string are a number (0 - 9).
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsNumeric(this string text) {
			foreach (char c in text) {
				bool isNumber = c >= 48 && c <= 57;
				if (!isNumber) return false;
			}
			return true;
		}

		/// <summary>
		/// Formats the given string to an ASCII string and then writes it to the array, starting at the given offset.<para/>
		/// Throws <see cref="IndexOutOfRangeException"/> if the array is too short to contain the string.
		/// </summary>
		/// <param name="str">The string to write.</param>
		/// <param name="array">The array to write the string into.</param>
		/// <param name="offset">The index to start writing at.</param>
		public static void WriteASCIIToByteArray(this string str, ref byte[] array, int offset = 0) {
			if (array.Length - offset > str.Length) throw new IndexOutOfRangeException("Attempt to write string into byte array failed: Array is not large enough to contain this string at the given offset.");
			byte[] data = Encoding.ASCII.GetBytes(str);
			for (int idx = 0; idx < data.Length; idx++) {
				int toArrayIdx = idx + offset;
				array[toArrayIdx] = data[idx];
			}
		}
	}
}

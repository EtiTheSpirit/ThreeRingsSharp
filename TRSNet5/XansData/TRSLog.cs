using EtiLogger.Logging;
using EtiLogger.Logging.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKAnimatorTools.PrimaryInterface {
	public class TRSLog : OutputRelay {

		public FileInfo CurrentLogFile = new FileInfo(@$".\output-log-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.log");

		public RichTextBox Target { get; }

		public SynchronizationContext? Context { get; }

		private readonly ConcurrentQueue<(LogMessage, LogLevel, bool, Logger)> MessageQueue = new ConcurrentQueue<(LogMessage, LogLevel, bool, Logger)>();

		private static readonly ManualResetEventSlim NextMessagesWritten = new ManualResetEventSlim(false);

		private Action FlushLogTask { get; set; }

		public TRSLog(RichTextBox rtb) {
			Target = rtb;
			Context = SynchronizationContext.Current;
			FlushLogTask = async delegate {
				while (true) {
					WriteNextMessages();
					NextMessagesWritten.Set();
					await Task.Delay(16);
					NextMessagesWritten.Reset(); // Immediately reset
					await Task.Delay(200);
				}
			};
			FlushLogTask();
		}

		/// <summary>
		/// Waits for the next messages to be written on any TRSLog instance (it should be a singleton anyway)
		/// </summary>
		public static void WaitForNextMessagesToWrite() => NextMessagesWritten.Wait();

		private static System.Drawing.Font GetFontFrom(System.Drawing.Font input, bool? bold = null, bool? italics = null, bool? underline = null, bool? strike = null) {
			string face = input.Name;
			bool makeBold = bold.GetValueOrDefault(input.Bold);
			bool makeItalics = italics.GetValueOrDefault(input.Italic);
			bool makeUnderline = underline.GetValueOrDefault(input.Underline);
			bool makeStrike = strike.GetValueOrDefault(input.Strikeout);

			System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
			if (makeBold) style |= System.Drawing.FontStyle.Bold;
			if (makeItalics) style |= System.Drawing.FontStyle.Italic;
			if (makeUnderline) style |= System.Drawing.FontStyle.Underline;
			if (makeStrike) style |= System.Drawing.FontStyle.Strikeout;
			return new System.Drawing.Font(face, input.Size, style);
		}

		private void OnLogWrittenMain(object? state) {
			try {
				(LogMessage message, LogLevel messageLevel, bool shouldWrite, Logger source) = (ValueTuple<LogMessage, LogLevel, bool, Logger>)state!;
				if (!shouldWrite) return;

				int orgStart = Target.SelectionStart;
				int orgLen = Target.SelectionLength;
				if (Target.TextLength > int.MaxValue - 10000) {
					Target.Clear();
					orgStart = 0;
					orgLen = 0;
				}

				Target.SelectionProtected = true;
				foreach (var cmp in message.Components) {
					Target.SelectionStart = Target.TextLength;
					Target.SelectionLength = 0;
					if (cmp.Color != null) Target.SelectionColor = cmp.Color.Value.ToSystemColor();
					// if (cmp.BackgroundColor != null) Target.SelectionBackColor = cmp.BackgroundColor.Value.ToSystemColor();
					Target.SelectionFont = GetFontFrom(Target.Font, cmp.Bold, cmp.Italics, cmp.Underline, cmp.Strike);

					Target.AppendText(cmp.Text);
					Debug.Write(cmp.Text);
				}
				Target.SelectionFont = GetFontFrom(Target.Font);
				if (orgLen > 0) {
					Target.SelectionStart = orgStart;
					Target.SelectionLength = orgLen;
				}
				Target.SelectionProtected = false;
			} catch (Exception exc) {
				MessageBox.Show(exc.Message + "\n\n" + exc.StackTrace, "Something broke when trying to write to the log!");
			}
		}

		private void WriteNextMessages(int amount = 10) {
			for (int idx = 0; idx < amount; idx++) {
				if (MessageQueue.TryDequeue(out ValueTuple<LogMessage, LogLevel, bool, Logger> data)) {
					if (!data.Item3) {
						idx--;
						continue; // Should not write this message.
						// This block was included because several thousands of trace messages were being dumped to the console, and since
						// it is only allowed to write 10 at a time, this means it took FOREVER to clear the queue.
					}
					if (SynchronizationContext.Current != Context) {
						Context?.Post(OnLogWrittenMain, data);
					} else {
						OnLogWrittenMain(data);
					}
				} else {
					break;
				}
			}
		}

		public override void OnLogWritten(LogMessage message, LogLevel messageLevel, bool shouldWrite, Logger source) {
			try {
				using StreamWriter writer = CurrentLogFile.AppendText();
				writer.Write(message.ToString());
				writer.Flush();
				writer.Close();
			} catch { }

			MessageQueue.Enqueue(ValueTuple.Create(message, messageLevel, shouldWrite, source));

			/*
			 * if (SynchronizationContext.Current != Context) {
				//Context?.Post(OnLogWrittenMain, (message, messageLevel, shouldWrite, source));
			} else {
				//OnLogWrittenMain((message, messageLevel, shouldWrite, source));
			}
			 */
		}
	}

}

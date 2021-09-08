using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities.Parameters.Implementation;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.XansData.XDataTreeExtension {
	public class ChoiceElement : KeyValueElement {
		public Choice Choice { get; set; }

		public ChoiceElement(string? key, string? value, bool editable = false, SilkImage icon = SilkImage.Value) : base(key, value, editable, icon) { }

		public override void OnActivated(SynchronizationContext? synchronizationContext, object mainWindow) {
			if (synchronizationContext != null) {
				synchronizationContext.Send(windowObject => {
					dynamic window = windowObject!;
					window.ShowChangeTargetPrompt(ValueHolder as ShadowClass, Choice);
				}, mainWindow);
			}
		}
	}
}

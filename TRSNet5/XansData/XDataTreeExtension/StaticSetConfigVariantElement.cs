using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.XansData.XDataTreeExtension {
	public class StaticSetConfigVariantElement : KeyValueElement {
		public StaticSetConfigVariantElement(string? key, string? value, bool editable = false, SilkImage icon = SilkImage.Value) : base(key, value, editable, icon) { }

		public override void OnActivated(SynchronizationContext? synchronizationContext, object mainWindow) {
			if (synchronizationContext != null) {
				synchronizationContext.Post(windowObject => {
					dynamic window = windowObject!; // lazy af but this is how it works
					window.ShowChangeTargetPrompt(ValueHolder as ShadowClass);
				}, mainWindow);
			}
		}

	}
}

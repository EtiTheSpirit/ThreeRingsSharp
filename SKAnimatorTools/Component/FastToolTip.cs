using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKAnimatorTools.Component {

	/// <summary>
	/// A variation of a <see cref="ToolTip"/> that shows its dialog instantly and then keeps it open for as long as Windows allows.
	/// </summary>
	public class FastToolTip : ToolTip {

		public FastToolTip() : base() {
			AutoPopDelay = short.MaxValue;
			ShowAlways = true;
			InitialDelay = 100;
			ReshowDelay = 100;
		}

		public FastToolTip(IContainer container) : base(container) {
			AutoPopDelay = short.MaxValue;
			ShowAlways = true;
			InitialDelay = 100;
			ReshowDelay = 100;
		}

	}
}

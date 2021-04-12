using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKAnimatorTools.Configuration.Attributes {

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class DefaultValueAttribute : Attribute {
		
		public object DefaultValue { get; }

		public DefaultValueAttribute(object value) {
			DefaultValue = value;
		}

	}

}

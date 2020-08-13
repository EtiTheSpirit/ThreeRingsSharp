﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKAnimatorTools.Configuration.Attributes {

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class ConfigEntryAttribute : Attribute {
		public ConfigEntryAttribute() { }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Utilities
{
    // From: https://github.com/mono/mono/blob/master/mcs/class/WindowsBase/System.Windows.Input/ModifierKeys.cs
	public enum ModifierKeys {
		None = 0,
		Alt = 1,
		Control = 2,
		Shift = 4,
		Windows = 8
	}
}

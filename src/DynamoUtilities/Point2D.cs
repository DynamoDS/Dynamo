using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Dynamo.Utilities
{
    // From https://github.com/mono/mono/blob/master/mcs/class/WindowsBase/System.Windows/Point.cs
	public struct Point2D : IFormattable
	{
		public Point2D (double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		public double X {
			get { return x; }
			set { x = value; }
		}

		public double Y {
			get { return y; }
			set { y = value; }
		}

		public override bool Equals (object o)
		{
			if (!(o is Point2D))
				return false;
			return Equals ((Point2D)o);
		}

		public bool Equals (Point2D value)
		{
			return x == value.X && y == value.Y;
		}

		public override int GetHashCode ()
		{
		    return (x.GetHashCode() ^ y.GetHashCode());
		}

		public override string ToString ()
		{
			return this.ToString(null, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return this.ToString(null, provider);
		}

		private string ToString(string format,IFormatProvider formatProvider)
		{
			CultureInfo ci = (CultureInfo)formatProvider;

			if (ci == null)
				ci = CultureInfo.CurrentCulture;
			string seperator = ci.NumberFormat.NumberDecimalSeparator;
			if (seperator.Equals(","))
				seperator = ";";
			else
				seperator = ",";
			object[] ob = { this.x, seperator, this.y };

			return string.Format(formatProvider, "{0:" + format + "}{1}{2:" + format + "}", ob);
		}

		string IFormattable.ToString (string format, IFormatProvider formatProvider)
		{
			return this.ToString(format, formatProvider);
		}

		double x;
		double y;
	}
}
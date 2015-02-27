using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Dynamo.Utilities
{
    // From: https://github.com/mono/mono/blob/master/mcs/class/WindowsBase/System.Windows/Rect.cs
	public struct Rect2D : IFormattable
	{
		public Rect2D (Point2D point1, Point2D point2)
		{
			if (point1.X < point2.X) {
				x = point1.X;
				width = point2.X - point1.X;
			}
			else {
				x = point2.X;
				width = point1.X - point2.X;
			}

			if (point1.Y < point2.Y) {
				y = point1.Y;
				height = point2.Y - point1.Y;
			}
			else {
				y = point2.Y;
				height = point1.Y - point2.Y;
			}
		}

		public Rect2D (double x, double y, double width, double height)
		{
			if (width < 0 || height < 0)
				throw new ArgumentException ("width and height must be non-negative.");
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public bool Equals (Rect2D value)
		{
			return (x == value.X &&
				y == value.Y &&
				width == value.Width &&
				height == value.Height);
		}

		public override bool Equals (object o)
		{
			if (!(o is Rect2D))
				return false;

			return Equals ((Rect2D)o);
		}

		public static bool Equals (Rect2D rect1, Rect2D rect2)
		{
			return rect1.Equals (rect2);
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public bool Contains (Rect2D rect)
		{
			if (rect.Left < this.Left ||
			    rect.Right > this.Right)
				return false;

			if (rect.Top < this.Top ||
			    rect.Bottom > this.Bottom)
				return false;

			return true;
		}

        public bool IntersectsWith(Rect2D rect)
        {
            return !((Left >= rect.Right) || (Right <= rect.Left) ||
                (Top >= rect.Bottom) || (Bottom <= rect.Top));
        }

        public void Intersect(Rect2D rect)
        {
            double _x = Math.Max(x, rect.x);
            double _y = Math.Max(y, rect.y);
            double _width = Math.Min(Right, rect.Right) - _x;
            double _height = Math.Min(Bottom, rect.Bottom) - _y;

            if (_width < 0 || _height < 0)
            {
                x = y = Double.PositiveInfinity;
                width = height = Double.NegativeInfinity;
            }
            else
            {
                x = _x;
                y = _y;
                width = _width;
                height = _height;
            }
        }

        public static Rect2D Intersect(Rect2D rect1, Rect2D rect2)
        {
            Rect2D result = rect1;
            result.Intersect(rect2);
            return result;
        }

		public bool Contains (double x, double y)
		{
			if (x < Left || x > Right)
				return false;
			if (y < Top || y > Bottom)
				return false;

			return true;
		}

        public bool Contains(Point2D point)
        {
            return Contains(point.X, point.Y);
        }


		public void Scale(double scaleX, double scaleY)
		{
			x *= scaleX;
			y *= scaleY;
			width *= scaleX;
			height *= scaleY;
		}

		public override string ToString ()
		{
			return ToString (null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		string IFormattable.ToString (string format, IFormatProvider provider)
		{
			return ToString (format, provider);
		}

		private string ToString (string format, IFormatProvider provider)
		{
			if (IsEmpty)
				return "Empty";

			if (provider == null)
				provider = CultureInfo.CurrentCulture;

			if (format == null)
				format = string.Empty;

			string separator = ",";
			NumberFormatInfo numberFormat =
				provider.GetFormat (typeof (NumberFormatInfo)) as NumberFormatInfo;
			if (numberFormat != null &&
			    numberFormat.NumberDecimalSeparator == separator)
				separator = ";";

			string rectFormat = String.Format (
				"{{0:{0}}}{1}{{1:{0}}}{1}{{2:{0}}}{1}{{3:{0}}}",
				format, separator);
			return String.Format (provider, rectFormat,
				x, y, width, height);
		}

		public static Rect2D Empty { 
			get {
				Rect2D r = new Rect2D ();
				r.x = r.y = Double.PositiveInfinity;
				r.width = r.height = Double.NegativeInfinity;
				return r;
			} 
		}
		
		public bool IsEmpty { 
			get {
				return (x == Double.PositiveInfinity &&
					y == Double.PositiveInfinity &&
					width == Double.NegativeInfinity &&
					height == Double.NegativeInfinity);
			}
		}
		
		public Point2D Location { 
			get {
				return new Point2D (x, y);
			}
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				x = value.X;
				y = value.Y;
			}
		}
		
		public double X {
			get { return x; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				x = value;
			}
		}

		public double Y {
			get { return y; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				y = value;
			}
		}

		public double Width {
			get { return width; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				if (value < 0)
					throw new ArgumentException ("width must be non-negative.");

				width = value;
			}
		}

		public double Height {
			get { return height; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				if (value < 0)
					throw new ArgumentException ("height must be non-negative.");

				height = value;
			}
		}

		public double Left { 
			get { return x; }
		}

		public double Top { 
			get { return y; }
		}
		
		public double Right { 
			get { return x + width; }
		}
		
		public double Bottom { 
			get { return y + height; }
		}
		
		public Point2D TopLeft { 
			get { return new Point2D (Left, Top); }
		}
		
		public Point2D TopRight { 
			get { return new Point2D (Right, Top); }
		}
		
		public Point2D BottomLeft { 
			get { return new Point2D (Left, Bottom); }
		}

		public Point2D BottomRight { 
			get { return new Point2D (Right, Bottom); }
		}
		
		double x;
		double y;
		double width;
		double height;
	}
}
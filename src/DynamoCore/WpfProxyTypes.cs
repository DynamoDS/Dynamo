using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo
{
    // SEPARATECORE: this needs to be remove completely

    public struct Thickness
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }

        public Thickness(double thickness)
            : this()
        {
            Top = thickness;
            Left = thickness;
            Right = thickness;
            Bottom = thickness;
        }

        public Thickness(double left, double top, double right, double bottom)
            : this()
        {
            Top = top;
            Left = left;
            Right = right;
            Bottom = bottom;
        }
    }

    public struct FontWeight
    {
        
    }

    public static class FontWeights
    {
        public static int Light
        {
            get { return 300;  }
        }
    }

}

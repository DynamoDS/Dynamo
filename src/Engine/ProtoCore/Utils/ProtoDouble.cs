using System;
using System.Globalization;

namespace ProtoCore.Utils
{
    public struct ProtoDouble
    {
        Double value;
        public static Double tolerance = 1e-6;

        #region casts
        //implicit cast to and from Double since it does not potentially cause any loss of data and/or precision
        public static implicit operator ProtoDouble(double value) { return new ProtoDouble(value); }
        public static implicit operator Double(ProtoDouble d) { return d.value; }

        //explicit casts that may possibly cause loss of data and/or precision
        public static explicit operator Int64(ProtoDouble d) { return (Int64)d.value; }
        public static explicit operator float(ProtoDouble d) { return (float)d.value; }
        public static explicit operator int(ProtoDouble d) { return (int)d.value; }
        #endregion

        #region constructors
        // constructors
        public ProtoDouble(double value) { this.value = value; }
        public ProtoDouble(ProtoDouble d) { this.value = d.value; }
        #endregion

        #region comparison operator overloads
        //comparison operator overloads
        public static bool operator <(ProtoDouble left, ProtoDouble right)
        {
            return (Math.Abs(left.value - right.value)) > tolerance && (left.value - right.value < 0) ? true : false;
        }

        public static bool operator >(ProtoDouble left, ProtoDouble right)
        {
            return (Math.Abs(left.value - right.value)) > tolerance && (left.value - right.value > 0) ? true : false;
        }

        public static bool operator <=(ProtoDouble left, ProtoDouble right)
        {
            return (Math.Abs(left.value - right.value)) < tolerance || (left.value - right.value < 0) ? true : false;
        }

        public static bool operator >=(ProtoDouble left, ProtoDouble right)
        {
            return (Math.Abs(left.value - right.value)) < tolerance || (left.value - right.value > 0) ? true : false;
        }

        public static bool operator ==(ProtoDouble left, ProtoDouble right)
        {
            return (Math.Abs(left.value - right.value)) < tolerance ? true : false;
        }

        public static bool operator !=(ProtoDouble left, ProtoDouble right)
        {
            return (Math.Abs(left.value - right.value)) < tolerance ? false : true;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region arithmetic operator overloads
        //arithmetic operator overloads
        public static ProtoDouble operator +(ProtoDouble left, ProtoDouble right)
        {
            return new ProtoDouble(left.value + right.value);
        }

        public static ProtoDouble operator -(ProtoDouble left, ProtoDouble right)
        {
            return new ProtoDouble(left.value - right.value);
        }

        public static ProtoDouble operator *(ProtoDouble left, ProtoDouble right)
        {
            return new ProtoDouble(left.value * right.value);
        }

        public static ProtoDouble operator /(ProtoDouble left, ProtoDouble right)
        {
            return new ProtoDouble(left.value / right.value);
        }
#endregion

        #region ToString, Parse and TryParse Functions
        //ToString functions
        public override String ToString() { return this.value.ToString(); }
        public String ToString(string format) { return this.value.ToString(format); }
        public String ToString(IFormatProvider provider) { return this.value.ToString(provider); }
        public String ToString(string format, IFormatProvider provider) { return this.value.ToString(format, provider); }

        //Parse functions
        public static ProtoDouble Parse(string s) { return new ProtoDouble(Double.Parse(s)); }
        public static ProtoDouble Parse(string s, IFormatProvider provider) { return new ProtoDouble(Double.Parse(s, provider)); }
        public static ProtoDouble Parse(string s, NumberStyles style) { return new ProtoDouble(Double.Parse(s, style)); }
        public static ProtoDouble Parse(string s, NumberStyles style, IFormatProvider provider) { return new ProtoDouble(Double.Parse(s, style, provider)); }

        //TryParse functions
        public static bool TryParse(string s, out ProtoDouble result)
        {
            double dResult;
            bool isSuccess = Double.TryParse(s, out dResult);
            result = new ProtoDouble(dResult);
            return isSuccess;
        }
        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ProtoDouble result)
        {
            double dResult;
            bool isSuccess = Double.TryParse(s, style, provider, out dResult);
            result = new ProtoDouble(dResult);
            return isSuccess;
        }
        #endregion       

    }
}

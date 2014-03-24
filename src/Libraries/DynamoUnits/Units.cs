using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Double = System.Double;
using Autodesk.DesignScript.Runtime;

namespace Dynamo.Units
{
    //[SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum DynamoLengthUnit
    {
        DecimalInch,
        FractionalInch,
        DecimalFoot,
        FractionalFoot,
        Millimeter,
        Centimeter,
        Meter
    }

    //[SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum DynamoAreaUnit
    {
        SquareInch, 
        SquareFoot,
        SquareMillimeter,
        SquareCentimeter,
        SquareMeter
    }

    //[SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum DynamoVolumeUnit
    {
        CubicInch,
        CubicFoot,
        CubicMillimeter,
        CubicCentimeter,
        CubicMeter
    }

    public abstract class SIUnit
    {
        //length conversions
        private static double meter_to_millimeter = 1000;
        private static double meter_to_centimeter = 100;
        private static double meter_to_inch = 39.37007874 ;
        private static double meter_to_foot = 3.280839895;

        //area conversions
        private static double square_meters_to_square_millimeters = 1000000;
        private static double square_meters_to_square_centimeters = 10000;
        private static double square_meters_to_square_inch = 1550.0031;
        private static double square_meters_to_square_foot = 10.763910417;

        //volume conversions
        private static double cubic_meters_to_cubic_millimeters = 1000000000;
        private static double cubic_meters_to_cubic_centimeters = 1000000;
        private static double cubic_meters_to_cubic_inches = 61023.744095;
        private static double cubic_meters_to_cubic_feet = 35.3147;

        private static double epsilon = 1e-6;
        internal double _value;

        private static double _uiLengthConversion = 1.0;
        private static double _uiAreaConversion = 1.0;
        private static double _uiVolumeConversion = 1.0;
        private static DynamoLengthUnit _hostApplicationInternalLengthUnit = DynamoLengthUnit.Meter;
        private static DynamoAreaUnit _hostApplicationInternalAreaUnit = DynamoAreaUnit.SquareMeter;
        private static DynamoVolumeUnit _hostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicMeter;
        private static string _numberFormat = "f3";
        private static DynamoLengthUnit _lengthUnit;
        private static DynamoAreaUnit _areaUnit;
        private static DynamoVolumeUnit _volumeUnit;

        public static double ToMillimeter
        {
            get { return meter_to_millimeter; }
        }

        public static double ToCentimeter
        {
            get { return meter_to_centimeter; }
        }

        public static double ToMeter
        {
            get { return 1.0; }
        }

        public static double ToInch
        {
            get { return meter_to_inch; }
        }

        public static double ToFoot
        {
            get { return meter_to_foot; }
        }

        public static double ToSquareMillimeters
        {
            get { return square_meters_to_square_millimeters; }
        }

        public static double ToSquareCentimeters
        {
            get { return square_meters_to_square_centimeters; }
        }

        public static double ToSquareInch
        {
            get { return square_meters_to_square_inch; }
        }

        public static double ToSquareFoot
        {
            get { return square_meters_to_square_foot; }
        }

        public static double ToCubicMillimeter
        {
            get { return cubic_meters_to_cubic_millimeters; }
        }

        public static double ToCubicCentimeter
        {
            get { return cubic_meters_to_cubic_centimeters; }
        }

        public static double ToCubicInch
        {
            get { return cubic_meters_to_cubic_inches; }
        }

        public static double ToCubicFoot
        {
            get { return cubic_meters_to_cubic_feet; }
        }

        public static double Epsilon
        {
            get { return epsilon; }
        }

        public static double UiLengthConversion
        {
            get { return _uiLengthConversion; }
            set { _uiLengthConversion = value; }
        }

        public static double UiAreaConversion
        {
            get { return _uiAreaConversion; }
            internal set { _uiAreaConversion = value; }
        }

        public static double UiVolumeConversion
        {
            get { return _uiVolumeConversion; }
            internal set { _uiVolumeConversion = value; }
        }

        public static DynamoLengthUnit HostApplicationInternalLengthUnit
        {
            get { return _hostApplicationInternalLengthUnit; }
            set { _hostApplicationInternalLengthUnit = value; }
        }

        public static DynamoAreaUnit HostApplicationInternalAreaUnit
        {
            get { return _hostApplicationInternalAreaUnit; }
            set { _hostApplicationInternalAreaUnit = value; }
        }

        public static DynamoVolumeUnit HostApplicationInternalVolumeUnit
        {
            get { return _hostApplicationInternalVolumeUnit; }
            set { _hostApplicationInternalVolumeUnit = value; }
        }

        public static string NumberFormat
        {
            get { return _numberFormat; }
            set { _numberFormat = value; }
        }

        public static DynamoLengthUnit LengthUnit
        {
            get { return _lengthUnit; }
            set
            {
                _lengthUnit = value;

                switch (_lengthUnit)
                {
                    case DynamoLengthUnit.Millimeter:
                        UiLengthConversion = SIUnit.ToMillimeter;
                        break;
                    case DynamoLengthUnit.Centimeter:
                        UiLengthConversion = SIUnit.ToCentimeter;
                        break;
                    case DynamoLengthUnit.Meter:
                        UiLengthConversion = 1.0;
                        break;
                    case DynamoLengthUnit.DecimalInch:
                        UiLengthConversion = SIUnit.ToInch;
                        break;
                    case DynamoLengthUnit.FractionalInch:
                        UiLengthConversion = SIUnit.ToInch;
                        break;
                    case DynamoLengthUnit.DecimalFoot:
                        UiLengthConversion = SIUnit.ToFoot;
                        break;
                    case DynamoLengthUnit.FractionalFoot:
                        UiLengthConversion = SIUnit.ToFoot;
                        break;
                }
            }
        }

        public static DynamoAreaUnit AreaUnit
        {
            get { return _areaUnit; }
            set
            {
                _areaUnit = value;

                switch (_areaUnit)
                {
                    case DynamoAreaUnit.SquareMillimeter:
                        UiAreaConversion = SIUnit.ToSquareMillimeters;
                        break;
                    case DynamoAreaUnit.SquareCentimeter:
                        UiAreaConversion = SIUnit.ToSquareCentimeters;
                        break;
                    case DynamoAreaUnit.SquareMeter:
                        UiAreaConversion = 1.0;
                        break;
                    case DynamoAreaUnit.SquareInch:
                        UiAreaConversion = SIUnit.ToSquareInch;
                        break;
                    case DynamoAreaUnit.SquareFoot:
                        UiAreaConversion = SIUnit.ToSquareFoot;
                        break;
                }
            }
        }

        public static DynamoVolumeUnit VolumeUnit
        {
            get { return _volumeUnit; }
            set
            {
                _volumeUnit = value;

                switch (_volumeUnit)
                {
                    case DynamoVolumeUnit.CubicMillimeter:
                        UiVolumeConversion = SIUnit.ToCubicMillimeter;
                        break;
                    case DynamoVolumeUnit.CubicCentimeter:
                        UiVolumeConversion = SIUnit.ToCubicCentimeter;
                        break;
                    case DynamoVolumeUnit.CubicMeter:
                        UiVolumeConversion = 1.0;
                        break;
                    case DynamoVolumeUnit.CubicInch:
                        UiVolumeConversion = SIUnit.ToCubicInch;
                        break;
                    case DynamoVolumeUnit.CubicFoot:
                        UiVolumeConversion = SIUnit.ToCubicFoot;
                        break;
                }
            }
        }

        /// <summary>
        /// The internal value of the unit.
        /// </summary>
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Construct an SIUnit object with a value.
        /// </summary>
        /// <param name="value"></param>
        protected SIUnit(double value)
        {
            _value = value;
        }

        /// <summary>
        /// Implemented in child classes to control how units are converted
        /// from a string representation to an SI value.
        /// </summary>
        /// <param name="value"></param>
        public abstract void SetValueFromString(string value);

        public abstract SIUnit Add(SIUnit x);
        public abstract SIUnit Add(double x);
        public abstract SIUnit Subtract(SIUnit x);
        public abstract SIUnit Subtract(double x);
        public abstract SIUnit Multiply(SIUnit x);
        public abstract SIUnit Multiply(double x);
        public abstract dynamic Divide(SIUnit x);
        public abstract SIUnit Divide(double x);
        public abstract SIUnit Modulo(SIUnit x);
        public abstract SIUnit Modulo(double x);
        public abstract SIUnit Round();
        public abstract SIUnit Ceiling();
        public abstract SIUnit Floor();

        #region operator overloads

        public static SIUnit operator +(SIUnit x, SIUnit y)
        {
            return x.Add(y);
        }

        public static SIUnit operator +(SIUnit x, double y)
        {
            return x.Add(y);
        }

        public static double operator +(double x, SIUnit y)
        {
            return x + y.Value;
        }

        public static SIUnit operator -(SIUnit x, SIUnit y)
        {
            return x.Subtract(y);
        }

        public static SIUnit operator -(SIUnit x, double y)
        {
            return x.Subtract(y);
        }

        public static double operator -(double x, SIUnit y)
        {
            return x - y.Value;
        }

        public static SIUnit operator *(SIUnit x, SIUnit y)
        {
            return x.Multiply(y);
        }

        public static SIUnit operator *(SIUnit x, double y)
        {
            return x.Multiply(y);
        }

        public static SIUnit operator *(double x, SIUnit y)
        {
            return y.Multiply(x);
        }

        public static dynamic operator /(SIUnit x, SIUnit y)
        {
            //units will cancel
            if (x.GetType() == y.GetType())
            {
                return x.Value / y.Value;
            }    

            return x.Divide(y);
        }

        public static SIUnit operator /(SIUnit x, double y)
        {
            return x.Divide(y);
        }

        public static SIUnit operator %(SIUnit x, SIUnit y)
        {
            return x.Modulo(y);
        }

        public static SIUnit operator %(SIUnit x, double y)
        {
            return x.Modulo(y);
        }

        public static double operator %(double x, SIUnit y)
        {
            return x % y.Value;
        }

        public static bool operator >(double x, SIUnit y)
        {
            return x > y.Value;
        }

        public static bool operator >(SIUnit x, double y)
        {
            return x.Value > y;
        }

        public static bool operator >(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value > y.Value;
        }

        public static bool operator <(double x, SIUnit y)
        {
            return x < y.Value;
        }

        public static bool operator <(SIUnit x, double y)
        {
            return x.Value < y;
        }

        public static bool operator <(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value < y.Value;
        }

        public static bool operator >=(double x, SIUnit y)
        {
            return x >= y.Value;
        }

        public static bool operator >=(SIUnit x, double y)
        {
            return x.Value >= y;
        }

        public static bool operator >=(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value >= y.Value;
        }

        public static bool operator <=(double x, SIUnit y)
        {
            return x <= y.Value;
        }

        public static bool operator <=(SIUnit x, double y)
        {
            return x.Value <= y;
        }

        public static bool operator <=(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value <= y.Value;
        }

        #endregion

        public abstract double ConvertToHostUnits();
    }

    /// <summary>
    /// A length stored as meters.
    /// </summary>
    public class Length : SIUnit, IComparable, IEquatable<Length>
    {
        public Length(double value):base(value){}

        public static Length FromFeet(double value)
        {
            return new Length(value/ToFoot);
        }

        #region math

        public override SIUnit Add(SIUnit x)
        {
            if(x is Length)
                return new Length(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Add(double x)
        {
            return new Length(_value + x);
        }

        public override SIUnit Subtract(SIUnit x)
        {
            if(x is Length)
                return new Length(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Subtract(double x)
        {
            return new Length(_value - x);
        }

        public override SIUnit Multiply(SIUnit x)
        {
            if (x is Length)
            {
                return new Area(_value * x.Value);
            }

            if (x is Area)
            {
                return new Volume(_value * x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Multiply(double x)
        {
            return new Length(_value * x);
        }

        public override dynamic Divide(SIUnit x)
        {
            if (x is Length)
            {
                return _value/x.Value;
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Divide(double x)
        {
            return new Length(_value / x);
        }

        public override SIUnit Modulo(SIUnit x)
        {
            if(x is Length)
                return new Length(_value % x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Modulo(double x)
        {
            return new Length(_value % x);
        }

        public override SIUnit Round()
        {
            var val = _value * UiLengthConversion;
            double round = Math.Round(val);
            return new Length(round / UiLengthConversion);
        }

        public override SIUnit Ceiling()
        {
            var val = _value * UiLengthConversion;
            double round = Math.Ceiling(val);
            return new Length(round / UiLengthConversion);
        }

        public override SIUnit Floor()
        {
            var val = _value * UiLengthConversion;
            double round = Math.Floor(val);
            return new Length(round / UiLengthConversion);
        }

        public override double ConvertToHostUnits()
        {
            switch (HostApplicationInternalLengthUnit)
            {
                case DynamoLengthUnit.DecimalFoot:
                    return _value * ToFoot;
                default:
                    return _value;
            }
        }

        #endregion

        #region string

        public override void SetValueFromString(string value)
        {
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out total))
            {
                _value = total/UiLengthConversion;
                return;
            }

            double fractionalInch = 0.0;
            double feet, inch, m, cm, mm, numerator, denominator;
            Utils.ParseLengthFromString(value, out feet, out inch, out m, out cm, out mm, out numerator, out denominator);

            if (denominator != 0)
                fractionalInch = numerator / denominator;

            if (feet < 0)
                total = (feet - inch / 12.0 - fractionalInch / 12.0) / ToFoot;
            else
                total = (feet + inch / 12.0 + fractionalInch / 12.0) / ToFoot;

            total += m;
            total += cm / ToCentimeter;
            total += mm / ToMillimeter;

            _value = total;
        }

        [IsVisibleInDynamoLibrary(false)]
        public bool Equals(Length other)
        {
            if (other == null)
                return false;

            if (Math.Abs(other.Value - _value) < SIUnit.Epsilon)
                return true;

            return false;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            return BuildString(LengthUnit);
        }

        private string BuildString(DynamoLengthUnit unit)
        {
            switch (unit)
            {
                case DynamoLengthUnit.Millimeter:
                    return (_value * SIUnit.ToMillimeter).ToString(NumberFormat, CultureInfo.InvariantCulture) + "mm";

                case DynamoLengthUnit.Centimeter:
                    return (_value * SIUnit.ToCentimeter).ToString(NumberFormat, CultureInfo.InvariantCulture) + "cm";

                case DynamoLengthUnit.Meter:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + "m";

                case DynamoLengthUnit.DecimalInch:
                    return (_value * SIUnit.ToInch).ToString(NumberFormat, CultureInfo.InvariantCulture) + "in";

                case DynamoLengthUnit.FractionalInch:
                    return Utils.ToFractionalInches(_value * SIUnit.ToInch);

                case DynamoLengthUnit.DecimalFoot:
                    return (_value * SIUnit.ToFoot).ToString(NumberFormat, CultureInfo.InvariantCulture) + "ft";

                case DynamoLengthUnit.FractionalFoot:
                    return Utils.ToFeetAndFractionalInches(_value * SIUnit.ToFoot);

                default:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + "m";
            }
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherLength = obj as Length;
            if (otherLength != null)
                return _value.CompareTo(otherLength.Value);
            else
                throw new ArgumentException("Object is not a Length");
        }

        [IsVisibleInDynamoLibrary(false)]
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var length = obj as Length;
            if (length == null)
            {
                return false;
            }
            return Math.Abs(length.Value - _value) < SIUnit.Epsilon;
        }
    }

    /// <summary>
    /// An area stored as square meters.
    /// </summary>
    public class Area : SIUnit, IComparable, IEquatable<Area>
    {
        public Area(double value):base(value){}

        public static Area FromSquareFeet(double value)
        {
            return new Area(value / ToSquareFoot);
        }

        #region math

        public override SIUnit Add(SIUnit x)
        {
            if(x is Area)
                return new Area(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Add(double x)
        {
            return new Area(_value + x);
        }

        public override SIUnit Subtract(SIUnit x)
        {
            if(x is Area)
                return new Area(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Subtract(double x)
        {
            return new Area(_value - x);
        }

        public override SIUnit Multiply(SIUnit x)
        {
            if (x is Length)
            {
                //return a volume
                return new Volume(_value * x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Multiply(double x)
        {
            return new Area(_value * x);
        }

        public override dynamic Divide(SIUnit x)
        {
            if (x is Area)
            {
                //return a double
                return _value/x.Value;
            }

            if (x is Length)
            {
                //return length
                return new Length(_value/x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Divide(double x)
        {
            return new Area(_value/x);
        }

        public override SIUnit Modulo(SIUnit x)
        {
            if (x is Area)
            {
                return new Area(_value % x.Value);
            }
            
            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Modulo(double x)
        {
            return new Area(_value % x);
        }

        public override SIUnit Round()
        {
            var val = _value * UiAreaConversion;
            double round = Math.Round(val);
            return new Area(round / UiAreaConversion);
        }

        public override SIUnit Ceiling()
        {
            var val = _value * UiAreaConversion;
            double round = Math.Ceiling(val);
            return new Area(round / UiAreaConversion);
        }

        public override SIUnit Floor()
        {
            var val = _value * UiAreaConversion;
            double round = Math.Floor(val);
            return new Area(round / UiAreaConversion);
        }

        public override double ConvertToHostUnits()
        {
            switch (HostApplicationInternalAreaUnit)
            {
                case DynamoAreaUnit.SquareFoot:
                    return _value/ToSquareFoot;
                default:
                    return _value;
            }
        }

        #endregion

        #region string
        
        public override void SetValueFromString(string value)
        {
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (Double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out total))
            {
                var v = total/UiAreaConversion;
                _value = v;
                return;
            }

            double sq_mm, sq_cm, sq_m, sq_in, sq_ft;
            Utils.ParseAreaFromString(value, out sq_in, out sq_ft, out sq_mm, out sq_cm, out sq_m);

            total += sq_mm / SIUnit.ToSquareMillimeters;
            total += sq_cm / SIUnit.ToSquareCentimeters;
            total += sq_m;
            total += sq_in / SIUnit.ToSquareInch;
            total += sq_ft / SIUnit.ToSquareFoot;

            _value = total < 0 ? 0.0 : total;
        }

        [IsVisibleInDynamoLibrary(false)]
        public bool Equals(Area other)
        {
            if (other == null)
                return false;

            if (Math.Abs(other.Value - _value) < SIUnit.Epsilon)
                return true;

            return false;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            return BuildString(AreaUnit);
        }

        private string BuildString(DynamoAreaUnit unit)
        {
            switch (unit)
            {
                case DynamoAreaUnit.SquareMillimeter:
                    return (_value*SIUnit.ToSquareMillimeters).ToString(NumberFormat, CultureInfo.InvariantCulture) + "mm²";

                case DynamoAreaUnit.SquareCentimeter:
                    return (_value*SIUnit.ToSquareCentimeters).ToString(NumberFormat, CultureInfo.InvariantCulture) + "cm²";

                case DynamoAreaUnit.SquareMeter:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + "m²";

                case DynamoAreaUnit.SquareInch:
                    return (_value * SIUnit.ToSquareInch).ToString(NumberFormat, CultureInfo.InvariantCulture) + "in²";

                case DynamoAreaUnit.SquareFoot:
                    return (_value * SIUnit.ToSquareFoot).ToString(NumberFormat, CultureInfo.InvariantCulture) + "ft²";

                default:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + "m²";
            }
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherArea = obj as Area;
            if (otherArea != null)
                return _value.CompareTo(otherArea.Value);
            else
                throw new ArgumentException("Object is not an Area");
        }

        [IsVisibleInDynamoLibrary(false)]
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var area = obj as Area;
            if (area == null)
            {
                return false;
            }
            return Math.Abs(area.Value - _value) < SIUnit.Epsilon;
        }
    }

    /// <summary>
    /// A volume stored as cubic meters.
    /// </summary>
    public class Volume : SIUnit, IComparable, IEquatable<Volume>
    {
        public Volume(double value) : base(value){}

        public static Volume FromCubicFeet(double value)
        {
            return new Volume(value / ToCubicFoot);
        }

        #region math

        public override SIUnit Add(SIUnit x)
        {
            if(x is Volume)
                return new Volume(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Add(double x)
        {
            return new Volume(_value + x);
        }

        public override SIUnit Subtract(SIUnit x)
        {
            if(x is Volume)
                return new Volume(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Subtract(double x)
        {
            return new Volume(_value - x);
        }

        public override SIUnit Multiply(SIUnit x)
        {
            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Multiply(double x)
        {
            return new Volume(_value * x);
        }

        public override dynamic Divide(SIUnit x)
        {
            if (x is Length)
            {
                return new Area(_value/x.Value);
            }
            else if (x is Area)
            {
                return new Length(_value/x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Divide(double x)
        {
            return new Volume(_value/x);
        }

        public override SIUnit Modulo(SIUnit x)
        {
            if (x is Volume)
            {
                return new Volume(_value % x.Value);
            }
            
            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Modulo(double x)
        {
            return new Volume(_value % x);
        }

        public override SIUnit Round()
        {
            var val = _value * UiVolumeConversion;
            double round = Math.Round(val);
            return new Volume(round / UiVolumeConversion);
        }

        public override SIUnit Ceiling()
        {
            var val = _value * UiVolumeConversion;
            double round = Math.Ceiling(val);
            return new Volume(round / UiVolumeConversion);
        }

        public override SIUnit Floor()
        {
            var val = _value * UiVolumeConversion;
            double round = Math.Floor(val);
            return new Volume(round / UiVolumeConversion);
        }

        public override double ConvertToHostUnits()
        {
            switch (VolumeUnit)
            {
                case DynamoVolumeUnit.CubicFoot:
                    return _value/ToCubicFoot;
                default:
                    return _value;
            }
        }

        #endregion

        #region string

        public override void SetValueFromString(string value)
        {
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (Double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out total))
            {
                var v = total/UiVolumeConversion;
                _value =  v;
                return;
            }

            double cu_mm, cu_cm, cu_m, cu_in, cu_ft;
            Utils.ParseVolumeFromString(value, out cu_in, out cu_ft, out cu_mm, out cu_cm, out cu_m);

            total += cu_mm / ToCubicMillimeter;
            total += cu_cm / ToCubicCentimeter;
            total += cu_m;
            total += cu_in / ToCubicInch;
            total += cu_ft / ToCubicFoot;

            _value = total < 0 ? 0.0 : total;
        }

        [IsVisibleInDynamoLibrary(false)]
        public bool Equals(Volume other)
        {
            if (other == null)
                return false;

            if (Math.Abs(other.Value - _value) < SIUnit.Epsilon)
                return true;

            return false;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            return BuildString(VolumeUnit);
        }

        private string BuildString(DynamoVolumeUnit unit)
        {
            switch (unit)
            {
                case DynamoVolumeUnit.CubicMillimeter:
                    return (_value * SIUnit.ToCubicMillimeter).ToString(NumberFormat, CultureInfo.InvariantCulture) + "mm³";

                case DynamoVolumeUnit.CubicCentimeter:
                    return (_value * SIUnit.ToCubicCentimeter).ToString(NumberFormat, CultureInfo.InvariantCulture) + "cm³";

                case DynamoVolumeUnit.CubicMeter:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + "m³";

                case DynamoVolumeUnit.CubicInch:
                    return (_value * SIUnit.ToCubicInch).ToString(NumberFormat, CultureInfo.InvariantCulture) + "in³";

                case DynamoVolumeUnit.CubicFoot:
                    return (_value * SIUnit.ToCubicFoot).ToString(NumberFormat, CultureInfo.InvariantCulture) + "ft³";

                default:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + "m³";
            }
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherVolume = obj as Volume;
            if (otherVolume != null)
                return _value.CompareTo(otherVolume.Value);
            else
                throw new ArgumentException("Object is not a Volume");
        }

        [IsVisibleInDynamoLibrary(false)]
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var volume = obj as Volume;
            if (volume == null)
            {
                return false;
            }
            return Math.Abs(volume.Value - _value) < SIUnit.Epsilon;
        }
    }

    /// <summary>
    /// A luminous intensity stored as candela
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class LuminousIntensity : SIUnit
    {
        public LuminousIntensity(double value) : base(value)
        {

        }

        public override void SetValueFromString(string value)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Add(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Add(double x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Subtract(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Subtract(double x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Multiply(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Multiply(double x)
        {
            throw new NotImplementedException();
        }

        public override dynamic Divide(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Divide(double x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Modulo(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Modulo(double x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Round()
        {
            throw new NotImplementedException();
        }

        public override SIUnit Ceiling()
        {
            throw new NotImplementedException();
        }

        public override SIUnit Floor()
        {
            throw new NotImplementedException();
        }

        public override double ConvertToHostUnits()
        {
            return _value;
        }
    }

    /// <summary>
    /// A luminance stored as candela/m²
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class Luminance : SIUnit
    {
        public Luminance(double value) : base(value)
        {
        }

        public override void SetValueFromString(string value)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Add(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Add(double x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Subtract(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Subtract(double x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Multiply(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Multiply(double x)
        {
            throw new NotImplementedException();
        }

        public override dynamic Divide(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Divide(double x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Modulo(SIUnit x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Modulo(double x)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Round()
        {
            throw new NotImplementedException();
        }

        public override SIUnit Ceiling()
        {
            throw new NotImplementedException();
        }

        public override SIUnit Floor()
        {
            throw new NotImplementedException();
        }

        public override double ConvertToHostUnits()
        {
            return _value;
        }
    }

    [SupressImportIntoVM]
    public static class UnitExtensions
    {
        public static bool AlmostEquals(this double double1, double double2, double precision)
        {
            return (Math.Abs(double1 - double2) <= precision);
        }

        public static Length ToLength(this Double value)
        {
            return new Length(value);
        }

        public static Area ToArea(this Double value)
        {
            return new Area(value);
        }

        public static Volume ToVolume(this Double value)
        {
            return new Volume(value);
        }
    }

    /// <summary>
    /// Utility class for operating on units of measure.
    /// </summary>
    [SupressImportIntoVM]
    public class Utils
    {
        public static string ParseWholeInchesToString(double value)
        {
            double result = value <0 ? 
                Math.Abs(System.Math.Ceiling(value)): 
                Math.Abs(System.Math.Floor(value));

            if (result.AlmostEquals(0.0, 0.00001))
                return "";
            return result.ToString();
        }

        public static string ParsePartialInchesToString(double value, double precision)
        {
            string result = value < 0? 
                Utils.CreateFraction(Math.Abs(value - Math.Ceiling(value)), 0.015625):
                Utils.CreateFraction(Math.Abs(value - Math.Floor(value)), 0.015625);

            return result;
        }

        /// <summary>
        /// Given a double value, create a string fraction with given precision.
        /// </summary>
        /// <param name="value">The value to convert to a fractional representation.</param>
        /// <param name="precision">Fractional precision described as a double. i.e. 1/64th -> 0.015625</param>
        /// <returns>A string representing the fraction.</returns>
        public static string CreateFraction(double value, double precision)
        {
            double numerator = 0.0;
            numerator = value < 0 ?
                Math.Floor(value / precision) :
                Math.Ceiling(value / precision);

            double denominator = 1 / precision;

            if (numerator.AlmostEquals(denominator, 0.00001))
                return "1";

            if (numerator != 0.0)
            {
                while (numerator % 2 == 0.0)
                {
                    numerator = numerator/2;
                    denominator = denominator/2;
                }

                return string.Format("{0}/{1}", numerator, denominator);
            }

            return "";
        }

        private static double RoundToSignificantDigits(double d, int digits)
        {
            if (d == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }

        public static double ParseUnit(string value, string unitSymbol)
        {
            double m;

            if (value.ToLower().Contains(unitSymbol))
                value = value.Replace(unitSymbol, "");

            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out m))
            {
                return 0.0;
            }

            return m;
        }

        ///<summary>
        ///Convert from feet and fractional inches to decimal feet.
        ///</summary>
        ///<param name="value"></param>
        ///<returns></returns>
        public static double FromFeetAndFractionalInches(string value)
        {
            double fractionalInch = 0.0;

            double feet, inch, m, cm, mm, numerator, denominator;
            Utils.ParseLengthFromString(value.ToString(), out feet, out inch, out m, out cm, out mm, out numerator, out denominator);

            if (denominator != 0.0)
                fractionalInch = numerator / denominator;

            double sign = 1;
            if (value.StartsWith("-"))
            {
                sign *= -1;
            }

            if (feet < 0)
                return feet - inch / 12.0 - fractionalInch / 12.0;
            else
                return feet + inch / 12.0 + fractionalInch / 12.0;
        }

        /// <summary>
        /// Convert from decimal feet to feet and fractional inches
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToFeetAndFractionalInches(double decimalFeet)
        {
            decimalFeet = RoundToSignificantDigits(decimalFeet, 5);

            double wholeFeet = 0.0;
            double partialFeet = 0.0;

            if (decimalFeet < 0)
            {
                wholeFeet = Math.Ceiling(decimalFeet);
                if (wholeFeet == 0)
                    partialFeet = decimalFeet;
                else
                    partialFeet = wholeFeet - decimalFeet;
            }
            else
            {
                wholeFeet = Math.Floor(decimalFeet);
                partialFeet = decimalFeet - wholeFeet;
            }

            string fractionalInches = ToFractionalInches(Math.Round(partialFeet * 12.0,4));

            if (fractionalInches == "11 1\"" ||
                fractionalInches == "12\"")
            {
                //add a foot to the whole feet
                wholeFeet += 1.0;
                fractionalInches = "";
            }

            string feet = "";
            if (wholeFeet != 0.0)
                feet = string.Format("{0}'", wholeFeet);

            if (wholeFeet.AlmostEquals(0.0, 0.00001) && (partialFeet * 12.0).AlmostEquals(0.0,0.00001))
                feet = "0'";

            return string.Format("{0} {1}", feet, fractionalInches).Trim();
        }

        public static string ToFractionalInches(double decimalInches)
        {
            decimalInches = RoundToSignificantDigits(decimalInches, 3);

            string inches = Utils.ParseWholeInchesToString(decimalInches);
            string fraction = Utils.ParsePartialInchesToString(decimalInches, 0.015625);

            string sign = decimalInches < 0?"-":"";
            
            if(string.IsNullOrEmpty(inches) && string.IsNullOrEmpty(fraction))
                return "0\"";
            else if(string.IsNullOrEmpty(fraction))
                return string.Format("{0}{1}\"", sign, inches).Trim();
            else if(string.IsNullOrEmpty(inches))
                return string.Format("{0}{1}\"", sign, fraction).Trim();

            if (fraction == "1")
            {
                fraction = "";
                inches = (double.Parse(inches) + 1).ToString(CultureInfo.InvariantCulture);
                return string.Format("{0}{1}\"", sign, inches).Trim();
            }
            return string.Format("{0}{1} {2}\"", sign, inches, fraction).Trim();
        }
    
        public static void ParseLengthFromString(string value, out double feet, 
            out double inch, out double m, out double cm, out double mm, out double numerator, out double denominator )
        {
            string pattern = @"(((?<ft>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)('|ft))*\s*((?<in>(?<num>(\+|-)?\d+)/(?<den>\d+)*( ?)(""|in))|(?<in>(?<wholeInch>(\+|-)?\d{1,}?)(\s|-)*(?<num>(\+|-)?\d+)/(?<den>\d+)*( ?)(""|in))|(?<in>(?<wholeInch>(\+|-)?\d+([.,]\d{1,})?)( ?)(""|in)))?)*((?<m>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)m($|\s))*((?<cm>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)cm($|\s))*((?<mm>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)mm($|\s))*";

            feet = 0.0;
            inch = 0.0;
            m = 0.0;
            cm = 0.0;
            mm = 0.0;
            numerator = 0.0;
            denominator = 0.0;

            const RegexOptions opts = RegexOptions.None;
            var regex = new Regex(pattern, opts);
            Match match = regex.Match(value.Trim().ToLower());
            if (match.Success)
            {
                //parse imperial values
                double.TryParse(match.Groups["ft"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out feet);
                double.TryParse(match.Groups["wholeInch"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out inch);
                double.TryParse(match.Groups["num"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                                out numerator);
                double.TryParse(match.Groups["den"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                                out denominator);

                //parse metric values
                double.TryParse(match.Groups["m"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out m);
                double.TryParse(match.Groups["cm"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out cm);
                double.TryParse(match.Groups["mm"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out mm);
            }
        }

        public static void ParseAreaFromString(string value, out double square_inch, out double square_foot, out double square_millimeter,  out double square_centimeter, out double square_meter)
        {
            const string pattern =
                @"((?<square_inches>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(in2|sqin|in²))*\s*((?<square_feet>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(ft2|sqft|ft²))*\s*((?<square_millimeters>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(mm2|sqmm|mm²))*\s*((?<square_centimeters>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(cm2|sqcm|cm²))*\s*((?<square_meters>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(m2|sqm|m²))*\s*";
            
            square_inch = 0.0;
            square_foot = 0.0;
            square_millimeter = 0.0;
            square_centimeter = 0.0;
            square_meter = 0.0;

            const RegexOptions opts = RegexOptions.None;
            var regex = new Regex(pattern, opts);
            Match match = regex.Match(value.Trim().ToLower());
            if (match.Success)
            {
                double.TryParse(match.Groups["square_inches"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out square_inch);
                double.TryParse(match.Groups["square_feet"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out square_foot);
                double.TryParse(match.Groups["square_millimeters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                    out square_millimeter);
                double.TryParse(match.Groups["square_centimeters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                    out square_centimeter);
                double.TryParse(match.Groups["square_meters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                    out square_meter);
            }
        }

        public static void ParseVolumeFromString(string value, out double cubic_inch, out double cubic_foot, out double cubic_millimeter, out double cubic_centimeter, out double cubic_meter)
        {
            const string pattern =
                @"((?<cubic_inches>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(in3|cuin|in³))*\s*((?<cubic_feet>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(ft3|cuft|ft³))*\s*((?<cubic_millimeters>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(mm3|cumm|mm³))*\s*((?<cubic_centimeters>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(cm3|cucm|cm³))*\s*((?<cubic_meters>((\+|-)?\d{0,}([.,]\d{1,})?))( ?)(m3|cum|m³))*\s*";

            cubic_inch = 0.0;
            cubic_foot = 0.0;
            cubic_millimeter = 0.0;
            cubic_centimeter = 0.0;
            cubic_meter = 0.0;

            const RegexOptions opts = RegexOptions.None;
            var regex = new Regex(pattern, opts);
            Match match = regex.Match(value.Trim().ToLower());
            if (match.Success)
            {
                double.TryParse(match.Groups["cubic_inches"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out cubic_inch);
                double.TryParse(match.Groups["cubic_feet"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out cubic_foot);
                double.TryParse(match.Groups["cubic_millimeters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                    out cubic_millimeter);
                double.TryParse(match.Groups["cubic_centimeters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                    out cubic_centimeter);
                double.TryParse(match.Groups["cubic_meters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
                    out cubic_meter);
            }
        }
    }

    [SupressImportIntoVM]
    public class MathematicalArgumentException : Exception
    {
        public MathematicalArgumentException() : base("The result could not be computed given the provided inputs.") { }
        public MathematicalArgumentException(string message) : base(message) { }
    }

    [SupressImportIntoVM]
    public class UnitsException : MathematicalArgumentException
    {
        public UnitsException(Type a, Type b) : base(string.Format("{0} and {1} are incompatible for this operation.", a, b)) { }
    }

    [SupressImportIntoVM]
    public interface IUnitInput
    {
        double ConvertToHostUnits();
    }
}

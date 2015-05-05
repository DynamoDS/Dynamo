using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using Autodesk.DesignScript.Runtime;

namespace DynamoUnits
{
    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum LengthUnit
    {
        DecimalInch,
        FractionalInch,
        DecimalFoot,
        FractionalFoot,
        Millimeter,
        Centimeter,
        Meter
    }

    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum AreaUnit
    {
        SquareInch, 
        SquareFoot,
        SquareMillimeter,
        SquareCentimeter,
        SquareMeter
    }

    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum VolumeUnit
    {
        CubicInch,
        CubicFoot,
        CubicMillimeter,
        CubicCentimeter,
        CubicMeter
    }

    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum InsolationUnit
    {
        WattHoursPerMeterSquared,
        KilowattHoursPerMeterSquared,
        BTUPerFootSquared
    }

    [SupressImportIntoVM]
    public class BaseUnit
    {
        private static double epsilon = 1e-6;
        internal double _value;

        private static LengthUnit _hostApplicationInternalLengthUnit = DynamoUnits.LengthUnit.Meter;
        private static AreaUnit _hostApplicationInternalAreaUnit = DynamoUnits.AreaUnit.SquareMeter;
        private static VolumeUnit _hostApplicationInternalVolumeUnit = DynamoUnits.VolumeUnit.CubicMeter;

        private static string _numberFormat = "f4";

        public static double Epsilon
        {
            get { return epsilon; }
        }

        public static LengthUnit HostApplicationInternalLengthUnit
        {
            get { return _hostApplicationInternalLengthUnit; }
            set { _hostApplicationInternalLengthUnit = value; }
        }

        public static AreaUnit HostApplicationInternalAreaUnit
        {
            get { return _hostApplicationInternalAreaUnit; }
            set { _hostApplicationInternalAreaUnit = value; }
        }

        public static VolumeUnit HostApplicationInternalVolumeUnit
        {
            get { return _hostApplicationInternalVolumeUnit; }
            set { _hostApplicationInternalVolumeUnit = value; }
        }

        public static string NumberFormat
        {
            get { return _numberFormat; }
            set { _numberFormat = value; }
        }

    }

    [IsVisibleInDynamoLibrary(false)]
    public abstract class SIUnit : BaseUnit
    {
        /// <summary>
        /// The internal value of the unit.
        /// </summary>
        [Obsolete("SIUnit.Value is obsolete")]
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// The value of the unit converted into the
        /// unit type stored on the unit. Ex. If the object
        /// has LengthUnit.DecimalFoot, for a Value of 1.0, this
        /// will return 3.28084
        /// </summary>
        [IsVisibleInDynamoLibrary(false)] 
        public abstract double UnitValue { get; }

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
        [Obsolete("SIUnit.SetValueFromString is obsolete.", false)]
        public abstract void SetValueFromString(string value);

        [Obsolete("SIUnit.Add is obsolete. Please use + instead.", false)]
        public abstract SIUnit Add(SIUnit x);

        [Obsolete("SIUnit.Add is obsolete. Please use + instead.", false)]
        public abstract SIUnit Add(double x);

        [Obsolete("SIUnit.Subtract is obsolete. Please use - instead.", false)]
        public abstract SIUnit Subtract(SIUnit x);

        [Obsolete("SIUnit.Subtract is obsolete. Please use - instead.", false)]
        public abstract SIUnit Subtract(double x);

        [Obsolete("SIUnit.Multiply is obsolete. Please use * instead.", false)]
        public abstract SIUnit Multiply(SIUnit x);

        [Obsolete("SIUnit.Multiply is obsolete. Please use * instead.", false)]
        public abstract SIUnit Multiply(double x);

        [Obsolete("SIUnit.Divide is obsolete. Please use / instead.", false)]
        public abstract dynamic Divide(SIUnit x);

        [Obsolete("SIUnit.Divide is obsolete. Please use / instead.", false)]
        public abstract SIUnit Divide(double x);

        [Obsolete("SIUnit.Modulo is obsolete. Please use % instead.", false)]
        public abstract SIUnit Modulo(SIUnit x);

        [Obsolete("SIUnit.Modulo is obsolete. Please use % instead.", false)]
        public abstract SIUnit Modulo(double x);

        [Obsolete("SIUnit.Round is obsolete. Please use Round instead.", false)]
        public abstract SIUnit Round();

        [Obsolete("SIUnit.Ceiling is obsolete. Please use Ceiling instead.", false)]
        public abstract SIUnit Ceiling();

        [Obsolete("SIUnit.Floor is obsolete. Please use Floor instead.", false)]
        public abstract SIUnit Floor();

        #region operator overloads;

        [Obsolete("SIUnit.+ is obsolete. Please use + instead.", false)]
        public static SIUnit operator +(SIUnit x, SIUnit y)
        {
            return x.Add(y);
        }

        [Obsolete("SIUnit.+ is obsolete. Please use + instead.", false)]
        public static SIUnit operator +(SIUnit x, double y)
        {
            return x.Add(y);
        }

        [Obsolete("SIUnit.+ is obsolete. Please use + instead.", false)]
        public static double operator +(double x, SIUnit y)
        {
            return x + y.Value;
        }

        [Obsolete("SIUnit.- is obsolete. Please use - instead.", false)]
        public static SIUnit operator -(SIUnit x, SIUnit y)
        {
            return x.Subtract(y);
        }

        [Obsolete("SIUnit.- is obsolete. Please use - instead.", false)]
        public static SIUnit operator -(SIUnit x, double y)
        {
            return x.Subtract(y);
        }

        [Obsolete("SIUnit.- is obsolete. Please use - instead.", false)]
        public static double operator -(double x, SIUnit y)
        {
            return x - y.Value;
        }

        [Obsolete("SIUnit.* is obsolete. Please use * instead.", false)]
        public static SIUnit operator *(SIUnit x, SIUnit y)
        {
            return x.Multiply(y);
        }

        [Obsolete("SIUnit.* is obsolete. Please use * instead.", false)]
        public static SIUnit operator *(SIUnit x, double y)
        {
            return x.Multiply(y);
        }

        [Obsolete("SIUnit.* is obsolete. Please use * instead.", false)]
        public static SIUnit operator *(double x, SIUnit y)
        {
            return y.Multiply(x);
        }

        [Obsolete("SIUnit./ is obsolete. Please use / instead.", false)]
        public static dynamic operator /(SIUnit x, SIUnit y)
        {
            //units will cancel
            if (x.GetType() == y.GetType())
            {
                return x.Value / y.Value;
            }

            return x.Divide(y);
        }

        [Obsolete("SIUnit./ is obsolete. Please use / instead.", false)]
        public static SIUnit operator /(SIUnit x, double y)
        {
            return x.Divide(y);
        }

        [Obsolete("SIUnit.% is obsolete. Please use % instead.", false)]
        public static SIUnit operator %(SIUnit x, SIUnit y)
        {
            return x.Modulo(y);
        }

        [Obsolete("SIUnit.% is obsolete. Please use % instead.", false)]
        public static SIUnit operator %(SIUnit x, double y)
        {
            return x.Modulo(y);
        }

        [Obsolete("SIUnit.% is obsolete. Please use % instead.", false)]
        public static double operator %(double x, SIUnit y)
        {
            return x % y.Value;
        }

        [Obsolete("SIUnit.> is obsolete. Please use > instead.", false)]
        public static bool operator >(double x, SIUnit y)
        {
            return x > y.Value;
        }

        [Obsolete("SIUnit.> is obsolete. Please use > instead.", false)]
        public static bool operator >(SIUnit x, double y)
        {
            return x.Value > y;
        }

        [Obsolete("SIUnit.> is obsolete. Please use > instead.", false)]
        public static bool operator >(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value > y.Value;
        }

        [Obsolete("SIUnit.< is obsolete. Please use < instead.", false)]
        public static bool operator <(double x, SIUnit y)
        {
            return x < y.Value;
        }

        [Obsolete("SIUnit.< is obsolete. Please use < instead.", false)]
        public static bool operator <(SIUnit x, double y)
        {
            return x.Value < y;
        }

        [Obsolete("SIUnit.< is obsolete. Please use < instead.", false)]
        public static bool operator <(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value < y.Value;
        }

        [Obsolete("SIUnit.>= is obsolete. Please use >= instead.", false)]
        public static bool operator >=(double x, SIUnit y)
        {
            return x >= y.Value;
        }

        [Obsolete("SIUnit.>= is obsolete. Please use >= instead.", false)]
        public static bool operator >=(SIUnit x, double y)
        {
            return x.Value >= y;
        }

        [Obsolete("SIUnit.>= is obsolete. Please use >= instead.", false)]
        public static bool operator >=(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value >= y.Value;
        }

        [Obsolete("SIUnit.<= is obsolete. Please use <= instead.", false)]
        public static bool operator <=(double x, SIUnit y)
        {
            return x <= y.Value;
        }

        [Obsolete("SIUnit.<= is obsolete. Please use <= instead.", false)]
        public static bool operator <=(SIUnit x, double y)
        {
            return x.Value <= y;
        }

        [Obsolete("SIUnit.<= is obsolete. Please use <= instead.", false)]
        public static bool operator <=(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value <= y.Value;
        }

        [Obsolete("SIUnit.ToSIUnit is obsolete.", false)]
        public static SIUnit ToSIUnit(object value)
        {
            return value as SIUnit;
        }

        #endregion

        public static Dictionary<string, double> Conversions {
            get
            {
                return new Dictionary<string,double>();
            }
        }

        [Obsolete("SIUnit.ConvertToHostUnits is obsolete. Please use Convert Between Units.", false)]
        public abstract double ConvertToHostUnits();
    }

    /// <summary>
    /// A length stored in meters. This length can represent any unit type, but internally this 
    /// is stored as meters to make algorithms simpler.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class Length : SIUnit, IComparable, IEquatable<Length>
    {
        //length conversions
        private const double METER_TO_MILLIMETER = 1000;
        private const double METER_TO_CENTIMETER = 100;
        private const double METER_TO_INCH = 39.37007874;
        private const double METER_TO_FOOT = 3.280839895;

        public const string METERS = "m";
        public const string MILLIMETERS = "mm";
        public const string CENTIMETERS = "cm";
        public const string INCHES = "in";
        public const string FEET = "ft";

        private LengthUnit _lengthUnit = LengthUnit.Meter;

        [IsVisibleInDynamoLibrary(false)]
        public LengthUnit LengthUnit
        {
            get { return _lengthUnit; }
            set
            {
                _lengthUnit = value;
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public double UiLengthConversion
        {
            get
            {
                switch (_lengthUnit)
                {
                    case LengthUnit.Millimeter:
                        return Length.ToMillimeter;
                    case LengthUnit.Centimeter:
                        return Length.ToCentimeter;
                    case LengthUnit.Meter:
                        return 1.0;
                    case LengthUnit.DecimalInch:
                        return Length.ToInch;
                    case LengthUnit.FractionalInch:
                        return Length.ToInch;
                    case LengthUnit.DecimalFoot:
                        return Length.ToFoot;
                    case LengthUnit.FractionalFoot:
                        return Length.ToFoot;
                    default:
                        return 1.0;
                }
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public override double UnitValue
        {
            get { return _value * UiLengthConversion; }
        }

        [Obsolete("Area.ToMillimeter is obsolete. Please use Convert Units.", false)]
        public static double ToMillimeter
        {
            get { return METER_TO_MILLIMETER; }
        }

        [Obsolete("Area.ToCentimeter is obsolete. Please use Convert Units.", false)]
        public static double ToCentimeter
        {
            get { return METER_TO_CENTIMETER; }
        }

        [Obsolete("Area.ToMeter is obsolete. Please use Convert Units.", false)]
        public static double ToMeter
        {
            get { return 1.0; }
        }

        [Obsolete("Area.ToInch is obsolete. Please use Convert Units.", false)]
        public static double ToInch
        {
            get { return METER_TO_INCH; }
        }

        [Obsolete("Area.ToFoot is obsolete. Please use Convert Units.", false)]
        public static double ToFoot
        {
            get { return METER_TO_FOOT; }
        }

        internal Length(double value):base(value){}

        internal Length(double value, LengthUnit unit) : base(value)
        {
            LengthUnit = unit;
        }

        [Obsolete("Length.FromDouble is obsolete. Please pass number values directly.", false)]
        public static Length FromDouble(double value)
        {
            return new Length(value);
        }

        public static Length FromDouble(double value, LengthUnit unit)
        {
            return new Length(value, unit);
        }

        [Obsolete("Length.From feet is obsolete. Please pass number values directly.", false)]
        public static Length FromFeet(double value)
        {
            return new Length(value/ToFoot);
        }

        public static Length FromFeet(double value, LengthUnit unit)
        {
            return new Length(value/ToFoot, unit);
        }

        [IsVisibleInDynamoLibrary(false)]
        public new static Dictionary<string,double> Conversions
        {
            get
            {
                var dict = new Dictionary<string, double>
                {
                    { METERS, 1.0 },
                    { MILLIMETERS, METER_TO_MILLIMETER },
                    { CENTIMETERS, METER_TO_CENTIMETER },
                    { INCHES, METER_TO_INCH },
                    { FEET, METER_TO_FOOT }
                };
                return dict;
            } 
        }

        #region math

        [Obsolete("Length.Add is obsolete. Please use + instead.", false)]
        public override SIUnit Add(SIUnit x)
        {
            if (x is Length)
                return new Length(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Length.Add is obsolete. Please use + instead.", false)]
        public override SIUnit Add(double x)
        {
            return new Length(_value + x);
        }

        [Obsolete("Length.Subtract is obsolete. Please use - instead.", false)]
        public override SIUnit Subtract(SIUnit x)
        {
            if (x is Length)
                return new Length(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Length.Subtract is obsolete. Please use - instead.", false)]
        public override SIUnit Subtract(double x)
        {
            return new Length(_value - x);
        }

        [Obsolete("Length.Multiply is obsolete. Please use * instead.", false)]
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

        [Obsolete("Length.Multiply is obsolete. Please use * instead.", false)]
        public override SIUnit Multiply(double x)
        {
            return new Length(_value * x);
        }

        [Obsolete("Length.Divide is obsolete. Please use / instead.", false)]
        public override dynamic Divide(SIUnit x)
        {
            if (x is Length)
            {
                return _value / x.Value;
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Length.Divide is obsolete. Please use / instead.", false)]
        public override SIUnit Divide(double x)
        {
            return new Length(_value / x);
        }

        [Obsolete("Length.Modulo is obsolete. Please use % instead.", false)]
        public override SIUnit Modulo(SIUnit x)
        {
            if (x is Length)
                return new Length(_value % x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Length.Modulo is obsolete. Please use % instead.", false)]
        public override SIUnit Modulo(double x)
        {
            return new Length(_value % x);
        }

        [Obsolete("Length.Round is obsolete. Please use Round instead.", false)]
        public override SIUnit Round()
        {
            var val = _value * UiLengthConversion;
            var round = Math.Round(val);
            var length = new Length(round/UiLengthConversion) { LengthUnit = LengthUnit };
            return length;
        }

        [Obsolete("Length.Ceiling is obsolete. Please use Ceiling instead.", false)]
        public override SIUnit Ceiling()
        {
            var val = _value * UiLengthConversion;
            var round = Math.Ceiling(val);
            var length = new Length(round/UiLengthConversion) { LengthUnit = LengthUnit };
            return length;
        }

        [Obsolete("Length.Floor is obsolete. Please use Floor instead.", false)]
        public override SIUnit Floor()
        {
            var val = _value * UiLengthConversion;
            var round = Math.Floor(val);
            var length = new Length(round / UiLengthConversion) { LengthUnit = LengthUnit };
            return length;
        }

        [Obsolete("Length.ConvertToHostUnits is obsolete. Please use Convert Between Units.", false)]
        public override double ConvertToHostUnits()
        {
            switch (HostApplicationInternalLengthUnit)
            {
                case LengthUnit.DecimalFoot:
                    return _value * ToFoot;
                default:
                    return _value;
            }
        }

        #endregion

        #region string

        [Obsolete("Length.SetValueFromString is obsolete.", false)]
        public override void SetValueFromString(string value)
        {
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (Double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out total))
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

            if (Math.Abs(other.Value - _value) < Epsilon)
                return true;

            return false;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            return BuildString(LengthUnit);
        }

        private string BuildString(LengthUnit unit)
        {
            switch (unit)
            {
                case LengthUnit.Millimeter:
                    return (_value * ToMillimeter).ToString(NumberFormat, CultureInfo.InvariantCulture) + MILLIMETERS;

                case LengthUnit.Centimeter:
                    return (_value * ToCentimeter).ToString(NumberFormat, CultureInfo.InvariantCulture) + CENTIMETERS;

                case LengthUnit.Meter:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + METERS;

                case LengthUnit.DecimalInch:
                    return (_value * ToInch).ToString(NumberFormat, CultureInfo.InvariantCulture) + INCHES;

                case LengthUnit.FractionalInch:
                    return Utils.ToFractionalInches(_value * ToInch);

                case LengthUnit.DecimalFoot:
                    return (_value * ToFoot).ToString(NumberFormat, CultureInfo.InvariantCulture) + FEET;

                case LengthUnit.FractionalFoot:
                    return Utils.ToFeetAndFractionalInches(_value * ToFoot);

                default:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + METERS;
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
            return Math.Abs(length.Value - _value) < Epsilon;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override int GetHashCode()
        {
            var volumeHashCode = Convert.ToInt32(_value);

            return volumeHashCode;
        }
    }

    /// <summary>
    /// An area stored in square meters.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class Area : SIUnit, IComparable, IEquatable<Area>
    {
        //area conversions
        private const double SQUARE_METERS_TO_SQUARE_MILLIMETERS = 1000000;
        private const double SQUARE_METERS_TO_SQUARE_CENTIMETERS = 10000;
        private const double SQUARE_METERS_TO_SQUARE_INCH = 1550.0031;
        private const double SQUARE_METERS_TO_SQUARE_FOOT = 10.763910417;
        
        public const string SQUARE_METERS = "m²";
        public const string SQUARE_MILLIMETERS = "mm²";
        public const string SQUARE_CENTIMETERS = "cm²";
        public const string SQUARE_INCHES = "in²";
        public const string SQUARE_FEET = "ft²";

        private AreaUnit _areaUnit = AreaUnit.SquareMeter;

        [IsVisibleInDynamoLibrary(false)]
        public AreaUnit AreaUnit
        {
            get { return _areaUnit; }
            set
            {
                _areaUnit = value;
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public double UiAreaConversion
        {
            get
            {
                switch (_areaUnit)
                {
                    case AreaUnit.SquareMillimeter:
                        return Area.ToSquareMillimeters;
                    case AreaUnit.SquareCentimeter:
                        return Area.ToSquareCentimeters;
                    case AreaUnit.SquareMeter:
                        return 1.0;
                    case AreaUnit.SquareInch:
                        return Area.ToSquareInch;
                    case AreaUnit.SquareFoot:
                        return Area.ToSquareFoot;
                    default:
                        return 1.0;
                }
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public override double UnitValue
        {
            get { return _value * UiAreaConversion; }
        }

        [Obsolete("Area.ToSquareMillimeters is obsolete. Please use Convert Units.", false)]
        public static double ToSquareMillimeters
        {
            get { return SQUARE_METERS_TO_SQUARE_MILLIMETERS; }
        }

        [Obsolete("Area.ToSquareFoot is obsolete. Please use Convert Units.", false)]
        public static double ToSquareCentimeters
        {
            get { return SQUARE_METERS_TO_SQUARE_CENTIMETERS; }
        }

        [Obsolete("Area.ToSquareInch is obsolete. Please use Convert Units.", false)]
        public static double ToSquareInch
        {
            get { return SQUARE_METERS_TO_SQUARE_INCH; }
        }

        [Obsolete("Area.ToSquareFoot is obsolete. Please use Convert Units.", false)]
        public static double ToSquareFoot
        {
            get { return SQUARE_METERS_TO_SQUARE_FOOT; }
        }

        internal Area(double value):base(value){}

        internal Area(double value, AreaUnit unit)
            : base(value)
        {
            AreaUnit = unit;
        }

        [Obsolete("Area.FromDouble is obsolete. Please use Number.", false)]
        public static Area FromDouble(double value)
        {
            return new Area(value);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Area FromDouble(double value, AreaUnit unit)
        {
            return new Area(value, unit);
        }

        [Obsolete("Area.FromSquareFeet is obsolete. Please use Number.", false)]
        public static Area FromSquareFeet(double value)
        {
            return new Area(value / ToSquareFoot);
        }

        #region math

        [Obsolete("Area.Add is obsolete. Please use + instead.", false)]
        public override SIUnit Add(SIUnit x)
        {
            if (x is Area)
                return new Area(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Area.Add is obsolete. Please use + instead.", false)]
        public override SIUnit Add(double x)
        {
            return new Area(_value + x);
        }

        [Obsolete("Area.Subtract is obsolete. Please use + instead.", false)]
        public override SIUnit Subtract(SIUnit x)
        {
            if (x is Area)
                return new Area(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Area.Subtract is obsolete. Please use + instead.", false)]
        public override SIUnit Subtract(double x)
        {
            return new Area(_value - x);
        }

        [Obsolete("Area.Multiply is obsolete. Please use * instead.", false)]
        public override SIUnit Multiply(SIUnit x)
        {
            if (x is Length)
            {
                //return a volume
                return new Volume(_value * x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Area.Multiply is obsolete. Please use * instead.", false)]
        public override SIUnit Multiply(double x)
        {
            return new Area(_value * x);
        }

        [Obsolete("Area.Divide is obsolete. Please use / instead.", false)]
        public override dynamic Divide(SIUnit x)
        {
            if (x is Area)
            {
                //return a double
                return _value / x.Value;
            }

            if (x is Length)
            {
                //return length
                return new Length(_value / x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Area.Divide is obsolete. Please use / instead.", false)]
        public override SIUnit Divide(double x)
        {
            return new Area(_value / x);
        }

        [Obsolete("Area.Modulo is obsolete. Please use % instead.", false)]
        public override SIUnit Modulo(SIUnit x)
        {
            if (x is Area)
            {
                return new Area(_value % x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Area.Modulo is obsolete. Please use % instead.", false)]
        public override SIUnit Modulo(double x)
        {
            return new Area(_value % x);
        }

        [Obsolete("Area.Round is obsolete. Please use Round instead.", false)]
        public override SIUnit Round()
        {
            var val = _value * UiAreaConversion;
            double round = Math.Round(val);
            return new Area(round / UiAreaConversion);
        }

        [Obsolete("Area.Ceiling is obsolete. Please use Ceiling instead.", false)]
        public override SIUnit Ceiling()
        {
            var val = _value * UiAreaConversion;
            double round = Math.Ceiling(val);
            return new Area(round / UiAreaConversion);
        }

        [Obsolete("Area.Floor is obsolete. Please use Floor instead.", false)]
        public override SIUnit Floor()
        {
            var val = _value * UiAreaConversion;
            double round = Math.Floor(val);
            return new Area(round / UiAreaConversion);
        }

        public new static Dictionary<string, double> Conversions
        {
            get
            {
                var dict = new Dictionary<string, double>
                {
                    { SQUARE_METERS, 1.0 },
                    { SQUARE_MILLIMETERS, SQUARE_METERS_TO_SQUARE_MILLIMETERS },
                    { SQUARE_CENTIMETERS, SQUARE_METERS_TO_SQUARE_CENTIMETERS },
                    { SQUARE_INCHES, SQUARE_METERS_TO_SQUARE_INCH },
                    { SQUARE_FEET, SQUARE_METERS_TO_SQUARE_FOOT }
                };
                return dict;
            }
        }

        [Obsolete("Area.ConvertToHostUnits is obsolete. Please use Convert Between Units.", false)]
        public override double ConvertToHostUnits()
        {
            switch (HostApplicationInternalAreaUnit)
            {
                case AreaUnit.SquareFoot:
                    return _value/ToSquareFoot;
                default:
                    return _value;
            }
        }

        #endregion

        #region string

        [Obsolete("Area.SetValueFromString is obsolete.", false)]
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

            total += sq_mm / ToSquareMillimeters;
            total += sq_cm / ToSquareCentimeters;
            total += sq_m;
            total += sq_in / ToSquareInch;
            total += sq_ft / ToSquareFoot;

            _value = total < 0 ? 0.0 : total;
        }

        [IsVisibleInDynamoLibrary(false)]
        public bool Equals(Area other)
        {
            if (other == null)
                return false;

            if (Math.Abs(other.Value - _value) < Epsilon)
                return true;

            return false;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            return BuildString(AreaUnit);
        }

        private string BuildString(AreaUnit unit)
        {
            switch (unit)
            {
                case AreaUnit.SquareMillimeter:
                    return (_value*ToSquareMillimeters).ToString(NumberFormat, CultureInfo.InvariantCulture) + SQUARE_MILLIMETERS;

                case AreaUnit.SquareCentimeter:
                    return (_value*ToSquareCentimeters).ToString(NumberFormat, CultureInfo.InvariantCulture) + SQUARE_CENTIMETERS;

                case AreaUnit.SquareMeter:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + SQUARE_METERS;

                case AreaUnit.SquareInch:
                    return (_value * ToSquareInch).ToString(NumberFormat, CultureInfo.InvariantCulture) + SQUARE_INCHES;

                case AreaUnit.SquareFoot:
                    return (_value * ToSquareFoot).ToString(NumberFormat, CultureInfo.InvariantCulture) + SQUARE_FEET;

                default:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + SQUARE_METERS;
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
            return Math.Abs(area.Value - _value) < Epsilon;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override int GetHashCode()
        {
            var volumeHashCode = Convert.ToInt32(_value);

            return volumeHashCode;
        }
    }

    /// <summary>
    /// A volume stored in cubic meters.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class Volume : SIUnit, IComparable, IEquatable<Volume>
    {
        //volume conversions
        private const double CUBIC_METERS_TO_CUBIC_MILLIMETERS = 1000000000;
        private const double CUBIC_METERS_TO_CUBIC_CENTIMETERS = 1000000;
        private const double CUBIC_METERS_TO_CUBIC_INCHES = 61023.744095;
        private const double CUBIC_METERS_TO_CUBIC_FEET = 35.3147;

        public const string CUBIC_METERS = "m³";
        public const string CUBIC_MILLIMETERS = "mm³";
        public const string CUBIC_CENTIMETERS = "cm³";
        public const string CUBIC_INCHES = "in³";
        public const string CUBIC_FEET = "ft³";

        private VolumeUnit _volumeUnit = VolumeUnit.CubicMeter;

        [IsVisibleInDynamoLibrary(false)]
        public VolumeUnit VolumeUnit
        {
            get { return _volumeUnit; }
            set
            {
                _volumeUnit = value;
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public double UiVolumeConversion
        {
            get
            {
                switch (_volumeUnit)
                {
                    case VolumeUnit.CubicMillimeter:
                        return Volume.ToCubicMillimeter;
                    case VolumeUnit.CubicCentimeter:
                        return Volume.ToCubicCentimeter;
                    case VolumeUnit.CubicMeter:
                        return 1.0;
                    case VolumeUnit.CubicInch:
                        return Volume.ToCubicInch;
                    case VolumeUnit.CubicFoot:
                        return Volume.ToCubicFoot;
                    default:
                        return 1.0;
                }
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public override double UnitValue
        {
            get { return _value * UiVolumeConversion; }
        }

        [Obsolete("Volume.ToCubicMillimeter is obsolete. Please use Convert Units.", false)]
        public static double ToCubicMillimeter
        {
            get { return CUBIC_METERS_TO_CUBIC_MILLIMETERS; }
        }

        [Obsolete("Volume.ToCubicCentimeter is obsolete. Please use Convert Units.", false)]
        public static double ToCubicCentimeter
        {
            get { return CUBIC_METERS_TO_CUBIC_CENTIMETERS; }
        }

        [Obsolete("Volume.ToCubicInch is obsolete. Please use Convert Units.", false)]
        public static double ToCubicInch
        {
            get { return CUBIC_METERS_TO_CUBIC_INCHES; }
        }

        [Obsolete("Volume.ToCubicFoot is obsolete. Please use Convert Units.", false)]
        public static double ToCubicFoot
        {
            get { return CUBIC_METERS_TO_CUBIC_FEET; }
        }

        internal Volume(double value) : base(value){}

        internal Volume(double value, VolumeUnit unit) : base(value)
        {
            VolumeUnit = unit;
        }

        [Obsolete("Volume.FromDouble is obsolete. Please use Number.", false)]
        public static Volume FromDouble(double value)
        {
            return new Volume(value);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Volume FromDouble(double value, VolumeUnit unit)
        {
            return new Volume(value, unit);
        }

        [Obsolete("Volume.FromCubicFeet is obsolete. Please use Number.", false)]
        public static Volume FromCubicFeet(double value)
        {
            return new Volume(value / ToCubicFoot);
        }

        #region math

        [Obsolete("Volume.Add is obsolete. Please use + instead.", false)]
        public override SIUnit Add(SIUnit x)
        {
            if (x is Volume)
                return new Volume(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Volume.Add is obsolete. Please use + instead.", false)]
        public override SIUnit Add(double x)
        {
            return new Volume(_value + x);
        }

        [Obsolete("Volume.Subtract is obsolete. Please use - instead.", false)]
        public override SIUnit Subtract(SIUnit x)
        {
            if (x is Volume)
                return new Volume(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Volume.Subtract is obsolete. Please use - instead.", false)]
        public override SIUnit Subtract(double x)
        {
            return new Volume(_value - x);
        }

        [Obsolete("Volume.Multiply is obsolete. Please use * instead.", false)]
        public override SIUnit Multiply(SIUnit x)
        {
            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Volume.Multiply is obsolete. Please use * instead.", false)]
        public override SIUnit Multiply(double x)
        {
            return new Volume(_value * x);
        }

        [Obsolete("Volume.Multiply is obsolete. Please use / instead.", false)]
        public override dynamic Divide(SIUnit x)
        {
            if (x is Length)
            {
                return new Area(_value / x.Value);
            }
            else if (x is Area)
            {
                return new Length(_value / x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Volume.Multiply is obsolete. Please use / instead.", false)]
        public override SIUnit Divide(double x)
        {
            return new Volume(_value / x);
        }

        [Obsolete("Volume.Multiply is obsolete. Please use % instead.", false)]
        public override SIUnit Modulo(SIUnit x)
        {
            if (x is Volume)
            {
                return new Volume(_value % x.Value);
            }

            throw new UnitsException(GetType(), x.GetType());
        }

        [Obsolete("Volume.Multiply is obsolete. Please use % instead.", false)]
        public override SIUnit Modulo(double x)
        {
            return new Volume(_value % x);
        }

        [Obsolete("Volume.Multiply is obsolete. Please use Round instead.", false)]
        public override SIUnit Round()
        {
            var val = _value * UiVolumeConversion;
            double round = Math.Round(val);
            return new Volume(round / UiVolumeConversion);
        }

        [Obsolete("Volume.Multiply is obsolete. Please use Ceiling instead.", false)]
        public override SIUnit Ceiling()
        {
            var val = _value * UiVolumeConversion;
            double round = Math.Ceiling(val);
            return new Volume(round / UiVolumeConversion);
        }

        [Obsolete("Volume.Multiply is obsolete. Please use Floor instead.", false)]
        public override SIUnit Floor()
        {
            var val = _value * UiVolumeConversion;
            double round = Math.Floor(val);
            return new Volume(round / UiVolumeConversion);
        }

        public new static Dictionary<string, double> Conversions
        {
            get
            {
                var dict = new Dictionary<string, double>
                {
                    { CUBIC_METERS, 1.0 },
                    { CUBIC_MILLIMETERS, CUBIC_METERS_TO_CUBIC_MILLIMETERS },
                    { CUBIC_CENTIMETERS, CUBIC_METERS_TO_CUBIC_CENTIMETERS },
                    { CUBIC_INCHES, CUBIC_METERS_TO_CUBIC_INCHES },
                    { CUBIC_FEET, CUBIC_METERS_TO_CUBIC_FEET }
                };
                return dict;
            }
        }

        [Obsolete("Volume.ConvertToHostUnits is obsolete. Please use Convert Between Units.", false)]
        public override double ConvertToHostUnits()
        {
            switch (VolumeUnit)
            {
                case VolumeUnit.CubicFoot:
                    return _value/ToCubicFoot;
                default:
                    return _value;
            }
        }

        #endregion

        #region string

        [Obsolete("Volume.SetValueFromString is obsolete.", false)]
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

            if (Math.Abs(other.Value - _value) < Epsilon)
                return true;

            return false;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            return BuildString(VolumeUnit);
        }

        private string BuildString(VolumeUnit unit)
        {
            switch (unit)
            {
                case VolumeUnit.CubicMillimeter:
                    return (_value * ToCubicMillimeter).ToString(NumberFormat, CultureInfo.InvariantCulture) + CUBIC_MILLIMETERS;

                case VolumeUnit.CubicCentimeter:
                    return (_value * ToCubicCentimeter).ToString(NumberFormat, CultureInfo.InvariantCulture) + CUBIC_CENTIMETERS;

                case VolumeUnit.CubicMeter:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + CUBIC_METERS;

                case VolumeUnit.CubicInch:
                    return (_value * ToCubicInch).ToString(NumberFormat, CultureInfo.InvariantCulture) + CUBIC_INCHES;

                case VolumeUnit.CubicFoot:
                    return (_value * ToCubicFoot).ToString(NumberFormat, CultureInfo.InvariantCulture) + CUBIC_FEET;

                default:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + CUBIC_METERS;
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
            return Math.Abs(volume.Value - _value) < Epsilon;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override int GetHashCode()
        {
            var volumeHashCode = Convert.ToInt32(_value);

            return volumeHashCode;
        }
    }

    /// <summary>
    /// An insolation stored in killowatt hours per meter squared.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class Insolation : SIUnit, IComparable, IEquatable<Insolation>
    {
        //insolation converstions
        private const double WHM2_TO_KWHM2 = 0.001;
        private const double WHM2_TO_BT_UFT2 = 0.3170;

        public const string WATT_HOURS_PER_SQUARE_METER = "Wh/m²";
        public const string KILLOWATT_HOURS_PER_SQUARE_METER = "kWh/m²";
        public const string BTU_PER_SQUARE_FOOT = "BTU/ft²";

        private InsolationUnit _insolationUnit = InsolationUnit.WattHoursPerMeterSquared;

        [IsVisibleInDynamoLibrary(false)]
        public InsolationUnit InsolationUnit
        {
            get { return _insolationUnit; }
            set
            {
                _insolationUnit = value;
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public double UiInsolationConversion
        {
            get
            {
                switch (_insolationUnit)
                {
                    case InsolationUnit.WattHoursPerMeterSquared:
                        return 1.0;
                    case InsolationUnit.KilowattHoursPerMeterSquared:
                        return Insolation.ToKwhMeter2;
                    case InsolationUnit.BTUPerFootSquared:
                        return Insolation.ToBTUFoot2;
                    default:
                        return 1.0;
                }
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public override double UnitValue
        {
            get { return _value * UiInsolationConversion; }
        }

        public static double ToKwhMeter2
        {
            get { return WHM2_TO_KWHM2; }
        }

        public static double ToBTUFoot2
        {
            get { return WHM2_TO_BT_UFT2; }
        }

        public static Insolation FromDouble(double value)
        {
            return new Insolation(value);
        }

        internal Insolation(double value) : base(value) { }

        public override void SetValueFromString(string value)
        {
            throw new NotImplementedException();
        }

        public override SIUnit Add(SIUnit x)
        {
            if (x is Insolation)
                return new Insolation(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Add(double x)
        {
            return new Insolation(_value + x);
        }

        public override SIUnit Subtract(SIUnit x)
        {
            if (x is Insolation)
                return new Insolation(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Subtract(double x)
        {
            return new Insolation(_value - x);
        }

        public override SIUnit Multiply(SIUnit x)
        {
            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Multiply(double x)
        {
            return new Insolation(_value * x);
        }

        public override dynamic Divide(SIUnit x)
        {
            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Divide(double x)
        {
            return new Insolation(_value / x);
        }

        public override SIUnit Modulo(SIUnit x)
        {
            if (x is Insolation)
                return new Insolation(_value % x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Modulo(double x)
        {
            return new Insolation(_value % x);
        }

        public override SIUnit Round()
        {
            return new Insolation(Math.Round(_value));
        }

        public override SIUnit Ceiling()
        {
            return new Insolation(Math.Ceiling(_value));
        }

        public override SIUnit Floor()
        {
            return new Insolation(Math.Floor(_value));
        }

        public new static Dictionary<string, double> Conversions
        {
            get
            {
                var dict = new Dictionary<string, double>
                {
                    { WATT_HOURS_PER_SQUARE_METER, 1.0 },
                    { KILLOWATT_HOURS_PER_SQUARE_METER, WHM2_TO_KWHM2 },
                    { BTU_PER_SQUARE_FOOT, WHM2_TO_BT_UFT2 },
                };
                return dict;
            }
        }

        public override double ConvertToHostUnits()
        {
            return Value;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherInsol = obj as Insolation;
            if (otherInsol != null)
                return _value.CompareTo(otherInsol.Value);
            else
                throw new ArgumentException("Object is not an Insolation.");
        }

        public bool Equals(Insolation other)
        {
            if (other == null)
            {
                return false;
            }

            var insol = other;

            return Math.Abs(insol.Value - _value) < Epsilon;
        }

        [IsVisibleInDynamoLibrary(false)]
        public override string ToString()
        {
            switch (InsolationUnit)
            {
                case InsolationUnit.WattHoursPerMeterSquared:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + WATT_HOURS_PER_SQUARE_METER;

                case InsolationUnit.KilowattHoursPerMeterSquared:
                    return (_value * ToKwhMeter2).ToString(NumberFormat, CultureInfo.InvariantCulture) + KILLOWATT_HOURS_PER_SQUARE_METER;

                case InsolationUnit.BTUPerFootSquared:
                    return (_value * ToBTUFoot2).ToString(NumberFormat, CultureInfo.InvariantCulture) + BTU_PER_SQUARE_FOOT;
                default:
                    return _value.ToString(NumberFormat, CultureInfo.InvariantCulture) + WATT_HOURS_PER_SQUARE_METER;
            }
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

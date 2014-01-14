using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Dynamo.Utilities;
using Double = System.Double;

namespace Dynamo.Measure
{
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

    public enum DynamoAreaUnit
    {
        SquareInch, 
        SquareFoot,
        SquareMillimeter,
        SquareCentimeter,
        SquareMeter
    }

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
        internal double _value;

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
        public abstract SIUnit Subtract(SIUnit x);
        public abstract SIUnit Multiply(SIUnit x);
        public abstract SIUnit Multiply(double x);
        public abstract dynamic Divide(SIUnit x);
        public abstract SIUnit Divide(double x);
        public abstract SIUnit Modulo(SIUnit x);

        #region operator overloads

        public static SIUnit operator +(SIUnit x, SIUnit y)
        {
            return x.Add(y);
        }

        public static SIUnit operator -(SIUnit x, SIUnit y)
        {
            return x.Subtract(y);
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
            else
            {
                return x.Divide(y);
            }
        }

        public static SIUnit operator /(SIUnit x, double y)
        {
            return x.Divide(y);
        }

        public static SIUnit operator %(SIUnit x, SIUnit y)
        {
            return x.Modulo(y);
        }

        #endregion

        public static SIUnit UnwrapFromValue(FScheme.Value value)
        {
            if (value.IsContainer)
            {
                var measure = ((FScheme.Value.Container)value).Item as SIUnit;
                if (measure != null)
                {
                    return measure;
                }
            }

            throw new Exception("SIUnit could not be unwrapped from value.");
        }
    }

    /// <summary>
    /// A length stored as meters.
    /// </summary>
    public class Length : SIUnit
    {
        private const double meter_to_millimeter = 1000;
        private const double meter_to_centimeter = 100;
        private const double meter_to_inch = 39.3701;
        private const double meter_to_foot = 3.28084;

        public Length(double value):base(value){}

        #region math

        public override SIUnit Add(SIUnit x)
        {
            if(x is Length)
                return new Length(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Subtract(SIUnit x)
        {
            if(x is Length)
                return new Length(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Multiply(SIUnit x)
        {
            if (x is Length)
            {
                return new Area(_value * x.Value);
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

        #endregion

        #region string

        public override void SetValueFromString(string value)
        {
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (double.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out total))
            {
                switch (dynSettings.Controller.PreferenceSettings.LengthUnit)
                {
                    case DynamoLengthUnit.Centimeter:
                        _value = total / meter_to_centimeter;
                        return;

                    case DynamoLengthUnit.Millimeter:
                        _value = total / meter_to_millimeter;
                        return;

                    case DynamoLengthUnit.Meter:
                        _value = total;
                        return;

                    case DynamoLengthUnit.FractionalInch:
                        _value = total / meter_to_inch;
                        return;

                    case DynamoLengthUnit.FractionalFoot:
                        _value = total / meter_to_foot;
                        return;

                    case DynamoLengthUnit.DecimalInch:
                        _value = total / meter_to_inch;
                        return;

                    case DynamoLengthUnit.DecimalFoot:
                        _value = total / meter_to_foot;
                        return;
                }
            }

            double fractionalInch = 0.0;
            double feet, inch, m, cm, mm, numerator, denominator;
            Utils.ParseLengthFromString(value, out feet, out inch, out m, out cm, out mm, out numerator, out denominator);

            if (denominator != 0)
                fractionalInch = numerator / denominator;

            if (feet < 0)
                total = (feet - inch / 12.0 - fractionalInch / 12.0) / meter_to_foot;
            else
                total = (feet + inch / 12.0 + fractionalInch / 12.0) / meter_to_foot;

            total += m;
            total += cm / meter_to_centimeter;
            total += mm / meter_to_millimeter;

            _value = total;
        }

        public override string ToString()
        {
            switch (dynSettings.Controller.PreferenceSettings.LengthUnit)
            {
                case DynamoLengthUnit.Millimeter:
                    return ToMillimeterString();
                case DynamoLengthUnit.Centimeter:
                    return ToCentimeterString();
                case DynamoLengthUnit.Meter:
                    return ToMeterString();
                case DynamoLengthUnit.DecimalInch:
                    return ToDecimalInchString();
                case DynamoLengthUnit.FractionalInch:
                    return ToFractionalInchString();
                case DynamoLengthUnit.DecimalFoot:
                    return ToDecimalFootString();
                case DynamoLengthUnit.FractionalFoot:
                    return ToFractionalFootString();
                default:
                    return ToMeterString();
            }
        }

        internal string ToString(DynamoLengthUnit lengthUnit)
        {
            switch (lengthUnit)
            {
                case DynamoLengthUnit.Millimeter:
                    return ToMillimeterString();
                case DynamoLengthUnit.Centimeter:
                    return ToCentimeterString();
                case DynamoLengthUnit.Meter:
                    return ToMeterString();
                case DynamoLengthUnit.DecimalInch:
                    return ToDecimalInchString();
                case DynamoLengthUnit.FractionalInch:
                    return ToFractionalInchString();
                case DynamoLengthUnit.DecimalFoot:
                    return ToDecimalFootString();
                case DynamoLengthUnit.FractionalFoot:
                    return ToFractionalFootString();
                default:
                    return ToMeterString();
            }
        }

        #endregion

        #region conversion

        internal double ToMillimeters()
        {
            return _value * 1000;
        }

        internal double ToCentimeters()
        {
            return _value * 100;
        }

        internal double ToMeters()
        {
            return _value;
        }

        internal double ToInches()
        {
            return _value * 39.370079;
        }

        internal double ToFeet()
        {
            return _value * 3.28084;
        }

        internal string ToMillimeterString()
        {
            return ToMillimeters().ToString("0.00", CultureInfo.InvariantCulture) + " mm";
        }

        internal string ToCentimeterString()
        {
            return ToCentimeters().ToString("0.00", CultureInfo.InvariantCulture) + " cm";
        }

        internal string ToMeterString()
        {
            return ToMeters().ToString("0.00", CultureInfo.InvariantCulture) + " m";
        }

        internal string ToDecimalInchString()
        {
            return ToInches().ToString("0.00", CultureInfo.CurrentCulture) + " in";
        }

        internal string ToFractionalInchString()
        {
            return Utils.ToFractionalInches(ToInches());
        }

        internal string ToDecimalFootString()
        {
            return ToFeet().ToString("0.00", CultureInfo.CurrentCulture) + " ft";
        }

        internal string ToFractionalFootString()
        {
            return Utils.ToFeetAndFractionalInches(ToFeet());
        }

        #endregion
    }

    /// <summary>
    /// An area stored as square meters.
    /// </summary>
    public class Area : SIUnit
    {
        private const double square_meters_to_square_millimeters = 1000000;
        private const double square_meters_to_square_centimeters = 10000;
        private const double square_meters_to_square_inch = 1550;
        private const double square_meters_to_square_foot = 10.7639;

        public Area():base(0.0){}

        public Area(double value) : base(value)
        {
            if (value < 0)
            {
                throw new MathematicalArgumentException("You can not create a negative volume.");
            }
        }

        #region math

        public override SIUnit Add(SIUnit x)
        {
            if(x is Area)
                return new Area(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Subtract(SIUnit x)
        {
            if(x is Area)
                return new Area(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
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

        #endregion

        #region string
        
        public override void SetValueFromString(string value)
        {
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (Double.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out total))
            {
                switch (dynSettings.Controller.PreferenceSettings.AreaUnit)
                {
                    case DynamoAreaUnit.SquareMillimeter:
                        _value = total / square_meters_to_square_millimeters;
                        return;

                    case DynamoAreaUnit.SquareCentimeter:
                        _value = total / square_meters_to_square_centimeters;
                        return;

                    case DynamoAreaUnit.SquareMeter:
                        _value = total;
                        return;

                    case DynamoAreaUnit.SquareInch:
                        _value = total / square_meters_to_square_inch;
                        return;

                    case DynamoAreaUnit.SquareFoot:
                        _value = total / square_meters_to_square_foot;
                        return;
                }
            }

            double sq_mm, sq_cm, sq_m, sq_in, sq_ft;
            Utils.ParseAreaFromString(value, out sq_in, out sq_ft, out sq_mm, out sq_cm, out sq_m);

            total += sq_mm / square_meters_to_square_millimeters;
            total += sq_cm / square_meters_to_square_centimeters;
            total += sq_m;
            total += sq_in / square_meters_to_square_inch;
            total += sq_ft / square_meters_to_square_foot;

            _value = total;
        }

        public override string ToString()
        {
            switch (dynSettings.Controller.PreferenceSettings.AreaUnit)
            {
                case DynamoAreaUnit.SquareMillimeter:
                    return ToSquareMillimeterString();
                case DynamoAreaUnit.SquareCentimeter:
                    return ToSquareCentimeterString();
                case DynamoAreaUnit.SquareMeter:
                    return ToSquareMeterString();
                case DynamoAreaUnit.SquareInch:
                    return ToSquareInchString();
                case DynamoAreaUnit.SquareFoot:
                    return ToSquareFootString();
                default:
                    return ToSquareMeterString();  
            }
        }

        public string ToString(DynamoAreaUnit unit)
        {
            switch (unit)
            {
                case DynamoAreaUnit.SquareMillimeter:
                    return ToSquareMillimeterString();
                case DynamoAreaUnit.SquareCentimeter:
                    return ToSquareCentimeterString();
                case DynamoAreaUnit.SquareMeter:
                    return ToSquareMeterString();
                case DynamoAreaUnit.SquareInch:
                    return ToSquareInchString();
                case DynamoAreaUnit.SquareFoot:
                    return ToSquareFootString();
                default:
                    return ToSquareMeterString();
            }
        }
        
        #endregion

        #region conversion

        internal double ToSquareMillimeters()
        {
            return _value * square_meters_to_square_millimeters;
        }

        internal double ToSquareCentimeters()
        {
            return _value * square_meters_to_square_centimeters;
        }

        internal double ToSquareMeters()
        {
            return _value;
        }

        internal double ToSquareInches()
        {
            return _value * square_meters_to_square_inch;
        }

        internal double ToSquareFeet()
        {
            return _value * square_meters_to_square_foot;
        }

        internal string ToSquareMillimeterString()
        {
            return ToSquareMillimeters().ToString("0.00", CultureInfo.InvariantCulture) + " mm²";
        }

        internal string ToSquareCentimeterString()
        {
            return ToSquareCentimeters().ToString("0.00", CultureInfo.InvariantCulture) + " cm²";
        }

        internal string ToSquareMeterString()
        {
            return ToSquareMeters().ToString("0.00", CultureInfo.InvariantCulture) + " m²";
        }

        internal string ToSquareInchString()
        {
            return ToSquareInches().ToString("0.00", CultureInfo.InvariantCulture) + " in²";
        }

        internal string ToSquareFootString()
        {
            return ToSquareFeet().ToString("0.00", CultureInfo.InvariantCulture) + " ft²";
        }

        #endregion
    }

    /// <summary>
    /// A volume stored as cubic meters.
    /// </summary>
    public class Volume : SIUnit
    {
        private const double cubic_meters_to_cubic_millimeters = 1000000000;
        private const double cubic_meters_to_cubic_centimeters = 1000000;
        private const double cubic_meters_to_cubic_inches = 61023.7;
        private const double cubic_meters_to_cubic_feet = 35.3147;

        public Volume():base(0.0){}

        public Volume(double value) : base(value)
        {
            if (value < 0)
            {
                throw new MathematicalArgumentException("You can not create a negative volume.");
            }
        }

        #region math

        public override SIUnit Add(SIUnit x)
        {
            if(x is Volume)
                return new Volume(_value + x.Value);

            throw new UnitsException(GetType(), x.GetType());
        }

        public override SIUnit Subtract(SIUnit x)
        {
            if(x is Volume)
                return new Volume(_value - x.Value);

            throw new UnitsException(GetType(), x.GetType());
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

        #endregion

        #region string

        public override void SetValueFromString(string value)
        {
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (Double.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out total))
            {
                switch (dynSettings.Controller.PreferenceSettings.VolumeUnit)
                {
                    case DynamoVolumeUnit.CubicMillimeter:
                        _value = total / cubic_meters_to_cubic_millimeters;
                        return;

                    case DynamoVolumeUnit.CubicCentimeter:
                        _value = total / cubic_meters_to_cubic_centimeters;
                        return;

                    case DynamoVolumeUnit.CubicMeter:
                        _value = total;
                        return;

                    case DynamoVolumeUnit.CubicInch:
                        _value = total / cubic_meters_to_cubic_inches;
                        return;

                    case DynamoVolumeUnit.CubicFoot:
                        _value = total / cubic_meters_to_cubic_feet;
                        return;
                }
            }

            double cu_mm, cu_cm, cu_m, cu_in, cu_ft;
            Utils.ParseVolumeFromString(value, out cu_in, out cu_ft, out cu_mm, out cu_cm, out cu_m);

            total += cu_mm / cubic_meters_to_cubic_millimeters;
            total += cu_cm / cubic_meters_to_cubic_centimeters;
            total += cu_m;
            total += cu_in / cubic_meters_to_cubic_inches;
            total += cu_ft / cubic_meters_to_cubic_feet;

            _value = total;
        }

        public override string ToString()
        {
            switch (dynSettings.Controller.PreferenceSettings.VolumeUnit)
            {
                case DynamoVolumeUnit.CubicMillimeter:
                    return ToCubicMillimeterString();
                case DynamoVolumeUnit.CubicCentimeter:
                    return ToCubicCentimeterString();
                case DynamoVolumeUnit.CubicMeter:
                    return ToCubicMeterString();
                case DynamoVolumeUnit.CubicInch:
                    return ToCubicInchString();
                case DynamoVolumeUnit.CubicFoot:
                    return ToCubicFootString();
                default:
                    return ToCubicMeterString();
            }
        }

        public string ToString(DynamoVolumeUnit unit)
        {
            switch (unit)
            {
                case DynamoVolumeUnit.CubicMillimeter:
                    return ToCubicMillimeterString();
                case DynamoVolumeUnit.CubicCentimeter:
                    return ToCubicCentimeterString();
                case DynamoVolumeUnit.CubicMeter:
                    return ToCubicMeterString();
                case DynamoVolumeUnit.CubicInch:
                    return ToCubicInchString();
                case DynamoVolumeUnit.CubicFoot:
                    return ToCubicFootString();
                default:
                    return ToCubicMeterString();
            }
        }

        #endregion

        #region conversion

        internal double ToCubicMillimeters()
        {
            return _value * cubic_meters_to_cubic_millimeters;
        }

        internal double ToCubicCentimeters()
        {
            return _value * cubic_meters_to_cubic_centimeters;
        }

        internal double ToCubicMeters()
        {
            return _value;
        }

        internal double ToCubicInches()
        {
            return _value * cubic_meters_to_cubic_inches;
        }

        internal double ToCubicFeet()
        {
            return _value * cubic_meters_to_cubic_feet;
        }

        internal string ToCubicMillimeterString()
        {
            return ToCubicMillimeters().ToString("0.00", CultureInfo.InvariantCulture) + " mm³";
        }

        internal string ToCubicCentimeterString()
        {
            return ToCubicCentimeters().ToString("0.00", CultureInfo.InvariantCulture) + " cm³";
        }

        internal string ToCubicMeterString()
        {
            return ToCubicMeters().ToString("0.00", CultureInfo.InvariantCulture) + " m³";
        }

        internal string ToCubicInchString()
        {
            return ToCubicInches().ToString("0.00", CultureInfo.InvariantCulture) + " in³";
        }

        internal string ToCubicFootString()
        {
            return ToCubicFeet().ToString("0.00", CultureInfo.InvariantCulture) + " ft³";
        }

        #endregion
    }

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

            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out m))
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
            decimalInches = RoundToSignificantDigits(decimalInches, 5);

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
            string pattern = @"(((?<ft>((\+|-)?\d+([.,]\d{1,2})?))( ?)('|ft))*\s*((?<in>(?<num>(\+|-)?\d+([.,]\d{1,2})?)/(?<den>\d+([.,]\d{1,2})?)*( ?)(""|in))|(?<in>(?<wholeInch>(\+|-)?\d+([.,]\d{1,2})?)*(\s|-)*(?<num>(\+|-)?\d+([.,]\d{1,2})?)/(?<den>\d+([.,]\d{1,2})?)*( ?)(""|in))|(?<in>(?<wholeInch>(\+|-)?\d+([.,]\d{1,2})?)( ?)(""|in)))?)*((?<m>((\+|-)?\d+([.,]\d{1,2})?))( ?)m($|\s))*((?<cm>((\+|-)?\d+([.,]\d{1,2})?))( ?)cm($|\s))*((?<mm>((\+|-)?\d+([.,]\d{1,2})?))( ?)mm($|\s))*";

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
                double.TryParse(match.Groups["ft"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out feet);
                double.TryParse(match.Groups["wholeInch"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out inch);
                double.TryParse(match.Groups["num"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                                out numerator);
                double.TryParse(match.Groups["den"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                                out denominator);

                //parse metric values
                double.TryParse(match.Groups["m"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out m);
                double.TryParse(match.Groups["cm"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out cm);
                double.TryParse(match.Groups["mm"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out mm);
            }
        }

        public static void ParseAreaFromString(string value, out double square_inch, out double square_foot, out double square_millimeter,  out double square_centimeter, out double square_meter)
        {
            const string pattern =
                @"((?<square_inches>((\+|-)?\d+([.,]\d{1,})?))( ?)(in2|sqin|in²))*\s*((?<square_feet>((\+|-)?\d+([.,]\d{1,})?))( ?)(ft2|sqft|ft²))*\s*((?<square_millimeters>((\+|-)?\d+([.,]\d{1,})?))( ?)(mm2|sqmm|mm²))*\s*((?<square_centimeters>((\+|-)?\d+([.,]\d{1,})?))( ?)(cm2|sqcm|cm²))*\s*((?<square_meters>((\+|-)?\d+([.,]\d{1,})?))( ?)(m2|sqm|m²))*\s*";
            
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
                double.TryParse(match.Groups["square_inches"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out square_inch);
                double.TryParse(match.Groups["square_feet"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out square_foot);
                double.TryParse(match.Groups["square_millimeters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                    out square_millimeter);
                double.TryParse(match.Groups["square_centimeters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                    out square_centimeter);
                double.TryParse(match.Groups["square_meters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                    out square_meter);
            }
        }

        public static void ParseVolumeFromString(string value, out double cubic_inch, out double cubic_foot, out double cubic_millimeter, out double cubic_centimeter, out double cubic_meter)
        {
            const string pattern =
                @"((?<cubic_inches>((\+|-)?\d+([.,]\d{1,})?))( ?)(in3|cuin|in³))*\s*((?<cubic_feet>((\+|-)?\d+([.,]\d{1,})?))( ?)(ft3|cuft|ft³))*\s*((?<cubic_millimeters>((\+|-)?\d+([.,]\d{1,})?))( ?)(mm3|cumm|mm³))*\s*((?<cubic_centimeters>((\+|-)?\d+([.,]\d{1,})?))( ?)(cm3|cucm|cm³))*\s*((?<cubic_meters>((\+|-)?\d+([.,]\d{1,})?))( ?)(m3|cum|m³))*\s*";

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
                double.TryParse(match.Groups["cubic_inches"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out cubic_inch);
                double.TryParse(match.Groups["cubic_feet"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out cubic_foot);
                double.TryParse(match.Groups["cubic_millimeters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                    out cubic_millimeter);
                double.TryParse(match.Groups["cubic_centimeters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                    out cubic_centimeter);
                double.TryParse(match.Groups["cubic_meters"].Value,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                    out cubic_meter);
            }
        }
    }

    public class MathematicalArgumentException : Exception
    {
        public MathematicalArgumentException() : base("The result could not be computed given the provided inputs.") { }
        public MathematicalArgumentException(string message) : base(message) { }
    }

    public class UnitsException : MathematicalArgumentException
    {
        public UnitsException(Type a, Type b) : base(string.Format("{0} and {1} are incompatible for this operation.", a, b)) { }
    }

    public interface ILength
    {
        Length ToLength();
    }

    public interface IArea
    {
        Area ToArea();
    }

    public interface IVolume
    {

        Volume ToVolume();
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Double = System.Double;

namespace Dynamo.Units
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

    public class UnitsManager
    {
        private static UnitsManager _instance;
        private double _uiLengthConversion;
        private double _uiAreaConversion;
        private double _uiVolumeConversion;
        private DynamoLengthUnit _lengthUnit;
        private DynamoAreaUnit _areaUnit;
        private DynamoVolumeUnit _volumeUnit;

        public DynamoLengthUnit HostApplicationInternalLengthUnit { get; set; }

        public DynamoAreaUnit HostApplicationInternalAreaUnit { get; set; }

        public DynamoVolumeUnit HostApplicationInternalVolumeUnit { get; set; }

        public DynamoLengthUnit LengthUnit
        {
            get { return _lengthUnit; }
            set
            {
                _lengthUnit = value;

                switch (LengthUnit)
                {
                    case DynamoLengthUnit.Millimeter:
                        _uiLengthConversion = SIUnit.ToMillimeter;
                        break;
                    case DynamoLengthUnit.Centimeter:
                        _uiLengthConversion = SIUnit.ToCentimeter;
                        break;
                    case DynamoLengthUnit.Meter:
                        _uiLengthConversion = 1.0;
                        break;
                    case DynamoLengthUnit.DecimalInch:
                        _uiLengthConversion = SIUnit.ToInch;
                        break;
                    case DynamoLengthUnit.FractionalInch:
                        _uiLengthConversion = SIUnit.ToInch;
                        break;
                    case DynamoLengthUnit.DecimalFoot:
                        _uiLengthConversion = SIUnit.ToFoot;
                        break;
                    case DynamoLengthUnit.FractionalFoot:
                        _uiLengthConversion = SIUnit.ToFoot;
                        break;

                }
            }
        }

        public DynamoAreaUnit AreaUnit
        {
            get { return _areaUnit; }
            set
            {
                _areaUnit = value;

                switch (AreaUnit)
                {
                    case DynamoAreaUnit.SquareMillimeter:
                        _uiAreaConversion = SIUnit.ToSquareMillimeters;
                        break;
                    case DynamoAreaUnit.SquareCentimeter:
                        _uiAreaConversion = SIUnit.ToSquareCentimeters;
                        break;
                    case DynamoAreaUnit.SquareMeter:
                        _uiAreaConversion = 1.0;
                        break;
                    case DynamoAreaUnit.SquareInch:
                        _uiAreaConversion = SIUnit.ToSquareInch;
                        break;
                    case DynamoAreaUnit.SquareFoot:
                        _uiAreaConversion = SIUnit.ToSquareFoot;
                        break;
                }
            }
        }

        public DynamoVolumeUnit VolumeUnit
        {
            get { return _volumeUnit; }
            set
            {
                _volumeUnit = value;

                switch (VolumeUnit)
                {
                    case DynamoVolumeUnit.CubicMillimeter:
                        _uiVolumeConversion = SIUnit.ToCubicMillimeter;
                        break;
                    case DynamoVolumeUnit.CubicCentimeter:
                        _uiVolumeConversion = SIUnit.ToCubicCentimeter;
                        break;
                    case DynamoVolumeUnit.CubicMeter:
                        _uiVolumeConversion = 1.0;
                        break;
                    case DynamoVolumeUnit.CubicInch:
                        _uiVolumeConversion = SIUnit.ToCubicInch;
                        break;
                    case DynamoVolumeUnit.CubicFoot:
                        _uiVolumeConversion = SIUnit.ToCubicFoot;
                        break;
                }
            }
        }

        public double UiLengthConversion
        {
            get { return _uiLengthConversion; }
        }

        public double UiAreaConversion
        {
            get { return _uiAreaConversion; }
        }

        public double UiVolumeConversion
        {
            get { return _uiVolumeConversion; }
        }

        public static UnitsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UnitsManager();
                }

                return _instance;
            }
        }

        private UnitsManager()
        {
            LengthUnit = DynamoLengthUnit.Meter;
            AreaUnit = DynamoAreaUnit.SquareMeter;
            VolumeUnit = DynamoVolumeUnit.CubicMeter;
        }
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

        internal double _value;

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
        public abstract SIUnit Round();
        public abstract SIUnit Ceiling();
        public abstract SIUnit Floor();

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

        /// <summary>
        /// Unwrap an FScheme value containing a number or a unit to a double.
        /// If the value contains a unit object, convert the internal value of the
        /// unit object to the units required by the host application as specified
        /// in the preference settings. If the value contains a number, do not 
        /// apply a conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FScheme.Value UnwrapToDoubleWithHostUnitConversion(FScheme.Value value)
        {
            if (value.IsList)
            {
                //recursively convert items in list
                return ConvertListToHostUnits((FScheme.Value.List)value);
            }

            if (value.IsContainer)
            {
                var unit = ((FScheme.Value.Container)value).Item as SIUnit;
                if (unit != null)
                {
                    return FScheme.Value.NewNumber(unit.ConvertToHostUnits());
                }
            }

            return value;
        }

        public static FScheme.Value ConvertListToHostUnits(FScheme.Value.List value)
        {
            var list = value.Item;
            return FScheme.Value.NewList(FSchemeInterop.Utils.SequenceToFSharpList(list.Select(UnwrapToDoubleWithHostUnitConversion)));
        }

        public static SIUnit UnwrapToSIUnit(FScheme.Value value)
        {
            if (value.IsContainer)
            {
                var measure = ((FScheme.Value.Container)value).Item as SIUnit;
                if (measure != null)
                {
                    return measure;
                }
            }

            throw new Exception("The value was not convertible to a unit of measure.");
        }

        public abstract double ConvertToHostUnits();

    }

    /// <summary>
    /// A length stored as meters.
    /// </summary>
    public class Length : SIUnit
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

        public override SIUnit Round()
        {
            var val = _value * UnitsManager.Instance.UiLengthConversion;
            double round = Math.Round(val);
            return new Length(round / UnitsManager.Instance.UiLengthConversion);
        }

        public override SIUnit Ceiling()
        {
            var val = _value * UnitsManager.Instance.UiLengthConversion;
            double round = Math.Ceiling(val);
            return new Length(round / UnitsManager.Instance.UiLengthConversion);
        }

        public override SIUnit Floor()
        {
            var val = _value * UnitsManager.Instance.UiLengthConversion;
            double round = Math.Floor(val);
            return new Length(round / UnitsManager.Instance.UiLengthConversion);
        }

        public override double ConvertToHostUnits()
        {
            switch (UnitsManager.Instance.HostApplicationInternalLengthUnit)
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
                _value = total/UnitsManager.Instance.UiLengthConversion;
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

        public override string ToString()
        {
            return BuildString(UnitsManager.Instance.LengthUnit);
        }

        public string ToString(DynamoLengthUnit unit)
        {
            return BuildString(unit);
        }

        private string BuildString(DynamoLengthUnit unit)
        {
            switch (unit)
            {
                case DynamoLengthUnit.Millimeter:
                    return (_value * SIUnit.ToMillimeter).ToString("0.00", CultureInfo.InvariantCulture) + " mm";

                case DynamoLengthUnit.Centimeter:
                    return (_value * SIUnit.ToCentimeter).ToString("0.00", CultureInfo.InvariantCulture) + " cm";

                case DynamoLengthUnit.Meter:
                    return _value.ToString("0.00", CultureInfo.InvariantCulture) + " m";

                case DynamoLengthUnit.DecimalInch:
                    return (_value * SIUnit.ToInch).ToString("0.00", CultureInfo.InvariantCulture) + " in";

                case DynamoLengthUnit.FractionalInch:
                    return Utils.ToFractionalInches(_value * SIUnit.ToInch);

                case DynamoLengthUnit.DecimalFoot:
                    return (_value * SIUnit.ToFoot).ToString("0.00", CultureInfo.InvariantCulture) + " ft";

                case DynamoLengthUnit.FractionalFoot:
                    return Utils.ToFeetAndFractionalInches(_value * SIUnit.ToFoot);

                default:
                    return _value.ToString("0.00", CultureInfo.InvariantCulture) + " m";
            }
        }

        #endregion
    }

    /// <summary>
    /// An area stored as square meters.
    /// </summary>
    public class Area : SIUnit
    {
        public Area():base(0.0){}

        public Area(double value) : base(value)
        {
            if (value < 0)
            {
                throw new MathematicalArgumentException("You can not create a negative volume.");
            }
        }

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

        public override SIUnit Round()
        {
            var val = _value * UnitsManager.Instance.UiAreaConversion;
            double round = Math.Round(val);
            return new Length(round / UnitsManager.Instance.UiAreaConversion);
        }

        public override SIUnit Ceiling()
        {
            var val = _value * UnitsManager.Instance.UiAreaConversion;
            double round = Math.Ceiling(val);
            return new Length(round / UnitsManager.Instance.UiAreaConversion);
        }

        public override SIUnit Floor()
        {
            var val = _value * UnitsManager.Instance.UiAreaConversion;
            double round = Math.Floor(val);
            return new Length(round / UnitsManager.Instance.UiAreaConversion);
        }

        public override double ConvertToHostUnits()
        {
            switch (UnitsManager.Instance.HostApplicationInternalAreaUnit)
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
                _value = total/UnitsManager.Instance.UiAreaConversion;
                return;
            }

            double sq_mm, sq_cm, sq_m, sq_in, sq_ft;
            Utils.ParseAreaFromString(value, out sq_in, out sq_ft, out sq_mm, out sq_cm, out sq_m);

            total += sq_mm / SIUnit.ToSquareMillimeters;
            total += sq_cm / SIUnit.ToSquareCentimeters;
            total += sq_m;
            total += sq_in / SIUnit.ToSquareInch;
            total += sq_ft / SIUnit.ToSquareFoot;

            _value = total;
        }

        public override string ToString()
        {
            return BuildString(UnitsManager.Instance.AreaUnit);
        }

        public string ToString(DynamoAreaUnit unit)
        {
            return BuildString(unit);
        }

        private string BuildString(DynamoAreaUnit unit)
        {
            switch (unit)
            {
                case DynamoAreaUnit.SquareMillimeter:
                    return (_value*SIUnit.ToSquareMillimeters).ToString("0.00", CultureInfo.InvariantCulture) + " mm²";

                case DynamoAreaUnit.SquareCentimeter:
                    return (_value*SIUnit.ToSquareCentimeters).ToString("0.00", CultureInfo.InvariantCulture) + " cm²";

                case DynamoAreaUnit.SquareMeter:
                    return _value.ToString("0.00", CultureInfo.InvariantCulture) + " m²";

                case DynamoAreaUnit.SquareInch:
                    return (_value*SIUnit.ToSquareInch).ToString("0.00", CultureInfo.InvariantCulture) + " in²";

                case DynamoAreaUnit.SquareFoot:
                    return (_value*SIUnit.ToSquareFoot).ToString("0.00", CultureInfo.InvariantCulture) + " ft²";

                default:
                    return _value.ToString("0.00", CultureInfo.InvariantCulture) + " m²";
            }
        }

        #endregion

    }

    /// <summary>
    /// A volume stored as cubic meters.
    /// </summary>
    public class Volume : SIUnit
    {
        public Volume():base(0.0){}

        public Volume(double value) : base(value)
        {
            if (value < 0)
            {
                throw new MathematicalArgumentException("You can not create a negative volume.");
            }
        }

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

        public override SIUnit Round()
        {
            var val = _value * UnitsManager.Instance.UiVolumeConversion;
            double round = Math.Round(val);
            return new Length(round / UnitsManager.Instance.UiVolumeConversion);
        }

        public override SIUnit Ceiling()
        {
            var val = _value * UnitsManager.Instance.UiVolumeConversion;
            double round = Math.Ceiling(val);
            return new Length(round / UnitsManager.Instance.UiVolumeConversion);
        }

        public override SIUnit Floor()
        {
            var val = _value * UnitsManager.Instance.UiVolumeConversion;
            double round = Math.Floor(val);
            return new Length(round / UnitsManager.Instance.UiVolumeConversion);
        }

        public override double ConvertToHostUnits()
        {
            switch (UnitsManager.Instance.VolumeUnit)
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
                _value = total/UnitsManager.Instance.UiVolumeConversion;
                return;
            }

            double cu_mm, cu_cm, cu_m, cu_in, cu_ft;
            Utils.ParseVolumeFromString(value, out cu_in, out cu_ft, out cu_mm, out cu_cm, out cu_m);

            total += cu_mm / ToCubicMillimeter;
            total += cu_cm / ToCubicCentimeter;
            total += cu_m;
            total += cu_in / ToCubicInch;
            total += cu_ft / ToCubicFoot;

            _value = total;
        }

        public override string ToString()
        {
            return BuildString(UnitsManager.Instance.VolumeUnit);
        }

        public string ToString(DynamoVolumeUnit unit)
        {
            return BuildString(unit);
        }

        private string BuildString(DynamoVolumeUnit unit)
        {
            switch (unit)
            {
                case DynamoVolumeUnit.CubicMillimeter:
                    return (_value * SIUnit.ToCubicMillimeter).ToString("0.00", CultureInfo.InvariantCulture) + " mm³";

                case DynamoVolumeUnit.CubicCentimeter:
                    return (_value * SIUnit.ToCubicCentimeter).ToString("0.00", CultureInfo.InvariantCulture) + " cm³";

                case DynamoVolumeUnit.CubicMeter:
                    return _value.ToString("0.00", CultureInfo.InvariantCulture) + " m³";

                case DynamoVolumeUnit.CubicInch:
                    return (_value * SIUnit.ToCubicInch).ToString("0.00", CultureInfo.InvariantCulture) + " in³";

                case DynamoVolumeUnit.CubicFoot:
                    return (_value * SIUnit.ToCubicFoot).ToString("0.00", CultureInfo.InvariantCulture) + " ft³";

                default:
                    return _value.ToString("0.00", CultureInfo.InvariantCulture) + " m³";
            }
        }

        #endregion

    }

    /// <summary>
    /// A luminous intensity stored as candela
    /// </summary>
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

        public override SIUnit Subtract(SIUnit x)
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

        public override SIUnit Subtract(SIUnit x)
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

    public class MathematicalArgumentException : Exception
    {
        public MathematicalArgumentException() : base("The result could not be computed given the provided inputs.") { }
        public MathematicalArgumentException(string message) : base(message) { }
    }

    public class UnitsException : MathematicalArgumentException
    {
        public UnitsException(Type a, Type b) : base(string.Format("{0} and {1} are incompatible for this operation.", a, b)) { }
    }

    public interface IUnitInput
    {
        double ConvertToHostUnits();
    }
}

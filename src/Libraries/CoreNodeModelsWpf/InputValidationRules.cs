using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using CoreNodeModels.Properties;
using Dynamo.Configuration;

namespace CoreNodeModelsWpf
{
    /// <summary>
    /// Accepts invariant numeric doubles/longs.
    /// </summary>
    public class NumericValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (IsNumeric(text)) return ValidationResult.ValidResult;

            return new ValidationResult(false, Resources.NumberNodeInputMustBeNumeric);
        }

        internal static bool IsNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _) || long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }
    }

    /// <summary>
    /// Accepts Int64 integers only. Non-numeric and overflow both fail (Error bubble via DynamoTextBox).
    /// </summary>
    public class Integer64ValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return new ValidationResult(false, Resources.NumberNodeInputMustBeNumeric);
            }

            try
            {
                Convert.ToInt64(text, CultureInfo.InvariantCulture);
                return ValidationResult.ValidResult;
            }
            catch (FormatException)
            {
                return new ValidationResult(false, Resources.IntegerSliderInputMustBeInteger);
            }
            catch (OverflowException)
            {
                return new ValidationResult(false, Resources.IntegerSliderInfoMessage);
            }
        }
    }

    /// <summary>
    /// Accepts PreferenceSettings.DefaultDateFormat only.
    /// </summary>
    public class DateTimeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (DateTime.TryParseExact(text, PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, Resources.DateTimeNodeInputInvalidFormat);
        }
    }
}

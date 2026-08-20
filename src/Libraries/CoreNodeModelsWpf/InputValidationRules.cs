using System.Globalization;
using System.Numerics;
using System.Windows.Controls;
using CoreNodeModels.Properties;

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
        private const NumberStyles IntegerStyles = NumberStyles.Integer | NumberStyles.AllowThousands;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
            {
                return new ValidationResult(false, Resources.NumberNodeInputMustBeNumeric);
            }

            if (long.TryParse(text, IntegerStyles, CultureInfo.InvariantCulture, out _))
            {
                return ValidationResult.ValidResult;
            }

            return BigInteger.TryParse(text, IntegerStyles, CultureInfo.InvariantCulture, out _)
                ? new ValidationResult(false, Resources.IntegerSliderInfoMessage)
                : new ValidationResult(false, Resources.IntegerSliderInputMustBeInteger);
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
            if (CoreNodeModels.Input.DateTime.TryParseDateTime(text, out _))
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, Resources.DateTimeNodeInputInvalidFormat);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dynamo.GraphMetadata.ValidationRules
{
    public class UriValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string input = value as string;
            if (String.IsNullOrEmpty(input)) // Valid input, converts to null.
            {
                return new ValidationResult(true, null);
            }
            Uri outUri;
            if (Uri.TryCreate(input, UriKind.Absolute, out outUri))
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, "String is not a valid URI");
            }
        }
    }
}

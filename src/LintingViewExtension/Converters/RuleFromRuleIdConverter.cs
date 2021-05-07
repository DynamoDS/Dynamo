using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Dynamo.Extensions;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension.Converters
{
    [ValueConversion(typeof(CollectionViewGroup), typeof(LinterRule))]
    public class RuleFromRuleIdConverter : DependencyObject, IValueConverter
    {
        // The property used as a parameter
        internal LinterExtensionBase ActiveLinter
        {
            get { return (LinterExtensionBase)GetValue(ActiveLinterProperty); }
            set { SetValue(ActiveLinterProperty, value); }
        }

        // The dependency property to allow the property to be used from XAML.
        public static readonly DependencyProperty ActiveLinterProperty =
            DependencyProperty.Register(
            nameof(ActiveLinter),
            typeof(LinterExtensionBase),
            typeof(RuleFromRuleIdConverter));


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CollectionViewGroup cvg) || !(cvg.Name is string ruleId))
                return null;

            var linterRule = GetLinterRuleById(ruleId, ActiveLinter);
            return linterRule;

        }
        private LinterRule GetLinterRuleById(string ruleId, LinterExtensionBase linter)
        {
            var linterRule = linter.LinterRules.Where(x => x.Id == ruleId).FirstOrDefault();
            return linterRule;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Boolean boolean))
            {
                return false;
            }

            return !boolean;
        }
    }
}

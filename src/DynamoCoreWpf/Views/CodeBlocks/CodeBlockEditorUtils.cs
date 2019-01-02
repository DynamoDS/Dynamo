using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Engine;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;

namespace Dynamo.Wpf.Views
{
    /// <summary>
    /// Help to create rules related to code highligh
    /// </summary>
    public static class CodeHighlightingRuleFactory
    {
        /// <summary>
        /// Create hight lighting rule for number.
        /// </summary>
        /// <returns></returns>
        public static HighlightingRule CreateNumberHighlightingRule()
        {
            var digitRule = new HighlightingRule();

            Color color = (Color)ColorConverter.ConvertFromString("#2585E5");
            digitRule.Color = new HighlightingColor()
            {
                Foreground = new CustomizedBrush(color)
            };

            // These Regex's must match with the grammars in the DS ATG for digits
            // Refer to the 'number' and 'float' tokens in Start.atg
            //*******************************************************************************
            // number = digit {digit} .
            // float = digit {digit} '.' digit {digit} [('E' | 'e') ['+'|'-'] digit {digit}].
            //*******************************************************************************

            string digit = @"(-?\b\d+)";
            string floatingPoint = @"(\.[0-9]+)";
            string numberWithOptionalDecimal = digit + floatingPoint + "?";

            string exponent = @"([eE][+-]?[0-9]+)";
            string numberWithExponent = digit + floatingPoint + exponent;

            digitRule.Regex = new Regex(numberWithExponent + "|" + numberWithOptionalDecimal);

            return digitRule;
        }

        /// <summary>
        /// Create hight lighting rule for class.
        /// </summary>
        /// <param name="engineController"></param>
        /// <returns></returns>
        public static HighlightingRule CreateClassHighlightRule(EngineController engineController)
        {
            Color color = (Color)ColorConverter.ConvertFromString("#2E998F");
            var classHighlightRule = new HighlightingRule
            {
                Color = new HighlightingColor()
                {
                    Foreground = new CodeHighlightingRuleFactory.CustomizedBrush(color)
                }
            };

            var wordList = engineController.CodeCompletionServices.GetClasses();
            String regex = String.Format(@"\b({0})\b", String.Join("|", wordList));
            classHighlightRule.Regex = new Regex(regex);

            return classHighlightRule;
        }

        /// <summary>
        /// Create hight lighting rule for method.
        /// </summary>
        /// <returns></returns>
        public static HighlightingRule CreateMethodHighlightRule(EngineController engineController)
        {
            Color color = (Color)ColorConverter.ConvertFromString("#417693");
            var methodHighlightRule = new HighlightingRule
            {
                Color = new HighlightingColor()
                {
                    Foreground = new CodeHighlightingRuleFactory.CustomizedBrush(color)
                }
            };

            var wordList = engineController.CodeCompletionServices.GetGlobals();
            String regex = String.Format(@"\b({0})\b", String.Join("|", wordList));
            methodHighlightRule.Regex = new Regex(regex);

            return methodHighlightRule;
        }


        // Refer to link: 
        // http://stackoverflow.com/questions/11806764/adding-syntax-highlighting-rules-to-avalonedit-programmatically
        internal sealed class CustomizedBrush : HighlightingBrush
        {
            private readonly SolidColorBrush brush;
            public CustomizedBrush(Color color)
            {
                brush = CreateFrozenBrush(color);
            }

            public override Brush GetBrush(ITextRunConstructionContext context)
            {
                return brush;
            }

            public override string ToString()
            {
                return brush.ToString();
            }

            private static SolidColorBrush CreateFrozenBrush(Color color)
            {
                SolidColorBrush brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
        }

        public static void CreateHighlightingRules(ICSharpCode.AvalonEdit.TextEditor editor, EngineController controller)
        {
            var stream = typeof(CodeHighlightingRuleFactory).Assembly.GetManifestResourceStream(
                            "Dynamo.Wpf.UI.Resources." + Configurations.HighlightingFile);

            editor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(stream), HighlightingManager.Instance);

            // Highlighting Digits
            var rules = editor.SyntaxHighlighting.MainRuleSet.Rules;

            rules.Add(CodeHighlightingRuleFactory.CreateNumberHighlightingRule());
            rules.Add(CodeHighlightingRuleFactory.CreateClassHighlightRule(controller));
            rules.Add(CodeHighlightingRuleFactory.CreateMethodHighlightRule(controller));
        }
    }
}

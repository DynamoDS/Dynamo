using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.UI;
using ICSharpCode.AvalonEdit;
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
        /// Reads a token color from the themed palette so that code block highlighting stays
        /// legible against the themed editor background.
        /// </summary>
        private static Color GetThemedColor(string resourceKey)
        {
            var brush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary[resourceKey] as SolidColorBrush;
            return brush.Color;
        }

        /// <summary>
        /// Create hight lighting rule for number.
        /// </summary>
        /// <returns></returns>
        public static HighlightingRule CreateNumberHighlightingRule()
        {
            var digitRule = new HighlightingRule();

            Color color = GetThemedColor("CodeEditorNumberBrush");
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
            Color color = GetThemedColor("CodeEditorClassBrush");
            var classHighlightRule = new HighlightingRule
            {
                Color = new HighlightingColor()
                {
                    Foreground = new CodeHighlightingRuleFactory.CustomizedBrush(color)
                }
            };

            var wordList = engineController.CodeCompletionServices.GetClasses();
            if (!wordList.Any()) return null;

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
            Color color = GetThemedColor("CodeEditorMethodBrush");
            var methodHighlightRule = new HighlightingRule
            {
                Color = new HighlightingColor()
                {
                    Foreground = new CodeHighlightingRuleFactory.CustomizedBrush(color)
                }
            };

            var wordList = engineController.CodeCompletionServices.GetGlobals();
            if (!wordList.Any()) return null;

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
            // The light theme needs its own definition: the default one is bright-on-dark, which
            // is unreadable on a light editor background.
            var highlightingFile = SharedDictionaryManager.CurrentTheme == DynamoTheme.Dark
                ? Configurations.HighlightingFile
                : Configurations.LightHighlightingFile;

            using var stream = typeof(CodeHighlightingRuleFactory).Assembly.GetManifestResourceStream(
                       "Dynamo.Wpf.UI.Resources." + highlightingFile);

            // Hyperlink color
            editor.TextArea.TextView.LinkTextForegroundBrush =
                SharedDictionaryManager.DynamoColorsAndBrushesDictionary["CodeEditorLinkBrush"] as SolidColorBrush;

            editor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader(stream), HighlightingManager.Instance);

            AddCommonHighlighingRules(editor, controller);
        }

        // Assigning custom highlighting rules moved to a separate method
        // So we can reuse for both design script and python script
        // Allows each individual eidtor to have their own set of rules, and aligns the common ones
        internal static void AddCommonHighlighingRules(TextEditor editor, EngineController controller)
        {
            // Highlighting Digits
            var rules = editor.SyntaxHighlighting.MainRuleSet.Rules;

            rules.Add(CodeHighlightingRuleFactory.CreateNumberHighlightingRule());

            var classRule = CodeHighlightingRuleFactory.CreateClassHighlightRule(controller);
            if (classRule != null) rules.Add(classRule);

            var methodRule = CodeHighlightingRuleFactory.CreateMethodHighlightRule(controller);
            if (methodRule != null) rules.Add(methodRule);
        }
    }
}

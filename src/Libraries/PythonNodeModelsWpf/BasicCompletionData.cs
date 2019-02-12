using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Media;

namespace PythonNodeModelsWpf
{
    /// <summary>
    /// Implements AvalonEdit ICompletionData interface to provide entries for a
    /// basic completion drop down, based on the script input text up to the current carret
    /// </summary>
    public class BasicCompletionData : ICompletionData
    {
        private static readonly string[] BUILTIN_PYTHON_COMMANDS = "ArithmeticError AssertionError AttributeError BaseException BufferError BytesWarning DeprecationWarning EOFError Ellipsis EnvironmentError Exception False FloatingPointError FutureWarning GeneratorExit IOError ImportError ImportWarning IndentationError IndexError KeyError KeyboardInterrupt LookupError MemoryError NameError None NotImplemented NotImplementedError OSError OverflowError PendingDeprecationWarning ReferenceError RuntimeError RuntimeWarning StandardError StopIteration SyntaxError SyntaxWarning SystemError SystemExit TabError True TypeError UnboundLocalError UnicodeDecodeError UnicodeEncodeError UnicodeError UnicodeTranslateError UnicodeWarning UserWarning UnwrapElement ValueError Warning WindowsError ZeroDivisionError apply assert basestring bool break buffer bytearray bytes callable class classmethod coerce compile complex continue delattr dict divmod elif else enumerate eval except exec execfile exit file filter finally float format from frozenset getattr global globals hasattr hash help import input intern isinstance issubclass iter lambda list locals long memoryview next object open pass print property quit raise range raw_input reduce reload repr return reversed round setattr slice sorted staticmethod super tuple type unichr unicode vars while with xrange yield".Split(new char[]{' '});
        
        private static readonly Dictionary<string, int> sortingDict = new Dictionary<string, int>();
        
        private static readonly Regex MATCH_EVERY_WORD = new Regex(@"\w+", RegexOptions.Compiled);
        
        public ImageSource Image { get { return null;} }

        public string Text { get; private set; }

        public object Content { get { return this.Text; } }

        public object Description { get { return "";} }

        public double Priority { get { return 0; } }
        
        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
        
        public BasicCompletionData(string text)
        {
            this.Text = text;
        }
        
        public static List<BasicCompletionData> PrepareAutocompletion(string lastWord, string code)
        {
            var data = new List<BasicCompletionData>();
            int temp;
            foreach (var s in BUILTIN_PYTHON_COMMANDS)
            {
                if (s.StartsWith(lastWord, StringComparison.CurrentCultureIgnoreCase))
                {
                    sortingDict[s] = 10;
                }
            }
            var allWords = MATCH_EVERY_WORD.Matches(code);
            foreach (Match w in allWords)
            {
                string s = w.Value;
                if (w.Length > 2 && !int.TryParse(s, out temp))
                {
                    int val = 0;
                    sortingDict.TryGetValue(s, out val);
                    sortingDict[s] = val + 1;
                }
            }
            sortingDict.Remove(lastWord);
            var completions = sortingDict.OrderByDescending(x => x.Value);
            int completionsToShow = 10;
            foreach (var c in completions)
            {
                if (c.Key.StartsWith(lastWord, StringComparison.CurrentCultureIgnoreCase))
                {
                    data.Add(new BasicCompletionData(c.Key));
                    completionsToShow -= 1;
                }
                
                if (completionsToShow <= 0)
                {
                    break;
                }
            }
            sortingDict.Clear();
            return data;
        }
    }
}

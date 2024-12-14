using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonNodeModelsWpf
{
    /// <summary>
    /// Custom Indentation Strategy for Python 
    /// https://csharp.hotexamples.com/site/file?hash=0x1abeea836b72c6db1910df1767e84d7bfcb620ed58a499d176fdf158851c1d5f&fullName=Yanitta/LuaIndentationStrategy.cs&project=Konctantin/Yanitta
    /// </summary>
    internal class PythonIndentationStrategy : DefaultIndentationStrategy
    {
        #region Fields

        const int IndentSpaceCount = 4;

        TextEditor textEditor;

        #endregion Fields

        #region Constructors

        internal PythonIndentationStrategy(TextEditor textEditor)
        {
            this.textEditor = textEditor;
        }

        #endregion Constructors


        /// <inheritdoc cref="IIndentationStrategy.IndentLine"/>
        public override void IndentLine(TextDocument document, DocumentLine line)
        {
            if (line?.PreviousLine == null)
                return;

            var prevLine = document.GetText(line.PreviousLine.Offset, line.PreviousLine.Length);
            var curLine = document.GetText(line.Offset, line.Length);
            int prev = CalcSpace(prevLine);

            var previousIsComment = prevLine.TrimStart().StartsWith("#", StringComparison.CurrentCulture);

            // If the current line ends with a column and was not followed by a comment
            if (curLine.EndsWith(":") && !previousIsComment)
            {
                var ind = new string(' ', prev);
                document.Insert(line.Offset, ind);
            }
            // If the previous line ends with a column and was not followed by a comment
            // We should indent
            else if (prevLine.EndsWith(":") && !previousIsComment)
            {
                var ind = new string(' ', prev + IndentSpaceCount);
                document.Insert(line.Offset, ind);
            }
            else
            {
                var ind = new string(' ', prev);
                if (line != null)
                    document.Insert(line.Offset, ind);
            }
        }

        /// <summary>
        /// Calculates the total width of leading whitespace in a string, where each space (' ') counts as 1
        /// and each tab ('\t') counts as 4. Ensures consistent behavior for indentation and folding strategy
        /// when transitioning from legacy code with tab indentations to modern conventions using spaces.
        /// </summary>
        private int CalcSpace(string str)
        {
            int count = 0;
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == ' ')
                {
                    count += 1;
                }
                else if (str[i] == '\t')
                {
                    count += 4;
                }
                else if (!char.IsWhiteSpace(str[i]))
                {
                    return count;
                }
            }
            return count;
        }

        /// <summary>
        /// Converts tabs to spaces in a legacy python code.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ConvertTabsToSpaces(string text)
        {
            return text.Replace("\t", new string(' ', IndentSpaceCount));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Wpf.ViewModels.Core.Converters
{
    /// <summary>
    /// Class to interact with ICSharp Text Editor Options
    /// </summary>
    public class DynamoPythonScriptEditorTextOptions
    {
        private static ICSharpCode.AvalonEdit.TextEditorOptions TextOptions;
        public DynamoPythonScriptEditorTextOptions()
        {
            if(TextOptions == null)
                TextOptions = new ICSharpCode.AvalonEdit.TextEditorOptions();
        }
        /// <summary>
        /// Sets the Python Script Editor options
        /// </summary>
        /// <param name="IsEnabled">Whitespaces and Tabs will be visible in the Python Script Editor if true.</param>
        internal void ShowWhiteSpaceCharacters(bool IsEnabled) {
            TextOptions.ShowSpaces = IsEnabled;
            TextOptions.ShowTabs = IsEnabled;
        }
        /// <summary>
        /// Gets the Python Script Editor options
        /// </summary>
        internal ICSharpCode.AvalonEdit.TextEditorOptions GetTextOptions()
        {
            return TextOptions;
        }

    }
}

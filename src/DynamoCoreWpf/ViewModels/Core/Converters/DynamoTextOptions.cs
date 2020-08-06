using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Wpf.ViewModels.Core.Converters
{
    public class DynamoTextOptions
    {
        private static ICSharpCode.AvalonEdit.TextEditorOptions TextOptions;

        /// <summary>
        /// Class to interact with ICSharp Text Editor Options
        /// </summary>
        public DynamoTextOptions()
        {
            if(TextOptions == null)
                TextOptions = new ICSharpCode.AvalonEdit.TextEditorOptions();
        }
        /// <summary>
        /// Sets the text editor options
        /// </summary>
        public void SetTextOptions(bool value) {
            TextOptions.ShowSpaces = value;
            TextOptions.ShowTabs = value;
        }
        /// <summary>
        /// Gets the text editor options
        /// </summary>
        public ICSharpCode.AvalonEdit.TextEditorOptions GetTextOptions()
        {
            return TextOptions;
        }

    }
}

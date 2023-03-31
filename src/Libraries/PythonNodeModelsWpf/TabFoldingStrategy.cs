using Dynamo.UI.Controls;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;

namespace PythonNodeModelsWpf
{
    /// <summary>
    /// Allows producing tab based foldings
    /// https://stackoverflow.com/questions/47224064/avalonedit-foldingstrategy-by-indent-python/47577500#47577500
    /// </summary>
    internal class TabFoldingStrategy : AbstractFoldingStrategy
    {
        // How many spaces == one tab
        private const int SpacesInTab = 4;

        /// <summary>
        /// Creates a new TabFoldingStrategy.
        /// </summary>
        internal TabFoldingStrategy()
        {
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldingsByLine(document);
        }

        /// <summary>
        /// Creates a new folding sequence 
        /// </summary>
        /// <param name="document">The current avalon edit text document</param>
        /// <returns></returns>
        internal IEnumerable<NewFolding> CreateNewFoldingsByLine(ITextSource document)
        {
            List<NewFolding> newFoldings = new List<NewFolding>();

            // Terminate if the document text is empty
            if (IsEmptyDocument(document)) return newFoldings;

            var startOffsets = new Stack<int>();
            var tabLevel = 0;

            foreach (DocumentLine line in (document as TextDocument).Lines)
            {
                var lineText = document.GetText(line.Offset, line.Length);
                if (lineText.StartsWith("#")) continue;

                var whiteSpaces = lineText.TakeWhile(Char.IsWhiteSpace).Count();

                // If the number of white spaces is multiple of tab spaces
                // then we have either a new folding or a folded text
                if (whiteSpaces > 0 && whiteSpaces % SpacesInTab == 0)
                {
                    var currentTabLevel = whiteSpaces / SpacesInTab;
                    if (tabLevel != currentTabLevel && currentTabLevel > tabLevel)
                    {
                        // Don't tab if the line is empty
                        if(string.IsNullOrEmpty(document.GetText(line.PreviousLine.Offset, line.PreviousLine.Length)))
                            continue;

                        // We found a new Tab
                        tabLevel = currentTabLevel;
                        startOffsets.Push(line.PreviousLine.Offset);
                    }
                    else if (tabLevel != currentTabLevel && currentTabLevel < tabLevel)
                    {
                        while (currentTabLevel < tabLevel)
                        {
                            // we close all nested tabs
                            var tempFolding = new NewFolding();
                            tempFolding.StartOffset = startOffsets.Pop();
                            tempFolding.EndOffset = line.PreviousLine.EndOffset;
                            newFoldings.Add(tempFolding);

                            tabLevel--;
                        }
                    }
                    else
                    {
                        // We keep going
                        continue;
                    }
                }
                else if (whiteSpaces == 0 && tabLevel > 0)
                {
                    while(tabLevel> 0)
                    { 
                        // we close all nested tabs
                        var tempFolding = new NewFolding();
                        tempFolding.StartOffset = startOffsets.Pop();
                        tempFolding.EndOffset = line.PreviousLine.EndOffset;
                        newFoldings.Add(tempFolding);

                        tabLevel --;
                    }
                }
            }

            while (startOffsets.Any())
            {
                var tempFolding = new NewFolding();
                tempFolding.StartOffset = startOffsets.Pop();
                tempFolding.EndOffset = document.TextLength;
                newFoldings.Add(tempFolding);
            }

            newFoldings.Sort((a, b) => (a.StartOffset.CompareTo(b.StartOffset)));
            return newFoldings;
        }


        private bool IsEmptyDocument(ITextSource document)
        {
            if (document == null || (document as TextDocument).LineCount <= 1)
            {
                return true;
            }
            
            return false;
        }
    }
}

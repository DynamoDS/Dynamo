using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
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
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        internal IEnumerable<NewFolding> CreateNewFoldingsByLine(ITextSource document)
        {
            List<NewFolding> newFoldings = new List<NewFolding>();

            if (document == null || (document as TextDocument).LineCount <= 1)
            {
                return newFoldings;
            }

            // Can keep track of offset ourself and from testing it seems to be accurate
            int offsetTracker = 0;

            // Keep track of start points since things nest
            Stack<int> startOffsets = new Stack<int>();

            StringBuilder lineBuffer = new StringBuilder();

            bool skip = false;

            foreach (DocumentLine line in (document as TextDocument).Lines)
            {
                if (offsetTracker >= document.TextLength)
                {
                    break;
                }

                // discard comment lines
                var lineText = document.GetText(line.Offset, line.Length).TrimStart();
                if (lineText.StartsWith("#")) skip = true;

                lineBuffer.Clear();


                // First task is to get the line and figure out the spacing in front of it
                int spaceCounter = 0;
                bool foundText = false;
                bool foundColon = false;
                int i = 0;
                
                while (i < line.Length && !(foundText && foundColon))
                {
                    char c = document.GetCharAt(offsetTracker + i);
                    char nc;
                    // we only want to tab if the line ends with ':'
                    // i.e. in case of "https://foo.com" we don't want to tab
                    try
                    {
                        nc = document.GetCharAt(offsetTracker + i + 1);  
                    }
                    catch(ArgumentOutOfRangeException ae)
                    {   
                        nc = '\r';
                    }

                    switch (c)
                    {
                        case ' ': // spaces count as one
                            if (!foundText)
                            {
                                spaceCounter++;
                            }

                            break;
                        case '\t': // Tabs count as N
                            if (!foundText)
                            {
                                spaceCounter += SpacesInTab;
                            }

                            break;
                        case ':': // Tabs count as N
                            if(!skip && IsNewLine(nc))
                                foundColon = true;
                            break;
                        default: // anything else means we encountered not spaces or tabs, so keep making the line but stop counting
                            foundText = true;
                            break;
                    }

                    i++;
                }

                // before we continue, we need to make sure its a correct multiple
                int remainder = spaceCounter % SpacesInTab;
                if (remainder > 0)
                {
                    continue;
                }

                int numTabs = spaceCounter / SpacesInTab; // we know this will be an int because of the above check
                if (numTabs >= startOffsets.Count && foundText && foundColon)    
                {

                    // we are starting a new folding
                    startOffsets.Push(offsetTracker);

                }
                else // numtabs < offsets
                {
                    // we know that this is the end of a folding. It could be the end of multiple foldings. So pop until it matches.
                    while (numTabs < startOffsets.Count)
                    {
                        int foldingStart = startOffsets.Pop();
                        NewFolding tempFolding = new NewFolding();
                        tempFolding.StartOffset = foldingStart;
                        tempFolding.EndOffset = offsetTracker - 2;
                        newFoldings.Add(tempFolding);
                    }
                }


                // Increment tracker. Much faster than getting it from the line
                offsetTracker += line.TotalLength;
                skip = false;
            }

            // Complete last foldings
            while (startOffsets.Count > 0)
            {
                int foldingStart = startOffsets.Pop();
                NewFolding tempFolding = new NewFolding();
                tempFolding.StartOffset = foldingStart;
                tempFolding.EndOffset = offsetTracker;
                newFoldings.Add(tempFolding);
            }

            newFoldings.Sort((a, b) => (a.StartOffset.CompareTo(b.StartOffset)));
            return newFoldings;
        }

        // Check if the character is a newline
        private bool IsNewLine(char c)
        {
            return c.Equals('\r');
        }
    }
}

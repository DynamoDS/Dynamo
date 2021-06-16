using System;
using System.Xml;
using Dynamo.Utilities;

namespace Dynamo.Graph.Notes
{
    /// <summary>
    /// NoteModel represents notes in Dynamo.
    /// </summary>
    public class NoteModel : ModelBase
    {
        private const string ROUND_FILLED_BULLET = "\u2022";
        private const string ROUND_EMPTY_BULLET = "\u25E6";
        private const string SQUARE_FILLED_BULLET = "\u2023";
        private string LEFT_INDENT = new String(' ', 1);
        private string TAB_INDENT = new String(' ', 2);
        private string RIGHT_INDENT = new String(' ', 1);

        private string text;
      
        /// <summary>
        /// Returns the text inside the note.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = AddBulletPointSupport(value);
                RaisePropertyChanged("Text");
            }
        }

        /// <summary>
        /// Creates NoteModel.
        /// </summary>
        /// <param name="x">X coordinate of note.</param>
        /// <param name="y">Y coordinate of note.</param>
        /// <param name="text">Text of note</param>
        /// <param name="guid">Unique id of note</param>
        public NoteModel(double x, double y, string text, Guid guid)
        {
            X = x;
            Y = y;
            Text = text;
            GUID = guid;
        }

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name != "Text") 
                return base.UpdateValueCore(updateValueParams);
            
            Text = value;
            return true;
        }        
        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("text", Text);
            helper.SetAttribute("x", X);
            helper.SetAttribute("y", Y);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            var helper = new XmlElementHelper(nodeElement);
            GUID = helper.ReadGuid("guid", GUID);
            Text = helper.ReadString("text", "New Note");
            X = helper.ReadDouble("x", 0.0);
            Y = helper.ReadDouble("y", 0.0);

            // Notify listeners that the position of the note has changed, 
            // then parent group will also redraw itself.
            ReportPosition();
        }

        #endregion

        #region BulletSupport

        /// <summary>
        /// Method used to change the text property of the note to support bullet points.
        /// It first personalizes the TAB spacing by converting it to space indentation. (The default spacing is too wide)
        /// It then converts dashes to bullet characters depending on their existing indentation.
        /// It then corrects bullet characters if their indentation has changed 
        /// </summary>
        /// <param name="text"> Input text with no bullet support </param>
        /// <returns> Bullet supported text </returns>
        private string AddBulletPointSupport(string text)
        {
            if (text.Length == 0)
                return text;

            // Definition of spacing used in indentation
            string firstTabIndent = LEFT_INDENT + TAB_INDENT;
            string secondTabIndent = LEFT_INDENT + TAB_INDENT + TAB_INDENT;
            
            // Characters and indentation used for the 3 bullet types
            string leftIndentedBullet = LEFT_INDENT + ROUND_FILLED_BULLET + RIGHT_INDENT;
            string firstTabIndentedBullet = firstTabIndent + ROUND_EMPTY_BULLET + RIGHT_INDENT;
            string secondTabIndentedBullet = secondTabIndent + SQUARE_FILLED_BULLET + RIGHT_INDENT;

            // Convert TAB input to the appropiate bullet and indentation
            text = TurnTabsToSpaces(
                text,
                leftIndentedBullet,
                firstTabIndent, firstTabIndentedBullet,
                secondTabIndentedBullet);

            // Convert dashes to bullets
            text = ConvertDashToBullet(
                text,
                leftIndentedBullet,
                firstTabIndent, firstTabIndentedBullet,
                secondTabIndent, secondTabIndentedBullet);

            // Corrects bullet characters if their indentation has changed
            text = ConvertIndentedBullets(
                text,
                firstTabIndent, firstTabIndentedBullet,
                secondTabIndent, secondTabIndentedBullet);

            return text;
        }

        /// <summary>
        /// Convert TAB input to the appropiate bullet and indentation.
        /// Supports both pressing tab before the bullet of after, similar to WORD
        /// </summary>
        /// <param name="text"> Text to transform </param>
        /// <param name="leftIndentedBullet"> Characters and indentation for normal bullet </param>
        /// <param name="firstTabIndent"> Spacing when user presses TAB 1 time </param>
        /// <param name="firstTabIndentedBullet"> Characters and indentation when user presses TAB 1 time </param>
        /// <param name="secondTabIndentedBullet"> Characters and indentation when user presses TAB 2 time </param>
        /// <returns> transformed text </returns>
        private string TurnTabsToSpaces(
            string text,
            string leftIndentedBullet,
            string firstTabIndent,
            string firstTabIndentedBullet,
            string secondTabIndentedBullet)
        {
            text = ReplaceEachLineStart(text, leftIndentedBullet + "\t", firstTabIndentedBullet);
            text = ReplaceEachLineStart(text, firstTabIndentedBullet + "\t", secondTabIndentedBullet);
            text = ReplaceEachLineStart(text, "\t" + LEFT_INDENT, firstTabIndent);
            text = ReplaceEachLineStart(text, "\t", firstTabIndent);
            text = text.Replace("\t", TAB_INDENT);
            return text;
        }

        /// <summary>
        /// Convert dashes to bullets according to their existing indentation
        /// </summary>
        /// <param name="text"> text to transform </param>
        /// <param name="leftIndentedBullet"> Characters and indentation for normal bullet </param>
        /// <param name="firstTabIndent"> Spacing when user presses TAB 1 time </param>
        /// <param name="firstTabIndentedBullet"> Characters and indentation when user presses TAB 1 time </param>
        /// <param name="secondTabIndent">  Spacing when user presses TAB 2 time </param>
        /// <param name="secondTabIndentedBullet">  Characters and indentation when user presses TAB 2 times </param>
        /// <returns></returns>
        private string ConvertDashToBullet(
            string text,
            string leftIndentedBullet,
            string firstTabIndent,
            string firstTabIndentedBullet,
            string secondTabIndent,
            string secondTabIndentedBullet
            )
        {
            text = ReplaceEachLineStart(text, "-", leftIndentedBullet);
            text = ReplaceEachLineStart(text, firstTabIndent + "-", firstTabIndentedBullet);
            text = ReplaceEachLineStart(text, secondTabIndent + "-", secondTabIndentedBullet);

            return text;
        }

        /// <summary>
        /// Corrects bullet characters if their indentation has changed
        /// </summary>
        /// <param name="text"> text to transform </param>
        /// <param name="firstTabIndent"> Spacing when user presses TAB 1 time </param>
        /// <param name="firstTabIndentedBullet"> Characters and indentation when user presses TAB 1 time </param>
        /// <param name="secondTabIndent"> Spacing when user presses TAB 2 time </param>
        /// <param name="secondTabIndentedBullet"> Characters and indentation when user presses TAB 2 times </param>
        /// <returns></returns>
        private string ConvertIndentedBullets(
            string text,
            string firstTabIndent,
            string firstTabIndentedBullet,
            string secondTabIndent,
            string secondTabIndentedBullet
            )
        {
            text = ReplaceEachLineStart(text, firstTabIndent + ROUND_FILLED_BULLET, firstTabIndentedBullet);
            text = ReplaceEachLineStart(text, secondTabIndent + ROUND_EMPTY_BULLET, secondTabIndentedBullet);
            return text;
        }

        /// <summary>
        /// Replaces text that is located at the beggining of a line.
        /// Used in bullet point support as bullet replacement 
        /// should only happen at beggining of line. 
        /// </summary>
        /// <param name="text"> text to transform </param>
        /// <param name="oldChars"> characters at begging of line to transform </param>
        /// <param name="newChars"> characters to replace it with </param>
        /// <returns></returns>
        private string ReplaceEachLineStart(string text, string oldChars, string newChars)
        {
            if (text.StartsWith(oldChars))
            {
                text = text.Remove(0, oldChars.Length);
                text = newChars + text;
            }
            text = text.Replace("\n" + oldChars, "\n" + newChars);
            return text;
        }

        #endregion
    }
}

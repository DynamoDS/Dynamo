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

        private string AddBulletPointSupport(string text)
        {
            if (text.Length == 0)
                return text;

            string firstTabIndent = LEFT_INDENT + TAB_INDENT;
            string secondTabIndent = LEFT_INDENT + TAB_INDENT + TAB_INDENT;
            string leftIndentedBullet = LEFT_INDENT + ROUND_FILLED_BULLET + RIGHT_INDENT;
            string firstTabIndentedBullet = firstTabIndent + ROUND_EMPTY_BULLET + RIGHT_INDENT;
            string secondTabIndentedBullet = secondTabIndent + SQUARE_FILLED_BULLET + RIGHT_INDENT;

            text = TurnTabsToSpaces(
                text,
                leftIndentedBullet,
                firstTabIndent, firstTabIndentedBullet,
                secondTabIndentedBullet);

            text = ConvertDashToBullet(
                text,
                leftIndentedBullet,
                firstTabIndent, firstTabIndentedBullet,
                secondTabIndent, secondTabIndentedBullet);
            text = ConvertIndentedBullets(
                text,
                firstTabIndent, firstTabIndentedBullet,
                secondTabIndent, secondTabIndentedBullet);

            return text;
        }

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

        private string AddNextBullet(
            string text,
            string leftIndentedBullet,
            string firstTabIndentedBullet,
            string secondTabIndentedBullet)
        {

            if (!text.EndsWith("\n"))
                return text;
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var previousLine = lines[lines.Length - 2];

            if (previousLine.StartsWith(leftIndentedBullet))
                text = text + leftIndentedBullet;

            if (previousLine.StartsWith(firstTabIndentedBullet))
                text = text + firstTabIndentedBullet;

            if (previousLine.StartsWith(secondTabIndentedBullet))
                text = text + secondTabIndentedBullet;

            return text;
        }

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

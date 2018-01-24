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
        private string text;
      
        /// <summary>
        /// Returns the text inside the note.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
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

        protected override void RuntimeSerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("text", Text);
            helper.SetAttribute("x", X);
            helper.SetAttribute("y", Y);
        }

        protected override void RuntimeDeserializeCore(XmlElement nodeElement, SaveContext context)
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
    }
}

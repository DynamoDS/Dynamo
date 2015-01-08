using System;
using System.Xml;

using Dynamo.Core;
using Dynamo.Utilities;
namespace Dynamo.Models
{
    public class NoteModel : ModelBase
    {
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged(/*NXLT*/"Text");
            }
        }

        public NoteModel(double x, double y, string text, Guid guid)
        {
            X = x;
            Y = y;
            Text = text;
            GUID = guid;
        }

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(string name, string value, UndoRedoRecorder recorder)
        {
            if (name != /*NXLT*/"Text") 
                return base.UpdateValueCore(name, value, recorder);
            
            Text = value;
            return true;
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute(/*NXLT*/"guid", GUID);
            helper.SetAttribute(/*NXLT*/"text", Text);
            helper.SetAttribute(/*NXLT*/"x", X);
            helper.SetAttribute(/*NXLT*/"y", Y);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            var helper = new XmlElementHelper(nodeElement);
            GUID = helper.ReadGuid(/*NXLT*/"guid", GUID);
            Text = helper.ReadString(/*NXLT*/"text", /*NXLT*/"New Note");
            X = helper.ReadDouble(/*NXLT*/"x", 0.0);
            Y = helper.ReadDouble(/*NXLT*/"y", 0.0);
        }

        #endregion
    }
}

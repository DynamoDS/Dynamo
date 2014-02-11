using System.Xml;
using Dynamo.Utilities;
namespace Dynamo.Models
{
    public class NoteModel:ModelBase
    {

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }
        }

        public NoteModel(double x, double y)
        {
            X = x;
            Y = y;
        }

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Text")
            {
                Text = value;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("text", Text);
            helper.SetAttribute("x", X);
            helper.SetAttribute("y", Y);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            GUID = helper.ReadGuid("guid", GUID);
            Text = helper.ReadString("text", "New Note");
            X = helper.ReadDouble("x", 0.0);
            Y = helper.ReadDouble("y", 0.0);
        }

        #endregion
    }
}

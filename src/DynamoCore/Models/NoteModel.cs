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

        #region Serialization/Deserialization Methods

        protected override XmlNode SerializeCore(XmlDocument xmlDocument)
        {
            string typeName = this.GetType().ToString();
            XmlElement noteElement = xmlDocument.CreateElement(typeName);
            XmlElementHelper helper = new XmlElementHelper(noteElement);

            helper.SetAttribute("text", this.Text);
            helper.SetAttribute("x", this.X);
            helper.SetAttribute("y", this.Y);
            return noteElement;
        }

        protected override void DeserializeCore(XmlNode xmlNode)
        {
        }

        #endregion
    }
}

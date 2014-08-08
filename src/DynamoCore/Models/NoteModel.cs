using System.Xml;
using Dynamo.Utilities;
namespace Dynamo.Models
{
    public class NoteModel:ModelBase
    {

        private readonly WorkspaceModel workspaceModel;

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

        public NoteModel(WorkspaceModel workspace, double x, double y)
        {
            this.workspaceModel = workspace;
            X = x;
            Y = y;
        }

        #region Command Framework Supporting Methods

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Text")
            {
                this.Text = value;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", this.GUID);
            helper.SetAttribute("text", this.Text);
            helper.SetAttribute("x", this.X);
            helper.SetAttribute("y", this.Y);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            this.GUID = helper.ReadGuid("guid", this.GUID);
            this.Text = helper.ReadString("text", "New Note");
            this.X = helper.ReadDouble("x", 0.0);
            this.Y = helper.ReadDouble("y", 0.0);
        }

        #endregion
    }
}

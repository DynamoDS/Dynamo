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
                RaisePropertyChanged(/*NXLT*/"Text");
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
            if (name == /*NXLT*/"Text")
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
            helper.SetAttribute(/*NXLT*/"guid", this.GUID);
            helper.SetAttribute(/*NXLT*/"text", this.Text);
            helper.SetAttribute(/*NXLT*/"x", this.X);
            helper.SetAttribute(/*NXLT*/"y", this.Y);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            this.GUID = helper.ReadGuid(/*NXLT*/"guid", this.GUID);
            this.Text = helper.ReadString(/*NXLT*/"text", /*NXLT*/"New Note");
            this.X = helper.ReadDouble(/*NXLT*/"x", 0.0);
            this.Y = helper.ReadDouble(/*NXLT*/"y", 0.0);
        }

        #endregion
    }
}

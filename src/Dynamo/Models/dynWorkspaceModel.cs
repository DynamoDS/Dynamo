using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Connectors;
using MS.Internal.Xml.XPath;

namespace Dynamo
{
    public class dynWorkspaceModel:dynModelBase
    {
        public event EventHandler NodeAdded;
        public event EventHandler ConnectorAdded;
        public event EventHandler NoteAdded;

        protected virtual void OnNodeAdded(object sender, DynamoModelUpdateArgs e)
        {
            if (NodeAdded != null)
                NodeAdded(this, e);
        }
        protected virtual void OnConnectorAdded(object sender, DynamoModelUpdateArgs e)
        {
            if (ConnectorAdded != null)
                ConnectorAdded(this, e);
        }
        protected virtual void OnNoteAdded(object sender, DynamoModelUpdateArgs e)
        {
            if (NoteAdded != null)
                NoteAdded(this, e);
        }

        public List<dynConnectorModel> Connectors { get; set; }
        public List<dynNodeModel> Nodes { get; set; }
        public List<dynNoteModel> Notes { get; set; } 

        public dynWorkspaceModel()
        {
            Connectors = new List<dynConnectorModel>();
            Nodes = new List<dynNodeModel>();
            Notes = new List<dynNoteModel>();
        }

        public void AddConnector()
        {
            dynConnectorModel connector = new dynConnectorModel();
            OnConnectorAdded(this, new DynamoModelUpdateArgs(connector));
        }
        public void AddNode()
        {
            dynNodeModel node = new dynNodeModel();
            OnNodeAdded(this, new DynamoModelUpdateArgs(node));
        }
        public void AddNote()
        {
            dynNoteModel note = new dynNoteModel();
            OnNoteAdded(this, new DynamoModelUpdateArgs(note));
        }

    }
}

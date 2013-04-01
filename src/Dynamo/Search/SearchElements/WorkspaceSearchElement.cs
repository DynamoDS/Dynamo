using System;
using Dynamo.Commands;

namespace Dynamo.Search
{
    public class WorkspaceSearchElement : SearchElementBase
    {
        private string _description;
        private string _name;

        public WorkspaceSearchElement(string symbol, string description)
        {
            this._name = symbol;
            this._description = description;
            this.Weight = 1;
        }

        public override void Execute()
        {
            var name = this.Name;
            if (name == "Home")
            {
                DynamoCommands.HomeCmd.Execute(null);
            }
            else
            {
                var guid = this.Guid;
                DynamoCommands.GoToWorkspaceCmd.Execute(guid);
            }
        }

        public Guid Guid { get; set; }
        public override string Name { get { return _name; } }
        public override string Type { get { return "Workspace"; } }
        public override string Description { get { return _description; } }
        public override double Weight { get; set; }
    }
}

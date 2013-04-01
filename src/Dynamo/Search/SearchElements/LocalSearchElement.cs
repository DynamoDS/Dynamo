using System;
using System.Collections.Generic;
using Dynamo.Commands;
using Dynamo.Nodes;

namespace Dynamo.Search
{
    public class LocalSearchElement : SearchElementBase
    {
        public LocalSearchElement(dynNode node)
        {
            this.Node = node;
            this.Weight = 1;
        }

        public override void Execute()
        {
            DynamoCommands.CreateNodeCmd.Execute(new Dictionary<string, object>()
                {
                    {"name", this.Name},
                    {"transformFromOuterCanvasCoordinates", true},
                    {"guid", Guid.NewGuid() }
                });
        }

        public dynNode Node { get; internal set; }

        public override string Type { get { return "Standard Node"; } }
        public override string Name { get { return Node.NodeUI.NickName; } }
        public override string Description { get { return Node.NodeUI.Description; } }
        public override double Weight { get; set; }
    }

}

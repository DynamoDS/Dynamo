using System;
using System.Collections.Generic;
using Dynamo.Commands;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Greg.Responses;

namespace Dynamo.Search
{

    public class PackageManagerSearchElement : SearchElementBase
    {

        public PackageManagerSearchElement(PackageHeader header)
        {
            this.Header = header;
            this.Guid = PackageManagerClient.ExtractFunctionDefinitionGuid(header, 0);
            this.Weight = 1;
        }

        public override void Execute()
        {
            Guid guid = this.Guid;

            if (!dynSettings.FunctionDict.ContainsKey(this.Guid))
            {
                // go get the node from online, place it in view asynchronously
                dynSettings.Controller.PackageManagerClient.Download(this.Id, "", (finalGuid) => DynamoCommands
                    .CreateNodeCmd.Execute(new Dictionary<string, object>()
                        {
                            { "name", guid.ToString() },
                            { "transformFromOuterCanvasCoordinates", true },
                            { "guid", Guid.NewGuid() }
                        })
                );
            }
            else
            {
                // get the node from here
                DynamoCommands.CreateNodeCmd.Execute(new Dictionary<string, object>()
                    {
                        {"name", this.Guid.ToString() },
                        {"transformFromOuterCanvasCoordinates", true},
                        {"guid", Guid.NewGuid() }
                    });
            }
        }

        public PackageHeader Header { get; internal set; }

        public override string Name { get { return Header.name; } }
        public override string Description { get { return Header.description; } }

        public Guid Guid { get; internal set; }
        public string Id { get { return Header._id; } }
        public override string Type { get { return "Community Node"; } }
        public List<String> Keywords { get { return Header.keywords; } }
        public string Group { get { return Header.group; } }
        public override double Weight { get; set; }

    }

}

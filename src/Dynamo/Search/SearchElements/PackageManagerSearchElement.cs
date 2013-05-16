//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Windows;
using Dynamo.Commands;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Greg.Responses;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// A search element representing an element from the package manager </summary>
    public class PackageManagerSearchElement : SearchElementBase
    {

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="header">The PackageHeader object describing the element</param>
        public PackageManagerSearchElement(PackageHeader header)
        {
            this.Header = header;
            this.Guid = PackageManagerClient.ExtractFunctionDefinitionGuid(header, 0);
            this.Weight = 1;
            if (header.keywords.Count > 0)
            {
                this.Keywords = String.Join(" ", header.keywords);
            } 
            else
            {
                this.Keywords = "";
            }
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.  This either attempts to download the node, 
        /// or gets the local node if already downloaded. </summary>
        public override void Execute()
        {
            Guid guid = this.Guid;

            //dynSettings.Controller.SearchViewModel.Visible = Visibility.Collapsed;

            if ( !dynSettings.Controller.CustomNodeLoader.Contains(guid) )
            {
                // go get the node from online, place it in view asynchronously
                dynSettings.Controller.PackageManagerClient.Download(this.Id, "", (finalGuid) => 
                    dynSettings.Controller.DynamoViewModel.CreateNodeCommand.Execute(new Dictionary<string, object>()
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
                dynSettings.Controller.DynamoViewModel.CreateNodeCommand.Execute(new Dictionary<string, object>()
                    {
                        {"name", this.Guid.ToString() },
                        {"transformFromOuterCanvasCoordinates", true},
                        {"guid", Guid.NewGuid() }
                    });
            }
        }

        #region Properties 
            /// <summary>
            /// Header property </summary>
            /// <value>
            /// The PackageHeader used to instantiate this object </value>
            public PackageHeader Header { get; internal set; }

            /// <summary>
            /// Type property </summary>
            /// <value>
            /// A string describing the type of object </value>
            public override string Type { get { return "Community Node"; } }

            /// <summary>
            /// Name property </summary>
            /// <value>
            /// The name of the node </value>
            public override string Name { get { return Header.name; } }

            /// <summary>
            /// Description property </summary>
            /// <value>
            /// A string describing what the node does</value>
            public override string Description { get { return Header.description; } }

            /// <summary>
            /// Weight property </summary>
            /// <value>
            /// Number defining the relative importance of the element in search. 
            /// Higher = closer to the top of search results </value>
            public override double Weight { get; set; }

            /// <summary>
            /// Guid property </summary>
            /// <value>
            /// A string that uniquely defines the FunctionDefinition </value>
            public Guid Guid { get; internal set; }

            /// <summary>
            /// Id property </summary>
            /// <value>
            /// A string that uniquely defines the Package on the server  </value>
            public string Id { get { return Header._id; } }

        #endregion
        
            public override string Keywords { get; set; }
    }

}

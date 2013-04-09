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
using Dynamo.Utilities;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// A search element representing a workspace that can be navigated to </summary>
    public class WorkspaceSearchElement : SearchElementBase
    {

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="symbol">The name of the symbol for the workspace</param>
        /// <param name="description">A description of the workspace</param>
        public WorkspaceSearchElement(string symbol, string description)
        {
            this._name = symbol;
            this._description = description;
            this.Weight = 1;
            this.Keywords = "";
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.  Navigates to the selected workspace </summary>
        public override void Execute()
        {
            //dynSettings.Controller.SearchViewModel.Visible = Visibility.Collapsed;

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

        #region Properties

            /// <summary>
            /// Guid property </summary>
            /// <value>
            /// A string that uniquely defines the Workspace </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Name property </summary>
            /// <value>
            /// The name of the node </value>
            private string _name;
            public override string Name { get { return _name; } }

            /// <summary>
            /// Type property </summary>
            /// <value>
            /// A string describing the type of object </value>
            public override string Type { get { return "Workspace"; } }

            /// <summary>
            /// Description property </summary>
            /// <value>
            /// A string describing what the node does</value>
            ///         
            private string _description;
            public override string Description { get { return _description; } }

            /// <summary>
            /// Weight property </summary>
            /// <value>
            /// Number defining the relative importance of the element in search.  Higher the better </value>
            public override double Weight { get; set; }

            /// <summary>
            /// Keywords property </summary>
            /// <value>
            /// Empty for workspaces</value>
            public override string Keywords { get; set; }

        #endregion

            
    }
}

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
using System.Linq;
using System.Windows;
using System.Windows.Input;
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
        public PackageManagerSearchElement(Greg.Responses.PackageHeader header)
        {
            this.Header = header;
            this.Weight = 1;
            if (header.keywords != null && header.keywords.Count > 0)
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
            string message = "";
            if (dynSettings.PackageLoader.LocalPackages.Any(pkg => this.Name == pkg.Name))
            {
                message = "Dynamo has already installed " + this.Name + ".  Please uninstall the existing package before downloading.";
                MessageBox.Show(message, "Cannot Download Package", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            message = "Are you sure you want to install " + this.Name +"?";

            var result = MessageBox.Show(message, "Package Download Confirmation",
                            MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                var dl = new PackageDownloadHandle(this.Header, this.LatestVersion); // download the most recent version

                dynSettings.Controller.PackageManagerClient.DownloadAndInstall(dl);
            }

        }

        #region Properties 
            
            public List<PackageVersion> Versions { get { return this.Header.versions; } }

            public string Maintainers { get { return String.Join(", ", this.Header.maintainers.Select(x=>x.username)); } }

            public int Votes { get { return this.Header.votes; } }

            public int Downloads { get { return this.Header.downloads; } }

            public string EngineVersion { get { return this.Header.engine_version; } }

            public int UsedBy { get { return this.Header.used_by.Count; } } 

            public string LatestVersion { get { return Header.versions[Header.versions.Count - 1].version; } }
            
            /// <summary>
            /// Header property </summary>
            /// <value>
            /// The PackageHeader used to instantiate this object </value>
            public Greg.Responses.PackageHeader Header { get; internal set; }

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
            public override string Description { get { return Header.description ?? ""; } }

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

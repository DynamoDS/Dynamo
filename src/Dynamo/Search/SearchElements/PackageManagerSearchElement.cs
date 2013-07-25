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
using Microsoft.Practices.Prism.Commands;

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
            var version = _versionToDownload ?? this.Header.versions.Last();

            string message = "Are you sure you want to install " + this.Name +" "+ version.version + "?";

            var result = MessageBox.Show(message, "Package Download Confirmation",
                            MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                // get all of the headers
                var headers = version.full_dependency_ids.Select((id) =>
                    {
                        PackageHeader pkgHeader;
                        var res = dynSettings.Controller.PackageManagerClient.DownloadPackageHeader(id, out pkgHeader);
                        
                        if (!res.Success)
                            MessageBox.Show("Failed to download package with id: " + id + ".  Please try again and report the package if you continue to have problems.", "Package Download Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);

                        return pkgHeader;
                    }).ToList();

                // if any header download fails, abort
                if (headers.Any(x => x == null))
                {
                    return;
                }

                var localPkgs = dynSettings.PackageLoader.LocalPackages;

                // if a package is already installed we need to uninstall it
                foreach ( var localPkg in headers.Select(x => localPkgs.FirstOrDefault(v => v.Name == x.name)) )
                {
                    if (localPkg == null) continue;
                    string msg;

                    // if the package is in use, we will not be able to uninstall it.  
                    if (!localPkg.UninstallCommand.CanExecute())
                    {
                        msg = "Dynamo needs to uninstall " + this.Name + " to continue, but cannot as one of its types appears to be in use.  Try restarting Dynamo.";
                        MessageBox.Show(msg, "Cannot Download Package", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        return;
                    }

                    // if the package is not in use, tell the user we will be uninstall it and give them the opportunity to cancel
                    msg = "Dynamo has already installed " + this.Name + ".  \n\nDynamo will attempt to uninstall this package before installing.  ";
                    if ( MessageBox.Show(msg, "Download Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                        return;
                }

                // form header version pairs and download and install all packages
                headers.Zip(version.full_dependency_versions, (header, v) => new Tuple<PackageHeader, string>(header, v))
                        .Select( x=> new PackageDownloadHandle(x.Item1, x.Item2))
                        .ToList()
                        .ForEach(x=>x.Start());

            }

        }

        #region Properties 

            public bool _showingFull;
            public bool ShowingFull { get { return _showingFull; } set { _showingFull = value; RaisePropertyChanged("ShowingFull"); } }
            private PackageVersion _versionToDownload = null;

            public List<Tuple<PackageVersion, DelegateCommand>> Versions
            {
                get
                {
                    return
                        Header.versions.Select(
                            x => new Tuple<PackageVersion, DelegateCommand>(x, new DelegateCommand(() =>
                                {
                                    this._versionToDownload = x;
                                    this.Execute();
                                }, () => true))).ToList();
                } 
            }
            public string Maintainers { get { return String.Join(", ", this.Header.maintainers.Select(x=>x.username)); } }
            public int Votes { get { return this.Header.votes; } }
            public int Downloads { get { return this.Header.downloads; } }
            public string EngineVersion { get { return Header.versions[Header.versions.Count - 1].engine_version; } }
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

            public override string Keywords { get; set; }

        #endregion
        

    }

}

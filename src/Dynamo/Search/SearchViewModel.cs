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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Commands;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Greg.Responses;

namespace Dynamo.Search
{
    public interface ISearchElement
    {
        string Type { get; }
        string Name { get; }
        string Description { get; }
    }
    
    public class LocalSearchElement : ISearchElement
    {
        public LocalSearchElement(dynNode node)
        {
            this.Node = node;
        }

        public dynNode Node { get; internal set; }
        public string Type { get { return "Standard Node"; } }
        public string Name { get { return Node.NodeUI.NickName; } }
        public string Description { get { return Node.NodeUI.Description; } }
    }

    public class WorkspaceSearchElement : ISearchElement
    {
        private string _description;
        private string _name;

        public WorkspaceSearchElement(string symbol, string description)
        {
            this._name = symbol;
            this._description = "Workspace";
        }

        public Guid Guid { get; set; }
        public string Name { get { return _name; } }
        public string Type { get { return "Workspace"; } }
        public string Description { get { return _description; } }
    }

    public class PackageManagerSearchElement : ISearchElement
    {
        
        public PackageManagerSearchElement(PackageHeader header )
        {
            this.Header = header;
            this.Guid = PackageManagerClient.ExtractFunctionDefinitionGuid(header, 0); 
        }

        public PackageHeader Header { get; internal set;  }

        public string Name { get { return Header.name; } }
        public string Description { get { return Header.description; } }

        public Guid Guid { get; internal set; }
        public string Id { get { return Header._id; } }
        public string Type { get { return "Community Node"; } }
        public List<String> Keywords { get { return Header.keywords; } }
        public string Group { get { return Header.group;  } }
        
    }

    public class SearchViewModel
    {

        #region Properties

            public SearchDictionary<ISearchElement> SearchDictionary { get; internal set; }
            public ObservableCollection<ISearchElement> VisibleNodes { get; internal set; }
            public int NumSearchResults { get; set; }
            public SearchUI View { get; internal set; }
            public dynBench Bench { get; internal set; }

        #endregion

        public SearchViewModel( dynBench bench )
        {
            
            this.SearchDictionary = new SearchDictionary<ISearchElement>();
            this.VisibleNodes = new ObservableCollection<ISearchElement>();
            this.NumSearchResults = 10;
            this.Bench = bench;
            this.View = new SearchUI(this);

            this.AddHomeToSearch();
        }

        private void AddHomeToSearch()
        {
            this.SearchDictionary.AddName(new WorkspaceSearchElement("Home", "The default workspace"), "Home");
        }

        internal void SearchAndUpdateUI(string search)
        {
            if (View.Visibility != Visibility.Visible)
                return;

            VisibleNodes.Clear();

            foreach (var node in this.Search(search))
            {
                VisibleNodes.Add(node);
            }

            this.View.SetSelected(0);
        }

        internal void SearchAndUpdateUI()
        {
            SearchAndUpdateUI(View.SearchTextBox.Text);
        }

        internal List<ISearchElement> Search(string search)
        {
            return SearchDictionary.RegexSearch(search, this.NumSearchResults);
        }

        public void KeyHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.SendSelectedToWorkspace();
            }
            else if (e.Key == Key.Down)
            {
                this.View.SelectNext();
            }
            else if (e.Key == Key.Up)
            {
                this.View.SelectPrevious();
            }
        }

        public void SendSelectedToWorkspace()
        {
            if (VisibleNodes.Count == 0) return;

            int selectedIndex = View.SelectedIndex();

            // none of the elems are selected, return 
            if (selectedIndex == -1)
                return;

            View.Visibility = Visibility.Collapsed;

            if (VisibleNodes[selectedIndex] is LocalSearchElement)
            {
                DynamoCommands.CreateNodeCmd.Execute(new Dictionary<string, object>()
                {
                    {"name", VisibleNodes[selectedIndex].Name},
                    {"transformFromOuterCanvasCoordinates", true},
                    {"guid", Guid.NewGuid() }
                });
            } else if (VisibleNodes[selectedIndex] is PackageManagerSearchElement)
            {
                var ele = (PackageManagerSearchElement) VisibleNodes[selectedIndex];

                Guid guid = ele.Guid;

                if (!dynSettings.FunctionDict.ContainsKey(ele.Guid))
                {
                    // go get the node from online, place it in view asynchronously
                    dynSettings.Controller.PackageManagerClient.Download(ele.Id, "", (finalGuid) => DynamoCommands
                            .CreateNodeCmd.Execute(new Dictionary<string
                                                                                                                      ,
                                                                                                                      object
                                                                                                                      >()
                                                                                                             {
                                                                                                                 {
                                                                                                                     "name"
                                                                                                                     ,
                                                                                                                     guid
                                                                                                                      .ToString
                                                                                                                      ()
                                                                                                                 },
                                                                                                                 {
                                                                                                                     "transformFromOuterCanvasCoordinates"
                                                                                                                     ,
                                                                                                                     true
                                                                                                                 },
                                                                                                                 {
                                                                                                                     "guid"
                                                                                                                     ,
                                                                                                                     Guid
                                                                                                                      .NewGuid
                                                                                                                      ()
                                                                                                                 }
                                                                                                             })
                        );


                }
                else
                {
                    // get the node from here
                    DynamoCommands.CreateNodeCmd.Execute(new Dictionary<string, object>()
                    {
                        {"name", ((PackageManagerSearchElement) VisibleNodes[selectedIndex]).Guid.ToString() },
                        {"transformFromOuterCanvasCoordinates", true},
                        {"guid", Guid.NewGuid() }
                    });
                }
                
               

            } else if ( VisibleNodes[selectedIndex] is WorkspaceSearchElement )
            {
                var name = VisibleNodes[selectedIndex].Name;
                if (name == "Home")
                {
                    DynamoCommands.HomeCmd.Execute(null);
                }
                else
                {
                    var guid = ((WorkspaceSearchElement)VisibleNodes[selectedIndex]).Guid;
                    DynamoCommands.GoToWorkspaceCmd.Execute(guid);
                }
                
            }

        }

        public void Add(PackageHeader packageHeader)
        {
            var searchEle = new PackageManagerSearchElement(packageHeader);
            SearchDictionary.AddName(searchEle, searchEle.Name);
            this.SearchAndUpdateUI();
        }

        public void Add(dynWorkspace workspace)
        {
            this.Add(workspace, workspace.Name);
        }

        public void Add(dynWorkspace workspace, string name )
        {
            var searchEle = new WorkspaceSearchElement(name, "Workspace");
            searchEle.Guid = dynSettings.FunctionDict.First(x => x.Value.Workspace == workspace).Key;
            // must store guid
            SearchDictionary.AddName(searchEle, searchEle.Name);
            this.SearchAndUpdateUI();
        }

        public void Add(Type type, string name)
        {

            dynNode dynNode = null;

            try
            {
                var obj = Activator.CreateInstance(type);
                dynNode = (dynNode)obj;
            }
            catch (Exception e)
            {
                Bench.Log("Error creating search element for: " + name);
                Bench.Log(e.InnerException);
                return;
            }

            var nodeUI = dynNode.NodeUI;
            nodeUI.DisableInteraction();
            nodeUI.Margin = new Thickness(5, 30, 5, 5);
            nodeUI.LayoutTransform = new ScaleTransform(0.8, 0.8);

            nodeUI.MouseDown += delegate
            {
                Bench.BeginDragElement(nodeUI, name, Mouse.GetPosition(nodeUI));
                nodeUI.Visibility = System.Windows.Visibility.Hidden;
            };

            nodeUI.GUID = new Guid();

            var searchEle = new LocalSearchElement(dynNode);

            SearchDictionary.AddName(searchEle, searchEle.Name);

        }

        public void Refactor(dynWorkspace currentSpace, string newName)
        {
            SearchDictionary.RemoveName(currentSpace.Name);
            this.Add( currentSpace, newName );
        }
    }
}

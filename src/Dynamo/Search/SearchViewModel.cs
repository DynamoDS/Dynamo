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
using Dynamo.Commands;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Greg.Responses;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Search
{
    public class SearchViewModel : NotificationObject
    {

        #region Properties

            public SearchDictionary<SearchElementBase> SearchDictionary { get; private set; }
            public ObservableCollection<SearchElementBase> SearchResults { get; private set; }
            public int MaxNumSearchResults { get; set; }
            public dynBench Bench { get; private set; }

            private int _selectedIndex;
            public int SelectedIndex
            {
                get { return _selectedIndex; }
                set
                {
                    if (this._selectedIndex != value)
                    {
                        this._selectedIndex = value;

                        //if (i < this.SearchResultsListBox.Items.Count)
                        //    this.SearchResultsListBox.ScrollIntoView(this.SearchResultsListBox.Items[i]);

                        RaisePropertyChanged("SelectedIndex");
                    }
                }
            }

            private Visibility _visible;
            public Visibility Visible
            {
                get { return _visible; }
                set
                {
                    if (this._visible != value)
                    {
                        this._visible = value;
                        RaisePropertyChanged("Visible");
                    }
                }
            }

            public string _SearchText;
            public string SearchText
            {
                get { return _SearchText; }
                set
                {
                    _SearchText = value; 
                    RaisePropertyChanged("SearchText");
                    DynamoCommands.SearchCmd.Execute(null);
            }}

            public bool _IncludePackageManagerSearchElements;
            public bool IncludePackageManagerSearchElements
            {
                get { return _IncludePackageManagerSearchElements; }
                set
                {
                    _IncludePackageManagerSearchElements = value;
                    RaisePropertyChanged("IncludePackageManagerSearchElements");
                    DynamoCommands.RefreshRemotePackagesCmd.Execute(null);
                }
            }

        #endregion

        public SearchViewModel( dynBench bench )
        {
            this.SelectedIndex = 0;
            this.SearchDictionary = new SearchDictionary<SearchElementBase>();
            this.SearchResults = new ObservableCollection<SearchElementBase>();
            this.MaxNumSearchResults = 10;
            this.Bench = bench;
            this.Visible = Visibility.Collapsed;
            this._SearchText = "";
            this.AddHomeToSearch();
        }

        private void AddHomeToSearch()
        {
            this.SearchDictionary.Add(new WorkspaceSearchElement("Home", "Workspace"), "Home");
        }

        internal void SearchAndUpdateResults(string search)
        {
            if (this.Visible != Visibility.Visible)
                return;

            SearchResults.Clear();

            foreach (var node in this.Search(search))
            {
                SearchResults.Add(node);
            }

            SelectedIndex = 0;
        }
        
        public void SelectNext()
        {
            if (SelectedIndex == SearchResults.Count - 1
                || SelectedIndex == -1)
                return;

            SelectedIndex = SelectedIndex + 1;
        }

        public void SelectPrevious()
        {
            if (SelectedIndex == 0 || SelectedIndex == -1)
                return;

            SelectedIndex = SelectedIndex - 1;
        }

        internal void SearchAndUpdateResults()
        {
            SearchAndUpdateResults(SearchText);
        }

        internal List<SearchElementBase> Search(string search)
        {
            return SearchDictionary.Search(search, this.MaxNumSearchResults);
        }

        public void KeyHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ExecuteSelected();
            }
            else if (e.Key == Key.Down)
            {
                this.SelectNext(); // nope
            }
            else if (e.Key == Key.Up)
            {
                this.SelectPrevious(); // nope
            }
        }

        public void ExecuteSelected()
        {
            if (SearchResults.Count == 0) return;

            // none of the elems are selected, return 
            if (SelectedIndex == -1)
                return;

            this.Visible = Visibility.Collapsed;
            SearchResults[SelectedIndex].Execute();

        }

        public void Add(PackageHeader packageHeader)
        {
            var searchEle = new PackageManagerSearchElement(packageHeader);
            SearchDictionary.Add(searchEle, searchEle.Name);
            this.SearchAndUpdateResults();
        }

        public void Add(dynWorkspace workspace)
        {
            this.Add(workspace, workspace.Name);
        }

        public void Add(dynWorkspace workspace, string name )
        {
            var searchEle = new WorkspaceSearchElement(name, "Workspace");
            searchEle.Guid = dynSettings.FunctionDict.First(x => x.Value.Workspace == workspace).Key;
            SearchDictionary.Add(searchEle, searchEle.Name);
            this.SearchAndUpdateResults();
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

            var searchEle = new LocalSearchElement(dynNode);
            SearchDictionary.Add(searchEle, searchEle.Name);

        }

        public void Refactor(dynWorkspace currentSpace, string newName)
        {
            SearchDictionary.Remove(currentSpace.Name);
            this.Add( currentSpace, newName );
        }

    }
}

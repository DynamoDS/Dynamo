using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dynamo.Commands;
using Dynamo.Nodes;
using Dynamo.Search.Regions;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Greg.Responses;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Search
{
    /// <summary>
    ///     This is the core ViewModel for searching
    /// </summary>
    public class SearchViewModel : NotificationObject
    {
        #region Properties

        /// <summary>
        ///     Regions property
        /// </summary>
        /// <value>
        ///     Specifies different regions to search over.  The command toggles whether searching
        ///     over that field or not.
        /// </value>
        public ObservableDictionary<string, RegionBase> Regions { get; set; }

        /// <summary>
        ///     IncludeOptionalElements property
        /// </summary>
        /// <value>
        ///     Specifies whether we are including Revit API elements in search.
        /// </value>
        public bool _IncludeOptionalElements;

        public bool IncludeOptionalElements
        {
            get { return _IncludeOptionalElements; }
            set
            {
                _IncludeOptionalElements = value;
                RaisePropertyChanged("IncludeOptionalElements");
                ToggleIncludingRevitAPIElements();
            }
        }

        /// <summary>
        ///     SearchText property
        /// </summary>
        /// <value>
        ///     This is the core UI for Dynamo, primarily used for logging.
        /// </value>
        public string _SearchText;

        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                _SearchText = value;
                RaisePropertyChanged("SearchText");
                DynamoCommands.Search.Execute(null);
            }
        }

        /// <summary>
        ///     SelectedIndex property
        /// </summary>
        /// <value>
        ///     This is the currently selected element in the UI.
        /// </value>
        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;

                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }

        /// <summary>
        ///     Categories property
        /// </summary>
        /// <value>
        ///     A set of categories
        /// </value>
        public IEnumerable<string> Categories
        {
            get { return NodeCategories.Keys; }
        }

        /// <summary>
        ///     NodeCategories property
        /// </summary>
        /// <value>
        ///     A set of categories
        /// </value>
        private Dictionary<string, CategorySearchElement> NodeCategories { get; set; }

        /// <summary>
        ///     RevitApiSearchElements property
        /// </summary>
        /// <value>
        ///     A collection of elements corresponding to the auto-generated Revit
        ///     API elements
        /// </value>
        public List<SearchElementBase> RevitApiSearchElements { get; private set; }

        /// <summary>
        ///     Visible property
        /// </summary>
        /// <value>
        ///     Tells whether the View is visible or not
        /// </value>
        private Visibility _visible;

        public Visibility Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                RaisePropertyChanged("Visible");
            }
        }

        /// <summary>
        ///     SearchDictionary property
        /// </summary>
        /// <value>
        ///     This is the dictionary used to search
        /// </value>
        public SearchDictionary<SearchElementBase> SearchDictionary { get; private set; }

        /// <summary>
        ///     SearchResults property
        /// </summary>
        /// <value>
        ///     This property is observed by SearchView to see the search results
        /// </value>
        public ObservableCollection<SearchElementBase> SearchResults { get; private set; }

        /// <summary>
        ///     MaxNumSearchResults property
        /// </summary>
        /// <value>
        ///     Internal limit on the number of search results returned by SearchDictionary
        /// </value>
        public int MaxNumSearchResults { get; set; }

        #endregion

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="bench"> Reference to dynBench object for logging </param>
        public SearchViewModel()
        {
            SelectedIndex = 0;
            RevitApiSearchElements = new List<SearchElementBase>();
            NodeCategories = new Dictionary<string, CategorySearchElement>();
            SearchDictionary = new SearchDictionary<SearchElementBase>();
            SearchResults = new ObservableCollection<SearchElementBase>();
            MaxNumSearchResults = 100;
            Visible = Visibility.Collapsed;
            _SearchText = "";
            IncludeOptionalElements = false; // revit api
            Regions = new ObservableDictionary<string, RegionBase>();
            //Regions.Add("Include Nodes from Package Manager", DynamoCommands.PackageManagerRegionCommand );
            Regions.Add("Include Experimental Revit API Nodes", new RevitAPIRegion());
            AddHomeToSearch();
            AddCommandElements();
        }

        /// <summary>
        ///     Adds the Home Workspace to search.
        /// </summary>
        private void AddHomeToSearch()
        {
            SearchDictionary.Add(new WorkspaceSearchElement("Home", "Navigate to Home Workspace"), "Home");
        }

        /// <summary>
        ///     Asyncrhonously erforms a search and updates the observable SearchResults property.
        /// </summary>
        /// <param name="query"> The search query </param>
        internal void SearchAndUpdateResults(string query)
        {
            if (Visible != Visibility.Visible)
                return;

            Task<IEnumerable<SearchElementBase>>.Factory.StartNew(() =>
                {
                    lock (SearchDictionary)
                    {
                        return Search(query);
                    }
                }).ContinueWith((t) =>
                    {
                        lock (SearchResults)
                        {
                            SearchResults.Clear();
                            foreach (var node in t.Result)
                            {
                                SearchResults.Add(node);
                            }
                            SelectedIndex = 0;
                        }
                    }
                                , TaskScheduler.FromCurrentSynchronizationContext()); // run continuation in ui thread
        }


        /// <summary>
        ///     Synchronously performs a search using the current SearchText
        /// </summary>
        /// <param name="query"> The search query </param>
        internal void SearchAndUpdateResultsSync()
        {
            SearchAndUpdateResultsSync(SearchText);
        }

        /// <summary>
        ///     Synchronously Performs a search and updates the observable SearchResults property
        ///     on the current thread.
        /// </summary>
        /// <param name="query"> The search query </param>
        internal void SearchAndUpdateResultsSync(string query)
        {
            if (Visible != Visibility.Visible)
                return;

            var result = Search(query);
            SearchResults.Clear();
            foreach (var node in result)
            {
                SearchResults.Add(node);
            }
            SelectedIndex = 0;
        }

        /// <summary>
        ///     Add command elements to search
        /// </summary>
        internal void AddCommandElements()
        {
            SearchDictionary.Add(new CommandElement("Note", "Add a note to the canvas", new List<string> {"doc"},
                                                    dynSettings.Controller.DynamoViewModel.AddNoteCommand), "Note");
            SearchDictionary.Add(new CommandElement("ToggleConsoleVisibility", "Toggle the visibility of the console",
                                                    new List<string> {"console", "view"},
                                                    dynSettings.Controller.DynamoViewModel.ToggleConsoleShowingCommand),
                                 "ToggleConsoleVisibility");
            SearchDictionary.Add(new CommandElement("Open", "Open a document...", new List<string>(),
                                                    dynSettings.Controller.DynamoViewModel
                                                               .ShowOpenDialogAndOpenResultCommand), "Open");
            SearchDictionary.Add(
                new CommandElement("Save", "Save the current document", new List<string>(),
                                   dynSettings.Controller.DynamoViewModel.ShowSaveDialogIfNeededAndSaveResultCommand),
                "Save");
            SearchDictionary.Add(
                new CommandElement("SaveAs", "Save the current document as...", new List<string>(),
                                   dynSettings.Controller.DynamoViewModel.ShowSaveDialogAndSaveResultCommand), "SaveAs");
            SearchDictionary.Add(
                new CommandElement("SaveImage", "Save the current workspace as an image...", new List<string>(),
                                   dynSettings.Controller.DynamoViewModel.ShowSaveImageDialogAndSaveResultCommand),
                "SaveImage");
            SearchDictionary.Add(
                new CommandElement("ClearWorkspace", "Clear the current workspace", new List<string>(),
                                   dynSettings.Controller.DynamoViewModel.ClearCommand), "ClearWorkspace");
            SearchDictionary.Add(
                new CommandElement("Exit", "Exit the application", new List<string>(),
                                   dynSettings.Controller.DynamoViewModel.ExitCommand), "Exit");
        }

        /// <summary>
        ///     Increments the selected element by 1, unless it is the last element already
        /// </summary>
        public void SelectNext()
        {
            if (SelectedIndex == SearchResults.Count - 1
                || SelectedIndex == -1)
                return;

            SelectedIndex = SelectedIndex + 1;
        }

        /// <summary>
        ///     Decrements the selected element by 1, unless it is the first element already
        /// </summary>
        public void SelectPrevious()
        {
            if (SelectedIndex <= 0)
                return;

            SelectedIndex = SelectedIndex - 1;
        }

        /// <summary>
        ///     Performs a search using the internal SearcText as the query and
        ///     updates the observable SearchResults property.
        /// </summary>
        internal void SearchAndUpdateResults()
        {
            SearchAndUpdateResults(SearchText);
        }

        /// <summary>
        ///     Performs a search using the given string as query, but does not update
        ///     the SearchResults object.
        /// </summary>
        /// <returns> Returns a list with a maximum MaxNumSearchResults elements.</returns>
        /// <param name="search"> The search query </param>
        internal List<SearchElementBase> Search(string search)
        {
            if (string.IsNullOrEmpty(search) || search == "Search...")
            {
                if (IncludeOptionalElements)
                    return NodeCategories.Select(kvp => (SearchElementBase) kvp.Value).OrderBy(val => val.Name).ToList();
                else
                    return
                        NodeCategories.Select(kvp => (SearchElementBase) kvp.Value)
                                      .Where((ele) => !ele.Name.StartsWith("Revit API"))
                                      .OrderBy(val => val.Name)
                                      .ToList();
            }

            return SearchDictionary.Search(search, MaxNumSearchResults);
        }

        /// <summary>
        ///     A KeyHandler method used by SearchView, increments decrements and executes based on input.
        /// </summary>
        /// <param name="sender">Originating object for the KeyHandler </param>
        /// <param name="e">Parameters describing the key push</param>
        public void KeyHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteSelected();
            }
            else if (e.Key == Key.Tab)
            {
                PopulateSearchTextWithSelectedResult();
            }
                //else if (e.Key == Key.Back)
                //{
                //    RemoveLastPartOfSearchText();
                //}
            else if (e.Key == Key.Down)
            {
                SelectNext();
            }
            else if (e.Key == Key.Up)
            {
                SelectPrevious();
            }
        }

        /// <summary>
        ///     If Revit API elements are shown, hides them.  Otherwise,
        ///     shows them.  Update search when done with either.
        /// </summary>
        public void ToggleIncludingRevitAPIElements()
        {
            if (!IncludeOptionalElements)
            {
                foreach (var ele in RevitApiSearchElements)
                {
                    SearchDictionary.Remove(ele, ele.Name);
                    if (!(ele is CategorySearchElement))
                        SearchDictionary.Remove(ele, "Revit API." + ele.Name);
                }
            }
            else
            {
                // add elements to search
                foreach (var ele in RevitApiSearchElements)
                {
                    SearchDictionary.Add(ele, ele.Name);
                    if (!(ele is CategorySearchElement))
                        SearchDictionary.Add(ele, "Revit API." + ele.Name);
                }
            }
            SearchAndUpdateResults();
        }

        /// <summary>
        ///     If there's a period in the SearchText property, remove text
        ///     to the end until you hit a period.  Otherwise, remove the
        ///     last character.  If the SearchText property is empty or null
        ///     return doing nothing.
        /// </summary>
        public void RemoveLastPartOfSearchText()
        {
            SearchText = RemoveLastPartOfText(SearchText);
        }

        /// <summary>
        ///     If there's a period in the argument, remove text
        ///     to the end until you hit a period.  Otherwise, remove the
        ///     last character.  If the argument is empty or null
        ///     return the empty string.
        /// </summary>
        /// <returns>The string cleaved of everything </returns>
        public static string RemoveLastPartOfText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var matches = Regex.Matches(text, Regex.Escape("."));

            // no period
            if (matches.Count == 0)
            {
                return "";
            }

            // if period is in last position, remove that period and recurse
            if (matches[matches.Count - 1].Index + 1 == text.Length)
            {
                return RemoveLastPartOfText(text.Substring(0, text.Length - 1));
            }

            // remove to the last period
            return text.Substring(0, matches[matches.Count - 1].Index + 2);
        }

        /// <summary>
        ///     If there are results, fill the search field with the text from the
        ///     name property of the first search result.
        /// </summary>
        public void PopulateSearchTextWithSelectedResult()
        {
            if (SearchResults.Count == 0) return;

            // none of the elems are selected, return 
            if (SelectedIndex == -1)
                return;

            SearchText = SearchResults[SelectedIndex].Name;
        }

        /// <summary>
        ///     Runs the Execute() method of the current selected SearchElementBase object
        ///     amongst the SearchResults.
        /// </summary>
        public void ExecuteSelected()
        {
            if (SearchResults.Count == 0) return;

            // none of the elems are selected, return 
            if (SelectedIndex == -1)
                return;

            SearchResults[SelectedIndex].Execute();
        }

        /// <summary>
        ///     Adds a PackageHeader, recently downloaded from the Package Manager, to Search
        /// </summary>
        /// <param name="packageHeader">A PackageHeader object</param>
        public void Add(PackageHeader packageHeader)
        {
            var searchEle = new PackageManagerSearchElement(packageHeader);
            SearchDictionary.Add(searchEle, searchEle.Name);
            if (packageHeader.keywords != null && packageHeader.keywords.Count > 0)
                SearchDictionary.Add(searchEle, packageHeader.keywords);
            SearchDictionary.Add(searchEle, searchEle.Description);
            SearchAndUpdateResultsSync(SearchText);
        }

        /// <summary>
        ///     Adds a Workspace object to the search dictionary using it's Name property for a name
        /// </summary>
        /// <param name="workspace">A dynWorkspace to add</param>
        public void Add(dynWorkspace workspace)
        {
            Add(workspace, workspace.Name);
        }

        /// <summary>
        ///     Adds a Workspace object with a given Name
        /// </summary>
        /// <param name="workspace">A dynWorkspace to add</param>
        /// <param name="name">The name to use</param>
        public void Add(dynWorkspace workspace, string name)
        {
            if (name == "Home")
                return;

            // create the workspace in search
            var searchEle = new WorkspaceSearchElement(name, "Go to " + name);
            var funcDef = dynSettings.Controller.CustomNodeLoader.GetDefinitionFromWorkspace(workspace);
            searchEle.Guid = funcDef.FunctionId;

            if (searchEle.Guid == Guid.Empty)
                return;

            SearchDictionary.Add(searchEle, searchEle.Name);

            // create the node in search
            var nodeEle = new LocalSearchElement(funcDef);
            SearchDictionary.Add(nodeEle, name);

            // update search
            SearchAndUpdateResultsSync(SearchText);
        }

        /// <summary>
        ///     Add a custom node to search.
        /// </summary>
        /// <param name="workspace">A dynWorkspace to add</param>
        /// <param name="name">The name to use</param>
        public void Add(string name, string category, Guid functionId)
        {
            if (name == "Home")
                return;

            // create the workspace in search
            var workspaceEle = new WorkspaceSearchElement(name, "Navigate to workspace called " + name);
            workspaceEle.Guid = functionId;

            if (!NodeCategories.ContainsKey(category))
            {
                NodeCategories.Add(category, new CategorySearchElement(category));
            }

            NodeCategories[category].NumElements++;

            SearchDictionary.Add(workspaceEle, workspaceEle.Name);

            // create the node in search
            var nodeEle = new LocalSearchElement(name, functionId);
            SearchDictionary.Add(nodeEle, nodeEle.Name);
            SearchDictionary.Add(nodeEle, category + "." + nodeEle.Name);
        }

        /// <summary>
        ///     Adds a local DynNode to search
        /// </summary>
        /// <param name="dynNode">A Dynamo node object</param>
        public void Add(Type t)
        {
            // get name, category, attributes 
            var attribs = t.GetCustomAttributes(typeof (NodeNameAttribute), false);
            var name = "";
            if (attribs.Length > 0)
            {
                name = (attribs[0] as NodeNameAttribute).Name;
            }

            attribs = t.GetCustomAttributes(typeof (NodeCategoryAttribute), false);
            var cat = "";
            if (attribs.Length > 0)
            {
                cat = (attribs[0] as NodeCategoryAttribute).ElementCategory;
            }

            attribs = t.GetCustomAttributes(typeof (NodeSearchTagsAttribute), false);
            var tags = new List<string>();
            if (attribs.Length > 0)
            {
                tags = (attribs[0] as NodeSearchTagsAttribute).Tags;
            }

            attribs = t.GetCustomAttributes(typeof (NodeDescriptionAttribute), false);
            var description = "";
            if (attribs.Length > 0)
            {
                description = (attribs[0] as NodeDescriptionAttribute).ElementDescription;
            }

            var searchEle = new LocalSearchElement(name, description, tags);

            if (!string.IsNullOrEmpty(cat))
            {
                if (!cat.StartsWith("Revit API"))
                {
                    SearchDictionary.Add(searchEle, cat + "." + searchEle.Name);

                    if (!NodeCategories.ContainsKey(cat))
                    {
                        var nameEle = new CategorySearchElement(cat);
                        NodeCategories.Add(cat, nameEle);
                        SearchDictionary.Add(nameEle, cat);
                    }
                }
                else
                {
                    if (!NodeCategories.ContainsKey(cat))
                    {
                        var nameEle = new CategorySearchElement(cat);
                        NodeCategories.Add(cat, nameEle);
                        RevitApiSearchElements.Add(nameEle);
                    }
                }
            }

            NodeCategories[cat].NumElements++;

            // add node to search
            if ((searchEle.Name.StartsWith("API_")))
            {
                RevitApiSearchElements.Add(searchEle);
            }
            else
            {
                SearchDictionary.Add(searchEle, searchEle.Name);
                if (tags.Count > 0)
                {
                    SearchDictionary.Add(searchEle, tags);
                }
                SearchDictionary.Add(searchEle, description);
            }
        }

        /// <summary>
        ///     Adds a local DynNode to search
        /// </summary>
        /// <param name="dynNode">A Dynamo node object</param>
        public void Add(dynNodeModel dynNode)
        {
            var searchEle = new LocalSearchElement(dynNode);

            // add category to search
            var cat = dynNode.Category;
            if (!string.IsNullOrEmpty(cat))
            {
                if (!cat.StartsWith("Revit API"))
                {
                    SearchDictionary.Add(searchEle, cat + "." + searchEle.Name);

                    if (!NodeCategories.ContainsKey(cat))
                    {
                        var nameEle = new CategorySearchElement(cat);
                        NodeCategories.Add(cat, nameEle);
                        SearchDictionary.Add(nameEle, cat);
                    }
                }
                else
                {
                    if (!NodeCategories.ContainsKey(cat))
                    {
                        var nameEle = new CategorySearchElement(cat);
                        NodeCategories.Add(cat, nameEle);
                        RevitApiSearchElements.Add(nameEle);
                    }
                }
            }

            NodeCategories[cat].NumElements++;

            // add node to search
            if ((searchEle.Name.StartsWith("API_")))
            {
                RevitApiSearchElements.Add(searchEle);
            }
            else
            {
                SearchDictionary.Add(searchEle, searchEle.Name);
                if (dynNode.Tags.Count > 0)
                {
                    SearchDictionary.Add(searchEle, dynNode.Tags);
                }
                SearchDictionary.Add(searchEle, dynNode.Description);
            }
        }

        /// <summary>
        ///     Rename a workspace that is currently part of the SearchDictionary
        /// </summary>
        /// <param name="def">The FunctionDefinition whose name must change</param>
        /// <param name="newName">The new name to assign to the workspace</param>
        public void Refactor(FunctionDefinition def, string oldName, string newName)
        {
            SearchDictionary.Remove((ele) => (ele).Name == oldName);
            Add(def.Workspace, newName);
        }
    }
}
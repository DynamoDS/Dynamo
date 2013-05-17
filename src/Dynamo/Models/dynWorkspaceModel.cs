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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    public abstract class dynWorkspaceModel : NotificationObject, ILocatable
    {

        #region Properties

        private string _name;
        private double x = 0.0;
        private double y = 0.0;
        private double height = 100;
        private double width = 100;

        /// <summary>
        ///     The date of the last save.
        /// </summary>
        private DateTime _lastSaved;
        public DateTime LastSaved
        {
            get { return _lastSaved; }
            set
            {
                _lastSaved = value;
                RaisePropertyChanged("LastSaved");
            }
        }

        /// <summary>
        ///     Are there unsaved changes in the workspace?
        /// </summary>
        private bool _hasUnsavedChanges;
        public bool HasUnsavedChanges
        {
            get { return _hasUnsavedChanges; }
            set
            {
                _hasUnsavedChanges = value;
                RaisePropertyChanged("HasUnsavedChanges");
            }
        }

        public ObservableCollection<dynNodeModel> Nodes { get; internal set; }
        public ObservableCollection<dynConnectorModel> Connectors { get; internal set; }
        public ObservableCollection<dynNoteModel> Notes { get; internal set; }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                RaisePropertyChanged("FilePath");
            }
        }

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        /// Get or set the X position of the workspace.
        /// </summary>
        public double X
        {
            get { return x; }
            set
            {
                x = value;
                RaisePropertyChanged("X");
            }
        }

        /// <summary>
        /// Get or set the Y position of the workspace
        /// </summary>
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                RaisePropertyChanged("Y");
            }
        }

        /// <summary>
        /// Get the height of the workspace's bounds.
        /// </summary>
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        /// Get the width of the workspace's bounds.
        /// </summary>
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        /// Get the bounds of the workspace.
        /// </summary>
        public Rect Rect
        {
            get { return new Rect(x, y, width, height); }
        }

        #endregion

        public event Action OnModified;

        public abstract void OnDisplayed();

        //Hide default constructor.
        private dynWorkspaceModel() { }

        protected dynWorkspaceModel(String name, List<dynNodeModel> e, List<dynConnectorModel> c, double x, double y)
        {
            Name = name;

            Nodes = new ObservableCollection<dynNodeModel>(e);
            Connectors = new ObservableCollection<dynConnectorModel>(c);
            Notes = new ObservableCollection<dynNoteModel>();
            X = x;
            Y = y;

            Nodes.CollectionChanged += MarkUnsaved;
            Notes.CollectionChanged += MarkUnsaved;
            Connectors.CollectionChanged += MarkUnsaved;

            this.HasUnsavedChanges = false;
            this.LastSaved = DateTime.Now;

            this.WorkspaceSaved += OnWorkspaceSaved;
        }

        public delegate void WorkspaceSavedEvent(dynWorkspaceModel model);
        public event WorkspaceSavedEvent WorkspaceSaved;

        /// <summary>
        ///     If there are observers for the save, notifies them
        /// </summary>
        private void OnWorkspaceSaved()
        {
            if (WorkspaceSaved != null)
                WorkspaceSaved(this);
        }

        /// <summary>
        ///     Updates relevant parameters on save
        /// </summary>
        /// <param name="model">The workspace that was just saved</param>
        private static void OnWorkspaceSaved(dynWorkspaceModel model)
        {
            model.LastSaved = DateTime.Now;
            model.HasUnsavedChanges = false;
        }

        private void MarkUnsaved(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.HasUnsavedChanges = true;
        }

        public void DisableReporting()
        {
            Nodes.ToList().ForEach(x => x.DisableReporting());
        }

        public void EnableReporting()
        {
            Nodes.ToList().ForEach(x => x.EnableReporting());
        }

        public virtual void Modified()
        {
            if (OnModified != null)
                OnModified();
        }

        public IEnumerable<dynNodeModel> GetTopMostNodes()
        {
            return this.Nodes.Where(
               x => x.OutPortData.Any() && x.OutPorts.All(y => !y.Connectors.Any())
            );
        }

        #region static methods

        /// <summary>
        ///     Generate an xml doc and write the workspace to the given path
        /// </summary>
        /// <param name="xmlPath">The path to save to</param>
        /// <param name="workSpace">The workspace</param>
        /// <returns>Whether the operation was successful</returns>
        public static bool SaveWorkspace(string xmlPath, dynWorkspaceModel workSpace)
        {
            dynSettings.Controller.DynamoViewModel.Log("Saving " + xmlPath + "...");
            try
            {
                var xmlDoc = GetXmlDocFromWorkspace(workSpace, workSpace is HomeWorkspace);
                xmlDoc.Save(xmlPath);
                workSpace.FilePath = xmlPath;

                workSpace.OnWorkspaceSaved();
            }
            catch (Exception ex)
            {
                //Log(ex);
                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Generate the xml doc of the workspace from memory
        /// </summary>
        /// <param name="workSpace">The workspace</param>
        /// <returns>The generated xmldoc</returns>
        public static XmlDocument GetXmlDocFromWorkspace(dynWorkspaceModel workSpace, bool savingHomespace)
        {
            try
            {
                //create the xml document
                var xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);

                XmlElement root = xmlDoc.CreateElement("dynWorkspace"); //write the root element
                root.SetAttribute("X", workSpace.X.ToString());
                root.SetAttribute("Y", workSpace.Y.ToString());

                if (!savingHomespace) //If we are not saving the home space
                {
                    root.SetAttribute("Name", workSpace.Name);
                    root.SetAttribute("Category", ((FuncWorkspace)workSpace).Category);

                    Guid guid = dynSettings.Controller.CustomNodeLoader.GetGuidFromName(workSpace.Name);

                    //friends don't let friends save an empty GUID
                    if (guid == Guid.Empty)
                        guid = Guid.NewGuid();
                    
                    root.SetAttribute("ID", guid.ToString());
                }

                xmlDoc.AppendChild(root);

                XmlElement elementList = xmlDoc.CreateElement("dynElements"); //write the root element
                root.AppendChild(elementList);

                foreach (dynNodeModel el in workSpace.Nodes)
                {
                    string typeName = el.GetType().ToString();

                    XmlElement dynEl = xmlDoc.CreateElement(typeName);
                    elementList.AppendChild(dynEl);

                    //set the type attribute
                    dynEl.SetAttribute("type", el.GetType().ToString());
                    dynEl.SetAttribute("guid", el.GUID.ToString());
                    dynEl.SetAttribute("nickname", el.NickName);
                    dynEl.SetAttribute("x", el.X.ToString());
                    dynEl.SetAttribute("y", el.Y.ToString());
                    dynEl.SetAttribute("lacing", el.ArgumentLacing.ToString());

                    el.SaveElement(xmlDoc, dynEl);
                }

                //write only the output connectors
                XmlElement connectorList = xmlDoc.CreateElement("dynConnectors"); //write the root element
                root.AppendChild(connectorList);

                foreach (dynNodeModel el in workSpace.Nodes)
                {
                    foreach (dynPortModel port in el.OutPorts)
                    {
                        foreach (dynConnectorModel c in port.Connectors.Where(c => c.Start != null && c.End != null))
                        {
                            XmlElement connector = xmlDoc.CreateElement(c.GetType().ToString());
                            connectorList.AppendChild(connector);
                            connector.SetAttribute("start", c.Start.Owner.GUID.ToString());
                            connector.SetAttribute("start_index", c.Start.Index.ToString());
                            connector.SetAttribute("end", c.End.Owner.GUID.ToString());
                            connector.SetAttribute("end_index", c.End.Index.ToString());

                            if (c.End.PortType == PortType.INPUT)
                                connector.SetAttribute("portType", "0");
                        }
                    }
                }

                //save the notes
                XmlElement noteList = xmlDoc.CreateElement("dynNotes"); //write the root element
                root.AppendChild(noteList);
                foreach (dynNoteModel n in workSpace.Notes)
                {
                    XmlElement note = xmlDoc.CreateElement(n.GetType().ToString());
                    noteList.AppendChild(note);
                    note.SetAttribute("text", n.Text);
                    note.SetAttribute("x", n.X.ToString());
                    note.SetAttribute("y", n.Y.ToString());
                }

                return xmlDoc;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return null;
            }
        }

        #endregion

        /// <summary>
        ///     Defines whether this is the current space in Dynamo
        /// </summary>
        private bool _isCurrentSpace = false;
        public bool IsCurrentSpace
        {
            get { return _isCurrentSpace; }
            set { 
                _isCurrentSpace = value;
                RaisePropertyChanged("IsCurrentSpace");
            }
        }
    }

    internal static class WorkspaceHelpers
    {
        public static Dictionary<string, dynNodeView> hiddenNodes =
            new Dictionary<string, dynNodeView>();
    }

    public class FuncWorkspace : dynWorkspaceModel
    {
        public String Category { get; set; }


        #region Contructors

        public FuncWorkspace()
            : this("", "", new List<dynNodeModel>(), new List<dynConnectorModel>(), 0, 0)
        { }

        public FuncWorkspace(String name, String category)
            : this(name, category, new List<dynNodeModel>(), new List<dynConnectorModel>(), 0, 0)
        { }

        public FuncWorkspace(String name, String category, double x, double y)
            : this(name, category, new List<dynNodeModel>(), new List<dynConnectorModel>(), x, y)
        { }

        public FuncWorkspace(String name, String category, List<dynNodeModel> e, List<dynConnectorModel> c, double x, double y)
            : base(name, e, c, x, y)
        {
            this.Category = category;
        }

        #endregion

        public override void Modified()
        {
            base.Modified();

            //dynSettings.Controller.DynamoViewModel.SaveFunction(
            //    dynSettings.Controller.CustomNodeLoader.GetDefinitionFromWorkspace(this));
        }

        public override void OnDisplayed()
        {
            var bench = dynSettings.Bench;


            //if (bench.addMenuItemsDictNew.ContainsKey("Variable"))
            //    return;

            //var variable = WorkspaceHelpers.hiddenNodes["Variable"];
            //var output = WorkspaceHelpers.hiddenNodes["Output"];
            //WorkspaceHelpers.hiddenNodes.Remove("Variable");
            //WorkspaceHelpers.hiddenNodes.Remove("Output");
            //variable.Visibility = Visibility.Visible;
            //variable.Visibility = Visibility.Visible;
            //bench.addMenuItemsDictNew["Variable"] = variable;
            //bench.addMenuItemsDictNew["Output"] = output;

            //dynSettings.Controller.UpdateSearch(bench.SearchBox.Text.Trim());
        }
    }

    public class HomeWorkspace : dynWorkspaceModel
    {

        public HomeWorkspace()
            : this(new List<dynNodeModel>(), new List<dynConnectorModel>(), 0, 0)
        { }

        public HomeWorkspace(double x, double y)
            : this(new List<dynNodeModel>(), new List<dynConnectorModel>(), x, y)
        { }

        public HomeWorkspace(List<dynNodeModel> e, List<dynConnectorModel> c, double x, double y)
            : base("Home", e, c, x, y)
        {
        }

        public override void Modified()
        {
            base.Modified();

            var controller = dynSettings.Controller;
            if (dynSettings.Controller.DynamoViewModel.DynamicRunEnabled)
            {
                if (!controller.Running)
                    controller.RunExpression(false);
                else
                    controller.QueueRun();
            }
        }

        public override void OnDisplayed()
        {
            var bench = dynSettings.Bench;  // ewwwy
        }
    }
}

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
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Dynamo.Controls;
using Dynamo.Nodes;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    public abstract class dynWorkspace : NotificationObject
    {
        private string _name;
        private System.Windows.Point currentOffset = new System.Windows.Point(0, 0);

        public ObservableCollection<dynNode> Nodes { get; private set; }
        public ObservableCollection<dynConnector> Connectors { get; private set; }
        public ObservableCollection<dynNoteModel> Notes { get; private set; }

        //private DynamoModel _model;

        public string FilePath { get; set; }

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public double PositionX { get; set; }

        public double PositionY { get; set; }

        /// <summary>
        /// Specifies the pan location of the view
        /// </summary>
        public Point CurrentOffset
        {
            get { return currentOffset; }
            set
            {
                currentOffset = value;
                RaisePropertyChanged("CurrentOffset");
            }
        }

        public event Action OnModified;

        public abstract void OnDisplayed();

        //public DynamoModel Model
        //{
        //    get { return _model; }
        //}

        //Hide default constructor.
        private dynWorkspace() { }

        protected dynWorkspace(String name, List<dynNode> e, List<dynConnector> c, double x, double y)
        {
            //_model = model;
            Name = name;
//MVVM : made all lists into observable collections
            Nodes = new ObservableCollection<dynNode>(e);
            Connectors = new ObservableCollection<dynConnector>(c);
            PositionX = x;
            PositionY = y;
            Notes = new ObservableCollection<dynNoteModel>();
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

        public IEnumerable<dynNode> GetTopMostNodes()
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
        public static bool SaveWorkspace(string xmlPath, dynWorkspace workSpace)
        {
            dynSettings.Controller.DynamoViewModel.Log("Saving " + xmlPath + "...");
            try
            {

//MVVM : Is this test valid for the Homeworskpace?
                var xmlDoc = GetXmlDocFromWorkspace(workSpace, workSpace is HomeWorkspace);
                xmlDoc.Save(xmlPath);

                //cache the file path for future save operations
                workSpace.FilePath = xmlPath;
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
        public static XmlDocument GetXmlDocFromWorkspace(dynWorkspace workSpace, bool savingHomespace)
        {
            try
            {
                //create the xml document
                var xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);

                XmlElement root = xmlDoc.CreateElement("dynWorkspace"); //write the root element
                root.SetAttribute("X", workSpace.PositionX.ToString());
                root.SetAttribute("Y", workSpace.PositionY.ToString());

                if (!savingHomespace) //If we are not saving the home space
                {
                    root.SetAttribute("Name", workSpace.Name);
                    root.SetAttribute("Category", ((FuncWorkspace)workSpace).Category);
                    root.SetAttribute(
                            "ID", 
                            dynSettings.Controller.CustomNodeLoader.GetGuidFromName(workSpace.Name).ToString());
                }

                xmlDoc.AppendChild(root);

                XmlElement elementList = xmlDoc.CreateElement("dynElements"); //write the root element
                root.AppendChild(elementList);

                foreach (dynNode el in workSpace.Nodes)
                {
                    XmlElement dynEl = xmlDoc.CreateElement(el.GetType().ToString());
                    elementList.AppendChild(dynEl);

                    //set the type attribute
                    dynEl.SetAttribute("type", el.GetType().ToString());
                    dynEl.SetAttribute("guid", el.GUID.ToString());
                    dynEl.SetAttribute("nickname", el.NickName);
                    //dynEl.SetAttribute("x", Canvas.GetLeft(el.NodeUI).ToString());
                    //dynEl.SetAttribute("y", Canvas.GetTop(el.NodeUI).ToString());
                    dynEl.SetAttribute("x", el.X.ToString());
                    dynEl.SetAttribute("y", el.Y.ToString());

                    el.SaveElement(xmlDoc, dynEl);
                }

                //write only the output connectors
                XmlElement connectorList = xmlDoc.CreateElement("dynConnectors"); //write the root element
                root.AppendChild(connectorList);

                foreach (dynNode el in workSpace.Nodes)
                {
                    foreach (dynPortModel port in el.OutPorts)
                    {
                        foreach (dynConnector c in port.Connectors.Where(c => c.Start != null && c.End != null))
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
                    //note.SetAttribute("x", Canvas.GetLeft(n).ToString());
                    //note.SetAttribute("y", Canvas.GetTop(n).ToString());
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

    }

    internal static class WorkspaceHelpers
    {
        public static Dictionary<string, dynNodeUI> hiddenNodes =
            new Dictionary<string, dynNodeUI>();
    }

    public class FuncWorkspace : dynWorkspace
    {
        public String Category { get; set; }


        #region Contructors

        public FuncWorkspace()
            : this("", "", new List<dynNode>(), new List<dynConnector>(), 0, 0)
        { }

        public FuncWorkspace(String name, String category)
            : this(name, category, new List<dynNode>(), new List<dynConnector>(), 0, 0)
        { }

        public FuncWorkspace(String name, String category, double x, double y)
            : this(name, category, new List<dynNode>(), new List<dynConnector>(), x, y)
        { }

        public FuncWorkspace(String name, String category, List<dynNode> e, List<dynConnector> c, double x, double y)
            : base(name, e, c, x, y)
        {
            this.Category = category;
        }

        #endregion

        public override void Modified()
        {
            base.Modified();

            dynSettings.Controller.DynamoViewModel.SaveFunction(
                dynSettings.Controller.CustomNodeLoader.GetDefinitionFromWorkspace(this));
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

    public class HomeWorkspace : dynWorkspace
    {
        #region Contructors

        private static bool initializedFunctionDefinition = false;

        public HomeWorkspace()
            : this(new List<dynNode>(), new List<dynConnector>(), 0, 0)
        { }

        public HomeWorkspace(double x, double y)
            : this(new List<dynNode>(), new List<dynConnector>(), x, y)
        { }

        public HomeWorkspace(List<dynNode> e, List<dynConnector> c, double x, double y)
            : base("Home", e, c, x, y)
        {
            if (!initializedFunctionDefinition)
            {
                // todo: this is a special case
                //var homeGuid = Guid.Parse("32AAC852-90A7-4FBD-B78A-8FDB69302670");
                //var homeWorkspaceFuncDef = new FunctionDefinition();
                //homeWorkspaceFuncDef.Workspace = this;
                //dynSettings.FunctionDict.Add(homeGuid, homeWorkspaceFuncDef);
                //initializedFunctionDefinition = true;
            }
        }

        #endregion

        public override void Modified()
        {
            base.Modified();

            var controller = dynSettings.Controller;
            dynSettings.Bench.Dispatcher.BeginInvoke(new Action(
                () =>
                {
                    if (dynSettings.Controller.DynamoViewModel.DynamicRunEnabled)
                    {
                        if (!controller.Running)
                            controller.RunExpression(false);
                        else
                            controller.QueueRun();
                    }
                }));
        }

        public override void OnDisplayed()
        {
            var bench = dynSettings.Bench;
            
            //if (!bench.addMenuItemsDictNew.ContainsKey("Variable"))
            //    return;

            //var variable = bench.addMenuItemsDictNew["Variable"];
            //var output = bench.addMenuItemsDictNew["Output"];
            //bench.addMenuItemsDictNew.Remove("Variable");
            //bench.addMenuItemsDictNew.Remove("Output");
            //variable.Visibility = Visibility.Collapsed;
            //variable.Visibility = Visibility.Collapsed;
            //WorkspaceHelpers.hiddenNodes["Variable"] = variable;
            //WorkspaceHelpers.hiddenNodes["Output"] = output;

            //dynSettings.Controller.UpdateSearch(bench.SearchBox.Text.Trim());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Globalization;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Models
{
    public abstract class dynWorkspaceModel : NotificationObject, ILocatable
    {
        #region Properties

        private string _filePath;
        private string _name;
        private double _height = 100;
        private double _width = 100;
        private double _x;
        private double _y;
        private double _zoom = 1.0;

        private string _category = "";
        public string Category
        {
            get { return _category; }
            set
            {
                _category = value;
                RaisePropertyChanged("Category");
            }
        }

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
        ///     A description of the workspace
        /// </summary>
        private string _author = "None provided";
        public string Author
        {
            get { return _author; }
            set
            {
                _author = value;
                RaisePropertyChanged("Author");
            }
        }

        /// <summary>
        ///     A description of the workspace
        /// </summary>
        private string _description = "";
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
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

        public ObservableCollection<dynNodeModel> Nodes
        {
            get { return _nodes; }
            internal set
            {
                if (Equals(value, _nodes)) return;
                _nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }

        public ObservableCollection<dynConnectorModel> Connectors
        {
            get { return _connectors; }
            internal set
            {
                if (Equals(value, _connectors)) return;
                _connectors = value;
                RaisePropertyChanged("Connectors");
            }
        }

        public ObservableCollection<dynNoteModel> Notes { get; internal set; }

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
        ///     Get or set the X position of the workspace.
        /// </summary>
        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                RaisePropertyChanged("X");
            }
        }

        /// <summary>
        ///     Get or set the Y position of the workspace
        /// </summary>
        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                RaisePropertyChanged("Y");
            }
        }

        public double Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                RaisePropertyChanged("Zoom");
            }
        }

        /// <summary>
        ///     Get the height of the workspace's bounds.
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        ///     Get the width of the workspace's bounds.
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        ///     Get the bounds of the workspace.
        /// </summary>
        public Rect Rect
        {
            get { return new Rect(_x, _y, _width, _height); }
        }

        #endregion

        public delegate void WorkspaceSavedEvent(dynWorkspaceModel model);

        /// <summary>
        ///     Defines whether this is the current space in Dynamo
        /// </summary>
        private bool _isCurrentSpace;

        private ObservableCollection<dynNodeModel> _nodes;
        private ObservableCollection<dynConnectorModel> _connectors;

        public double CenterX
        {
            get { return 0; }
            set
            {
                
            }
        }

        public double CenterY
        {
            get { return 0; }
            set
            {

            }
        }

        protected dynWorkspaceModel(
            String name, IEnumerable<dynNodeModel> e, IEnumerable<dynConnectorModel> c, double x, double y)
        {
            Name = name;

            Nodes = new TrulyObservableCollection<dynNodeModel>(e);
            Connectors = new TrulyObservableCollection<dynConnectorModel>(c);
            Notes = new ObservableCollection<dynNoteModel>();
            X = x;
            Y = y;

            HasUnsavedChanges = false;
            LastSaved = DateTime.Now;

            WorkspaceSaved += OnWorkspaceSaved;

        }

        public bool WatchChanges
        {
            set
            {
                if (value)
                {
                    
                    Nodes.CollectionChanged += MarkUnsavedAndModified;
                    Notes.CollectionChanged += MarkUnsaved;
                    Connectors.CollectionChanged += MarkUnsavedAndModified;
                }
                else
                {
                    Nodes.CollectionChanged -= MarkUnsavedAndModified;
                    Notes.CollectionChanged -= MarkUnsaved;
                    Connectors.CollectionChanged -= MarkUnsavedAndModified;
                }
            }
        }

        public bool IsCurrentSpace
        {
            get { return _isCurrentSpace; }
            set
            {
                _isCurrentSpace = value;
                RaisePropertyChanged("IsCurrentSpace");
            }
        }

        public event Action OnModified;

        public abstract void OnDisplayed();

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

        private void MarkUnsaved(
            object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            HasUnsavedChanges = true;
        }

        private void MarkUnsavedAndModified(
            object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            HasUnsavedChanges = true;
            Modified();
        }

        //TODO: Replace all Modified calls with RaisePropertyChanged-stlye system, that way observable collections can catch any changes
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

        public IEnumerable<dynNodeModel> GetHangingNodes()
        {
            return Nodes.Where(x => x.OutPortData.Any() && x.OutPorts.Any(y => !y.Connectors.Any()));
        }

        public IEnumerable<dynNodeModel> GetTopMostNodes()
        {
            return Nodes.Where(
                x =>
                    x.OutPortData.Any()
                    && x.OutPorts.All(y => y.Connectors.All(c => c.End.Owner is dynOutput)));
        }

        public event EventHandler Updated;
        public void OnUpdated(EventArgs e)
        {
            if (Updated != null)
                Updated(this, e);
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
            DynamoLogger.Instance.Log("Saving " + xmlPath + "...");
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
        /// <param name="savingHomespace"></param>
        /// <returns>The generated xmldoc</returns>
        public static XmlDocument GetXmlDocFromWorkspace(
            dynWorkspaceModel workSpace, bool savingHomespace)
        {
            try
            {
                //create the xml document
                var xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);

                var root = xmlDoc.CreateElement("dynWorkspace"); //write the root element
                root.SetAttribute("X", workSpace.X.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("Y", workSpace.Y.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("zoom", workSpace.Zoom.ToString(CultureInfo.InvariantCulture));
                root.SetAttribute("Description", workSpace.Description);
                root.SetAttribute("Category", workSpace.Category);
                root.SetAttribute("Name", workSpace.Name);

                if (!savingHomespace) //If we are not saving the home space
                {
                    var guid =
                        dynSettings.Controller.CustomNodeManager.GetGuidFromName(workSpace.Name);

                    //friends don't let friends save an empty GUID
                    if (guid == Guid.Empty)
                        guid = Guid.NewGuid();

                    root.SetAttribute("ID", guid.ToString());
                }

                xmlDoc.AppendChild(root);

                var elementList = xmlDoc.CreateElement("dynElements");
                //write the root element
                root.AppendChild(elementList);

                foreach (var el in workSpace.Nodes)
                {
                    var typeName = el.GetType().ToString();

                    var dynEl = xmlDoc.CreateElement(typeName);
                    elementList.AppendChild(dynEl);

                    //set the type attribute
                    dynEl.SetAttribute("type", el.GetType().ToString());
                    dynEl.SetAttribute("guid", el.GUID.ToString());
                    dynEl.SetAttribute("nickname", el.NickName);
                    dynEl.SetAttribute("x", el.X.ToString(CultureInfo.InvariantCulture));
                    dynEl.SetAttribute("y", el.Y.ToString(CultureInfo.InvariantCulture));
                    dynEl.SetAttribute("isVisible", el.IsVisible.ToString().ToLower());
                    dynEl.SetAttribute("isUpstreamVisible", el.IsUpstreamVisible.ToString().ToLower());
                    dynEl.SetAttribute("lacing", el.ArgumentLacing.ToString());

                    el.SaveNode(xmlDoc, dynEl, SaveContext.File);
                }

                //write only the output connectors
                var connectorList = xmlDoc.CreateElement("dynConnectors");
                //write the root element
                root.AppendChild(connectorList);

                foreach (var el in workSpace.Nodes)
                {
                    foreach (var port in el.OutPorts)
                    {
                        foreach (
                            var c in
                                port.Connectors.Where(c => c.Start != null && c.End != null))
                        {
                            var connector = xmlDoc.CreateElement(c.GetType().ToString());
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
                var noteList = xmlDoc.CreateElement("dynNotes"); //write the root element
                root.AppendChild(noteList);
                foreach (var n in workSpace.Notes)
                {
                    var note = xmlDoc.CreateElement(n.GetType().ToString());
                    noteList.AppendChild(note);
                    note.SetAttribute("text", n.Text);
                    note.SetAttribute("x", n.X.ToString(CultureInfo.InvariantCulture));
                    note.SetAttribute("y", n.Y.ToString(CultureInfo.InvariantCulture));
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
        //public static Dictionary<string, dynNodeView> HiddenNodes =
        //    new Dictionary<string, dynNodeView>();
    }

    public class FuncWorkspace : dynWorkspaceModel
    {


        #region Contructors

        public FuncWorkspace()
            : this("", "", "", new List<dynNodeModel>(), new List<dynConnectorModel>(), 0, 0)
        {
        }

        public FuncWorkspace(String name, String category)
            : this(name, category, "",new List<dynNodeModel>(), new List<dynConnectorModel>(), 0, 0)
        {
        }

        public FuncWorkspace(String name, String category, string description, double x, double y)
            : this(name, category, description, new List<dynNodeModel>(), new List<dynConnectorModel>(), x, y)
        {
        }

        public FuncWorkspace(
            String name, String category, string description, IEnumerable<dynNodeModel> e, IEnumerable<dynConnectorModel> c, double x, double y)
            : base(name, e, c, x, y)
        {
            WatchChanges = true; 
            HasUnsavedChanges = false;
            Category = category;
            Description = description;

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Name" || args.PropertyName == "Category" || args.PropertyName == "Description")
            {
                this.HasUnsavedChanges = true;
            }
        }

        #endregion

        public override void Modified()
        {
            base.Modified();

            //add a check if any loaded defs match this workspace
            if (dynSettings.Controller.CustomNodeLoader.GetLoadedDefinitions().All(x => x.Workspace != this))
                return;

            var def =
                dynSettings.Controller.CustomNodeManager
                           .GetLoadedDefinitions()
                           .FirstOrDefault(x => x.Workspace == this);

            if (def == null) return;

            def.RequiresRecalc = true;

            try
            {
                var expression = CustomNodeManager.CompileFunction(def);

                dynSettings.Controller.FSchemeEnvironment.DefineSymbol(
                    def.FunctionId.ToString(), expression);
            }
            catch { }
        }

        public override void OnDisplayed()
        {

        }
    }

    public class HomeWorkspace : dynWorkspaceModel
    {
        public HomeWorkspace()
            : this(new List<dynNodeModel>(), new List<dynConnectorModel>(), 0, 0)
        {
        }

        public HomeWorkspace(double x, double y)
            : this(new List<dynNodeModel>(), new List<dynConnectorModel>(), x, y)
        {
        }

        public HomeWorkspace(IEnumerable<dynNodeModel> e, IEnumerable<dynConnectorModel> c, double x, double y)
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
            //DynamoView bench = dynSettings.Bench; // ewwwy
        }
    }
}
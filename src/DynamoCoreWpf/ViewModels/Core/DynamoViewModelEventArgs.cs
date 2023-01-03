using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Dynamo.PackageManager;
using Dynamo.UI.Prompts;

namespace Dynamo.ViewModels
{
    public class NoteEventArgs : EventArgs
    {
        public NoteModel Note { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public NoteEventArgs(NoteModel n, Dictionary<string, object> d)
        {
            Note = n;
            Data = d;
        }
    }

    public class ViewEventArgs : EventArgs
    {
        public object View { get; set; }

        public ViewEventArgs(object v)
        {
            View = v;
        }
    }

    public class SelectionBoxUpdateArgs : EventArgs
    {
        public enum UpdateFlags
        {
            Position = 0x00000001,
            Dimension = 0x00000002,
            Visibility = 0x00000004,
            Mode = 0x00000008
        }

        public SelectionBoxUpdateArgs(Visibility visibility)
        {
            this.Visibility = visibility;
            this.UpdatedProps = UpdateFlags.Visibility;
        }

        public SelectionBoxUpdateArgs(double x, double y)
        {
            this.X = x;
            this.Y = y;
            this.UpdatedProps = UpdateFlags.Position;
        }

        public SelectionBoxUpdateArgs(double x, double y, double width, double height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.UpdatedProps = UpdateFlags.Position | UpdateFlags.Dimension;
        }

        public void SetSelectionMode(bool isCrossSelection)
        {
            this.IsCrossSelection = isCrossSelection;
            this.UpdatedProps |= UpdateFlags.Mode;
        }

        public void SetVisibility(Visibility visibility)
        {
            this.Visibility = visibility;
            this.UpdatedProps |= UpdateFlags.Visibility;
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public bool IsCrossSelection { get; private set; }
        public Visibility Visibility { get; private set; }
        public UpdateFlags UpdatedProps { get; private set; }
    }

    public class WorkspaceSaveEventArgs : EventArgs
    {
        public WorkspaceModel Workspace { get; set; }
        public bool AllowCancel { get; set; }
        public bool Success { get; set; }
        public WorkspaceSaveEventArgs(WorkspaceModel ws, bool allowCancel)
        {
            Workspace = ws;
            AllowCancel = allowCancel;
            Success = false;
        }
    }

    public class ImageSaveEventArgs : EventArgs
    {
        public string Path { get; set; }

        public ImageSaveEventArgs(string path)
        {
            Path = path;
        }
    }

    public class IconRequestEventArgs : EventArgs
    {
        public string IconAssembly { get; private set; }

        public string IconFullPath { get; private set; }

        public ImageSource Icon { get; private set; }

        public bool UseAdditionalResolutionPaths { get; private set; }

        public IconRequestEventArgs(string assembly, string fullPath, bool useAdditionalPaths= true)
        {
            IconAssembly = assembly;
            IconFullPath = fullPath;
            UseAdditionalResolutionPaths = useAdditionalPaths;
        }

        public void SetIcon(ImageSource icon)
        {
            Icon = icon;
        }
    }

    /// <summary>
    /// Provides information about the Dynamo RequestOpenDocumentationLink event,
    /// such as the link that was requested to be opened and whether it is a remote resource. 
    /// </summary>
    public class OpenDocumentationLinkEventArgs : EventArgs
    {
        /// <summary>
        /// The documentation link that was requested to be opened.
        /// </summary>
        public Uri Link { get; }

        /// <summary>
        /// Indicates whether the requested link points to a remote resource.
        /// A resource is considered remote if it is not a file on the local filesystem.
        /// Examples of remote resources include a web address or a file on a network share.
        /// </summary>
        public bool IsRemoteResource { get; }

        public OpenDocumentationLinkEventArgs(Uri link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));

            Link = link;
            IsRemoteResource = link.IsAbsoluteUri && !link.IsFile;
        }
    }

    /// <summary>
    /// A node documentation request compatible with the Dynamo RequestOpenDocumentationLink event,
    /// such as the namespace of the node used to lookup the .md documentation file
    /// and additional Node Info gathered from the NodeModel properties.
    /// </summary>
    public class OpenNodeAnnotationEventArgs : OpenDocumentationLinkEventArgs
    {
        /// <summary>
        /// Minimum qualified name of the node, used to match nodes with documentation markdown files.
        /// </summary>
        public string MinimumQualifiedName { get; }
        /// <summary>
        /// Node type.
        /// </summary>
        public string Type { get; }
        /// <summary>
        /// Short description of the node.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Nodes category.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Name of the package this node belongs to
        /// </summary>
        public string PackageName { get; private set; }

        /// <summary>
        /// The original name of the Node
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// Collection of the nodes input names.
        /// </summary>
        public IEnumerable<string> InputNames { get; private set; }
        /// <summary>
        /// Collection of the nodes output names
        /// </summary>
        public IEnumerable<string> OutputNames { get; private set; }
        /// <summary>
        /// Collection of the nodes inputs description.
        /// </summary>
        public IEnumerable<string> InputDescriptions { get; private set; }
        /// <summary>
        /// Collection of the nodes outputs description.
        /// </summary>
        public IEnumerable<string> OutputDescriptions { get; private set; }
        /// <summary>
        /// Collection of the nodes collection of warnings/errors/infos.
        /// </summary>
        public IEnumerable<Info> NodeInfos { get; private set; }

        /// <summary>
        /// Creates a new instance of OpenNodeAnnotationEventArgs, which contains data used
        /// to create documentation for the node.
        /// </summary>
        /// <param name="model">NodeModel to document</param>
        /// <param name="dynamoViewModel"></param>
        public OpenNodeAnnotationEventArgs(NodeModel model, DynamoViewModel dynamoViewModel) : base(new Uri(String.Empty,UriKind.Relative))
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var packageInfo = dynamoViewModel.Model.CurrentWorkspace.GetNodePackage(model);
            PackageName = packageInfo?.Name ?? string.Empty;
            MinimumQualifiedName = GetMinimumQualifiedName(model, dynamoViewModel);
            OriginalName = model.GetOriginalName();
            Type = model.Name;
            Description = model.Description;
            Category = model.Category;
            NodeInfos = model.NodeInfos;
            SetInputs(model);
            SetOutputs(model);
        }

        private void SetOutputs(NodeModel nodeModel)
        {
            var outputNames = new List<string>();
            var outputDescriptions = new List<string>();

            var outputs = nodeModel.OutPorts;
            foreach (var output in outputs)
            {
                outputNames.Add(output.Name);
                outputDescriptions.Add(output.ToolTip);
            }

            OutputNames = outputNames;
            OutputDescriptions = outputDescriptions;
        }

        private void SetInputs(NodeModel nodeModel)
        {
            var inputNames = new List<string>();
            var inputDescriptions = new List<string>();

            var inputs = nodeModel.InPorts;
            foreach (var input in inputs)
            {
                inputNames.Add(input.Name);
                inputDescriptions.Add(input.ToolTip);
            }

            InputNames = inputNames;
            InputDescriptions = inputDescriptions;

        }

        static private string GetMinimumQualifiedName(NodeModel nodeModel, DynamoViewModel viewModel)
        {
            switch (nodeModel)
            {
                case Function function:
                    var category = function.Category;
                    var name = function.Name;
                    if (CustomNodeHasCollisons(name, GetMainCategory(nodeModel), viewModel))
                    {
                        var inputString = GetInputNames(function);
                        return $"{category}.{name}({inputString})";
                    }
                    return $"{category}.{name}";

                case DSFunctionBase dSFunction:
                    var descriptor = dSFunction.Controller.Definition;
                    if (descriptor.IsOverloaded)
                    {
                        var inputString = GetInputNames(nodeModel);
                        return $"{descriptor.QualifiedName}({inputString})";
                    }

                    return descriptor.QualifiedName;

                case NodeModel node:
                    var type = node.GetType();
                    if (NodeModelHasCollisions(type.FullName, viewModel))
                    {
                        return $"{type.FullName}({GetInputNames(nodeModel)})";
                    }
                    
                    return type.FullName;

                default:
                    return string.Empty;
            }
        }

        private static bool CustomNodeHasCollisons(string nodeName, string packageName, DynamoViewModel viewModel)
        {
            var pmExtension = viewModel.Model.GetPackageManagerExtension();
            if (pmExtension is null)
                return false;

            var package = pmExtension.PackageLoader.LocalPackages
                .Where(x => x.Name == packageName)
                .FirstOrDefault();

            if (package is null)
                return false;

            var loadedNodesWithSameName =  package.LoadedCustomNodes
                .Where(x => x.Name == nodeName)
                .ToList();

            if (loadedNodesWithSameName.Count == 1)
                return false;
            return true;
        }

        private static bool NodeModelHasCollisions(string typeName, DynamoViewModel viewModel)
        {     
            var searchEntries = viewModel.Model.SearchModel.SearchEntries
                .Where(x => x.CreationName == typeName)
                .Select(x => x).ToList();

            if (searchEntries.Count() > 1)
                return true;

            return false;
        }

        private static string GetMainCategory(NodeModel node)
        {
            return node.Category.Split(new char[] { '.' }).FirstOrDefault();
        }

        private static string GetInputNames(NodeModel node)
        {
            var inputNames = node.InPorts.Select(x => x.Name).ToArray();
            // Match https://github.com/DynamoDS/Dynamo/blame/master/src/DynamoCore/Search/SearchElements/ZeroTouchSearchElement.cs#L51 
            return string.Join(", ", inputNames);
        }
    }

    /// <summary>
    /// Provides information about the task dialog used when saving a graph while there are unresolved linter issues in the graph.
    /// This is meant to be used only for unit tests to verify that the dialog has been showed.
    /// </summary>
    internal class SaveWarningOnUnresolvedIssuesArgs : EventArgs
    {
        internal GenericTaskDialog TaskDialog;

        internal SaveWarningOnUnresolvedIssuesArgs(GenericTaskDialog taskDialog)
        {
            TaskDialog = taskDialog;
        }
    }
}

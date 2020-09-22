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
    /// Provides information about the Dynamo RequestOpenDocumentaitonLink event,
    /// such as the namespace of the node used to lookup the .md documentation file
    /// and additional Node Info gathered from the NodeModel properties.
    /// </summary>
    public class OpenNodeAnnotationEventArgs : OpenDocumentationLinkEventArgs
    {
        public string Namespace { get; }
        public string Type { get; }
        public string Description { get; }
        public string Category { get; }
        public List<string> InputNames { get; private set; }
        public List<string> OutputNames { get; private set; }
        public List<string> InputDescriptions { get; private set; }
        public List<string> OutputDescriptions { get; private set; }

        public OpenNodeAnnotationEventArgs(NodeModel model) : base(new Uri(String.Empty,UriKind.Relative))
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            Namespace = GetNamespace(model);
            Type = model.Name;
            Description = model.Description;
            Category = model.Category;
            SetInputs(model);
            SetOutputs(model);

        }

        private void SetOutputs(NodeModel nodeModel)
        {
            OutputNames = new List<string>();
            OutputDescriptions = new List<string>();

            var outputs = nodeModel.OutPorts;
            foreach (var output in outputs)
            {
                OutputNames.Add(output.Name);
                OutputDescriptions.Add(output.ToolTip);
            }
        }

        private void SetInputs(NodeModel nodeModel)
        {
            InputNames = new List<string>();
            InputDescriptions = new List<string>();

            var inputs = nodeModel.InPorts;
            foreach (var input in inputs)
            {
                InputNames.Add(input.Name);
                InputDescriptions.Add(input.ToolTip);
            }
        }

        static private string GetNamespace(NodeModel nodeModel)
        {
            switch (nodeModel)
            {
                case Function function:
                    // implementation for custom nodes goes here.
                    return string.Empty;

                case DSFunctionBase dSFunction:
                    var descriptor = dSFunction.Controller.Definition;
                    var className = descriptor.ClassName;
                    var functionName = descriptor.FunctionName;

                    return string.Format("{0}.{1}", className, functionName);

                case NodeModel node:
                    var type = node.GetType();
                    var inPortAttribute = type.GetCustomAttributes().OfType<InPortTypesAttribute>().FirstOrDefault();
                    return type.FullName;

                default:
                    return string.Empty;

            }
        }
    }
}

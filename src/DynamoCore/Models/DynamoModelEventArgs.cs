using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Models
{
#if BLOODSTONE

    public class UpdateBloodstoneVisualEventArgs : EventArgs
    {
        public UpdateBloodstoneVisualEventArgs(Dictionary<Guid, IRenderPackage> geometries)
        {
            this.Geometries = geometries;
        }

        public Dictionary<Guid, IRenderPackage> Geometries { get; private set; }
    }

#endif

    public class DynamoModelUpdateArgs : EventArgs
    {
        public object Item { get; set; }

        public DynamoModelUpdateArgs(object item)
        {
            Item = item;
        }
    }

    public class FunctionNamePromptEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool CanEditName { get; set; }
        public bool Success { get; set; }

        public FunctionNamePromptEventArgs()
        {
            Name = "";
            Category = "";
            Description = "";
            CanEditName = true;
        }
    }

    public class ViewOperationEventArgs : EventArgs
    {
        public enum Operation
        {
            FitView, ZoomIn, ZoomOut
        }

        public ViewOperationEventArgs(Operation operation)
        {
            this.ViewOperation = operation;
        }

        public Operation ViewOperation { get; private set; }
    }

    public class PointEventArgs : EventArgs
    {
        public Point Point { get; set; }

        public PointEventArgs(Point p)
        {
            Point = p;
        }
    }

    public class WorkspaceEventArgs
    {
        public WorkspaceModel Workspace { get; set; }

        public WorkspaceEventArgs(WorkspaceModel workspace)
        {
            this.Workspace = workspace;
        }
    }

    public class ModelEventArgs : EventArgs
    {
        public ModelBase Model { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public bool PositionSpecified { get; private set; }
        public bool TransformCoordinates { get; private set; }

        public ModelEventArgs(ModelBase model)
            : this(model, false)
        {
        }

        public ModelEventArgs(ModelBase model, bool transformCoordinates)
        {
            Model = model;
            PositionSpecified = false;
            TransformCoordinates = transformCoordinates;
        }

        public ModelEventArgs(ModelBase model, double x, double y, bool transformCoordinates)
        {
            Model = model;
            X = x;
            Y = y;
            PositionSpecified = true;
            TransformCoordinates = transformCoordinates;
        }
    }

}

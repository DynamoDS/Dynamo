using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media.Media3D;

using Dynamo.Utilities;

using HelixToolkit.Wpf;

namespace Dynamo
{

    /// <summary>
    /// RenderDescriptions provides the final geometry to be bound by the visualization.
    /// Collections are of the type ThreadSafeList which allows for the RenderDescription to be
    /// written to and read from asynchronously.
    /// </summary>
    public class RenderDescription
    {
        /// <summary>
        /// A collection of Point objects to be used for rendering points as selected.
        /// </summary>
        public ThreadSafeList<Point3D> SelectedPoints{ get ; internal set;}
 
        /// <summary>
        /// A collection of Point objects to be used for rendering lines as selected.
        /// </summary>
        public ThreadSafeList<Point3D> SelectedLines { get; internal set; }

        /// <summary>
        /// A collection of mesh objects to be used for rendering as selected.
        /// </summary>
        public ThreadSafeList<MeshGeometry3D> SelectedMeshes { get; internal set; }
 
        /// <summary>
        /// A collection of Point objects used to render points
        /// </summary>
        public ThreadSafeList<Point3D> Points { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render lines
        /// </summary>
        public ThreadSafeList<Point3D> Lines { get; internal set; }

        /// <summary>
        /// A collection of mesh objects to be used for rendering
        /// </summary>
        public ThreadSafeList<MeshGeometry3D> Meshes { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the x axes of transforms
        /// </summary>
        public ThreadSafeList<Point3D> XAxisPoints { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the y axes of transforms
        /// </summary>
        public ThreadSafeList<Point3D> YAxisPoints { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the z axes of transforms
        /// </summary>
        public ThreadSafeList<Point3D> ZAxisPoints { get; internal set; }

        /// <summary>
        /// A collection of text items with locations.
        /// </summary>
        public ThreadSafeList<BillboardTextItem> Text { get; internal set; }
 
        public RenderDescription()
        {
            Points = new ThreadSafeList<Point3D>();
            Lines = new ThreadSafeList<Point3D>();
            Meshes = new ThreadSafeList<MeshGeometry3D>();
            XAxisPoints = new ThreadSafeList<Point3D>();
            YAxisPoints = new ThreadSafeList<Point3D>();
            ZAxisPoints = new ThreadSafeList<Point3D>();

            SelectedPoints = new ThreadSafeList<Point3D>();
            SelectedLines = new ThreadSafeList<Point3D>();
            SelectedMeshes = new ThreadSafeList<MeshGeometry3D>();

            Text = new ThreadSafeList<BillboardTextItem>();
        }

        public void Clear()
        {
            Points.Clear();
            Lines.Clear();
            Meshes.Clear();
            XAxisPoints.Clear();
            YAxisPoints.Clear();
            ZAxisPoints.Clear();
            SelectedPoints.Clear();
            SelectedLines.Clear();
            SelectedMeshes.Clear();
            Text.Clear();
        }
    }


}

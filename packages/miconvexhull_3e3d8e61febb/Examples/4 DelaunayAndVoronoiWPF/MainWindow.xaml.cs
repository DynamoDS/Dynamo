#region

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using MIConvexHull;
using System.Linq;

#endregion

namespace ExampleWithGraphics
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NumberOfVertices = 500;
        private List<Vertex> vertices;
        VoronoiMesh<Vertex, Cell, VoronoiEdge<Vertex, Cell>> voronoiMesh;

        public MainWindow()
        {
            InitializeComponent();
            this.Title += string.Format(" ({0} points)", NumberOfVertices);

            btnFindDelaunay.IsEnabled = false;
            btnFindVoronoi.IsEnabled = false;
        }

        void ShowVertices()
        {
            for (var i = 0; i < NumberOfVertices; i++)
            {
                drawingCanvas.Children.Add(vertices[i]);
            }
        }

        private void btnMakePoints_Click(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Children.Clear();
            var sizeX = drawingCanvas.ActualWidth;
            var sizeY = drawingCanvas.ActualHeight;
            vertices = new List<Vertex>();
            var r = new Random();

            /****** Random Vertices ******/
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var vi = new Vertex(sizeX * r.NextDouble(), sizeY * r.NextDouble());
                vertices.Add(vi);
            }
            
            ShowVertices();

            var now = DateTime.Now;
            voronoiMesh = VoronoiMesh.Create<Vertex, Cell>(vertices);
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = string.Format("{0:0.000}s ({1} faces)", interval.TotalSeconds, voronoiMesh.Vertices.Count());

            btnFindDelaunay.IsEnabled = true;
            btnFindVoronoi.IsEnabled = true;
        }

        private void btnFindDelaunay_Click(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Children.Clear();

            btnFindDelaunay.IsEnabled = false;
            btnFindVoronoi.IsEnabled = true;

            foreach (var cell in voronoiMesh.Vertices)
            {
                drawingCanvas.Children.Add(cell.Visual);
            }

            ////foreach (var cell in voronoiMesh.Vertices)
            ////{
            ////    foreach (var n in cell.Adjacency)
            ////    {
            ////        if (n == null) continue;
            ////        drawingCanvas.Children.Add(new Line { X1 = cell.Centroid.X, Y1 = cell.Centroid.Y, X2 = n.Centroid.X, Y2 = n.Centroid.Y, StrokeThickness = 1, Stroke = Brushes.Blue });
            ////    }
            ////}

            ////foreach (var cell in voronoiMesh.Vertices)
            ////{
            ////    drawingCanvas.Children.Add(new Vertex(cell.Circumcenter.X, cell.Circumcenter.Y, cell.Brush));
            ////    drawingCanvas.Children.Add(new Line { X1 = cell.Centroid.X, Y1 = cell.Centroid.Y, X2 = cell.Circumcenter.X, Y2 = cell.Circumcenter.Y, StrokeThickness = 1, Stroke = Brushes.Blue });
            ////    ////foreach (var v in cell.Vertices)
            ////    ////{
            ////    ////    drawingCanvas.Children.Add(new Vertex(v.Position[0], v.Position[1], Brushes.Red));
            ////    ////}
            ////}

            ShowVertices();
        }

        static bool PointInCell(Cell c, Point p)
        {
            var v1 = c.Vertices[0].ToPoint();
            var v2 = c.Vertices[1].ToPoint();
            var v3 = c.Vertices[2].ToPoint();

            var s0 = IsLeft(v1, v2, p);
            var s1 = IsLeft(v2, v3, p);
            var s2 = IsLeft(v3, v1, p);

            return (s0 == s1) && (s1 == s2);
        }

        static int IsLeft(Point a, Point b, Point c)
        {
            return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0 ? 1 : -1;
        }

        static Point Center(Cell c)
        {
            var v1 = (Vector)c.Vertices[0].ToPoint();
            var v2 = (Vector)c.Vertices[1].ToPoint();
            var v3 = (Vector)c.Vertices[2].ToPoint();

            return (Point)((v1 + v2 + v3) / 3);
        }

        private void btnFindVoronoi_Click(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Children.Clear();
            btnFindVoronoi.IsEnabled = false;
            btnFindDelaunay.IsEnabled = true;   
            
            foreach (var edge in voronoiMesh.Edges)
            {
                var from = edge.Source.Circumcenter;
                var to = edge.Target.Circumcenter;
                drawingCanvas.Children.Add(new Line { X1 = from.X, Y1 = from.Y, X2 = to.X, Y2 = to.Y, Stroke = Brushes.Black });
            }

            foreach (var cell in voronoiMesh.Vertices)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (cell.Adjacency[i] == null)
                    {
                        var from = cell.Circumcenter;
                        var t = cell.Vertices.Where((_, j) => j != i).ToArray();
                        var factor = 100 * IsLeft(t[0].ToPoint(), t[1].ToPoint(), from) * IsLeft(t[0].ToPoint(), t[1].ToPoint(), Center(cell));
                        var dir = new Point(0.5 * (t[0].Position[0] + t[1].Position[0]), 0.5 * (t[0].Position[1] + t[1].Position[1])) - from;
                        var to = from + factor * dir;
                        drawingCanvas.Children.Add(new Line { X1 = from.X, Y1 = from.Y, X2 = to.X, Y2 = to.Y, Stroke = Brushes.Black });
                    }                    
                }
            }

            ShowVertices();
            drawingCanvas.Children.Add(new Rectangle { Width = drawingCanvas.ActualWidth, Height = drawingCanvas.ActualHeight, Stroke = Brushes.Black, StrokeThickness = 3 });
        }
    }
}
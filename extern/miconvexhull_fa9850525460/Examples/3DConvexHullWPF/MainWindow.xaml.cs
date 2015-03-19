#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MIConvexHull;
using Petzold.Media3D;

#endregion

namespace ExampleWithGraphics
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NumberOfVertices = 1000;
        private const double size = 50;
        private List<Vertex> convexHullVertices;
        private List<Face> faces;
        private ModelVisual3D modViz;
        private List<Vertex> vertices;

        public MainWindow()
        {
            InitializeComponent();
            btnMakeSquarePoints_Click(null, null);
        }

        private void ClearAndDrawAxes()
        {
            var light = viewport.Children[0];
            viewport.Children.Clear();
            viewport.Children.Add(light);
            // Rendering the axis made the viewport render incorrectly.
            //var ax = new Axes { Extent = 60 };
            //viewport.Children.Add(ax);
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Running...");
            var now = DateTime.Now;
            var convexHull = ConvexHull.Create<Vertex, Face>(vertices);
            convexHullVertices = convexHull.Points.ToList();
            faces = convexHull.Faces.ToList();
            var interval = DateTime.Now - now;
            txtBlkTimer.Text = interval.Hours + ":" + interval.Minutes
                               + ":" + interval.Seconds + "." + interval.TotalMilliseconds;

            Display();
        }

        private void Display()
        {
            viewport.Children.Remove(modViz);

            var CVPoints = new Point3DCollection();
            foreach (var chV in convexHullVertices)
            {
                CVPoints.Add(chV.Center);
                viewport.Children.Add(chV.AsHullVertex());
            }
            
            var faceTris = new Int32Collection();
            foreach (var f in faces)
            {
                // The vertices are stored in clockwise order.
                faceTris.Add(convexHullVertices.IndexOf(f.Vertices[0]));
                faceTris.Add(convexHullVertices.IndexOf(f.Vertices[1]));
                faceTris.Add(convexHullVertices.IndexOf(f.Vertices[2]));
            }
            var mg3d = new MeshGeometry3D
            {
                Positions = CVPoints,
                TriangleIndices = faceTris
            };

            var material = new MaterialGroup
            {
                Children = new MaterialCollection
                {
                    new DiffuseMaterial(Brushes.Red),
                    new SpecularMaterial(Brushes.Beige, 2.0)
                }
            };

            var geoMod = new GeometryModel3D
            {
                Geometry = mg3d,
                Material = material
            };

            modViz = new ModelVisual3D { Content = geoMod };
            viewport.Children.Add(modViz);
        }

        private void btnMakeSquarePoints_Click(object sender, RoutedEventArgs e)
        {
            ClearAndDrawAxes();
            vertices = new List<Vertex>();
            var r = new Random();

            /****** Random Vertices ******/
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var vi = new Vertex(size * r.NextDouble() - size / 2, size * r.NextDouble() - size / 2,
                                    size * r.NextDouble() - size / 2);
                vertices.Add(vi);
                viewport.Children.Add(vi);
            }

            btnRun.IsDefault = true;
            txtBlkTimer.Text = "00:00:00.000";
        }

        private void btnMakeRegularPoints_Click(object sender, RoutedEventArgs e)
        {
            ClearAndDrawAxes();
            vertices = new List<Vertex>();
            var r = new Random();
            
            for (int i = 0; i <= 10; i++)
            {
                for (int j = 0; j <= 10; j++)
                {
                    for (int k = 0; k <= 10; k++)
                    {
                        var v = new Vertex(-20 + 4 * i, -20 + 4 * j, -20 + 4 * k);
                        vertices.Add(v);
                        viewport.Children.Add(v);
                    }
                }
            }

            btnRun.IsDefault = true;
            txtBlkTimer.Text = "00:00:00.000";
        }

        private void btnMakeCirclePoints_Click(object sender, RoutedEventArgs e)
        {
            ClearAndDrawAxes();
            vertices = new List<Vertex>();
            var r = new Random();

            /****** Random Vertices ******/
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var radius = size + r.NextDouble();
                // if (i < NumberOfVertices / 2) radius /= 2;
                var theta = 2 * Math.PI * r.NextDouble();
                var azimuth = Math.PI * r.NextDouble();
                var x = radius * Math.Cos(theta) * Math.Sin(azimuth);
                var y = radius * Math.Sin(theta) * Math.Sin(azimuth);
                var z = radius * Math.Cos(azimuth);
                var vi = new Vertex(x, y, z);
                vertices.Add(vi);
                /*
                 *          do {
                 x1 = 2.0 * ranf() - 1.0;
                 x2 = 2.0 * ranf() - 1.0;
                 w = x1 * x1 + x2 * x2;
         } while ( w >= 1.0 );

         w = sqrt( (-2.0 * ln( w ) ) / w );
         y1 = x1 * w;
         y2 = x2 * w;

                 */

                viewport.Children.Add(vi);
            }
            btnRun.IsDefault = true;
            txtBlkTimer.Text = "00:00:00.000";
        }
    }
}
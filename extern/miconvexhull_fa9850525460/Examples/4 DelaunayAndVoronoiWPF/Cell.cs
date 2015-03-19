/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with MIConvexHull.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://miconvexhull.codeplex.com
 *************************************************************************/

namespace ExampleWithGraphics
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using MIConvexHull;

    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Cell : TriangulationCell<Vertex, Cell>
    {
        static Random rnd = new Random();

        public Brush Brush { get; private set; }

        public class FaceVisual : Shape
        {
            Cell f;

            Geometry geometry;
            protected override Geometry DefiningGeometry
            {
                get
                {
                    if (geometry != null) return geometry;

                    var myPathGeometry = new PathGeometry() { };
                    var pathFigure1 = new PathFigure
                    {
                        StartPoint = new Point(f.Vertices[0].Position[0], f.Vertices[0].Position[1])
                    };
                    for (int i = 1; i < 3; i++)
                    {
                        pathFigure1.Segments.Add(
                            new LineSegment(
                                new Point(f.Vertices[i].Position[0],
                                          f.Vertices[i].Position[1]), true) { IsSmoothJoin = true });
                    }
                    pathFigure1.IsClosed = true;
                    myPathGeometry.Figures.Add(pathFigure1);

                    Fill = f.Brush;
                    geometry = myPathGeometry;
                    return geometry;
                }
            }

            public FaceVisual(Cell f)
            {
                Stroke = Brushes.Black;
                StrokeThickness = 1.0;
                Opacity = 0.3;
                this.f = f;

                var fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255)));
                f.Brush = fill;
            }
        }

        double Det(double[,] m)
        {
            return m[0, 0] * ((m[1, 1] * m[2, 2]) - (m[2, 1] * m[1, 2])) - m[0, 1] * (m[1, 0] * m[2, 2] - m[2, 0] * m[1, 2]) + m[0, 2] * (m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1]);
        }

        double LengthSquared(double[] v)
        {
            double norm = 0;
            for (int i = 0; i < v.Length; i++)
            {
                var t = v[i];
                norm += t * t;
            }
            return norm;
        }

        Point GetCircumcenter()
        {
            // From MathWorld: http://mathworld.wolfram.com/Circumcircle.html

            var points = Vertices;

            double[,] m = new double[3, 3];

            // x, y, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 0] = points[i].Position[0];
                m[i, 1] = points[i].Position[1];
                m[i, 2] = 1;
            }
            var a = Det(m);

            // size, y, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 0] = LengthSquared(points[i].Position);
            }
            var dx = -Det(m);

            // size, x, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 1] = points[i].Position[0];
            }
            var dy = Det(m);

            // size, x, y
            for (int i = 0; i < 3; i++)
            {
                m[i, 2] = points[i].Position[1];
            }
            var c = -Det(m);

            var s = -1.0 / (2.0 * a);
            var r = System.Math.Abs(s) * System.Math.Sqrt(dx * dx + dy * dy - 4 * a * c);
            return new Point(s * dx, s * dy);
        }

        Point GetCentroid()
        {
            return new Point(Vertices.Select(v => v.Position[0]).Average(), Vertices.Select(v => v.Position[1]).Average());
        }

        public Shape Visual { get; private set; }
        Point? circumCenter;
        public Point Circumcenter
        {
            get
            {
                circumCenter = circumCenter ?? GetCircumcenter();
                return circumCenter.Value;
            }
        }

        Point? centroid;
        public Point Centroid
        {
            get
            {
                centroid = centroid ?? GetCentroid();
                return centroid.Value;
            }
        }

        public Cell()
        {
            Visual = new FaceVisual(this);
        }      
    }
}

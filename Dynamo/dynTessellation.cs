using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;
using MIConvexHull;

namespace Dynamo.Elements
{
    [ElementName("Delaunay Tessellation")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("Create a delaunay tesselation from a number of reference points.")]
    [RequiresTransaction(true)]
    public class dynDelaunayTessellation : dynNode
    {
        public dynDelaunayTessellation()
        {
            InPortData.Add(new PortData("pts", "List of reference points.", typeof(object)));
            InPortData.Add(new PortData("face", "The face on which to tessellate.", typeof(object)));
            OutPortData = new PortData("out", "Tessellation data.", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            foreach (var e in this.Elements)
            {
                this.DeleteElement(e);
            }
            
            var input = args[0];
            var result = FSharpList<Expression>.Empty;

            //see dynSketchPlane
            if (input.IsList)
            {
                var uvList = (input as Expression.List).Item;
                int length = uvList.Length;

                List<Vertex> verts = new List<Vertex>();

                for (int i = 0; i < length; i++)
                {
                    UV uv = (UV)((Expression.Container)uvList[i]).Item;
                    Vertex vert = new Vertex(uv.U, uv.V);
                    verts.Add(vert);
                }

                //ConvexHull<IVertex, DefaultConvexFace<IVertex>> ch = ConvexHull.Create(verts);
                VoronoiMesh<Vertex, Cell, VoronoiEdge<Vertex, Cell>> voronoiMesh = voronoiMesh = VoronoiMesh.Create<Vertex, Cell>(verts);

                object arg0 = ((Expression.Container)args[1]).Item;
                if (arg0 is Reference)
                {
                    Reference faceRef = arg0 as Reference;
                    Face f = this.UIDocument.Document.GetElement(faceRef.ElementId).GetGeometryObjectFromReference(faceRef) as Face;

                    foreach (VoronoiEdge<Vertex, Cell> edge in voronoiMesh.Edges)
                    {
                        var from = edge.Source.Circumcenter;
                        var to = edge.Target.Circumcenter;

                        UV uv1 = new UV(from.X, from.Y);
                        UV uv2 = new UV(to.X, to.Y);

                        //don't add points that are not inside the face
                        if (!f.IsInside(uv1) || !f.IsInside(uv2))
                        {
                            continue;
                        }

                        XYZ start = f.Evaluate(uv1);
                        XYZ end = f.Evaluate(uv2);
                        if (start.DistanceTo(end) < .1)
                        {
                            continue;
                        }

                        //FSharpList<Expression> pts = FSharpList<Expression>.Empty;
                        //pts = FSharpList<Expression>.Cons(Expression.NewContainer(start), pts);
                        //pts = FSharpList<Expression>.Cons(Expression.NewContainer(end), pts);

                        Line l = this.UIDocument.Application.Application.Create.NewLineBound(start, end);

                        //result = FSharpList<Expression>.Cons(Expression.NewList(pts), result);
                        result = FSharpList<Expression>.Cons(Expression.NewContainer(l), result);

                    }
                }
                return Expression.NewList(result);
            }

            return Expression.NewList(result);
        }
    }

    public class Vertex : IVertex
    {

        public double[] Position { get; set; }

        public Vertex(double x, double y)
        {
            Position = new double[] { x, y};
        }
    }

    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Cell : TriangulationCell<Vertex, Cell>
    {

        System.Windows.Point GetCircumcenter()
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
            var a = StarMath.determinant(m);

            // size, y, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 0] = StarMath.norm2(points[i].Position, 2, true);
            }
            var dx = -StarMath.determinant(m);

            // size, x, 1
            for (int i = 0; i < 3; i++)
            {
                m[i, 1] = points[i].Position[0];
            }
            var dy = StarMath.determinant(m);

            // size, x, y
            for (int i = 0; i < 3; i++)
            {
                m[i, 2] = points[i].Position[1];
            }
            var c = -StarMath.determinant(m);

            var s = -1.0 / (2.0 * a);
            var r = System.Math.Abs(s) * System.Math.Sqrt(dx * dx + dy * dy - 4 * a * c);
            return new System.Windows.Point(s * dx, s * dy);
        }

        System.Windows.Point GetCentroid()
        {
            return new System.Windows.Point(Vertices.Select(v => v.Position[0]).Average(), Vertices.Select(v => v.Position[1]).Average());
        }

        System.Windows.Point? circumCenter;
        public System.Windows.Point Circumcenter
        {
            get
            {
                circumCenter = circumCenter ?? GetCircumcenter();
                return circumCenter.Value;
            }
        }

        System.Windows.Point? centroid;
        public System.Windows.Point Centroid
        {
            get
            {
                centroid = centroid ?? GetCentroid();
                return centroid.Value;
            }
        }

        public Cell()
        {
        }
    }

}

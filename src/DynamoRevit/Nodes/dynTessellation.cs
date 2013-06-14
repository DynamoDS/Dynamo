using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Revit;
using MIConvexHull;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Voronoi On Face")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TESSELATE)]
    [NodeDescription("Create a Voronoi tesselation on a face.")]
    public class dynVoronoiOnFace : dynRevitTransactionNodeWithOneOutput
    {
        List<Line> _tessellationLines = new List<Line>();

        public dynVoronoiOnFace()
        {
            InPortData.Add(new PortData("UVs", "List of UV coordinates.", typeof(Value.List)));
            InPortData.Add(new PortData("face", "The face on which to tessellate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("out", "Tessellation data.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            var result = FSharpList<Value>.Empty;

            if (input.IsList)
            {
                var uvList = (input as Value.List).Item;
                int length = uvList.Length;

                var verts = new List<Vertex2>();

                for (int i = 0; i < length; i++)
                {
                    UV uv = (UV)((Value.Container)uvList[i]).Item;
                    Vertex2 vert = new Vertex2(uv.U, uv.V);
                    verts.Add(vert);
                }

                var voronoiMesh = VoronoiMesh.Create<Vertex2, Cell2>(verts);

                _tessellationLines.Clear();

                object arg1 = ((Value.Container)args[1]).Item;
                var f = arg1 as Face;

                if (f != null)
                {
                    foreach (VoronoiEdge<Vertex2, Cell2> edge in voronoiMesh.Edges)
                    {
                        var from = edge.Source.Circumcenter;
                        var to = edge.Target.Circumcenter;

                        var uv1 = new UV(from.X, from.Y);
                        var uv2 = new UV(to.X, to.Y);

                        if (!f.IsInside(uv1) || !f.IsInside(uv2))
                        {
                            continue;
                        }

                        var start = f.Evaluate(uv1);
                        var end = f.Evaluate(uv2);

                        if (start.DistanceTo(end) > 0.1)
                        {
                            var l = this.UIDocument.Application.Application.Create.NewLineBound(start, end);
                            _tessellationLines.Add(l);

                            result = FSharpList<Value>.Cons(Value.NewContainer(l), result);
                        }
                       
                    }
                }

                return Value.NewList(result);
            }

            return Value.NewList(result);
        }

        public override void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new Nodes.RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (Line l in _tessellationLines)
            {
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(0).X, 
                    l.get_EndPoint(0).Y, l.get_EndPoint(0).Z));
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(1).X,
                    l.get_EndPoint(1).Y, l.get_EndPoint(1).Z));
            }
        }
    }

    [NodeName("Delaunay On Face")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TESSELATE)]
    [NodeDescription("Create a Delaunay triangulation on a face.")]
    public class dynDelaunayOnFace : dynRevitTransactionNodeWithOneOutput
    {
        List<Line> _tessellationLines = new List<Line>();

        public dynDelaunayOnFace()
        {
            InPortData.Add(new PortData("UVs", "List of UV coordinates.", typeof(Value.List)));
            InPortData.Add(new PortData("face", "The face on which to tessellate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("out", "Tessellation data.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            var result = FSharpList<Value>.Empty;

            if (input.IsList)
            {
                var uvList = (input as Value.List).Item;
                int length = uvList.Length;

                var verts = new List<Vertex2>();

                for (int i = 0; i < length; i++)
                {
                    UV uv = (UV)((Value.Container)uvList[i]).Item;
                    Vertex2 vert = new Vertex2(uv.U, uv.V);
                    verts.Add(vert);
                }

                var triangulation = DelaunayTriangulation<Vertex2, Cell2>.Create(verts);

                _tessellationLines.Clear();

                object arg1 = ((Value.Container)args[1]).Item;
                var f = arg1 as Face;

                if (f != null)
                {
                    // there are three vertices per cell in 2D
                    foreach (var cell in triangulation.Cells)
                    {
                        var v1 = cell.Vertices[0];
                        var v2 = cell.Vertices[1];
                        var v3 = cell.Vertices[2];

                        var uv1 = new UV(v1.Position[0], v1.Position[1]);
                        var uv2 = new UV(v2.Position[0], v2.Position[1]);
                        var uv3 = new UV(v3.Position[0], v3.Position[1]);

                        if (!f.IsInside(uv1) || !f.IsInside(uv2) || !f.IsInside(uv3))
                        {
                            continue;
                        }

                        var xyz1 = f.Evaluate(uv1);
                        var xyz2 = f.Evaluate(uv2);
                        var xyz3 = f.Evaluate(uv3);

                        if (xyz1.DistanceTo(xyz2) > 0.1)
                        {
                            var l1 = this.UIDocument.Application.Application.Create.NewLineBound(xyz1, xyz2);
                            _tessellationLines.Add(l1);
                            result = FSharpList<Value>.Cons(Value.NewContainer(l1), result);
                        }

                        if (xyz2.DistanceTo(xyz3) > 0.1)
                        {
                            var l1 = this.UIDocument.Application.Application.Create.NewLineBound(xyz3, xyz2);
                            _tessellationLines.Add(l1);
                            result = FSharpList<Value>.Cons(Value.NewContainer(l1), result);
                        }

                        if (xyz3.DistanceTo(xyz1) > 0.1)
                        {
                            var l1 = this.UIDocument.Application.Application.Create.NewLineBound(xyz1, xyz3);
                            _tessellationLines.Add(l1);
                            result = FSharpList<Value>.Cons(Value.NewContainer(l1), result);
                        }
                    }
                }

                return Value.NewList(result);
            }

            return Value.NewList(result);
        }

        public override void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new Nodes.RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (Line l in _tessellationLines)
            {
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(0).X,
                    l.get_EndPoint(0).Y, l.get_EndPoint(0).Z));
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(1).X,
                    l.get_EndPoint(1).Y, l.get_EndPoint(1).Z));
            }
        }
    }

    /*

    // The results of this node seem correct, but we'll need to do more work to
    // produce the cells and clip them inside of a bounding box
     
    [NodeName("Voronoi 3D")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TESSELATE)]
    [NodeDescription("Find the voronoi cells of a set of 3d points.")]
    public class dynVoronoi3d : dynRevitTransactionNodeWithOneOutput
    {
        List<Line> _tessellationLines = new List<Line>();

        public dynVoronoi3d()
        {
            InPortData.Add(new PortData("XYZs", "List of xyz's.", typeof(Value.List)));
            OutPortData.Add(new PortData("out", "Tessellation data.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            var result = FSharpList<Value>.Empty;

            if (input.IsList)
            {
                var ptList = (input as Value.List).Item;
                int length = ptList.Length;

                var verts = new List<Vertex3>();

                for (int i = 0; i < length; i++)
                {
                    var pt = (XYZ)((Value.Container)ptList[i]).Item;
                    var vert = new Vertex3(pt.X, pt.Y, pt.Z);
                    verts.Add(vert);
                }

                // make triangulation
                var triResult = VoronoiMesh.Create<Vertex3, Cell3>(verts);

                _tessellationLines.Clear();

                // make edges
                foreach (var edge in triResult.Edges)
                {
                    var source = edge.Source.Circumcenter.ToXYZ();
                    var target = edge.Target.Circumcenter.ToXYZ();

                    var l1 = this.UIDocument.Application.Application.Create.NewLineBound(source, target);
                    _tessellationLines.Add(l1);

                    result = FSharpList<Value>.Cons(Value.NewContainer(l1), result);
                }

                return Value.NewList(result);
            }

            return Value.NewList(result);
        }

        public override void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new Nodes.RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (Line l in _tessellationLines)
            {
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(0).X,
                    l.get_EndPoint(0).Y, l.get_EndPoint(0).Z));
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(1).X,
                    l.get_EndPoint(1).Y, l.get_EndPoint(1).Z));
            }
        }
    }

     */

    [NodeName("Convex Hull 3D")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TESSELATE)]
    [NodeDescription("Find the convex hull of a set of XYZs.")]
    public class dynConvexHull3d : dynRevitTransactionNodeWithOneOutput
    {
        List<Line> _tessellationLines = new List<Line>();
        List<HelixToolkit.Wpf.Mesh3D> _tesselationMeshes = new List<HelixToolkit.Wpf.Mesh3D>();

        public dynConvexHull3d()
        {
            InPortData.Add(new PortData("XYZs", "List of xyz's.", typeof(Value.List)));
            OutPortData.Add(new PortData("out", "Tessellation data.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            var result = FSharpList<Value>.Empty;

            if (input.IsList)
            {
                var ptList = (input as Value.List).Item;
                int length = ptList.Length;

                var verts = new List<Vertex3>();

                for (int i = 0; i < length; i++)
                {
                    var pt = (XYZ)((Value.Container)ptList[i]).Item;
                    var vert = new Vertex3(pt.X, pt.Y, pt.Z);
                    verts.Add(vert);
                }

                // make triangulation
                var triResult = ConvexHull<Vertex3, TriangleFace>.Create(verts);

                _tessellationLines.Clear();
                _tesselationMeshes.Clear();
                var mesh = new HelixToolkit.Wpf.Mesh3D();

                // make edges
                foreach (var face in triResult.Faces)
                {
                    // form mesh
                    mesh.Vertices.Add( face.Vertices[1].ToPoint3D() );
                    mesh.Vertices.Add( face.Vertices[0].ToPoint3D() );
                    mesh.Vertices.Add( face.Vertices[2].ToPoint3D() );
                    mesh.AddFace( new int[3] { mesh.Vertices.Count-1, mesh.Vertices.Count-2, mesh.Vertices.Count-3 });
                    _tesselationMeshes.Add(mesh);

                    // form lines for use in dynamo or revit
                    var start1 = face.Vertices[0].ToXYZ();
                    var end1 = face.Vertices[1].ToXYZ();

                    var start2 = face.Vertices[1].ToXYZ();
                    var end2 = face.Vertices[2].ToXYZ();

                    var start3 = face.Vertices[2].ToXYZ();
                    var end3 = face.Vertices[0].ToXYZ();

                    if (start1.DistanceTo(end1) > 0.1)
                    {
                        var l1 = this.UIDocument.Application.Application.Create.NewLineBound(start1, end1);
                        _tessellationLines.Add(l1);
                        result = FSharpList<Value>.Cons(Value.NewContainer(l1), result);
                    }

                    if (start2.DistanceTo(end2) > 0.1)
                    {
                        var l1 = this.UIDocument.Application.Application.Create.NewLineBound(start2, end2);
                        _tessellationLines.Add(l1);
                        result = FSharpList<Value>.Cons(Value.NewContainer(l1), result);
                    }

                    if (start3.DistanceTo(end3) > 0.1)
                    {
                        var l1 = this.UIDocument.Application.Application.Create.NewLineBound(start3, end3);
                        _tessellationLines.Add(l1);
                        result = FSharpList<Value>.Cons(Value.NewContainer(l1), result);
                    }
                }

                return Value.NewList(result);
            }

            return Value.NewList(result);
        }

        public override void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new Nodes.RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (var mesh in _tesselationMeshes)
            {
                RenderDescription.meshes.Add(mesh);
            }

            foreach (Line l in _tessellationLines)
            {
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(0).X,
                    l.get_EndPoint(0).Y, l.get_EndPoint(0).Z));
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(1).X,
                    l.get_EndPoint(1).Y, l.get_EndPoint(1).Z));
            }
        }
    }

    [NodeName("Delaunay 3D")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_TESSELATE)]
    [NodeDescription("Create a 3d delaunay tesselation of XYZs.")]
    public class dynDelaunay3d : dynRevitTransactionNodeWithOneOutput
    {
        List<Line> _tessellationLines = new List<Line>();

        public dynDelaunay3d()
        {
            InPortData.Add(new PortData("XYZs", "List of xyz's.", typeof(Value.List)));
            OutPortData.Add(new PortData("out", "Tessellation data.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];
            var result = FSharpList<Value>.Empty;

            if (input.IsList)
            {
                var ptList = (input as Value.List).Item;
                int length = ptList.Length;

                var verts = new List<Vertex3>();

                for (int i = 0; i < length; i++)
                {
                    var pt = (XYZ) ((Value.Container) ptList[i]).Item;
                    var vert = new Vertex3(pt.X, pt.Y, pt.Z);
                    verts.Add(vert);
                }

                // make triangulation
                var triResult = DelaunayTriangulation<Vertex3, Tetrahedron>.Create(verts);

                _tessellationLines.Clear();

                // make edges
                foreach (var cell in triResult.Cells)
                {
                    foreach (var face in cell.MakeFaces())
                    {
                        var start1 = cell.Vertices[face[0]].ToXYZ();
                        var end1 = cell.Vertices[face[1]].ToXYZ();

                        var start2 = cell.Vertices[face[1]].ToXYZ();
                        var end2 = cell.Vertices[face[2]].ToXYZ();

                        var start3 = cell.Vertices[face[2]].ToXYZ();
                        var end3 = cell.Vertices[face[0]].ToXYZ();

                        var l1 = this.UIDocument.Application.Application.Create.NewLineBound(start1, end1);
                        _tessellationLines.Add(l1);

                        var l2 = this.UIDocument.Application.Application.Create.NewLineBound(start2, end2);
                        _tessellationLines.Add(l2);

                        var l3 = this.UIDocument.Application.Application.Create.NewLineBound(start3, end3);
                        _tessellationLines.Add(l3);

                        result = FSharpList<Value>.Cons(Value.NewContainer(l1), result);
                        result = FSharpList<Value>.Cons(Value.NewContainer(l2), result);
                        result = FSharpList<Value>.Cons(Value.NewContainer(l3), result);
                    }
                }

                return Value.NewList(result);
            }

            return Value.NewList(result);
        }

        public override void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new Nodes.RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (Line l in _tessellationLines)
            {
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(0).X,
                    l.get_EndPoint(0).Y, l.get_EndPoint(0).Z));
                RenderDescription.lines.Add(new Point3D(l.get_EndPoint(1).X,
                    l.get_EndPoint(1).Y, l.get_EndPoint(1).Z));
            }
        }
    }

    public class Vertex2 : IVertex
    {

        public double[] Position { get; set; }

        public Vertex2(double x, double y)
        {
            Position = new double[] { x, y};
        }
    }

    public class Vertex3 : IVertex
    {

        public double[] Position { get; set; }

        public Vertex3(double x, double y, double z)
        {
            Position = new double[] { x, y, z};
        }
    
        internal Point3D ToPoint3D()
        {
 	        return new Point3D(Position[0], Position[1], Position[2]); 
        }

        internal XYZ ToXYZ()
        {
            return new XYZ(Position[0], Position[1], Position[2]);
        }

        internal double NormSquared()
        {
            return Position[0]*Position[0] + Position[1]*Position[1] + Position[2]*Position[2];
        }

        internal double Norm()
        {
            return Math.Sqrt(NormSquared());
        }
    
    }
    
    /// <summary>
    /// A cell for a 2d tesselation
    /// </summary>
    public class Cell2 : TriangulationCell<Vertex2, Cell2>
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

        public Cell2()
        {
        }
    }

    /// <summary>
    /// A cell for a 3d tesselation
    /// </summary>
    public class Cell3 : TriangulationCell<Vertex3, Cell3>
    {

        Vertex3 GetCircumcenter()
        {
            // From MathWorld: http://mathworld.wolfram.com/Circumsphere.html

            if ( Vertices.Count() != 4)
                throw new Exception("Malformed voronoi cell");

            var points = Vertices;
            var norms = points.Select((pt) => pt.NormSquared()).ToList();

            double[,] aM = new double[4, 4];

            for (var i = 0; i < 4; i++)
            {
                var pt = points[i];

                aM[i, 0] = pt.Position[0];
                aM[i, 1] = pt.Position[1];
                aM[i, 2] = pt.Position[2];
                aM[i, 3] = 1;
            }

            double a2 = StarMath.determinant(aM)*2;

            double[,] DxM = new double[4, 4];

            for (var i = 0; i < 4; i++)
            {
                var pt = points[i];

                DxM[i, 0] = norms[i];
                DxM[i, 1] = pt.Position[1];
                DxM[i, 2] = pt.Position[2];
                DxM[i, 3] = 1;
            }

            double Dx = StarMath.determinant(DxM);

            double[,] DyM = new double[4, 4];

            for (var i = 0; i < 4; i++)
            {
                var pt = points[i];

                DyM[i, 0] = norms[i];
                DyM[i, 1] = pt.Position[0];
                DyM[i, 2] = pt.Position[2];
                DyM[i, 3] = 1;
            }

            double Dy = -StarMath.determinant(DyM);

            double[,] DzM = new double[4, 4];

            for (var i = 0; i < 4; i++)
            {
                var pt = points[i];

                DzM[i, 0] = norms[i];
                DzM[i, 1] = pt.Position[0];
                DzM[i, 2] = pt.Position[1];
                DzM[i, 3] = 1;
            }

            double Dz = StarMath.determinant(DzM);

            // used for obtaining circumradius
            //double[,] cM = new double[4, 4];

            //for (var i = 0; i < 4; i++)
            //{
            //    var pt = points[i];

            //    cM[i, 0] = norms[i];
            //    cM[i, 1] = pt.Position[0];
            //    cM[i, 2] = pt.Position[1];
            //    cM[i, 3] = pt.Position[2];
            //}

            return new Vertex3(Dx / a2, Dy / a2, Dz / a2);

        }

        private Vertex3 circumCenter = null;
        public Vertex3 Circumcenter
        {
            get
            {
                circumCenter = circumCenter ?? GetCircumcenter();
                return circumCenter;
            }
        }

        public Cell3()
        {
        }
    }

     /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class TriangleFace : TriangulationCell<Vertex3, TriangleFace>
    {
        
    }

    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Tetrahedron : TriangulationCell<Vertex3, Tetrahedron>
    {
        /// <summary>
        /// Helper function to get the position of the i-th vertex.
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Position of the i-th vertex</returns>
        Point3D GetPosition(int i)
        {
            return Vertices[i].ToPoint3D();
        }

        /// <summary>
        /// This function adds indices for a triangle representing the face.
        /// The order is in the CCW (counter clock wise) order so that the automatically calculated normals point in the right direction.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="center"></param>
        /// <param name="indices"></param>
        int[] MakeFace(int i, int j, int k, Vector3D center)
        {
            var u = GetPosition(j) - GetPosition(i);
            var v = GetPosition(k) - GetPosition(j);
                       
            // compute the normal and the plane corresponding to the side [i,j,k]
            var n = Vector3D.CrossProduct(u, v);
            var d = -Vector3D.DotProduct(n, center);
                        
            // check if the normal faces towards the center
            var t = Vector3D.DotProduct(n, (Vector3D)GetPosition(i)) + d;            
            if (t >= 0)
            {
                // swapping indices j and k also changes the sign of the normal, because cross product is anti-commutative
                return new int[]{ k, j, i};
            }
            return new int[]{ i, j ,k };
        }

        /// <summary>
        /// Creates a model of the tetrahedron. Transparency is applied to the color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="radius"></param>
        /// <returns>A model representing the tetrahedron</returns>
        public int[][] MakeFaces()
        {
            var points = new Point3DCollection(Enumerable.Range(0, 4).Select(i => GetPosition(i)));
            
            var center = points.Aggregate(new Vector3D(), (a, c) => a + (Vector3D)c) / (double)points.Count;

            var indices = new int[4][];
            indices[0] = MakeFace(0, 1, 2, center);
            indices[1] = MakeFace(0, 1, 3, center);
            indices[2] = MakeFace(0, 2, 3, center);
            indices[3] = MakeFace(1, 2, 3, center);

            return indices;
        }

    }

}

//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls; //for boolean option
using System.Xml;              //for boolean option  
using System.Windows.Media.Media3D;
using System.Reflection;

using Autodesk.Revit;
using Autodesk.Revit.DB;

using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using Dynamo.Revit;
using Dynamo.Connectors;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    public abstract class dynGeometryBase : dynNodeWithOneOutput
    {
        protected dynGeometryBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    public abstract class dynXYZBase : dynGeometryBase, IDrawable, IClearable
    {
        protected List<XYZ> pts = new List<XYZ>();
        public RenderDescription RenderDescription { get; set; }
        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            lock (pts)
            {
                foreach (XYZ pt in pts)
                {
                    if (pt == null)
                        continue;

                    this.RenderDescription.points.Add(new Point3D(pt.X, pt.Y, pt.Z));
                }
            }
            
        }

        public void ClearReferences()
        {
            pts.Clear();
        }
    }

    public abstract class dynCurveBase : dynGeometryBase, IDrawable, IClearable
    {
        protected List<Curve> crvs = new List<Curve>();
        public RenderDescription RenderDescription { get; set; }

        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            lock (crvs)
            {
                foreach (Curve c in crvs)
                {
                    if (c == null)
                        continue;

                    DrawCurve(this.RenderDescription, c);
                }
            }
            
        }

        public void ClearReferences()
        {
            crvs.Clear();
        }

        private void DrawCurve(RenderDescription description, Curve curve)
        {
            IList<XYZ> points = curve.Tessellate();

            for (int i = 0; i < points.Count; ++i)
            {
                XYZ xyz = points[i];

                description.lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));

                if (i == 0 || i == (points.Count - 1))
                    continue;

                description.lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));
            }
        }
    }

    public abstract class dynSolidBase : dynGeometryBase, IDrawable, IClearable
    {
        protected List<Solid> solids = new List<Solid>();
        public RenderDescription RenderDescription { get; set; }

        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            lock (solids)
            {
                foreach (Solid s in solids)
                {
                    if (s == null)
                        continue;

                    dynRevitTransactionNode.DrawSolid(this.RenderDescription, s);
                }
            }
           
        }

        public void ClearReferences()
        {
            solids.Clear();
        }

        //use this only for test run
        public List<Solid> resultingSolidForTestRun()
        {
            return solids;
        }
    }

    public abstract class dynTransformBase : dynGeometryBase, IDrawable, IClearable
    {
        protected List<Transform> transforms = new List<Transform>();
        public RenderDescription RenderDescription { get; set; }

        public void Draw()
        {
            if(this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            foreach (Transform t in transforms)
            {
                Point3D origin = new Point3D(t.Origin.X, t.Origin.Y, t.Origin.Z);
                XYZ x1 = t.Origin + t.BasisX.Multiply(3);
                XYZ y1 = t.Origin + t.BasisY.Multiply(3);
                XYZ z1 = t.Origin + t.BasisZ.Multiply(3);
                Point3D xEnd = new Point3D(x1.X, x1.Y, x1.Z);
                Point3D yEnd = new Point3D(y1.X, y1.Y, y1.Z);
                Point3D zEnd = new Point3D(z1.X, z1.Y, z1.Z);

                this.RenderDescription.xAxisPoints.Add(origin);
                this.RenderDescription.xAxisPoints.Add(xEnd);

                this.RenderDescription.yAxisPoints.Add(origin);
                this.RenderDescription.yAxisPoints.Add(yEnd);

                this.RenderDescription.zAxisPoints.Add(origin);
                this.RenderDescription.zAxisPoints.Add(zEnd);
            }
        }

        public void ClearReferences()
        {
            transforms.Clear();
        }
    }

    [NodeName("XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an XYZ from three numbers.")]
    public class dynXYZ: dynXYZBase
    {
        public dynXYZ()
        {
            InPortData.Add(new PortData("X", "X", typeof(Value.Number)));
            InPortData.Add(new PortData("Y", "Y", typeof(Value.Number)));
            InPortData.Add(new PortData("Z", "Z", typeof(Value.Number)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double x, y, z;
            x = ((Value.Number)args[0]).Item;
            y = ((Value.Number)args[1]).Item;
            z = ((Value.Number)args[2]).Item;

            XYZ pt = new XYZ(x, y, z);
            pts.Add(pt);
            return Value.NewContainer(pt);
        }
    }

    [NodeName("XYZ from List of Numbers")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates a list of XYZs by taking sets of 3 numbers from an list.")]
    public class dynXYZFromListOfNumbers : dynXYZBase
    {
        public dynXYZFromListOfNumbers()
        {
            InPortData.Add(new PortData("list", "The list of numbers from which to extract the XYZs.", typeof(Value.Number)));
            OutPortData.Add(new PortData("list", "A list of XYZs", typeof(Value.List)));

            RegisterAllPorts();
            this.ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
            {
                throw new Exception("Input must be a list of numbers.");
            }

            FSharpList<Value> vals = ((Value.List)args[0]).Item;
            var len = vals.Length;
            if (len % 3 != 0)
                throw new Exception("List size must be a multiple of 3");

            var result = new Value[len / 3];
            int count = 0;
            while (!vals.IsEmpty)
            {
                result[count] = Value.NewContainer(new XYZ(
                    ((Value.Number)vals.Head).Item,
                    ((Value.Number)vals.Tail.Head).Item,
                    ((Value.Number)vals.Tail.Tail.Head).Item));
                vals = vals.Tail.Tail.Tail;
                count++;
            }

            return Value.NewList(Utils.SequenceToFSharpList(result));
        }
    }

    [NodeName("XYZ From Reference Point")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Extracts an XYZ from a Reference Point.")]
    [NodeSearchTags("xyz", "derive", "from", "reference", "point")]
    public class dynXYZFromReferencePoint : dynXYZBase
    {
        public dynXYZFromReferencePoint()
        {
            InPortData.Add(new PortData("pt", "Reference Point", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "Location of the reference point.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ReferencePoint point;
            point = (ReferencePoint)((Value.Container)args[0]).Item;

            pts.Add(point.Position);

            return Value.NewContainer(point.Position);
        }
    }

    [NodeName("XYZ -> X")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Fetches the X value of the given XYZ")]
    public class dynXYZGetX: dynGeometryBase
    { 
        public dynXYZGetX()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("X", "X value of given XYZ", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((XYZ)((Value.Container)args[0]).Item).X);
        }
    }

    [NodeName("XYZ -> Y")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Fetches the Y value of the given XYZ")]
    public class dynXYZGetY : dynGeometryBase
    {
        public dynXYZGetY()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("Y", "Y value of given XYZ", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((XYZ)((Value.Container)args[0]).Item).Y);
        }
    }

    [NodeName("XYZ -> Z")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Fetches the Z value of the given XYZ")]
    public class dynXYZGetZ : dynGeometryBase
    {
        public dynXYZGetZ()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("Z", "Z value of given XYZ", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((XYZ)((Value.Container)args[0]).Item).Z);
        }
    }

    [NodeName("XYZ Zero")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an XYZ at the origin (0,0,0).")]
    public class dynXYZZero: dynXYZBase
    {
        public dynXYZZero()
        {
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            pts.Add(XYZ.Zero);
            return Value.NewContainer(XYZ.Zero);
        }
    }

    [NodeName("X Axis")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an XYZ representing the X basis (1,0,0).")]
    public class dynXYZBasisX : dynGeometryBase
    {
        public dynXYZBasisX()
        {
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ pt = XYZ.BasisX;
            return Value.NewContainer(pt);
        }
    }

    [NodeName("Y Axis")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an XYZ representing the Y basis (0,1,0).")]
    public class dynXYZBasisY : dynGeometryBase
    {
        public dynXYZBasisY()
        {
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ pt = XYZ.BasisY;
            return Value.NewContainer(pt);
        }
    }

    [NodeName("Z Axis")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates an XYZ representing the Z basis (0,0,1).")]
    public class dynXYZBasisZ : dynGeometryBase
    {
        public dynXYZBasisZ()
        {
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            XYZ pt = XYZ.BasisZ;
            return Value.NewContainer(pt);
        }
    }

    [NodeName("Scale XYZ with Base Point")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Scales an XYZ relative to the supplies base point.")]
    public class dynXYZScaleOffset : dynXYZBase
    {
        public dynXYZScaleOffset()
        {
            InPortData.Add(new PortData("xyz", "XYZ to scale", typeof(Value.Container)));
            InPortData.Add(new PortData("n", "Scale amount", typeof(Value.Number)));
            InPortData.Add(new PortData("base", "XYZ serving as the base point of the scale operation", typeof(Value.Container)));

            OutPortData.Add(new PortData("xyz", "Scaled XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ xyz = (XYZ)((Value.Container)args[0]).Item;
            double n = ((Value.Number)args[1]).Item;
            XYZ base_xyz = (XYZ)((Value.Container)args[2]).Item;

            XYZ pt = n * (xyz - base_xyz) + base_xyz;
            pts.Add(pt);
            return Value.NewContainer(pt);
        }
    }

    [NodeName("Scale XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Multiplies each component of an XYZ by a number.")]
    public class dynXYZScale : dynXYZBase
    {
        public dynXYZScale()
        {
            InPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("n", "Scale amount", typeof(Value.Number)));

            OutPortData.Add(new PortData("xyz", "Scaled XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ xyz = (XYZ)((Value.Container)args[0]).Item;
            double n = ((Value.Number)args[1]).Item;

            XYZ pt = xyz.Multiply(n);
            pts.Add(pt);
            return Value.NewContainer(pt);
        }
    }

    [NodeName("Add XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Adds the components of two XYZs.")]
    public class dynXYZAdd: dynXYZBase
    {
        public dynXYZAdd()
        {
            InPortData.Add(new PortData("XYZ(a)", "XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("XYZ(b)", "XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZ(a+b)", "a + b", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ xyza = (XYZ)((Value.Container)args[0]).Item;
            XYZ xyzb = (XYZ)((Value.Container)args[1]).Item;

            XYZ pt = xyza + xyzb;
            pts.Add(pt);
            return Value.NewContainer(pt);
        }
    }

    [NodeName("Subtract XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Subtracts the components of two XYZs.")]
    public class dynXYZSubtract : dynXYZBase
    {
        public dynXYZSubtract()
        {
            InPortData.Add(new PortData("XYZ(a)", "XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("XYZ(b)", "XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("XYZ(a-b)", "a - b", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ xyza = (XYZ)((Value.Container)args[0]).Item;
            XYZ xyzb = (XYZ)((Value.Container)args[1]).Item;

            XYZ pt = xyza - xyzb;
            pts.Add(pt);
            return Value.NewContainer(pt);
        }
    }

    [NodeName("Average XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Averages a list of XYZs.")]
    public class dynXYZAverage : dynXYZBase
    {
        public dynXYZAverage()
        {
            InPortData.Add(new PortData("XYZs", "The list of XYZs to average.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list of XYZs is required to average.");

            var lst = ((Value.List)args[0]).Item;
            var average = dynBestFitLine.MeanXYZ(dynBestFitLine.AsGenericList<XYZ>(lst));

            return Value.NewContainer(average);
        }
    }

    [NodeName("Negate XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Negate an XYZ.")]
    public class dynXYZNegate : dynXYZBase
    {
        public dynXYZNegate()
        {
            InPortData.Add(new PortData("XYZ", "The XYZ to negate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ pt = (XYZ)((Value.Container)args[0]).Item;
            pts.Add(pt);
            return Value.NewContainer(pt.Negate());
        }
    }

    [NodeName("XYZ Cross Product")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Calculate the cross product of two XYZs.")]
    public class dynXYZCrossProduct : dynGeometryBase
    {
        public dynXYZCrossProduct()
        {
            InPortData.Add(new PortData("a", "XYZ A.", typeof(Value.Container)));
            InPortData.Add(new PortData("b", "XYZ B.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "The cross product of vectors A and B. ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ a = (XYZ)((Value.Container)args[0]).Item;
            XYZ b = (XYZ)((Value.Container)args[1]).Item;

            return Value.NewContainer(a.CrossProduct(b));
        }
    }

    [NodeName("XYZ Start End Vector")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Calculate the normalized vector from one xyz to another.")]
    public class dynXYZStartEndVector : dynGeometryBase
    {
        public dynXYZStartEndVector()
        {
            InPortData.Add(new PortData("start", "The start of the vector.", typeof(Value.Container)));
            InPortData.Add(new PortData("end", "The end of the vector.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "The normalized vector from start to end. ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ a = (XYZ)((Value.Container)args[0]).Item;
            XYZ b = (XYZ)((Value.Container)args[1]).Item;

            return Value.NewContainer((b-a).Normalize());
        }
    }

    [NodeName("UV Grid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates a grid of UVs from a domain.")]
    public class dynUVGrid: dynNodeWithOneOutput
    {
        public dynUVGrid()
        {
            InPortData.Add(new PortData("dom", "A domain.", typeof(Value.List)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(Value.Number)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(Value.Number)));
            OutPortData.Add(new PortData("UVs", "List of UVs in the grid", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> domain = ((Value.List)args[0]).Item;
            double ui = ((Value.Number)args[1]).Item;
            double vi = ((Value.Number)args[2]).Item;
            double us = ((Value.Number)domain[2]).Item / ui;
            double vs = ((Value.Number)domain[3]).Item / vi;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            var min = ((Value.Container)domain[0]).Item as UV;
            var max = ((Value.Container)domain[1]).Item as UV;

            //for (double u = min.U; u <= max.U; u += us)
            for (int i = 0; i <= ui; i++ )
            {
                double u = min.U + i*us;

                //for (double v = min.V; v <= max.V; v += vs)
                for (int j = 0; j <= vi; j++ )
                {
                    double v = min.V + j*vs;

                    result = FSharpList<Value>.Cons(
                        Value.NewContainer(new UV(u, v)),
                        result
                    );
                }
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("UV Random")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates a grid of UVs froma domain.")]
    public class dynUVRandom: dynNodeWithOneOutput
    {
        public dynUVRandom()
        {
            InPortData.Add(new PortData("dom", "A domain.", typeof(Value.List)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(Value.Number)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(Value.Number)));
            OutPortData.Add(new PortData("UVs", "List of UVs in the grid", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            FSharpList<Value> domain;
            double ui, vi;

            domain = ((Value.List)args[0]).Item;
            ui = ((Value.Number)args[1]).Item;
            vi = ((Value.Number)args[2]).Item;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            UV min = ((Value.Container)domain[0]).Item as UV;
            UV max = ((Value.Container)domain[1]).Item as UV;
            
            Random r = new Random();
            double uSpan = max.U-min.U;
            double vSpan = max.V-min.V;

            for (int i = 0; i < ui; i++)
            {
                for (int j = 0; j < vi; j++)
                {
                    result = FSharpList<Value>.Cons(
                        Value.NewContainer(new UV(min.U + r.NextDouble()*uSpan, min.V + r.NextDouble()*vSpan)),
                        result
                    );
                }
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("XYZ Grid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates a grid of XYZs.")]
    public class dynReferencePtGrid: dynXYZBase
    {
        public dynReferencePtGrid()
        {
            InPortData.Add(new PortData("x-count", "Number in the X direction.", typeof(Value.Number)));
            InPortData.Add(new PortData("y-count", "Number in the Y direction.", typeof(Value.Number)));
            InPortData.Add(new PortData("z-count", "Number in the Z direction.", typeof(Value.Number)));
            InPortData.Add(new PortData("x0", "Starting X Coordinate", typeof(Value.Number)));
            InPortData.Add(new PortData("y0", "Starting Y Coordinate", typeof(Value.Number)));
            InPortData.Add(new PortData("z0", "Starting Z Coordinate", typeof(Value.Number)));
            InPortData.Add(new PortData("x-space", "The X spacing.", typeof(Value.Number)));
            InPortData.Add(new PortData("y-space", "The Y spacing.", typeof(Value.Number)));
            InPortData.Add(new PortData("z-space", "The Z spacing.", typeof(Value.Number)));
            OutPortData.Add(new PortData("XYZs", "List of XYZs in the grid", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double xi, yi, zi, x0, y0, z0, xs, ys, zs;

            xi = ((Value.Number)args[0]).Item;
            yi = ((Value.Number)args[1]).Item;
            zi = ((Value.Number)args[2]).Item;
            x0 = ((Value.Number)args[3]).Item;
            y0 = ((Value.Number)args[4]).Item;
            z0 = ((Value.Number)args[5]).Item;
            xs = ((Value.Number)args[6]).Item;
            ys = ((Value.Number)args[7]).Item;
            zs = ((Value.Number)args[8]).Item;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            double z = z0;
            for (int zCount = 0; zCount < zi; zCount++)
            {
                double y = y0;
                for (int yCount = 0; yCount < yi; yCount++)
                {
                    double x = x0;
                    for (int xCount = 0; xCount < xi; xCount++)
                    {
                        XYZ pt = new XYZ(x, y, z);
                        pts.Add(pt);
                        result = FSharpList<Value>.Cons(
                           Value.NewContainer(pt),
                           result
                        );
                        x += xs;
                    }
                    y += ys;
                }
                z += zs;
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("XYZ Array On Curve")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a list of XYZs along a curve.")]
    public class dynXYZArrayAlongCurve : dynXYZBase
    {
        public dynXYZArrayAlongCurve()
        {
            InPortData.Add(new PortData("curve", "Curve", typeof(Value.Container)));
            InPortData.Add(new PortData("count", "Number", typeof(Value.Number))); // just divide equally for now, dont worry about spacing and starting point
            OutPortData.Add(new PortData("XYZs", "List of XYZs in the array", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            double xi;//, x0, xs;
            xi = ((Value.Number)args[1]).Item;// Number
            xi = Math.Round(xi);
            if (xi < Double.Epsilon)
                throw new Exception("The point count must be larger than 0.");
            xi = xi - 1;

            //x0 = ((Value.Number)args[2]).Item;// Starting Coord
            //xs = ((Value.Number)args[3]).Item;// Spacing


            var result = FSharpList<Value>.Empty;

            Curve crvRef = null;

            if (((Value.Container)args[0]).Item is CurveElement)
            {
                var c = (CurveElement)((Value.Container)args[0]).Item; // Curve 
                crvRef = c.GeometryCurve;
            }
            else
            {
                crvRef = (Curve)((Value.Container)args[0]).Item; // Curve 
            }

            double t = 0;

            if (xi < Double.Epsilon)
            {
                var pt = !dynXYZOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                result = FSharpList<Value>.Cons(Value.NewContainer(pt), result);
                pts.Add(pt);
                return Value.NewList(
                  ListModule.Reverse(result)
               );
            }

            for (int xCount = 0; xCount <= xi; xCount++)
            {
                t = xCount / xi; // create normalized curve param by dividing current number by total number
                var pt = !dynXYZOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                result = FSharpList<Value>.Cons(Value.NewContainer( pt ), result );
                pts.Add(pt);
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("Plane")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Creates a geometric plane.")]
    public class dynPlane: dynGeometryBase
    {
        public dynPlane()
        {
            InPortData.Add(new PortData("normal", "Normal Point (XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("origin", "Origin Point (XYZ)", typeof(Value.Container)));
            OutPortData.Add(new PortData("P", "Plane", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ ptA = (XYZ)((Value.Container)args[0]).Item;
            XYZ ptB = (XYZ)((Value.Container)args[1]).Item;

            var plane = dynRevitSettings.Doc.Application.Application.Create.NewPlane(
               ptA, ptB
            );

            return Value.NewContainer(plane);
        }
    }

    [NodeName("Sketch Plane")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Creates a geometric sketch plane.")]
    public class dynSketchPlane : dynRevitTransactionNodeWithOneOutput
    {
        public dynSketchPlane()
        {
            InPortData.Add(new PortData("plane", "The plane in which to define the sketch.", typeof(Value.Container))); // SketchPlane can accept Plane, Reference or PlanarFace
            OutPortData.Add(new PortData("sp", "SketchPlane", typeof(Value.Container)));

            RegisterAllPorts();
        }

        bool resetPlaneofSketchPlaneElement(SketchPlane sp, Plane p)
        {
            XYZ newOrigin = p.Origin;
            XYZ newNorm = p.Normal;
            var oldP = sp.Plane;
            XYZ oldOrigin = oldP.Origin;
            XYZ oldNorm = oldP.Normal;
            
            Transform trfP = null;
            if (oldNorm.IsAlmostEqualTo(newNorm))
            {
                XYZ moveVec = newOrigin - oldOrigin;
                if (moveVec.GetLength() > 0.000000001)
                    ElementTransformUtils.MoveElement(this.UIDocument.Document, sp.Id, moveVec);
                return true;
            }
            //rotation might not work for sketch planes
            return false;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            //TODO: If possible, update to handle mutation rather than deletion...
            //foreach (var e in this.Elements)
            //    this.DeleteElement(e);

            if (input.IsList)
            {
                //TODO: If possible, update to handle mutation rather than deletion...
                //but: how to preserve elements when list size changes or user reshuffles elements in the list?
                foreach (var e in this.Elements)
                    this.DeleteElement(e);

                var planeList = (input as Value.List).Item;

                var result = Utils.SequenceToFSharpList(
                   planeList.Select(
                      delegate(Value x)
                      {
                          SketchPlane sp = null;

                          //handle Plane, Reference or PlanarFace, also test for family or project doc. there probably is a cleaner way to test for all these conditions.
                          if (((Value.Container)x).Item is Plane) //TODO: ensure this is correctly casting and testing.
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (Plane)((Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (Plane)((Value.Container)x).Item
                              );
                          }
                          else if (((Value.Container)x).Item is Reference)
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (Reference)((Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (Reference)((Value.Container)x).Item
                              );
                          }
                          else if (((Value.Container)x).Item is PlanarFace)
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (PlanarFace)((Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (PlanarFace)((Value.Container)x).Item
                              );
                          }


                          this.Elements.Add(sp.Id);
                          return Value.NewContainer(sp);
                      }
                   )
                );

                return Value.NewList(result);
            }
            else
            {
                SketchPlane sp = null;
                bool keepExistingElement = false;
                var x = ((Value.Container)input).Item;

                //TODO: If possible, update to handle mutation rather than deletion...
                if (this.Elements.Count == 1)
                {
                    Element e = this.UIDocument.Document.GetElement(this.Elements[0]);
                    if (e != null && ( e is SketchPlane))
                    {
                       sp = (SketchPlane) e;
                       
                       if (x is Reference)
                           keepExistingElement = true;
                       else if (x is Plane && resetPlaneofSketchPlaneElement(sp, (Plane) x))
                           keepExistingElement = true;
                    }
                }
                if (!keepExistingElement)
                {
                    foreach (var e in this.Elements)
                        this.DeleteElement(e);

                    //handle Plane, Reference or PlanarFace, also test for family or project doc. there probably is a cleaner way to test for all these conditions.
                    if (x is Plane)
                    {
                        Plane p = x as Plane;
                        sp  = (this.UIDocument.Document.IsFamilyDocument)
                           ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
                           : this.UIDocument.Document.Create.NewSketchPlane(p);
                    }
                    else if (x is Reference)
                    {
                        Reference r = x as Reference;
                        sp  = (this.UIDocument.Document.IsFamilyDocument)
                           ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(r)
                           : this.UIDocument.Document.Create.NewSketchPlane(r);
                    } else if (x is PlanarFace)
                    {
                        PlanarFace p = x as PlanarFace;
                        sp = (this.UIDocument.Document.IsFamilyDocument)
                           ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
                           : this.UIDocument.Document.Create.NewSketchPlane(p);
                    }

                    this.Elements.Add(sp.Id);
                }

                return Value.NewContainer(sp);
            }
        }
    }

    [NodeName("Line")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a geometric line.")]
    public class dynLineBound: dynCurveBase
    {
        public dynLineBound()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(Value.Container)));
            //InPortData.Add(new PortData("bound?", "Boolean: Is this line bounded?", typeof(bool)));

            OutPortData.Add(new PortData("line", "Line", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var ptB = ((Value.Container)args[1]).Item;

            Line line = null;

            if (ptA is XYZ)
            {

                line = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(
                  (XYZ)ptA, (XYZ)ptB
                  );


            }
            else if (ptA is ReferencePoint)
            {
                line = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(
                  (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position
               );

            }

            crvs.Add(line);

            return Value.NewContainer(line);
        }
    }

    [NodeName("Arc By Start Mid End")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a geometric arc given start, middle and end points in XYZ.")]
    public class dynArcStartMiddleEnd : dynCurveBase
    {
        public dynArcStartMiddleEnd()
        {
            InPortData.Add(new PortData("start", "Start XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("mid", "XYZ on Curve", typeof(Value.Container)));
            InPortData.Add(new PortData("end", "End XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("arc", "Arc", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            Arc a = null;

            var ptA = ((Value.Container)args[0]).Item;//start
            var ptB = ((Value.Container)args[1]).Item;//middle
            var ptC = ((Value.Container)args[2]).Item;//end

            if (ptA is XYZ)
            {

                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                   (XYZ)ptA, (XYZ)ptC, (XYZ)ptB //start, end, middle 
                );


            }else if (ptA is ReferencePoint)
            {
                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, (XYZ)((ReferencePoint)ptB).Position, (XYZ)((ReferencePoint)ptC).Position //start, end, middle 
                );

            }

            crvs.Add(a);

            return Value.NewContainer(a);
        }
    }

    [NodeName("Arc by Ctr Pt")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a geometric arc given a center point and two end parameters. Start and End Values may be between 0 and 2*PI in Radians")]
    public class dynArcCenter : dynCurveBase
    {
        public dynArcCenter()
        {
            InPortData.Add(new PortData("center", "Center XYZ or Coordinate System", typeof(Value.Container)));
            InPortData.Add(new PortData("radius", "Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("start", "Start Param", typeof(Value.Number)));
            InPortData.Add(new PortData("end", "End Param", typeof(Value.Number)));
            OutPortData.Add(new PortData("arc", "Arc", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radius = (double)((Value.Number)args[1]).Item;
            var start = (double)((Value.Number)args[2]).Item;
            var end = (double)((Value.Number)args[3]).Item;

            Arc a = null;

            if (ptA is XYZ)
            {
                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                   (XYZ)ptA, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is ReferencePoint)
            {
                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                   (XYZ)((ReferencePoint)ptA).Position, radius, start, end, XYZ.BasisX, XYZ.BasisY
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                a = dynRevitSettings.Doc.Application.Application.Create.NewArc(
                             center, radius, start, end, trf.BasisX, trf.BasisY
                );
            }

            crvs.Add(a);

            return Value.NewContainer(a);
        }
    }

    [NodeName("Transform Crv")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Returns the curve (c) transformed by the transform (t).")]
    public class dynCurveTransformed: dynCurveBase
    {
        public dynCurveTransformed()
        {
            InPortData.Add(new PortData("cv", "Curve(Curve)", typeof(Value.Container)));
            InPortData.Add(new PortData("t", "Transform(Transform)", typeof(Value.Container)));
            OutPortData.Add(new PortData("tcv", "Transformed Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            var curve = (Curve)((Value.Container)args[0]).Item;
            var trans = (Transform)((Value.Container)args[1]).Item;

            var crvTrans = curve.get_Transformed(trans);
            crvs.Add(crvTrans);
            return Value.NewContainer(crvTrans);
        }
    }

    [NodeName("Circle")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a geometric circle.")]
    public class dynCircle: dynCurveBase
    {
        public dynCircle()
        {
            InPortData.Add(new PortData("center", "Start XYZ", typeof(Value.Container)));
            InPortData.Add(new PortData("rad", "Radius", typeof(Value.Number)));
            OutPortData.Add(new PortData("circle", "Circle CurveLoop", typeof(Value.Container)));

            RegisterAllPorts();
        }

        const double RevitPI = 3.14159265358979;

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radius = (double)((Value.Number)args[1]).Item;

            Curve circle = null;

            if (ptA is XYZ)
            {
                //Curve circle = this.UIDocument.Application.Application.Create.NewArc(ptA, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                circle = dynRevitSettings.Doc.Application.Application.Create.NewArc((XYZ)ptA, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);

            }
            else if (ptA is ReferencePoint)
            {
                //Curve circle = this.UIDocument.Application.Application.Create.NewArc(ptA, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                circle = dynRevitSettings.Doc.Application.Application.Create.NewArc((XYZ)((ReferencePoint)ptA).Position, radius, 0, 2 * RevitPI, XYZ.BasisX, XYZ.BasisY);
            }

            crvs.Add(circle);

            return Value.NewContainer(circle);
        }
    }

    [NodeName("Ellipse")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a geometric ellipse.")]
    public class dynEllipse: dynCurveBase
    {
        public dynEllipse()
        {
            InPortData.Add(new PortData("center", "Center XYZ or Coordinate System", typeof(Value.Container)));
            InPortData.Add(new PortData("radX", "Major Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("radY", "Minor Radius", typeof(Value.Number)));
            OutPortData.Add(new PortData("ell", "Ellipse", typeof(Value.Container)));

            RegisterAllPorts();
        }

        const double RevitPI = 3.14159265358979;

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radX = (double)((Value.Number)args[1]).Item;
            var radY = (double)((Value.Number)args[2]).Item;

            Ellipse ell = null;

            if (ptA is XYZ)
            {
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                  (XYZ)ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * RevitPI
               );

            }
            else if (ptA is ReferencePoint)
            {
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
               (XYZ)((ReferencePoint)ptA).Position, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * RevitPI
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                     center, radX, radY, trf.BasisX, trf.BasisY, 0, 2 * RevitPI
                  );
            }

            crvs.Add(ell);

            return Value.NewContainer(ell);
        }
    }

    [NodeName("Ellipse Arc")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a geometric elliptical arc. Start and End Values may be between 0 and 2*PI in Radians")]
    public class dynEllipticalArc: dynCurveBase
    {
        public dynEllipticalArc()
        {
            InPortData.Add(new PortData("center", "Center XYZ or Coordinate System", typeof(Value.Container)));
            InPortData.Add(new PortData("radX", "Major Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("radY", "Minor Radius", typeof(Value.Number)));
            InPortData.Add(new PortData("start", "Start Param", typeof(Value.Number)));
            InPortData.Add(new PortData("end", "End Param", typeof(Value.Number)));
            OutPortData.Add(new PortData("ell", "Ellipse", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = ((Value.Container)args[0]).Item;
            var radX = (double)((Value.Number)args[1]).Item;
            var radY = (double)((Value.Number)args[2]).Item;
            var start = (double)((Value.Number)args[3]).Item;
            var end = (double)((Value.Number)args[4]).Item;

            Ellipse ell = null;

            if (ptA is XYZ)
            {
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                  (XYZ)ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, start, end
               );

            }
            else if (ptA is ReferencePoint)
            {
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
               (XYZ)((ReferencePoint)ptA).Position, radX, radY, XYZ.BasisX, XYZ.BasisY, start, end
                );
            }
            else if (ptA is Transform)
            {
                Transform trf = ptA as Transform;
                XYZ center = trf.Origin;
                ell = dynRevitSettings.Doc.Application.Application.Create.NewEllipse(
                    //ptA, radX, radY, XYZ.BasisX, XYZ.BasisY, 0, 2 * Math.PI
                     center, radX, radY, trf.BasisX, trf.BasisY, start, end
                  );
            }

            crvs.Add(ell);

            return Value.NewContainer(ell);
        }
    }

    [NodeName("UV")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_POINT)]
    [NodeDescription("Creates a UV from two double values.")]
    public class dynUV : dynGeometryBase
    {
        public dynUV()
        {
            InPortData.Add(new PortData("U", "U", typeof(Value.Number)));
            InPortData.Add(new PortData("V", "V", typeof(Value.Number)));
            OutPortData.Add(new PortData("uv", "UV", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double u, v;
            u = ((Value.Number)args[0]).Item;
            v = ((Value.Number)args[1]).Item;


            return FScheme.Value.NewContainer(new UV(u, v));
        }
    }

    [NodeName("Line From Vector")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a line in the direction of an XYZ normal.")]
    public class dynLineVectorfromXYZ: dynNodeWithOneOutput
    {
        public dynLineVectorfromXYZ()
        {
            InPortData.Add(new PortData("normal", "Normal Point (XYZ)", typeof(Value.Container)));
            InPortData.Add(new PortData("origin", "Origin Point (XYZ)", typeof(Value.Container)));
            OutPortData.Add(new PortData("C", "Curve", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var ptA = (XYZ)((Value.Container)args[0]).Item;
            var ptB = (XYZ)((Value.Container)args[1]).Item;

            // CurveElement c = MakeLine(this.UIDocument.Document, ptA, ptB);
            CurveElement c = MakeLineCBP(dynRevitSettings.Doc.Document, ptA, ptB);

            return FScheme.Value.NewContainer(c);
        }


        public ModelCurve MakeLine(Document doc, XYZ ptA, XYZ ptB)
        {
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            // Create plane by the points
            Line line = app.Create.NewLine(ptA, ptB, true);
            XYZ norm = ptA.CrossProduct(ptB);
            double length = norm.GetLength();
            if (length == 0) norm = XYZ.BasisZ;
            Plane plane = app.Create.NewPlane(norm, ptB);
            SketchPlane skplane = doc.FamilyCreate.NewSketchPlane(plane);
            // Create line here
            ModelCurve modelcurve = doc.FamilyCreate.NewModelCurve(line, skplane);
            return modelcurve;
        }

        public CurveByPoints MakeLineCBP(Document doc, XYZ ptA, XYZ ptB)
        {
            ReferencePoint sunRP = doc.FamilyCreate.NewReferencePoint(ptA);
            ReferencePoint originRP = doc.FamilyCreate.NewReferencePoint(ptB);
            ReferencePointArray sunRPArray = new ReferencePointArray();
            sunRPArray.Append(sunRP);
            sunRPArray.Append(originRP);
            CurveByPoints sunPath = doc.FamilyCreate.NewCurveByPoints(sunRPArray);
            return sunPath;
        }
    }

    [NodeName("Hermite Spline")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Creates a geometric hermite spline.")]
    public class dynHermiteSpline: dynCurveBase
    {
        HermiteSpline hs;

        public dynHermiteSpline()
        {
            InPortData.Add(new PortData("xyzs", "List of pts.(List XYZ)", typeof(Value.List)));
            OutPortData.Add(new PortData("spline", "Spline", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var pts = ((Value.List)args[0]).Item;

            hs = null;

            FSharpList<Value> containers = Utils.SequenceToFSharpList(pts);

            List<XYZ> ctrlPts = new List<XYZ>();
            foreach (Value e in containers)
            {
                if (e.IsContainer)
                {
                    XYZ pt = (XYZ)((Value.Container)(e)).Item;
                    ctrlPts.Add(pt);
                }
            }
            if (pts.Count() > 0)
            {
                hs = dynRevitSettings.Doc.Application.Application.Create.NewHermiteSpline(ctrlPts, false);
            }

            crvs.Add(hs);

            return Value.NewContainer(hs);
        }
    }

    [NodeName("Element Geometry Objects")]
    [NodeCategory(BuiltinNodeCategories.REVIT_BAKE)]
    [NodeDescription("Creates list of geometry object references in the element.")]
    public class dynElementGeometryObjects : dynNodeWithOneOutput
    {
        List<GeometryObject> instanceGeometryObjects;

        public dynElementGeometryObjects()
        {
            InPortData.Add(new PortData("element", "element to create geometrical references to", typeof(Value.List)));
            OutPortData.Add(new PortData("Geometry objects of the element", "List", typeof(Value.List)));

            RegisterAllPorts();

            instanceGeometryObjects = null;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Element thisElement = (Element) ((Value.Container)args[0]).Item;

            instanceGeometryObjects = new List<GeometryObject>();

            var result = FSharpList<Value>.Empty;

            GeometryObject geomObj = thisElement.get_Geometry(new Autodesk.Revit.DB.Options());
            GeometryElement geomElement = geomObj as GeometryElement;

            if ((thisElement is GenericForm) && (geomElement.Count() < 1))
            {
                GenericForm gF = (GenericForm)thisElement;
                if (!gF.Combinations.IsEmpty)
                {
                    Autodesk.Revit.DB.Options geoOptions = new Autodesk.Revit.DB.Options();
                    geoOptions.IncludeNonVisibleObjects = true;
                    geomObj = thisElement.get_Geometry(geoOptions);
                    geomElement = geomObj as GeometryElement;
                }
            }
 
            foreach (GeometryObject geob in geomElement)
            {
                GeometryInstance ginsta = geob as GeometryInstance;
                if (ginsta != null)
                {
                    GeometryElement instanceGeom = ginsta.GetInstanceGeometry();
                    instanceGeometryObjects.Add(instanceGeom);
                    foreach (GeometryObject geobInst in instanceGeom)
                    {
                        result = FSharpList<Value>.Cons(Value.NewContainer(geobInst), result);
                    }
                }
                else
                {
                    result = FSharpList<Value>.Cons(Value.NewContainer(geob), result);
                }
            }

            return Value.NewList(result);
        }
    }

    [NodeName("Extract Solid from Element")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Creates reference to the solid in the element's geometry objects.")]
    public class dynElementSolid : dynSolidBase
    {
        Dictionary <ElementId, List<GeometryObject> > instanceSolids;

        public dynElementSolid()
        {
            InPortData.Add(new PortData("element", "element to create geometrical references to", typeof(Value.List)));
            OutPortData.Add(new PortData("solid", "solid in the element's geometry objects", typeof(object)));

            RegisterAllPorts();

            instanceSolids = new Dictionary <ElementId, List<GeometryObject> >();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Element thisElement = (Element)((Value.Container)args[0]).Item;

            ElementId thisId = ElementId.InvalidElementId;

            if (thisElement != null)
            {
                thisId = thisElement.Id;
                instanceSolids[thisId] = new List<GeometryObject>();
            }

            Solid mySolid = null;

            //because of r2013 used GenericForm  which is superclass of FreeFromElement
            if ((thisElement is GenericForm) && (dynFreeForm.freeFormSolids != null &&
                  dynFreeForm.freeFormSolids.ContainsKey(thisElement.Id)))
            {
                mySolid = dynFreeForm.freeFormSolids[thisElement.Id];
            }
            else
            {
                bool bNotVisibleOption = false;
                if (thisElement is GenericForm)
                {
                    GenericForm gF = (GenericForm) thisElement;
                    if (!gF.Combinations.IsEmpty)
                       bNotVisibleOption = true;
                }
                int nTry = (bNotVisibleOption) ? 2 : 1;
                for (int iTry = 0; iTry < nTry && (mySolid == null); iTry++)
                {
                    Autodesk.Revit.DB.Options geoOptions = new Autodesk.Revit.DB.Options();
                    if (bNotVisibleOption && (iTry == 1))
                        geoOptions.IncludeNonVisibleObjects = true;

                    GeometryObject geomObj = thisElement.get_Geometry(geoOptions);
                    GeometryElement geomElement = geomObj as GeometryElement;

                    if (geomElement != null)
                    {
                        foreach (GeometryObject geob in geomElement)
                        {
                            GeometryInstance ginsta = geob as GeometryInstance;
                            if (ginsta != null && thisId != ElementId.InvalidElementId)
                            {
                                GeometryElement instanceGeom = ginsta.GetInstanceGeometry();

                                instanceSolids[thisId].Add(instanceGeom);

                                foreach (GeometryObject geobInst in instanceGeom)
                                {
                                    mySolid = geobInst as Solid;
                                    if (mySolid != null)
                                    {
                                        FaceArray faceArr = mySolid.Faces;
                                        var thisEnum = faceArr.GetEnumerator();
                                        bool hasFace = false;
                                        for (; thisEnum.MoveNext(); )
                                        {
                                            hasFace = true;
                                            break;
                                        }
                                        if (!hasFace)
                                            mySolid = null;
                                        else
                                            break;
                                    }
                                }
                                if (mySolid != null)
                                    break;
                            }
                            else
                            {
                                mySolid = geob as Solid;
                                if (mySolid != null)
                                {
                                    FaceArray faceArr = mySolid.Faces;
                                    var thisEnum = faceArr.GetEnumerator();
                                    bool hasFace = false;
                                    for (; thisEnum.MoveNext(); )
                                    {
                                        hasFace = true;
                                        break;
                                    }
                                    if (!hasFace)
                                        mySolid = null;
                                    else
                                        break;
                                }

                            }
                        }
                    }
                }
            }

            solids.Add(mySolid);

            return Value.NewContainer(mySolid);
        }
    }

    [NodeName("Create Extrusion Geometry")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Creates a solid by linearly extruding one or more closed coplanar curve loops.")]
    public class CreateExtrusionGeometry : dynSolidBase
    {
        public CreateExtrusionGeometry()
        {
            InPortData.Add(new PortData("profiles", "A list of curve loops to be extruded.", typeof(Value.List)));
            InPortData.Add(new PortData("direction", "The direction in which to extrude the profile.", typeof(Value.Container)));
            InPortData.Add(new PortData("distance", "The positive distance by which the loops are to be extruded.", typeof(Value.Number)));
            OutPortData.Add(new PortData("geometry", "The extrusion.", typeof(Value.Container)));
            
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ direction = (XYZ)((Value.Container)args[1]).Item;
            double distance = ((Value.Number)args[2]).Item;

            //incoming list will have two lists in it
            //each list will contain curves. convert the curves
            //into curve loops
            FSharpList<Value> profileList = ((Value.List)args[0]).Item;
            List<CurveLoop> loops = new List<CurveLoop>();
            foreach (var item in profileList)
            {
                if (item.IsList)
                {
                    var innerList = ((Value.List)item).Item;
                    foreach (var innerItem in innerList)
                    {
                        loops.Add((CurveLoop)((Value.Container)item).Item);
                    }
                }
                else
                {
                    //we'll assume a container
                    loops.Add((CurveLoop)((Value.Container)item).Item);
                }
            }

            var result = GeometryCreationUtilities.CreateExtrusionGeometry(loops, direction, distance);

            solids.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Create Blend Geometry")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Creates a solid by blending two closed curve loops lying in non-coincident planes.")]
    public class CreateBlendGeometry : dynSolidBase
    {
        public CreateBlendGeometry()
        {
            InPortData.Add(new PortData("first loop", "The first curve loop. The loop must be a closed planar loop.", typeof(Value.Container)));
            InPortData.Add(new PortData("second loop", "The second curve loop, which also must be a closed planar loop.", typeof(Value.Container)));
            OutPortData.Add(new PortData("geometry", "The blend geometry.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            CurveLoop firstLoop = (CurveLoop)((Value.Container)args[0]).Item;
            CurveLoop secondLoop = (CurveLoop)((Value.Container)args[1]).Item;

            List<VertexPair> vertPairs = null;

            if (dynRevitSettings.Revit.Application.VersionName.Contains("2013"))
            {
                vertPairs = new List<VertexPair>();

                int i = 0;
                int nCurves1 = firstLoop.Count();
                int nCurves2 = secondLoop.Count();
                for (; i < nCurves1 && i < nCurves2; i++)
                {
                    vertPairs.Add(new VertexPair(i, i));
                }
            }

            var result = GeometryCreationUtilities.CreateBlendGeometry(firstLoop, secondLoop, vertPairs);


            solids.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Rectangle")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Create a rectangle by specifying the center, width, height, and normal.  Outputs a CurveLoop object directed counter-clockwise from upper right.")]
    public class Rectangle : dynCurveBase
    {
        public Rectangle()
        {
            InPortData.Add(new PortData("transform", "The a transform for the rectangle.", typeof(Value.Container)));
            InPortData.Add(new PortData("width", "The width of the rectangle.", typeof(Value.Number)));
            InPortData.Add(new PortData("height", "The height of the rectangle.", typeof(Value.Number)));
            OutPortData.Add(new PortData("geometry", "The curve loop representing the rectangle.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var t = (Transform)((Value.Container)args[0]).Item;
            double width = ((Value.Number)args[1]).Item;
            double height = ((Value.Number)args[2]).Item;

            //ccw from upper right
            var p0 = new XYZ(width / 2, height/2, 0);
            var p3 = new XYZ(-width / 2, height / 2, 0);
            var p2 = new XYZ(-width / 2, -height / 2, 0);
            var p1 = new XYZ(width / 2, -height / 2, 0);

            p0 = t.OfPoint(p0);
            p1 = t.OfPoint(p1);
            p2 = t.OfPoint(p2);
            p3 = t.OfPoint(p3);

            var l1 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p0, p1);
            var l2 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p1, p2);
            var l3 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p2, p3);
            var l4 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p3, p0);

            var cl = new CurveLoop();
            cl.Append(l1);
            cl.Append(l2);
            cl.Append(l3);
            cl.Append(l4);

            crvs.Add(l1);
            crvs.Add(l2);
            crvs.Add(l3);
            crvs.Add(l4);

            return Value.NewContainer(cl);
        }
    }

    [NodeName("Faces of Solid Along Line")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Creates list of faces of the solid intersecting given line.")]
    public class dynFacesByLine : dynNodeWithOneOutput
    {
        public dynFacesByLine()
        {
            InPortData.Add(new PortData("solid", "solid to extract faces from", typeof(Value.Container)));
            InPortData.Add(new PortData("line", "line to extract faces from", typeof(Value.Container)));
            OutPortData.Add(new PortData("faces of solid along the line", "extracted list of faces", typeof(object)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid thisSolid = (Solid)((Value.Container)args[0]).Item;
            Line selectLine = (Line)((Value.Container)args[1]).Item;

            FaceArray faceArr = thisSolid.Faces;
            var thisEnum = faceArr.GetEnumerator();

            SortedList<double, Autodesk.Revit.DB.Face> intersectingFaces = new SortedList<double, Autodesk.Revit.DB.Face>();

            for (; thisEnum.MoveNext(); )
            {
                Autodesk.Revit.DB.Face thisFace = (Autodesk.Revit.DB.Face) thisEnum.Current;
                IntersectionResultArray resultArray = null;

                SetComparisonResult resultIntersect = thisFace.Intersect(selectLine, out resultArray);
                if (resultIntersect != SetComparisonResult.Overlap)
                    continue;
                bool first = true;
                double linePar = -1.0;
                foreach (IntersectionResult ir in resultArray)
                {
                    double irPar = ir.Parameter;
                    if (first == true)
                    {
                        linePar = irPar;
                        first = false;
                    }
                    else if (irPar < linePar)
                        linePar = irPar;
                }
                intersectingFaces.Add(linePar, thisFace);
            }

            var result = FSharpList<Value>.Empty;

            var intersectingFacesEnum = intersectingFaces.Reverse().GetEnumerator();
            for (; intersectingFacesEnum.MoveNext(); )
            {
                Autodesk.Revit.DB.Face faceObj = intersectingFacesEnum.Current.Value;
                result = FSharpList<Value>.Cons(Value.NewContainer(faceObj), result);      
            }

            return Value.NewList(result);
        }
    }

    [NodeName("Explode Geometry Object")]
    [NodeCategory(BuiltinNodeCategories.REVIT_BAKE)]
    [NodeDescription("Creates list of faces of solid or edges of face")]
    public class dynGeometryObjectsFromRoot : dynNodeWithOneOutput
    {

        public dynGeometryObjectsFromRoot()
        {
            InPortData.Add(new PortData("Explode Geometry Object", "Solid to extract faces or face to extract edges", typeof(Value.Container)));
            OutPortData.Add(new PortData("Exploded Geometry objects", "List", typeof(Value.List)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid thisSolid = null;
            if (((Value.Container)args[0]).Item is Solid)
                thisSolid = (Solid)((Value.Container)args[0]).Item;

            Autodesk.Revit.DB.Face thisFace = thisSolid == null ? (Autodesk.Revit.DB.Face)(((Value.Container)args[0]).Item) : null;

            var result = FSharpList<Value>.Empty;

            if (thisSolid != null)
            {
                FaceArray faceArr = thisSolid.Faces;
                var thisEnum = faceArr.GetEnumerator();
                for (; thisEnum.MoveNext(); )
                {
                    Autodesk.Revit.DB.Face curFace = (Autodesk.Revit.DB.Face) thisEnum.Current;
                    if (curFace != null)
                        result = FSharpList<Value>.Cons(Value.NewContainer(curFace), result);   
                 }
            }
            else if (thisFace != null)
            {
                EdgeArrayArray loops = thisFace.EdgeLoops;
                var loopsEnum = loops.GetEnumerator();
                for (; loopsEnum.MoveNext(); )
                {
                    EdgeArray thisArr = (EdgeArray) loopsEnum.Current;
                    if (thisArr == null)
                        continue;
                    var oneLoopEnum = thisArr.GetEnumerator();
                    for (; oneLoopEnum.MoveNext(); )
                    {
                        Edge curEdge = (Edge) oneLoopEnum.Current;
                        if (curEdge != null)
                            result = FSharpList<Value>.Cons(Value.NewContainer(curEdge), result);   
                    }
                }
            }
            
            return Value.NewList(result);
        }
    }

    [NodeName("Boolean Geometric Operation")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Creates solid by union, intersection or difference of two solids.")]
    public class dynBooleanOperation : dynSolidBase
    {
        ComboBox combo;
        int selectedItem = -1;

        public dynBooleanOperation()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for boolean geometrical operation", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for boolean geometrical operation", typeof(object)));
         
            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));
            selectedItem = 2;
            RegisterAllPorts();

        }
        public override void SetupCustomUIElements(Controls.dynNodeView nodeUI)
        {
            //add a drop down list to the window
            combo = new ComboBox();
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    this.RequiresRecalc = true;
            };
            if (selectedItem >= 0 && selectedItem <= 2)
            {
                PopulateComboBox();
                combo.SelectedIndex = selectedItem;
                selectedItem = -1;
            }
            if (combo.SelectedIndex < 0 || combo.SelectedIndex > 2)
                combo.SelectedIndex = 2;
        }
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateComboBox();
        }

        public enum BooleanOperationOptions {Union, Intersect, Difference};

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("index", this.combo.SelectedIndex.ToString());
        }

        public override void LoadNode(XmlNode elNode)
        {
            try
            {
                selectedItem = Convert.ToInt32(elNode.Attributes["index"].Value);
                if (combo != null)
                    combo.SelectedIndex = selectedItem;
            }
            catch { }
        }

        private void PopulateComboBox()
        {

            combo.Items.Clear();
            ComboBoxItem cbiUnion = new ComboBoxItem();
            cbiUnion.Content = "Union";
            combo.Items.Add(cbiUnion);

            ComboBoxItem cbiIntersect = new ComboBoxItem();
            cbiIntersect.Content = "Intersect";
            combo.Items.Add(cbiIntersect);

            ComboBoxItem cbiDifference = new ComboBoxItem();
            cbiDifference.Content = "Difference";
            combo.Items.Add(cbiDifference);
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid firstSolid = (Solid)((Value.Container)args[0]).Item;
            Solid secondSolid = (Solid)((Value.Container)args[1]).Item;

            int n = combo.SelectedIndex;


            BooleanOperationsType opType = (n == 0) ? BooleanOperationsType.Union :
                ((n == 2)  ? BooleanOperationsType.Difference : BooleanOperationsType.Intersect);

            Solid result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, opType);

            solids.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Face Through Points")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Creates face on grid of points")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynFaceThroughPoints : dynNodeWithOneOutput
    {

        public dynFaceThroughPoints()
        {
            InPortData.Add(new PortData("Points", "Points to create face, list or list of lists", typeof(Value.List)));
            InPortData.Add(new PortData("NumberOfRows", "Number of rows in the grid of the face", typeof(object)));
            OutPortData.Add(new PortData("face", "Face", typeof(object)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var listIn = ((Value.List)args[0]).Item.Select(
                    x => ((XYZ)((Value.Container)x).Item)
                       ).ToList();
            /* consider passing n x m grid of points instead of flat list
            var in1 = ((Value.Container)args[0]).Item;
            List<XYZ> listIn = in1 as List<XYZ>;
            List<List<XYZ>> listOfListsIn = (listIn != null) ? null : (in1 as List<List<XYZ>>);

            if (listIn == null && listOfListsIn == null)
                throw new Exception("no XYZ list or list of XYZ lists in Face Through Points node");

            if (listOfListsIn != null)
            {
                listIn = new List<XYZ>();
                for (int indexL = 0; indexL < listOfListsIn.Count; indexL++)
                {
                    listIn.Concat(listOfListsIn[indexL]);
                }
            }
            */

            int numberOfRows = (int)((Value.Number)args[1]).Item;
            if (numberOfRows < 2 || listIn.Count % numberOfRows != 0)
                throw new Exception("number of rows should  match number of points Face Through Points node");

            bool periodicU = false;
            bool periodicV = false;

            Type HermiteFaceType = typeof(Autodesk.Revit.DB.HermiteFace);

            MethodInfo[] hermiteFaceStaticMethods = HermiteFaceType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            String nameOfMethodCreate = "Create";
            Autodesk.Revit.DB.Face result = null;

            foreach (MethodInfo m in hermiteFaceStaticMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[7];
                    argsM[0] = numberOfRows;
                    argsM[1] = listIn;
                    argsM[2] = new List<XYZ>();
                    argsM[3] = new List<XYZ>();
                    argsM[4] = new List<XYZ>();
                    argsM[5] = periodicU;
                    argsM[6] = periodicV;

                    result = (Autodesk.Revit.DB.Face) m.Invoke(null, argsM);

                    break;
                }
            }

            return Value.NewContainer(result);
        }
    }
    
    [NodeName("Transform Solid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Creates solid by transforming solid")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynTransformSolid : dynSolidBase
    {
        public dynTransformSolid()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(Value.Container)));
            InPortData.Add(new PortData("Transform", "Transform to apply", typeof(Value.Container)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(Value.Container)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid thisSolid = (Solid)((Value.Container)args[0]).Item;
            Transform transform = (Transform)((Value.Container)args[1]).Item;

            Solid result = null;

            Type GeometryCreationUtilitiesType = typeof(Autodesk.Revit.DB.GeometryCreationUtilities);

            MethodInfo[] geometryCreationUtilitiesStaticMethods = GeometryCreationUtilitiesType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            String nameOfReplaceMethod = "CreateGeometryByFaceReplacement";

            foreach (MethodInfo ms in geometryCreationUtilitiesStaticMethods)
            {
                if (ms.Name == nameOfReplaceMethod)
                {
                    object[] argsM = new object[3];
                    argsM[0] = thisSolid;
                    argsM[1] = new List<GeometryObject>();
                    argsM[2] = new List<GeometryObject>();
                    result = (Solid)ms.Invoke(null, argsM);
                    break;
                }
            }
            if (result == null)
                throw new Exception(" could not copy solid or validation during copy failed");

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);
            MethodInfo[] solidInstanceMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
             
            String nameOfMethodTransform = "transform";

            foreach (MethodInfo m in solidInstanceMethods)
            {
                if (m.Name == nameOfMethodTransform)
                {
                    object[] argsM = new object[1];
                    argsM[0] = transform;

                    m.Invoke(result, argsM);

                    break;
                }
            }
            solids.Add(result);

            return Value.NewContainer(result);
        }
    }
    
    [NodeName("Replace Faces")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Build solid replacing faces of input solid by supplied faces")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynReplaceFacesOfSolid : dynSolidBase
    {
        public dynReplaceFacesOfSolid()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(Value.Container)));
            InPortData.Add(new PortData("Faces", "Faces to be replaced", typeof(Value.List)));
            InPortData.Add(new PortData("Faces", "Faces to use", typeof(Value.List)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(Value.Container)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid thisSolid = (Solid)((Value.Container)args[0]).Item;

            var facesToBeReplaced = ((Value.List)args[1]).Item.Select(
               x => ((GeometryObject)((Value.Container)x).Item)).ToList();

            var facesToReplaceWith = ((Value.List)args[2]).Item.Select(
               x => ((GeometryObject)((Value.Container)x).Item)).ToList();

            Type GeometryCreationUtilitiesType = typeof(Autodesk.Revit.DB.GeometryCreationUtilities);

            MethodInfo[] geometryCreationUtilitiesStaticMethods = GeometryCreationUtilitiesType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            String nameOfReplaceMethod = "CreateGeometryByFaceReplacement";

            Solid result = null;

            foreach (MethodInfo ms in geometryCreationUtilitiesStaticMethods)
            {
                if (ms.Name == nameOfReplaceMethod)
                {
                    object[] argsM = new object[3];
                    argsM[0] = thisSolid;
                    argsM[1] = facesToBeReplaced;
                    argsM[2] = facesToReplaceWith;
                    result = (Solid)ms.Invoke(null, argsM);
                }
            }
            if (result == null)
                throw new Exception(" could not make solid by replacement of face or faces");
            
            solids.Add(result);

            return Value.NewContainer(result);
        }
    }
    
    [NodeName("Blend Edges")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Build solid by replace edges with round blends")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynBlendEdges : dynSolidBase
    {
        public dynBlendEdges()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(Value.Container)));
            InPortData.Add(new PortData("Edges", "Edges to be blends", typeof(Value.List)));
            InPortData.Add(new PortData("Radius", "Radius of blend", typeof( Value.Number)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(Value.Container)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid thisSolid = (Solid)((Value.Container)args[0]).Item;

            FSharpList<Value> vals = ((Value.List)args[1]).Item;
            List <GeometryObject> edgesToBeReplaced = new List <GeometryObject>();

            var doc = dynRevitSettings.Doc;

            for (int ii = 0; ii < vals.Count(); ii++)
            {
                 var item = ((Value.Container)vals[ii]).Item;

                 if (item is Reference)
                 {
                     Reference refEdge = (Reference)item;
                     Element selectedElement = doc.Document.GetElement(refEdge);

                     GeometryObject edge = selectedElement.GetGeometryObjectFromReference(refEdge);
                     if (edge is Edge)
                         edgesToBeReplaced.Add(edge);
                 }
                 else if (item is Edge)
                 {
                     GeometryObject edge = (Edge)item;
                     edgesToBeReplaced.Add(edge);
                 }
            }

            double radius = ((Value.Number)args[2]).Item;

            System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GeometryCreationUtilities));
            Type SolidModificationUtilsType = revitAPIAssembly.GetType("Autodesk.Revit.DB.SolidModificationUtils", false);

            if (SolidModificationUtilsType == null)
                throw new Exception(" could not make edge chamfer");

            MethodInfo[] solidModificationUtilsStaticMethods = SolidModificationUtilsType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            String nameOfReplaceMethod = "ExecuteShapingOfEdges";

            Solid result = null;

            foreach (MethodInfo ms in solidModificationUtilsStaticMethods)
            {
                if (ms.Name == nameOfReplaceMethod)
                {
                    object[] argsM = new object[4];
                    bool isRound = true;
                    argsM[0] = thisSolid;
                    argsM[1] = isRound;
                    argsM[2] = radius;
                    argsM[3] = edgesToBeReplaced;
                    result = (Solid)ms.Invoke(null, argsM);
                    break;
                }
            }
            if (result == null)
                throw new Exception(" could not make solid by blending requested edges with given radius");
            
            solids.Add(result);

            return Value.NewContainer(result);
        }
    }
    
    [NodeName("Chamfer Edges")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Build solid by replace edges with chamfers")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynChamferEdges : dynSolidBase
    {
        public dynChamferEdges()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(Value.Container)));
            InPortData.Add(new PortData("Edges", "Edges to be blends", typeof(Value.List)));
            InPortData.Add(new PortData("Size", "Size of chamfer ", typeof(Value.Number)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(Value.Container)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid thisSolid = (Solid)((Value.Container)args[0]).Item;

            FSharpList<Value> vals = ((Value.List)args[1]).Item;
            List<GeometryObject> edgesToBeReplaced = new List<GeometryObject>();
            var doc = dynRevitSettings.Doc;

            for (int ii = 0; ii < vals.Count(); ii++)
            {
                var item = ((Value.Container)vals[ii]).Item;

                if (item is Reference)
                {
                    Reference refEdge = (Reference)item;
                    Element selectedElement = doc.Document.GetElement(refEdge);

                    GeometryObject edge = selectedElement.GetGeometryObjectFromReference(refEdge);
                    if (edge is Edge)
                        edgesToBeReplaced.Add(edge);
                }
                else if (item is Edge)
                {
                    GeometryObject edge = (Edge)item;
                    edgesToBeReplaced.Add(edge);
                }
            }

            double size = ((Value.Number)args[2]).Item;

            System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(typeof(GeometryCreationUtilities));
            Type SolidModificationUtilsType = revitAPIAssembly.GetType("Autodesk.Revit.DB.SolidModificationUtils", false);

            if (SolidModificationUtilsType == null)
                throw new Exception(" could not make edge chamfer");


            MethodInfo[] solidModificationUtilsStaticMethods = SolidModificationUtilsType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            String nameOfReplaceMethod = "ExecuteShapingOfEdges";

            Solid result = null;

            foreach (MethodInfo ms in solidModificationUtilsStaticMethods)
            {
                if (ms.Name == nameOfReplaceMethod)
                {
                    object[] argsM = new object[4];
                    bool isRound = false;
                    argsM[0] = thisSolid;
                    argsM[1] = isRound;
                    argsM[2] = size;
                    argsM[3] = edgesToBeReplaced;
                    result = (Solid)ms.Invoke(null, argsM);
                    break;
                }
            }
            if (result == null)
                throw new Exception(" could not make solid by chamfering requested edges with given chamfer size");
            
            solids.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Create Revolved Geometry")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Creates a solid by revolving  closed curve loops lying in xy plane of Transform.")]
    public class CreateRevolvedGeometry : dynSolidBase
    {
        public CreateRevolvedGeometry()
        {
            InPortData.Add(new PortData("curve loop", "The curve loop to revolve must be a closed planar loop.", typeof(Value.Container)));
            InPortData.Add(new PortData("transform", "Coordinate system for revolve, loop should be in xy plane of this transform on the right side of z axis used for rotate.", typeof(Value.Container)));
            InPortData.Add(new PortData("start angle", "start angle measured counter-clockwise from x-axis of transform", typeof(Value.Number)));
            InPortData.Add(new PortData("end angle", "end angle measured counter-clockwise from x-axis of transform", typeof(Value.Number)));
            OutPortData.Add(new PortData("geometry", "The revolved geometry.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            CurveLoop cLoop = (CurveLoop)((Value.Container)args[0]).Item;
            Transform trf = (Transform)((Value.Container)args[1]).Item;
            double sAngle =  ((Value.Number)args[2]).Item;
            double eAngle =  ((Value.Number)args[3]).Item;

            List<CurveLoop> loopList = new List<CurveLoop>();
            loopList.Add(cLoop);

            Autodesk.Revit.DB.Frame thisFrame = new Autodesk.Revit.DB.Frame();
            thisFrame.Transform(trf);

            Solid result = GeometryCreationUtilities.CreateRevolvedGeometry(thisFrame, loopList, sAngle, eAngle);

            solids.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Create Swept Geometry")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Creates a solid by sweeping curve loop along the path")]
    public class CreateSweptGeometry : dynSolidBase
    {
        public CreateSweptGeometry()
        {
            InPortData.Add(new PortData("sweep path", "The curve loop to sweep along.", typeof(Value.Container)));
            InPortData.Add(new PortData("attachment curve index", "index of the curve where profile loop is attached", typeof(Value.Number)));
            InPortData.Add(new PortData("attachment parameter", "end angle measured counter-clockwise from x-axis of transform", typeof(Value.Number)));
            InPortData.Add(new PortData("profile loop", "The curve loop to sweep lie in orthogonal plane to path at attachement point.", typeof(Value.Container)));
            OutPortData.Add(new PortData("geometry", "The swept geometry.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            CurveLoop pathLoop = (CurveLoop)((Value.Container)args[0]).Item;
            int attachementIndex = (int)((Value.Number)args[1]).Item;
            double attachementPar = ((Value.Number)args[2]).Item;
            CurveLoop profileLoop = (CurveLoop)((Value.Container)args[3]).Item;
            List<CurveLoop> loopList = new List<CurveLoop>();
            loopList.Add(profileLoop);

            Solid result = GeometryCreationUtilities.CreateSweptGeometry(pathLoop, attachementIndex, attachementPar, loopList);

            solids.Add(result);

            return Value.NewContainer(result);
        }
    }

    [NodeName("List Onesided Edges")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("List onesided edges of solid as CurveLoops")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynOnesidedEdgesAsCurveLoops : dynNodeWithOneOutput
    {

        public dynOnesidedEdgesAsCurveLoops()
        {
            InPortData.Add(new PortData("Incomplete Solid", "Geoemtry to check for being Solid", typeof(object)));
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(Value.List)));
            OutPortData.Add(new PortData("Onesided boundaries", "Onesided Edges as CurveLoops", typeof(Value.List)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid thisSolid = (Solid)((Value.Container)args[0]).Item;
            var listIn = ((Value.List)args[1]).Item.Select(
                    x => ((CurveLoop)((Value.Container)x).Item)
                       ).ToList();

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            String nameOfMethodCreate = "oneSidedEdgesAsCurveLoops";
            List <CurveLoop> oneSidedAsLoops = null;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[1];
                    argsM[0] = listIn;

                    oneSidedAsLoops = (List<CurveLoop>)m.Invoke(thisSolid, argsM);

                    break;
                }
            }

            var result = FSharpList<Value>.Empty;
            var thisEnum = oneSidedAsLoops.GetEnumerator();
        
            for (; thisEnum.MoveNext(); )
            {
                result = FSharpList<Value>.Cons(Value.NewContainer((CurveLoop) thisEnum.Current), result);
            }


            return Value.NewList(result);
        }
    }

    [NodeName("Patch Solid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Patch set of faces as Solid ")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynPatchSolid : dynSolidBase
    {

        public dynPatchSolid()
        {
            InPortData.Add(new PortData("Incomplete Solid", "Geoemtry to check for being Solid", typeof(object)));
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(Value.List)));
            InPortData.Add(new PortData("Faces", "Faces to exclude", typeof(Value.List)));
            OutPortData.Add(new PortData("Result", "Computed Solid", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Solid thisSolid = (Solid)((Value.Container)args[0]).Item;
            var listInCurveLoops = ((Value.List)args[1]).Item.Select(
                    x => ((CurveLoop)((Value.Container)x).Item)
                       ).ToList();
            var listInFacesToExclude = ((Value.List)args[2]).Item.Select(
                    x => ((Autodesk.Revit.DB.Face)((Value.Container)x).Item)
                       ).ToList();

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            String nameOfMethodCreate = "patchSolid";
            Solid resultSolid = null;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[2];
                    argsM[0] = listInCurveLoops;
                    argsM[1] = listInFacesToExclude;

                    resultSolid = (Solid)m.Invoke(thisSolid, argsM);

                    break;
                }
            }
            if (resultSolid == null)
                throw new Exception("Could not make patched solid, list Onesided Edges to investigate");
            
            solids.Add(resultSolid);

            return Value.NewContainer(resultSolid);
        }
    }

    [NodeName("Solid On Curve Loops")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Skin Solid by patch faces on set of curve loops.")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class dynSkinCurveLoops : dynSolidBase
    {

        public dynSkinCurveLoops()
        {
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(Value.List)));
            OutPortData.Add(new PortData("Result", "Computed Solid", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var listInCurveLoops = ((Value.List)args[0]).Item.Select(
                    x => ((CurveLoop)((Value.Container)x).Item)
                       ).ToList();

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            String nameOfMethodCreate = "skinCurveLoopsIntoSolid";
            Solid resultSolid = null;
            bool methodFound = false;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[1];
                    argsM[0] = listInCurveLoops;

                    resultSolid = (Solid)m.Invoke(null, argsM);
                    methodFound = true;

                    break;
                }
            }

            if (!methodFound)
                throw new Exception("This method uses later version of RevitAPI.dll with skinCurveLoopsIntoSolid method. Please use Patch Solid node instead.");
            if (resultSolid == null)
                throw new Exception("Failed to make solid, please check the input.");

            solids.Add(resultSolid);

            return Value.NewContainer(resultSolid);
        }
    }

    [NodeName("Lines Through XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_CURVE)]
    [NodeDescription("Create a series of linear curves through a set of points.")]
    [NodeSearchTags("lines", "line", "through", "passing", "thread", "xyz")]
    public class dynCurvesThroughPoints : dynCurveBase
    {
        public dynCurvesThroughPoints()
        {
            InPortData.Add(new PortData("xyzs", "List of points (xyz) through which to create lines.", typeof(Value.List)));
            OutPortData.Add(new PortData("lines", "Lines created through points.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //Build a sequence that unwraps the input list from it's Value form.
            IEnumerable<XYZ> pts = ((Value.List)args[0]).Item.Select(
               x => (XYZ)((Value.Container)x).Item
            );

            var results = FSharpList<Value>.Empty;

            var enumerable = pts as XYZ[] ?? pts.ToArray();
            for (int i = 1; i < enumerable.Count(); i++)
            {
                Line l = dynRevitSettings.Revit.Application.Create.NewLineBound(enumerable.ElementAt(i), enumerable.ElementAt(i-1));
                crvs.Add(l);
                results = FSharpList<Value>.Cons(Value.NewContainer(l), results);
            }

            return Value.NewList(results);
        }
    }
}

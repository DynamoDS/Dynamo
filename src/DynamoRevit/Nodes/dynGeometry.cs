using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Controls; //for boolean option
using System.Xml;              //for boolean option  
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    public abstract class GeometryBase : NodeWithOneOutput
    {
        protected GeometryBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    #region Vectors

    [NodeName("XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates an XYZ from three coordinates.")]
    [NodeSearchTags("vector", "point")]
    public class Xyz: GeometryBase
    {
        public Xyz()
        {
            InPortData.Add(new PortData("X", "X", typeof(Value.Number), Value.NewNumber(0)));
            InPortData.Add(new PortData("Y", "Y", typeof(Value.Number), Value.NewNumber(0)));
            InPortData.Add(new PortData("Z", "Z", typeof(Value.Number), Value.NewNumber(0)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double x, y, z;
            x = ((Value.Number)args[0]).Item;
            y = ((Value.Number)args[1]).Item;
            z = ((Value.Number)args[2]).Item;

            var pt = new XYZ(x, y, z);

            return Value.NewContainer(pt);
        }
    }

    [NodeName("XYZ by Polar")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates an XYZ from sphereical coordinates.")]
    public class XyzFromPolar : GeometryBase
    {
        public XyzFromPolar()
        {
            InPortData.Add(new PortData("radius", "Radius from origin in radians", typeof(Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("xy rotation", "Rotation around Z axis in radians", typeof(Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("offset", "Offset from xy plane", typeof(Value.Number), FScheme.Value.NewNumber(0)));

            OutPortData.Add(new PortData("xyz", "XYZ formed from polar coordinates", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public static XYZ FromPolarCoordinates(double r, double theta, double offset)
        {
            // if degenerate, return 0
            if (Math.Abs(r) < System.Double.Epsilon)
            {
                return new XYZ(0,0,offset);
            }

            // do some trig
            var x = r * Math.Cos(theta);
            var y = r * Math.Sin(theta);
            var z = offset;

            // all done
            return new XYZ(x, y, z);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var r = ((Value.Number)args[0]).Item;
            var theta = ((Value.Number)args[1]).Item;
            var phi = ((Value.Number)args[2]).Item;

            return Value.NewContainer(FromPolarCoordinates(r, theta, phi));
        }
    }

    [NodeName("XYZ to Polar")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates an XYZ from spherical coordinates.")]
    public class XyzToPolar : NodeModel
    {
        private readonly PortData _rPort = new PortData("radius", "Radius from origin in radians", typeof(Value.Number));
        private readonly PortData _thetaPort = new PortData("xy rotation", "Rotation around Z axis in radians", typeof(Value.Number));
        private readonly PortData _offsetPort = new PortData("offset", "Offset from the XY plane", typeof(Value.Number));

        public XyzToPolar()
        {
            InPortData.Add(new PortData("xyz", "Input XYZ", typeof(Value.Container), Value.NewContainer(new XYZ(1,0,0))));
            
            OutPortData.Add(_rPort);
            OutPortData.Add(_thetaPort);
            OutPortData.Add(_offsetPort);

            this.ArgumentLacing = LacingStrategy.Longest;

            RegisterAllPorts();
        }

        public static void ToPolarCoordinates(XYZ input, out double r, out double theta, out double offset)
        {
            // this is easy
            offset = input.Z;

            // set length
            r = (new XYZ(input.X, input.Y, 0)).GetLength();

            // if the length is too small the angles will be degenerate, just set them as 0
            if (Math.Abs(input.X) < System.Double.Epsilon)
            {
                theta = 0;
                return;
            }

            theta = Math.Atan(input.Y / input.X);
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var xyz = ((XYZ)((Value.Container)args[0]).Item);
            double r, theta, phi;

            ToPolarCoordinates(xyz, out r, out theta, out phi);

            outPuts[_rPort] = FScheme.Value.NewNumber(r);
            outPuts[_thetaPort] = FScheme.Value.NewNumber(theta);
            outPuts[_offsetPort] = FScheme.Value.NewNumber(phi);
        }
    }

    [NodeName("XYZ by Spherical")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates an XYZ from spherical coordinates.")]
    public class XyzFromSpherical : GeometryBase
    {
        public XyzFromSpherical()
        {
            InPortData.Add(new PortData("radius", "Radius from origin in radians", typeof(Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("xy rotation", "Rotation around Z axis in radians", typeof(Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("z rotation", "Rotation down form axis in radians", typeof(Value.Number), FScheme.Value.NewNumber(0)));

            OutPortData.Add(new PortData("xyz", "XYZ formed from polar coordinates", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public static XYZ FromPolarCoordinates(double r, double theta, double phi)
        {
            // if degenerate, return 0
            if (Math.Abs(r) < System.Double.Epsilon)
            {
                return new XYZ();
            }

            // do some trig
            var x = r * Math.Cos(theta);
            var y = r * Math.Sin(theta);
            var z = r * Math.Cos(phi);

            // all done
            return new XYZ(x, y, z);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var r = ((Value.Number)args[0]).Item;
            var theta = ((Value.Number)args[1]).Item;
            var phi = ((Value.Number)args[2]).Item;
    
            return Value.NewContainer(FromPolarCoordinates(r, theta, phi));
        }
    }

    [NodeName("XYZ to Spherical")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Decompose an XYZ into spherical coordinates.")]
    public class XyzToSpherical : NodeModel
    {
        private readonly PortData _rPort = new PortData("radius", "Radius from origin in radians", typeof(Value.Number));
        private readonly PortData _thetaPort = new PortData("xy rotation", "Rotation around Z axis in radians", typeof(Value.Number));
        private readonly PortData _phiPort = new PortData("z rotation", "Rotation from axis in radians (north pole is 0, south pole is PI)", typeof(Value.Number));

        public XyzToSpherical()
        {
            InPortData.Add(new PortData("xyz", "XYZ to decompose", typeof(Value.Container)));

            OutPortData.Add(_rPort);
            OutPortData.Add(_thetaPort);
            OutPortData.Add(_phiPort);

            this.ArgumentLacing = LacingStrategy.Longest;

            RegisterAllPorts();
        }

        public static void ToPolarCoordinates(XYZ input, out double r, out double theta, out double phi)
        {
            // set length
            r = input.GetLength();

            // if the length is too small the angles will be degenerate, just set them as 0
            if (Math.Abs(r) < System.Double.Epsilon)
            {
                theta = 0;
                phi = 0;
                return;
            }

            // get the length of the projection on the xy plane
            var rInXYPlane = (new XYZ(input.X, input.Y, 0)).GetLength();

            // if projected length is 0, xyz is pointing up, down, or is origin
            if (Math.Abs(rInXYPlane) < System.Double.Epsilon)
            {
                // this should have already been detected when r is 0, but check anyway
                if (Math.Abs(input.Z) < System.Double.Epsilon)
                {
                    theta = 0;
                    phi = 0;
                    return;
                }
                else // determine whether vector is above or below - if above phi is 0
                {
                    theta = 0;
                    phi = input.Y > 0 ? 0 : Math.PI;
                    return;
                }
            }
            
            // if x is 0, this indicates vector is at 90 or 270
            if (Math.Abs(input.X) < System.Double.Epsilon)
            {
                theta = input.Y > 0 ? Math.PI/2 : 3*Math.PI/2;
            }
            else
            {
                theta = Math.Atan(input.Y / input.X);
            }
            
            // phew...
            phi = Math.Atan(input.Z / rInXYPlane);
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var xyz = ((XYZ)((Value.Container)args[0]).Item);
            double r, theta, phi;

            ToPolarCoordinates(xyz, out r, out theta, out phi);

            outPuts[_rPort] = FScheme.Value.NewNumber(r);
            outPuts[_thetaPort] = FScheme.Value.NewNumber(theta);
            outPuts[_phiPort] = FScheme.Value.NewNumber(phi);
        }
    }

    [NodeName("XYZ from List of Numbers")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates a list of XYZs by taking sets of 3 numbers from an list.")]
    public class XyzFromListOfNumbers : GeometryBase
    {
        public XyzFromListOfNumbers()
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

    [NodeName("XYZ from Reference Point")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Extracts an XYZ from a Reference Point.")]
    [NodeSearchTags("xyz", "derive", "from", "reference", "point")]
    public class XyzFromReferencePoint : GeometryBase
    {
        public XyzFromReferencePoint()
        {
            InPortData.Add(new PortData("pt", "Reference Point", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "Location of the reference point.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            ReferencePoint point;
            point = (ReferencePoint)((Value.Container)args[0]).Item;

            return Value.NewContainer(point.Position);
        }
    }

    [NodeName("XYZ Components")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Get the components of an XYZ")]
    public class XyzComponents : NodeModel
    {

        private readonly PortData _xPort = new PortData("x", "X value of given XYZ", typeof(Value.Number));
        private readonly PortData _yPort = new PortData("y", "Y value of given XYZ", typeof (Value.Number));
        private readonly PortData _zPort = new PortData("z", "Z value of given XYZ", typeof(Value.Number));

        public XyzComponents()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(Value.Container)));
            OutPortData.Add(_xPort);
            OutPortData.Add(_yPort);
            OutPortData.Add(_zPort);
            ArgumentLacing = LacingStrategy.Longest;

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var xyz = ((XYZ)((Value.Container)args[0]).Item);
            var x = xyz.X;
            var y = xyz.Y;
            var z = xyz.Z;

            outPuts[_xPort] = FScheme.Value.NewNumber(x);
            outPuts[_yPort] = FScheme.Value.NewNumber(y);
            outPuts[_zPort] = FScheme.Value.NewNumber(z);
        }
    }

    [NodeName("XYZ X")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Fetches the X value of the given XYZ")]
    public class XyzGetX: GeometryBase
    { 
        public XyzGetX()
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

    [NodeName("XYZ Y")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Fetches the Y value of the given XYZ")]
    public class XyzGetY : GeometryBase
    {
        public XyzGetY()
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

    [NodeName("XYZ Z")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Fetches the Z value of the given XYZ")]
    public class XyzGetZ : GeometryBase
    {
        public XyzGetZ()
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

    [NodeName("XYZ Distance")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Returns the distance between a(XYZ) and b(XYZ).")]
    public class XyzDistance : MeasurementBase
    {
        public XyzDistance()
        {
            InPortData.Add(new PortData("a", "Start (XYZ).", typeof(Value.Container)));//Ref to a face of a form
            InPortData.Add(new PortData("b", "End (XYZ)", typeof(Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("d", "The distance between the two XYZs (Number).", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var a = (XYZ)((Value.Container)args[0]).Item;
            var b = (XYZ)((Value.Container)args[1]).Item;

            return Value.NewNumber(a.DistanceTo(b));
        }
    }

    [NodeName("XYZ Length")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Gets the length of an XYZ")]
    [NodeSearchTags("vector", "magnitude", "amplitude")]
    public class XyzLength : GeometryBase
    {
        public XyzLength()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("X", "X value of given XYZ", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((XYZ)((Value.Container)args[0]).Item).GetLength());
        }
    }

    [NodeName("Unitize XYZ")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Scale the given XYZ so its length is 1.")]
    [NodeSearchTags("normalize", "length", "vector")]
    public class XyzNormalize : GeometryBase
    {
        public XyzNormalize()
        {
            InPortData.Add(new PortData("xyz", "An XYZ to normalize", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "The normalized XYZ", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(((XYZ)((Value.Container)args[0]).Item).Normalize());
        }
    }

    [NodeName("XYZ Is Zero Length")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_MEASURE)]
    [NodeDescription("Determines whether an XYZ has zero length")]
    [NodeSearchTags("vector", "length", "xyz", "magnitude", "amplitude")]
    public class XyzIsZeroLength : GeometryBase
    {
        public XyzIsZeroLength()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(Value.Container)));
            OutPortData.Add(new PortData("X", "X value of given XYZ", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber( ((XYZ) (((Value.Container)args[0]).Item)).IsZeroLength() ? 1 : 0);
        }
    }

    [NodeName("XYZ Origin")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates an XYZ at the origin (0,0,0).")]
    [NodeSearchTags("xyz", "zero")]
    public class XyzZero: GeometryBase
    {
        public XyzZero()
        {
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(XYZ.Zero);
        }
    }

    [NodeName("Unit X")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates an XYZ representing the X basis (1,0,0).")]
    [NodeSearchTags("axis","xyz")]
    public class XyzBasisX : GeometryBase
    {
        public XyzBasisX()
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

    [NodeName("Unit Y")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates an XYZ representing the Y basis (0,1,0).")]
    [NodeSearchTags("axis", "xyz")]
    public class XyzBasisY : GeometryBase
    {
        public XyzBasisY()
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

    [NodeName("Unit Z")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates an XYZ representing the Z basis (0,0,1).")]
    [NodeSearchTags("axis", "xyz")]
    public class XyzBasisZ : GeometryBase
    {
        public XyzBasisZ()
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

    [NodeName("Scale XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Multiplies each component of an XYZ by a number.")]
    public class XyzScale : GeometryBase
    {
        public XyzScale()
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

            return Value.NewContainer(pt);
        }
    }

    [NodeName("Scale XYZ with Base Point")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Scales an XYZ relative to the supplies base point.")]
    public class XyzScaleOffset : GeometryBase
    {
        public XyzScaleOffset()
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

            return Value.NewContainer(pt);
        }
    }

    [NodeName("Add XYZs")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Adds the components of two XYZs.")]
    public class XyzAdd: GeometryBase
    {
        public XyzAdd()
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

            return Value.NewContainer(pt);
        }
    }

    [NodeName("Subtract Vectors")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Subtracts the components of two XYZs.")]
    public class XyzSubtract : GeometryBase
    {
        public XyzSubtract()
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

            return Value.NewContainer(pt);
        }
    }

    [NodeName("Average XYZs")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Averages a list of XYZs.")]
    public class XyzAverage : GeometryBase
    {
        public XyzAverage()
        {
            InPortData.Add(new PortData("XYZs", "The list of XYZs to average.", typeof(Value.List)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list of XYZs is required to average.");

            var lst = ((Value.List)args[0]).Item;
            var average = BestFitLine.MeanXYZ(BestFitLine.AsGenericList<XYZ>(lst));

            return Value.NewContainer(average);
        }
    }

    [NodeName("Negate XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Negate an XYZ.")]
    public class XyzNegate : GeometryBase
    {
        public XyzNegate()
        {
            InPortData.Add(new PortData("XYZ", "The XYZ to negate.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ pt = (XYZ)((Value.Container)args[0]).Item;

            return Value.NewContainer(pt.Negate());
        }
    }

    [NodeName("XYZ Cross Product")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Calculate the cross product of two XYZs.")]
    public class XyzCrossProduct : GeometryBase
    {
        public XyzCrossProduct()
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

    [NodeName("XYZ Dot Product")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Calculate the dot product of two XYZs.")]
    [NodeSearchTags("inner")]
    public class XyzDotProduct : GeometryBase
    {
        public XyzDotProduct()
        {
            InPortData.Add(new PortData("a", "XYZ A.", typeof(Value.Container)));
            InPortData.Add(new PortData("b", "XYZ B.", typeof(Value.Container)));
            OutPortData.Add(new PortData("xyz", "The dot product of vectors A and B. ", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            XYZ a = (XYZ)((Value.Container)args[0]).Item;
            XYZ b = (XYZ)((Value.Container)args[1]).Item;

            return Value.NewContainer(a.DotProduct(b));
        }
    }

    [NodeName("Direction to XYZ")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Calculate the normalized vector from one xyz to another.")]
    [NodeSearchTags("unitized", "normalized", "vector")]
    public class XyzStartEndVector : GeometryBase
    {
        public XyzStartEndVector()
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

    [NodeName("XYZ Grid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates a grid of XYZs.")]
    [NodeSearchTags("point", "array", "collection", "field")]
    public class ReferencePtGrid: GeometryBase
    {
        public ReferencePtGrid()
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
    [NodeSearchTags("divide", "array", "curve", "repeat")]
    public class XyzArrayAlongCurve : GeometryBase
    {
        public XyzArrayAlongCurve()
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
            if (xi < System.Double.Epsilon)
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

            if (xi < System.Double.Epsilon)
            {
                var pt = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                result = FSharpList<Value>.Cons(Value.NewContainer(pt), result);

                return Value.NewList(
                  ListModule.Reverse(result)
               );
            }

            for (int xCount = 0; xCount <= xi; xCount++)
            {
                t = xCount / xi; // create normalized curve param by dividing current number by total number
                var pt = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                result = FSharpList<Value>.Cons(Value.NewContainer( pt ), result );
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    #endregion

    #region Plane

    [NodeName("Plane by Normal Origin")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Creates a geometric plane.")]
    public class Plane: GeometryBase
    {
        public Plane()
        {
            InPortData.Add(new PortData("normal", "normal", typeof(Value.Container)));
            InPortData.Add(new PortData("origin", "origin", typeof(Value.Container)));
            OutPortData.Add(new PortData("plane", "Plane", typeof(Value.Container)));

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

    [NodeName("XY Plane")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("The plane containing the x and y axis")]
    public class XyPlane : GeometryBase
    {
        public XyPlane()
        {
            OutPortData.Add(new PortData("plane", "The XY Plane", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var plane = dynRevitSettings.Doc.Application.Application.Create.NewPlane(
               new XYZ(0,0,1), new XYZ()
            );

            return Value.NewContainer(plane);
        }
    }

    [NodeName("XZ Plane")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("The plane containing the x and y axis")]
    public class XzPlane : GeometryBase
    {
        public XzPlane()
        {
            OutPortData.Add(new PortData("plane", "The XZ Plane", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var plane = dynRevitSettings.Doc.Application.Application.Create.NewPlane(
               new XYZ(0, 1, 0), new XYZ()
            );

            return Value.NewContainer(plane);
        }
    }

    [NodeName("YZ Plane")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("The plane containing the x and y axis")]
    public class YzPlane : GeometryBase
    {
        public YzPlane()
        {
            OutPortData.Add(new PortData("plane", "The YZ Plane", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var plane = dynRevitSettings.Doc.Application.Application.Create.NewPlane(
               new XYZ(1,0,0), new XYZ()
            );
            return Value.NewContainer(plane);
        }
    }

    [NodeName("Sketch Plane from Plane")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Creates a geometric sketch plane.")]
    public class SketchPlane : RevitTransactionNodeWithOneOutput
    {
        public SketchPlane()
        {
            InPortData.Add(new PortData("plane", "The plane in which to define the sketch.", typeof(Value.Container))); // SketchPlane can accept Plane, Reference or PlanarFace
            OutPortData.Add(new PortData("sketch plane", "SketchPlane", typeof(Value.Container)));

            RegisterAllPorts();
        }

        bool resetPlaneofSketchPlaneElement(Autodesk.Revit.DB.SketchPlane sp, Autodesk.Revit.DB.Plane p)
        {
            XYZ newOrigin = p.Origin;
            XYZ newNorm = p.Normal;
            var oldP = sp.Plane;
            XYZ oldOrigin = oldP.Origin;
            XYZ oldNorm = oldP.Normal;
            
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
                          Autodesk.Revit.DB.SketchPlane sp = null;

                          //handle Plane, Reference or PlanarFace, also test for family or project doc. there probably is a cleaner way to test for all these conditions.
                          if (((Value.Container)x).Item is Autodesk.Revit.DB.Plane) //TODO: ensure this is correctly casting and testing.
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (Autodesk.Revit.DB.Plane)((Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (Autodesk.Revit.DB.Plane)((Value.Container)x).Item
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
                Autodesk.Revit.DB.SketchPlane sp = null;
                bool keepExistingElement = false;
                var x = ((Value.Container)input).Item;

                //TODO: If possible, update to handle mutation rather than deletion...
                if (this.Elements.Count == 1)
                {
                    Element e = this.UIDocument.Document.GetElement(this.Elements[0]);
                    if (e != null && ( e is Autodesk.Revit.DB.SketchPlane))
                    {
                       sp = (Autodesk.Revit.DB.SketchPlane) e;
                       
                       if (x is Reference)
                           keepExistingElement = true;
                       else if (x is Autodesk.Revit.DB.Plane && resetPlaneofSketchPlaneElement(sp, (Autodesk.Revit.DB.Plane) x))
                           keepExistingElement = true;
                    }
                }
                if (!keepExistingElement)
                {
                    foreach (var e in this.Elements)
                        this.DeleteElement(e);

                    //handle Plane, Reference or PlanarFace, also test for family or project doc. there probably is a cleaner way to test for all these conditions.
                    if (x is Autodesk.Revit.DB.Plane)
                    {
                        Autodesk.Revit.DB.Plane p = x as Autodesk.Revit.DB.Plane;
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

    #endregion

    #region Solid Creation

    [NodeName("Revolve")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by revolving closed curve loops lying in xy plane of Transform.")]
    public class CreateRevolvedGeometry : GeometryBase
    {
        public CreateRevolvedGeometry()
        {
            InPortData.Add(new PortData("profile", "The curve loop to use as a profile", typeof(Value.Container)));
            InPortData.Add(new PortData("transform", "Coordinate system for revolve, loop should be in xy plane of this transform on the right side of z axis used for rotate.", typeof(Value.Container)));
            InPortData.Add(new PortData("angle domain", "start angle measured counter-clockwise from x-axis of transform", typeof(Value.Number)));

            OutPortData.Add(new PortData("solid", "The revolved geometry.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var cLoop = (Autodesk.Revit.DB.CurveLoop)((Value.Container)args[0]).Item;
            var trf = (Transform)((Value.Container)args[1]).Item;
            var domain = (DSCoreNodes.Domain) ((Value.Container)args[2]).Item;

            var loopList = new List<Autodesk.Revit.DB.CurveLoop> {cLoop};
            var thisFrame = new Autodesk.Revit.DB.Frame();
            thisFrame.Transform(trf);

            var result = GeometryCreationUtilities.CreateRevolvedGeometry(thisFrame, loopList, domain.Min, domain.Max);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Sweep")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by sweeping curve loop along the path")]
    public class CreateSweptGeometry : GeometryBase
    {
        public CreateSweptGeometry()
        {
            InPortData.Add(new PortData("rail", "The rail curve to sweep along (a CurveLoop or Curve)", typeof(Value.Container)));
            InPortData.Add(new PortData("profile", "A closed curve loop to sweep to be swept along curve", typeof(Value.Container)));
            
            OutPortData.Add(new PortData("solid", "The swept geometry.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public static Autodesk.Revit.DB.CurveLoop CurveLoopFromContainer(FScheme.Value.Container curveOrCurveLoop)
        {
            var pathLoopBoxed = curveOrCurveLoop.Item;
            Autodesk.Revit.DB.CurveLoop curveLoop;
            var loop = pathLoopBoxed as Autodesk.Revit.DB.CurveLoop;
            if (loop != null)
            {
                curveLoop = loop;
            }
            else
            {
                curveLoop = new Autodesk.Revit.DB.CurveLoop();
                curveLoop.Append((Autodesk.Revit.DB.Curve)pathLoopBoxed);
            }

            return curveLoop;
        }

        public static Solid SolidBySweep( Autodesk.Revit.DB.CurveLoop pathLoop, Autodesk.Revit.DB.CurveLoop profileLoop )
        {
            // these are the defaults
            int attachementIndex = 0;
            double attachementPar = 0.0;

            // align profile to axis of curve
            if (profileLoop.HasPlane())
            {
                var profileLoopPlane = profileLoop.GetPlane();
                var CLiter = pathLoop.GetCurveLoopIterator();

                for (int indexCurve = 0; indexCurve < attachementIndex && CLiter.MoveNext(); indexCurve++)
                {
                    CLiter.MoveNext();
                }

                Autodesk.Revit.DB.Curve pathCurve = CLiter.Current;
                if (pathCurve != null)
                {
                    double angleTolerance = Math.PI / 1800.0;
                    Transform pathTrf = pathCurve.ComputeDerivatives(attachementPar, false);
                    XYZ pathDerivative = pathTrf.BasisX.Normalize();
                    double distAttachment = profileLoopPlane.Normal.DotProduct(profileLoopPlane.Origin - pathTrf.Origin);
                    if (Math.Abs(distAttachment) > 0.000001 ||
                         Math.Abs(profileLoopPlane.Normal.DotProduct(pathDerivative)) < 1.0 - angleTolerance * angleTolerance
                       )
                    {
                        //put profile at proper plane
                        double distOrigin = profileLoopPlane.Normal.DotProduct(profileLoopPlane.Origin);
                        XYZ fromPoint = pathTrf.Origin;
                        if (Math.Abs(distAttachment) > 0.000001 + Math.Abs(distOrigin))
                            fromPoint = (-distOrigin) * profileLoopPlane.Normal;
                        else
                            fromPoint = pathTrf.Origin - distOrigin * profileLoopPlane.Normal;
                        XYZ fromVecOne = profileLoopPlane.Normal;
                        XYZ toVecOne = pathDerivative;
                        XYZ fromVecTwo = XYZ.BasisZ.CrossProduct(profileLoopPlane.Normal);
                        if (fromVecTwo.IsZeroLength())
                            fromVecTwo = XYZ.BasisX;
                        else
                            fromVecTwo = fromVecTwo.Normalize();
                        XYZ toVecTwo = XYZ.BasisZ.CrossProduct(pathDerivative);
                        if (toVecTwo.IsZeroLength())
                            toVecTwo = XYZ.BasisX;
                        else
                            toVecTwo = toVecTwo.Normalize();

                        var trfToAttach = Transform.CreateTranslation(pathTrf.Origin);
                        trfToAttach.BasisX = toVecOne;
                        trfToAttach.BasisY = toVecTwo;
                        trfToAttach.BasisZ = toVecOne.CrossProduct(toVecTwo);

                        var trfToProfile = Transform.CreateTranslation(fromPoint);
                        trfToProfile.BasisX = fromVecOne;
                        trfToProfile.BasisY = fromVecTwo;
                        trfToProfile.BasisZ = fromVecOne.CrossProduct(fromVecTwo);

                        var trfFromProfile = trfToProfile.Inverse;

                        var combineTrf = trfToAttach.Multiply(trfFromProfile);

                        //now get new curve loop
                        var transformedCurveLoop = new Autodesk.Revit.DB.CurveLoop();
                        Autodesk.Revit.DB.CurveLoopIterator CLiterT = profileLoop.GetCurveLoopIterator();
                        for (; CLiterT.MoveNext(); )
                        {
                            var curCurve = CLiterT.Current;
                            var curCurveTransformed = curCurve.CreateTransformed(combineTrf);

                            transformedCurveLoop.Append(curCurveTransformed);
                        }
                        profileLoop = transformedCurveLoop;
                    }
                }
            }

            return GeometryCreationUtilities.CreateSweptGeometry(pathLoop, 0, 0, new List<Autodesk.Revit.DB.CurveLoop>(){profileLoop});
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            // unbox
            var path = CurveLoopFromContainer((FScheme.Value.Container) args[0]);
            var profile = (Autodesk.Revit.DB.CurveLoop)((Value.Container)args[1]).Item;

            return Value.NewContainer(SolidBySweep( path, profile ));
        }
    }

    [NodeName("Extrude")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by linearly extruding a closed curve.")]
    public class CreateExtrusionGeometry : GeometryBase
    {

        public CreateExtrusionGeometry()
        {
            InPortData.Add(new PortData("profile", "The profile curve (Can be a Curve or CurveLoops", typeof(Value.Container)));
            InPortData.Add(new PortData("direction", "The direction in which to extrude the profile.", typeof(Value.Container)));
            InPortData.Add(new PortData("distance", "The positive distance by which the loops are to be extruded.", typeof(Value.Number)));

            OutPortData.Add(new PortData("solid", "The extrusion.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var direction = (XYZ)((Value.Container)args[1]).Item;
            var distance = ((Value.Number)args[2]).Item;

            //incoming list will have two lists in it
            //each list will contain curves. convert the curves
            //into curve loops
            var profileList = ((Value.List)args[0]).Item;
            var loops = new List<Autodesk.Revit.DB.CurveLoop>();
            foreach (var item in profileList)
            {
                if (item.IsList)
                {
                    var innerList = ((Value.List)item).Item;
                    foreach (var innerItem in innerList)
                    {
                        loops.Add((Autodesk.Revit.DB.CurveLoop)((Value.Container)item).Item);
                    }
                }
                else
                {
                    //we'll assume a container
                    loops.Add((Autodesk.Revit.DB.CurveLoop)((Value.Container)item).Item);
                }
            }

            var result = GeometryCreationUtilities.CreateExtrusionGeometry(loops, direction, distance);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Blend")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_CREATE)]
    [NodeDescription("Creates a solid by blending two closed curve loops lying in non-coincident planes.")]
    public class CreateBlendGeometry : GeometryBase
    {
        public CreateBlendGeometry()
        {
            InPortData.Add(new PortData("first loop", "The first curve loop. The loop must be a closed planar loop.", typeof(Value.Container)));
            InPortData.Add(new PortData("second loop", "The second curve loop, which also must be a closed planar loop.", typeof(Value.Container)));
            OutPortData.Add(new PortData("solid", "The blended geometry.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var firstLoop = CreateSweptGeometry.CurveLoopFromContainer((Value.Container)args[0]);
            var secondLoop = CreateSweptGeometry.CurveLoopFromContainer((Value.Container)args[1]);

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

            return Value.NewContainer(result);
        }
    }

    [NodeName("Boolean Operation")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_BOOLEAN)]
    [NodeDescription("Creates solid by union, intersection or difference of two solids.")]
    public class BooleanOperation : GeometryBase
    {
        ComboBox combo;
        int selectedItem = -1;

        public BooleanOperation()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for boolean geometrical operation", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for boolean geometrical operation", typeof(object)));

            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));
            selectedItem = 2;
            RegisterAllPorts();

        }
        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

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

        public enum BooleanOperationOptions { Union, Intersect, Difference };

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("index", this.combo.SelectedIndex.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                selectedItem = Convert.ToInt32(nodeElement.Attributes["index"].Value);
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
                ((n == 2) ? BooleanOperationsType.Difference : BooleanOperationsType.Intersect);

            Solid result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, opType);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Boolean Difference")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_BOOLEAN)]
    [NodeDescription("Creates solid by boolean difference of two solids")]
    public class SolidDifference : GeometryBase
    {

        public SolidDifference()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for boolean geometrical operation", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for boolean geometrical operation", typeof(object)));

            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var firstSolid = (Solid)((Value.Container)args[0]).Item;
            var secondSolid = (Solid)((Value.Container)args[1]).Item;

            var result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, BooleanOperationsType.Difference);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Boolean Union")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_BOOLEAN)]
    [NodeDescription("Creates solid by boolean union of two solids")]
    public class SolidUnion : GeometryBase
    {

        public SolidUnion()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for union", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for union", typeof(object)));

            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var firstSolid = (Solid)((Value.Container)args[0]).Item;
            var secondSolid = (Solid)((Value.Container)args[1]).Item;

            var result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, BooleanOperationsType.Union);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Boolean Intersect")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_BOOLEAN)]
    [NodeDescription("Creates solid by boolean difference of two solids")]
    public class SolidIntersection : GeometryBase
    {

        public SolidIntersection()
        {
            InPortData.Add(new PortData("First Solid", "First solid input for intersection", typeof(object)));
            InPortData.Add(new PortData("Second Solid", "Second solid input for intersection", typeof(object)));

            OutPortData.Add(new PortData("solid in the element's geometry objects", "Solid", typeof(object)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var firstSolid = (Solid)((Value.Container)args[0]).Item;
            var secondSolid = (Solid)((Value.Container)args[1]).Item;

            var result = BooleanOperationsUtils.ExecuteBooleanOperation(firstSolid, secondSolid, BooleanOperationsType.Intersect);

            return Value.NewContainer(result);
        }
    }

    [NodeName("Solid from Element")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Creates reference to the solid in the element's geometry objects.")]
    public class ElementSolid : GeometryBase
    {
        Dictionary<ElementId, List<GeometryObject>> instanceSolids;

        public ElementSolid()
        {
            InPortData.Add(new PortData("element", "element to create geometrical reference to", typeof(Value.Container)));
            OutPortData.Add(new PortData("solid", "solid in the element's geometry objects", typeof(object)));

            RegisterAllPorts();

            instanceSolids = new Dictionary<ElementId, List<GeometryObject>>();
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
            if ((thisElement is GenericForm) && (FreeForm.freeFormSolids != null &&
                  FreeForm.freeFormSolids.ContainsKey(thisElement.Id)))
            {
                mySolid = FreeForm.freeFormSolids[thisElement.Id];
            }
            else
            {
                bool bNotVisibleOption = false;
                if (thisElement is GenericForm)
                {
                    GenericForm gF = (GenericForm)thisElement;
                    if (!gF.Combinations.IsEmpty)
                        bNotVisibleOption = true;
                }
                int nTry = (bNotVisibleOption) ? 2 : 1;
                for (int iTry = 0; iTry < nTry && (mySolid == null); iTry++)
                {
                    Autodesk.Revit.DB.Options geoOptions = new Autodesk.Revit.DB.Options();
                    geoOptions.ComputeReferences = true;
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

            return Value.NewContainer(mySolid);
        }
    }

    [NodeName("Cylinder")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Create a cylinder from the axis, origin, radius, and height")]
    public class SolidCylinder : GeometryBase
    {
        public SolidCylinder()
        {
            InPortData.Add(new PortData("axis", "Axis of cylinder", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0, 0, 1))));
            InPortData.Add(new PortData("origin", "Base point of cylinder", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0, 0, 0))));
            InPortData.Add(new PortData("radius", "Radius of cylinder", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("height", "Height of cylinder", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(2)));
           
            OutPortData.Add(new PortData("cylinder", "Solid cylinder", typeof(object)));

            RegisterAllPorts();

        }

        public static Solid CylinderByAxisOriginRadiusHeight(XYZ axis, XYZ origin, double radius, double height)
        {
            // get axis that is perp to axis by first generating random vector
            var r = new System.Random();
            axis = axis.Normalize();
            var randXyz = new XYZ(r.NextDouble(), r.NextDouble(), r.NextDouble());
            var axisPerp1 = randXyz.CrossProduct(axis).Normalize();

            // get second axis that is perp to axis
            var axisPerp2 = axisPerp1.CrossProduct(axis);

            // create circle
            var circle = dynRevitSettings.Doc.Application.Application.Create.NewArc(origin, radius, 0, 2 * Circle.RevitPI, axisPerp1, axisPerp2);

            // create curve loop from cirle
            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { circle });

            // extrude the curve and return
            return GeometryCreationUtilities.CreateExtrusionGeometry(new List<Autodesk.Revit.DB.CurveLoop>() { circleLoop }, axis, height);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            // unpack input
            var axis = (XYZ) ((Value.Container) args[0]).Item;
            var origin = (XYZ)((Value.Container)args[1]).Item;
            var radius = ((Value.Number)args[2]).Item;
            var height = ((Value.Number)args[3]).Item;

            // create and return geom
            return Value.NewContainer( CylinderByAxisOriginRadiusHeight(axis, origin, radius, height) );
        }
    }

    [NodeName("Sphere")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Creates sphere from a center point and axis")]
    public class SolidSphere : GeometryBase
    {

        public SolidSphere()
        {
            InPortData.Add(new PortData("center", "Center point of sphere", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0, 0, 0))));
            InPortData.Add(new PortData("radius", "Radius of sphere", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));

            OutPortData.Add(new PortData("sphere", "Solid sphere", typeof(object)));

            RegisterAllPorts();
        }

        public static Solid SphereByCenterRadius(XYZ center, double radius)
        {

            // create semicircular arc
            var semicircle = dynRevitSettings.Doc.Application.Application.Create.NewArc(center, radius, 0, Circle.RevitPI, XYZ.BasisZ, XYZ.BasisX);

            // create axis curve of cylinder - running from north to south pole
            var axisCurve = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(new XYZ(0, 0, -radius),
                new XYZ(0, 0, radius));

            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { semicircle, axisCurve });

            // revolve arc to form sphere
            return
                GeometryCreationUtilities.CreateRevolvedGeometry(
                    new Autodesk.Revit.DB.Frame(center, XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ), new List<Autodesk.Revit.DB.CurveLoop>() { circleLoop }, 0,
                    2 * Circle.RevitPI);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            // unpack input
            var center = (XYZ)((Value.Container)args[0]).Item;
            var radius = ((Value.Number)args[1]).Item;

            return Value.NewContainer( SphereByCenterRadius(center, radius) );
        }
    }

    [NodeName("Torus")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Creates torus from axis, radius, and outer radius")]
    public class SolidTorus : GeometryBase
    {
        public SolidTorus()
        {
            InPortData.Add(new PortData("axis", "Axis of torus", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0,0,1))));
            InPortData.Add(new PortData("center", "Center point of torus", typeof(object), FScheme.Value.NewContainer(new XYZ(0, 0, 1))));
            InPortData.Add(new PortData("radius", "The distance from the center to the cross-sectional center", typeof(FScheme.Value.Number), 
                FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("section radius", "The radius of the cross-section of the torus", typeof(FScheme.Value.Number), 
                FScheme.Value.NewNumber(0.25)));

            OutPortData.Add(new PortData("torus", "Solid torus", typeof(object)));

            RegisterAllPorts();
        }

        public static Solid TorusByAxisOriginRadiusCrossSectionRadius(XYZ zAxis, XYZ center, double radius, double sectionRadius)
        {

            // get axis that is perp to axis by first generating random vector
            var r = new System.Random();
            zAxis = zAxis.Normalize();
            var randXyz = new XYZ(r.NextDouble(), r.NextDouble(), r.NextDouble());
            var xAxis = randXyz.CrossProduct(zAxis).Normalize();

            // get second axis that is perp to axis
            var yAxis = xAxis.CrossProduct(zAxis);

            // create circle
            var circle = dynRevitSettings.Doc.Application.Application.Create.NewArc(center + radius * xAxis, radius, 
                0, 2 * Circle.RevitPI, xAxis, zAxis);

            // create curve loop from cirle
            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { circle });

            // extrude the curve and return
            return 
                GeometryCreationUtilities.CreateRevolvedGeometry(
                    new Autodesk.Revit.DB.Frame(center, xAxis, yAxis, zAxis), new List<Autodesk.Revit.DB.CurveLoop>() { circleLoop }, 0,
                    2 * Circle.RevitPI);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            // unpack input
            var axis = (XYZ) ((Value.Container) args[0]).Item;
            var center = (XYZ)((Value.Container)args[1]).Item;
            var radius = ((Value.Number)args[2]).Item;
            var sectionradius = ((Value.Number)args[3]).Item;

            // build and return geom
            return Value.NewContainer(TorusByAxisOriginRadiusCrossSectionRadius(axis, center, radius, sectionradius));
        }
    }

    [NodeName("Box by Two Corners")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Create solid box aligned with the world coordinate system given two corner points")]
    public class SolidBoxByTwoCorners : GeometryBase
    {
        public SolidBoxByTwoCorners()
        {
            InPortData.Add(new PortData("bottom", "Bottom point of box", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(-1, -1, -1))));
            InPortData.Add(new PortData("top", "Top point of box", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(1, 1, 1))));

            OutPortData.Add(new PortData("box", "Solid box", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public static Solid AlignedBoxByTwoCorners(XYZ bottom, XYZ top)
        {

            // obtain coordinates of base rectangle
            var p0 = bottom;
            var p1 = p0 + new XYZ(top.X - bottom.X, 0, 0);
            var p2 = p1 + new XYZ(0, top.Y - bottom.Y, 0);
            var p3 = p2 - new XYZ(top.X - bottom.X, 0, 0);

            // form edges of base rect
            var l1 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p0, p1);
            var l2 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p1, p2);
            var l3 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p2, p3);
            var l4 = dynRevitSettings.Doc.Application.Application.Create.NewLineBound(p3, p0);

            // form curve loop from lines of base rect
            var cl = new Autodesk.Revit.DB.CurveLoop();
            cl.Append(l1);
            cl.Append(l2);
            cl.Append(l3);
            cl.Append(l4);

            // get height of box
            var height = top.Z - bottom.Z;

            // extrude the curve and return
            return
                GeometryCreationUtilities.CreateExtrusionGeometry(new List<Autodesk.Revit.DB.CurveLoop>() { cl },
                    XYZ.BasisZ, height);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            // unpack input
            var bottom = (XYZ) ((Value.Container) args[0]).Item;
            var top = (XYZ)((Value.Container)args[1]).Item;

            // build and return geom
            return Value.NewContainer(AlignedBoxByTwoCorners(bottom, top));
        }
    }

    [NodeName("Box by Center and Dimensions")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_PRIMITIVES)]
    [NodeDescription("Create solid box aligned with the world coordinate system given the center of the box and the length of its axes")]
    public class SolidBoxByCenterAndDimensions : GeometryBase
    {
        public SolidBoxByCenterAndDimensions()
        {
            InPortData.Add(new PortData("center", "Center of box", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(0,0,0))));
            InPortData.Add(new PortData("x", "Dimension of box in x direction", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("y", "Dimension of box in y direction", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("z", "Dimension of box in z direction", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));

            OutPortData.Add(new PortData("box", "Solid box", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public static Solid AlignedBoxByCenterAndDimensions(XYZ center, double x, double y, double z)
        {

            var bottom = center - new XYZ(x/2, y/2, z/2);
            var top = center + new XYZ(x/2, y/2, z/2);

            return SolidBoxByTwoCorners.AlignedBoxByTwoCorners(bottom, top);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            // unpack input
            var center = (XYZ) ((Value.Container) args[0]).Item;
            var x  = ((Value.Number)args[1]).Item;
            var y  = ((Value.Number)args[2]).Item;
            var z  = ((Value.Number)args[3]).Item;

            // build and return geom
            return Value.NewContainer(AlignedBoxByCenterAndDimensions( center, x, y, z) );
        }
    }

    #endregion

    #region Faces

    [NodeName("Faces Intersecting Line")]
    [NodeCategory(BuiltinNodeCategories.MODIFYGEOMETRY_INTERSECT)]
    [NodeDescription("Creates list of faces of the solid intersecting given line.")]
    public class FacesByLine : NodeWithOneOutput
    {
        public FacesByLine()
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
                Autodesk.Revit.DB.Face thisFace = (Autodesk.Revit.DB.Face)thisEnum.Current;
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

    [NodeName("Face From Points")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Creates face on grid of points")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class FaceThroughPoints : NodeWithOneOutput
    {

        public FaceThroughPoints()
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

            System.String nameOfMethodCreate = "Create";
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

                    result = (Autodesk.Revit.DB.Face)m.Invoke(null, argsM);

                    break;
                }
            }

            return Value.NewContainer(result);
        }
    }

    #endregion

    #region Solid Manipulation

    [NodeName("Explode Solid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID_EXTRACT)]
    [NodeDescription("Creates list of faces of solid or edges of face")]
    public class GeometryObjectsFromRoot : NodeWithOneOutput
    {

        public GeometryObjectsFromRoot()
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

    [NodeName("Transform Solid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Creates solid by transforming solid")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class TransformSolid : GeometryBase
    {
        public TransformSolid()
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

            System.String nameOfReplaceMethod = "CreateGeometryByFaceReplacement";

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
             
            System.String nameOfMethodTransform = "transform";

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

            return Value.NewContainer(result);
        }
    }
    
    [NodeName("Replace Solid Faces")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Build solid replacing faces of input solid by supplied faces")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class ReplaceFacesOfSolid : GeometryBase
    {
        public ReplaceFacesOfSolid()
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

            System.String nameOfReplaceMethod = "CreateGeometryByFaceReplacement";

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

            return Value.NewContainer(result);
        }
    }
    
    [NodeName("Fillet Solid Edges")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Build solid by replace edges with round blends")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class BlendEdges : GeometryBase
    {
        public BlendEdges()
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

            System.String nameOfReplaceMethod = "ExecuteShapingOfEdges";

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

            return Value.NewContainer(result);
        }
    }
    
    [NodeName("Chamfer Solid Edges")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SOLID)]
    [NodeDescription("Build solid by replace edges with chamfers")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class ChamferEdges : GeometryBase
    {
        public ChamferEdges()
        {
            InPortData.Add(new PortData("Solid", "Solid to transform", typeof(Value.Container)));
            InPortData.Add(new PortData("Edges", "Edges to be blends", typeof(Value.List)));
            InPortData.Add(new PortData("Size", "Size of chamfer ", typeof(Value.Number)));
            OutPortData.Add(new PortData("Solid", "Resulting Solid", typeof(Value.Container)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var thisSolid = (Solid)((Value.Container)args[0]).Item;

            var vals = ((Value.List)args[1]).Item;
            var edgesToBeReplaced = new List<GeometryObject>();
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

            System.String nameOfReplaceMethod = "ExecuteShapingOfEdges";

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

            return Value.NewContainer(result);
        }
    }

    [NodeName("Holes in Solid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("List open faces of solid as CurveLoops")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class OnesidedEdgesAsCurveLoops : NodeWithOneOutput
    {

        public OnesidedEdgesAsCurveLoops()
        {
            InPortData.Add(new PortData("Incomplete Solid", "Geometry to check for being Solid", typeof(object)));
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(Value.List)));
            OutPortData.Add(new PortData("Onesided boundaries", "Onesided Edges as CurveLoops", typeof(Value.List)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var thisSolid = (Solid)((Value.Container)args[0]).Item;
            var listIn = ((Value.List)args[1]).Item.Select(
                    x => ((Autodesk.Revit.DB.CurveLoop)((Value.Container)x).Item)
                       ).ToList();

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            var nameOfMethodCreate = "oneSidedEdgesAsCurveLoops";
            List <Autodesk.Revit.DB.CurveLoop> oneSidedAsLoops = null;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    object[] argsM = new object[1];
                    argsM[0] = listIn;

                    oneSidedAsLoops = (List<Autodesk.Revit.DB.CurveLoop>)m.Invoke(thisSolid, argsM);

                    break;
                }
            }

            var result = FSharpList<Value>.Empty;
            var thisEnum = oneSidedAsLoops.GetEnumerator();
        
            for (; thisEnum.MoveNext(); )
            {
                result = FSharpList<Value>.Cons(Value.NewContainer((Autodesk.Revit.DB.CurveLoop) thisEnum.Current), result);
            }


            return Value.NewList(result);
        }
    }

    [NodeName("Cap Holes in Solid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Patch set of faces as Solid ")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class PatchSolid : GeometryBase
    {

        public PatchSolid()
        {
            InPortData.Add(new PortData("Incomplete Solid", "Geoemtry to check for being Solid", typeof(object)));
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(Value.List)));
            InPortData.Add(new PortData("Faces", "Faces to exclude", typeof(Value.List)));

            OutPortData.Add(new PortData("Result", "Computed Solid", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var thisSolid = (Solid)((Value.Container)args[0]).Item;

            var listInCurveLoops = ((Value.List)args[1]).Item.Select(
                    x => ((Autodesk.Revit.DB.CurveLoop)((Value.Container)x).Item)
                       ).ToList();

            var listInFacesToExclude = ((Value.List)args[2]).Item.Select(
                    x => ((Autodesk.Revit.DB.Face)((Value.Container)x).Item)
                       ).ToList();

            var SolidType = typeof(Autodesk.Revit.DB.Solid);

            var solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            var nameOfMethodCreate = "patchSolid";
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

            return Value.NewContainer(resultSolid);
        }
    }

    [NodeName("Solid From Curve Loops")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_SURFACE)]
    [NodeDescription("Created a ")]
    [DoNotLoadOnPlatforms(Context.REVIT_2013, Context.REVIT_2014, Context.VASARI_2013)]
    public class SkinCurveLoops : GeometryBase
    {

        public SkinCurveLoops()
        {
            InPortData.Add(new PortData("CurveLoops", "Additional curve loops ready for patching", typeof(Value.List)));
            OutPortData.Add(new PortData("Result", "Computed Solid", typeof(object)));

            RegisterAllPorts();
        }

        public static bool noSkinSolidMethod()
        {
            
            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreate = "skinCurveLoopsIntoSolid";
            bool methodFound = false;

            foreach (MethodInfo m in solidTypeMethods)
            {
                if (m.Name == nameOfMethodCreate)
                {
                    methodFound = true;

                    break;
                }
            }

            return !methodFound;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var listInCurveLoops = ((Value.List)args[0]).Item.Select(
                    x => ((Autodesk.Revit.DB.CurveLoop)((Value.Container)x).Item)
                       ).ToList();

            Type SolidType = typeof(Autodesk.Revit.DB.Solid);

            MethodInfo[] solidTypeMethods = SolidType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            System.String nameOfMethodCreate = "skinCurveLoopsIntoSolid";
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

            return Value.NewContainer(resultSolid);
        }
    }

    #endregion

    #region UV

    [NodeName("UV")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates a UV from two double values.")]
    public class Uv : GeometryBase
    {
        public Uv()
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

    [NodeName("UV Domain")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Create a two dimensional domain specifying the Minimum and Maximum UVs.")]
    public class Domain2D : NodeWithOneOutput
    {
        public Domain2D()
        {
            InPortData.Add(new PortData("min", "The minimum UV of the domain.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("max", "The maximum UV of the domain.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("domain", "A domain.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var min = (UV)((FScheme.Value.Container)args[0]).Item;
            var max = (UV)((FScheme.Value.Container)args[1]).Item;

            var vmax = Autodesk.LibG.Vector.by_coordinates(max.U, max.V);
            var vmin = Autodesk.LibG.Vector.by_coordinates(min.U, min.V);

            return FScheme.Value.NewContainer(DSCoreNodes.Domain2D.ByMinimumAndMaximum(vmin, vmax));
        }
    }

    [NodeName("UV Grid")]
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates a grid of UVs from a domain.")]
    [NodeSearchTags("point", "array", "collection", "field", "uv")]
    public class UvGrid : NodeWithOneOutput
    {
        public UvGrid()
        {
            InPortData.Add(new PortData("domain", "A two dimensional domain.", typeof(Value.Container)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(Value.Number)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(Value.Number)));
            OutPortData.Add(new PortData("UVs", "List of UVs in the grid", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var domain = (DSCoreNodes.Domain2D)((Value.Container)args[0]).Item;
            double ui = ((Value.Number)args[1]).Item;
            double vi = ((Value.Number)args[2]).Item;
            double us = domain.USpan / ui;
            double vs = domain.VSpan / vi;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            for (int i = 0; i <= ui; i++)
            {
                double u = domain.Min.x() + i * us;

                for (int j = 0; j <= vi; j++)
                {
                    double v = domain.Min.y() + j * vs;

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
    [NodeCategory(BuiltinNodeCategories.CREATEGEOMETRY_VECTOR)]
    [NodeDescription("Creates a grid of UVs froma domain.")]
    [NodeSearchTags("point", "array", "collection", "field")]
    public class UvRandom : NodeWithOneOutput
    {
        public UvRandom()
        {
            InPortData.Add(new PortData("dom", "A domain.", typeof(Value.Container)));
            InPortData.Add(new PortData("U-count", "Number in the U direction.", typeof(Value.Number)));
            InPortData.Add(new PortData("V-count", "Number in the V direction.", typeof(Value.Number)));
            OutPortData.Add(new PortData("UVs", "List of UVs in the grid", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            double ui, vi;

            var domain = (DSCoreNodes.Domain2D)((Value.Container)args[0]).Item;
            ui = ((Value.Number)args[1]).Item;
            vi = ((Value.Number)args[2]).Item;

            FSharpList<Value> result = FSharpList<Value>.Empty;

            //UV min = ((Value.Container)domain[0]).Item as UV;
            //UV max = ((Value.Container)domain[1]).Item as UV;

            var min = new UV(domain.Min.x(), domain.Min.y());
            var max = new UV(domain.Max.x(), domain.Max.y());

            var r = new System.Random();
            double uSpan = max.U - min.U;
            double vSpan = max.V - min.V;

            for (int i = 0; i < ui; i++)
            {
                for (int j = 0; j < vi; j++)
                {
                    result = FSharpList<Value>.Cons(
                        Value.NewContainer(new UV(min.U + r.NextDouble() * uSpan, min.V + r.NextDouble() * vSpan)),
                        result
                    );
                }
            }

            return Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    #endregion
} 

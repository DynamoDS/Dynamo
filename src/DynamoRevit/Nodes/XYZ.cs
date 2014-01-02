using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Utilities;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("XYZ")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_CREATE)]
    [NodeDescription("Creates an XYZ from three coordinates.")]
    [NodeSearchTags("vector", "point", "xyz", "coordinate")]
    public class Xyz : GeometryBase
    {
        public Xyz()
        {
            InPortData.Add(new PortData("X", "X", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("Y", "Y", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("Z", "Z", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            double x, y, z;
            x = ((FScheme.Value.Number)args[0]).Item;
            y = ((FScheme.Value.Number)args[1]).Item;
            z = ((FScheme.Value.Number)args[2]).Item;

            var pt = new XYZ(x, y, z);

            return FScheme.Value.NewContainer(pt);
        }
    }

    [NodeName("XYZ by Polar Coordinates")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_CREATE)]
    [NodeDescription("Creates an XYZ from polar coordinates.")]
    [NodeSearchTags("spherical", "xyz", "polar", "coordinates")]
    public class XyzFromPolar : GeometryBase
    {
        public XyzFromPolar()
        {
            InPortData.Add(new PortData("radius", "Radius from origin in radians", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("xy rotation", "Rotation around Z axis in radians", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("offset", "Offset from xy plane", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));

            OutPortData.Add(new PortData("xyz", "XYZ formed from polar coordinates", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public static XYZ FromPolarCoordinates(double r, double theta, double offset)
        {
            // if degenerate, return 0
            if (Math.Abs(r) < System.Double.Epsilon)
            {
                return new XYZ(0, 0, offset);
            }

            // do some trig
            var x = r * Math.Cos(theta);
            var y = r * Math.Sin(theta);
            var z = offset;

            // all done
            return new XYZ(x, y, z);

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var r = ((FScheme.Value.Number)args[0]).Item;
            var theta = ((FScheme.Value.Number)args[1]).Item;
            var phi = ((FScheme.Value.Number)args[2]).Item;

            return FScheme.Value.NewContainer(FromPolarCoordinates(r, theta, phi));
        }
    }

    [NodeName("XYZ to Polar Coordinates")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Decompose an XYZ to polar coordinates.")]
    [NodeSearchTags("spherical", "xyz", "polar", "coordinates", "decompose")]
    public class XyzToPolar : NodeModel
    {
        private readonly PortData _rPort = new PortData("radius", "Radius from origin in radians", typeof(FScheme.Value.Number));
        private readonly PortData _thetaPort = new PortData("xy rotation", "Rotation around Z axis in radians", typeof(FScheme.Value.Number));
        private readonly PortData _offsetPort = new PortData("offset", "Offset from the XY plane", typeof(FScheme.Value.Number));

        public XyzToPolar()
        {
            InPortData.Add(new PortData("xyz", "Input XYZ", typeof(FScheme.Value.Container), FScheme.Value.NewContainer(new XYZ(1, 0, 0))));

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

        public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            var xyz = ((XYZ)((FScheme.Value.Container)args[0]).Item);
            double r, theta, phi;

            ToPolarCoordinates(xyz, out r, out theta, out phi);

            outPuts[_rPort] = FScheme.Value.NewNumber(r);
            outPuts[_thetaPort] = FScheme.Value.NewNumber(theta);
            outPuts[_offsetPort] = FScheme.Value.NewNumber(phi);
        }
    }

    [NodeName("XYZ by Spherical Coordinates")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_CREATE)]
    [NodeDescription("Creates an XYZ from spherical coordinates.")]
    [NodeSearchTags("spherical", "xyz", "polar", "coordinates")]
    public class XyzFromSpherical : GeometryBase
    {
        public XyzFromSpherical()
        {
            InPortData.Add(new PortData("radius", "Radius from origin in radians", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(1)));
            InPortData.Add(new PortData("xy rotation", "Rotation around Z axis in radians", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));
            InPortData.Add(new PortData("z rotation", "Rotation down form axis in radians", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(0)));

            OutPortData.Add(new PortData("xyz", "XYZ formed from polar coordinates", typeof(FScheme.Value.Container)));

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
            var x = r * Math.Cos(theta) * Math.Sin(phi);
            var y = r * Math.Sin(theta) * Math.Sin(phi);
            var z = r * Math.Cos(phi);

            // all done
            return new XYZ(x, y, z);

        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var r = ((FScheme.Value.Number)args[0]).Item;
            var theta = ((FScheme.Value.Number)args[1]).Item;
            var phi = ((FScheme.Value.Number)args[2]).Item;

            return FScheme.Value.NewContainer(FromPolarCoordinates(r, theta, phi));
        }
    }

    [NodeName("XYZ to Spherical Coordinates")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Decompose an XYZ into spherical coordinates.")]
    [NodeSearchTags("spherical", "xyz", "polar", "coordinates", "decompose")]
    public class XyzToSpherical : NodeModel
    {
        private readonly PortData _rPort = new PortData("radius", "Radius from origin in radians", typeof(FScheme.Value.Number));
        private readonly PortData _thetaPort = new PortData("xy rotation", "Rotation around Z axis in radians", typeof(FScheme.Value.Number));
        private readonly PortData _phiPort = new PortData("z rotation", "Rotation from axis in radians (north pole is 0, south pole is PI)", typeof(FScheme.Value.Number));

        public XyzToSpherical()
        {
            InPortData.Add(new PortData("xyz", "XYZ to decompose", typeof(FScheme.Value.Container)));

            OutPortData.Add(_rPort);
            OutPortData.Add(_thetaPort);
            OutPortData.Add(_phiPort);

            this.ArgumentLacing = LacingStrategy.Longest;

            RegisterAllPorts();
        }

        public static void ToSphericalCoordinates(XYZ input, out double r, out double theta, out double phi)
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
                    phi = input.Z > 0 ? 0 : Math.PI;
                    return;
                }
            }

            // if x is 0, this indicates vector is at 90 or 270
            if (Math.Abs(input.X) < System.Double.Epsilon)
            {
                theta = input.Y > 0 ? Math.PI / 2 : 3 * Math.PI / 2;
            }
            else
            {
                theta = Math.Atan(input.Y / input.X);
            }

            // phew...
            phi = Math.Acos(input.Z / r);
        }

        public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            var xyz = ((XYZ)((FScheme.Value.Container)args[0]).Item);
            double r, theta, phi;

            ToSphericalCoordinates(xyz, out r, out theta, out phi);

            outPuts[_rPort] = FScheme.Value.NewNumber(r);
            outPuts[_thetaPort] = FScheme.Value.NewNumber(theta);
            outPuts[_phiPort] = FScheme.Value.NewNumber(phi);
        }
    }

    [NodeName("XYZ from List of Numbers")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_CREATE)]
    [NodeDescription("Creates a list of XYZs by taking sets of 3 numbers from an list.")]
    [NodeSearchTags("parse", "xyz", "numbers")]
    public class XyzFromListOfNumbers : GeometryBase
    {
        public XyzFromListOfNumbers()
        {
            InPortData.Add(new PortData("list", "The list of numbers from which to extract the XYZs.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("list", "A list of XYZs", typeof(FScheme.Value.List)));

            RegisterAllPorts();
            this.ArgumentLacing = LacingStrategy.Disabled;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            if (!args[0].IsList)
            {
                throw new Exception("Input must be a list of numbers.");
            }

            FSharpList<FScheme.Value> vals = ((FScheme.Value.List)args[0]).Item;
            var len = vals.Length;
            if (len % 3 != 0)
                throw new Exception("List size must be a multiple of 3");

            var result = new FScheme.Value[len / 3];
            int count = 0;
            while (!vals.IsEmpty)
            {
                result[count] = FScheme.Value.NewContainer(new XYZ(
                    ((FScheme.Value.Number)vals.Head).Item,
                    ((FScheme.Value.Number)vals.Tail.Head).Item,
                    ((FScheme.Value.Number)vals.Tail.Tail.Head).Item));
                vals = vals.Tail.Tail.Tail;
                count++;
            }

            return FScheme.Value.NewList(Utils.SequenceToFSharpList(result));
        }
    }

    [NodeName("XYZ from Reference Point")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_CREATE)]
    [NodeDescription("Extracts an XYZ from a Reference Point.")]
    [NodeSearchTags("derive", "reference")]
    public class XyzFromReferencePoint : GeometryBase
    {
        public XyzFromReferencePoint()
        {
            InPortData.Add(new PortData("pt", "Reference Point", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("xyz", "Location of the reference point.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            ReferencePoint point;
            point = (ReferencePoint)((FScheme.Value.Container)args[0]).Item;

            return FScheme.Value.NewContainer(point.Position);
        }
    }

    [NodeName("XYZ Components")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Get the components of an XYZ")]
    [NodeSearchTags("decompose", "xyz", "components")]
    public class XyzComponents : NodeModel
    {

        private readonly PortData _xPort = new PortData("x", "X value of given XYZ", typeof(FScheme.Value.Number));
        private readonly PortData _yPort = new PortData("y", "Y value of given XYZ", typeof(FScheme.Value.Number));
        private readonly PortData _zPort = new PortData("z", "Z value of given XYZ", typeof(FScheme.Value.Number));

        public XyzComponents()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(_xPort);
            OutPortData.Add(_yPort);
            OutPortData.Add(_zPort);
            ArgumentLacing = LacingStrategy.Longest;

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            var xyz = ((XYZ)((FScheme.Value.Container)args[0]).Item);
            var x = xyz.X;
            var y = xyz.Y;
            var z = xyz.Z;

            outPuts[_xPort] = FScheme.Value.NewNumber(x);
            outPuts[_yPort] = FScheme.Value.NewNumber(y);
            outPuts[_zPort] = FScheme.Value.NewNumber(z);
        }
    }

    [NodeName("XYZ X")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Fetches the X value of the given XYZ")]
    [NodeSearchTags("x", "xyz", "components", "decompose", "Fetches")]
    public class XyzGetX : GeometryBase
    {
        public XyzGetX()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("X", "X value of given XYZ", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewNumber(((XYZ)((FScheme.Value.Container)args[0]).Item).X);
        }
    }

    [NodeName("XYZ Y")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Fetches the Y value of the given XYZ")]
    [NodeSearchTags("y", "xyz", "components", "decompose", "Fetches")]
    public class XyzGetY : GeometryBase
    {
        public XyzGetY()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("Y", "Y value of given XYZ", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewNumber(((XYZ)((FScheme.Value.Container)args[0]).Item).Y);
        }
    }

    [NodeName("XYZ Z")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Fetches the Z value of the given XYZ")]
    [NodeSearchTags("xyz", "z", "components", "decompose", "Fetches")]
    public class XyzGetZ : GeometryBase
    {
        public XyzGetZ()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("Z", "Z value of given XYZ", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewNumber(((XYZ)((FScheme.Value.Container)args[0]).Item).Z);
        }
    }

    [NodeName("XYZ Distance")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Returns the distance between a(XYZ) and b(XYZ).")]
    [NodeSearchTags("xyz", "distance", "measure")]
    public class XyzDistance : MeasurementBase
    {
        public XyzDistance()
        {
            InPortData.Add(new PortData("a", "Start (XYZ).", typeof(FScheme.Value.Container)));//Ref to a face of a form
            InPortData.Add(new PortData("b", "End (XYZ)", typeof(FScheme.Value.Container)));//Ref to a face of a form
            OutPortData.Add(new PortData("d", "The distance between the two XYZs (Number).", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var a = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var b = (XYZ)((FScheme.Value.Container)args[1]).Item;

            return FScheme.Value.NewNumber(a.DistanceTo(b));
        }
    }

    [NodeName("XYZ Length")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Gets the length of an XYZ")]
    [NodeSearchTags("vector", "magnitude", "amplitude")]
    public class XyzLength : GeometryBase
    {
        public XyzLength()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("X", "X value of given XYZ", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewNumber(((XYZ)((FScheme.Value.Container)args[0]).Item).GetLength());
        }
    }

    [NodeName("Unitize XYZ")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Scale the given XYZ so its length is 1.")]
    [NodeSearchTags("normalize", "length", "vector")]
    public class XyzNormalize : GeometryBase
    {
        public XyzNormalize()
        {
            InPortData.Add(new PortData("xyz", "An XYZ to normalize", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("xyz", "The normalized XYZ", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewContainer(((XYZ)((FScheme.Value.Container)args[0]).Item).Normalize());
        }
    }

    [NodeName("XYZ Is Zero Length")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Determines whether an XYZ has zero length")]
    [NodeSearchTags("vector", "length", "xyz", "magnitude", "amplitude")]
    public class XyzIsZeroLength : GeometryBase
    {
        public XyzIsZeroLength()
        {
            InPortData.Add(new PortData("xyz", "An XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("X", "X value of given XYZ", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewNumber(((XYZ)(((FScheme.Value.Container)args[0]).Item)).IsZeroLength() ? 1 : 0);
        }
    }

    [NodeName("X Axis")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_CREATE)]
    [NodeDescription("Creates an XYZ representing the X basis (1,0,0).")]
    [NodeSearchTags("unit", "xyz", "x", "components", "axis", "basis")]
    public class XyzBasisX : GeometryBase
    {
        public XyzBasisX()
        {
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ pt = XYZ.BasisX;
            return FScheme.Value.NewContainer(pt);
        }
    }

    [NodeName("Y Axis")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_CREATE)]
    [NodeDescription("Creates an XYZ representing the Y basis (0,1,0).")]
    [NodeSearchTags("unit", "xyz", "y", "components", "axis", "basis")]
    public class XyzBasisY : GeometryBase
    {
        public XyzBasisY()
        {
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ pt = XYZ.BasisY;
            return FScheme.Value.NewContainer(pt);
        }
    }

    [NodeName("Z Axis")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_CREATE)]
    [NodeDescription("Creates an XYZ representing the Z basis (0,0,1).")]
    [NodeSearchTags("unit", "xyz", "z", "components", "axis", "basis")]
    public class XyzBasisZ : GeometryBase
    {
        public XyzBasisZ()
        {
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {

            XYZ pt = XYZ.BasisZ;
            return FScheme.Value.NewContainer(pt);
        }
    }

    [NodeName("Scale XYZ")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Multiplies each component of an XYZ by a number.")]
    [NodeSearchTags("multiply", "xyz", "scale")]
    public class XyzScale : GeometryBase
    {
        public XyzScale()
        {
            InPortData.Add(new PortData("xyz", "XYZ", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("n", "Scale amount", typeof(FScheme.Value.Number)));

            OutPortData.Add(new PortData("xyz", "Scaled XYZ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ xyz = (XYZ)((FScheme.Value.Container)args[0]).Item;
            double n = ((FScheme.Value.Number)args[1]).Item;

            XYZ pt = xyz.Multiply(n);

            return FScheme.Value.NewContainer(pt);
        }
    }

    [NodeName("Scale XYZ with Base Point")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Scales an XYZ relative to the supplies base point.")]
    [NodeSearchTags("scale", "xyz", "multiply", "base")]
    public class XyzScaleOffset : GeometryBase
    {
        public XyzScaleOffset()
        {
            InPortData.Add(new PortData("xyz", "XYZ to scale", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("n", "Scale amount", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("base", "XYZ serving as the base point of the scale operation", typeof(FScheme.Value.Container)));

            OutPortData.Add(new PortData("xyz", "Scaled XYZ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ xyz = (XYZ)((FScheme.Value.Container)args[0]).Item;
            double n = ((FScheme.Value.Number)args[1]).Item;
            XYZ base_xyz = (XYZ)((FScheme.Value.Container)args[2]).Item;

            XYZ pt = n * (xyz - base_xyz) + base_xyz;

            return FScheme.Value.NewContainer(pt);
        }
    }

    [NodeName("Add XYZs")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Adds the components of two XYZs.")]
    [NodeSearchTags("add", "xyz")]
    public class XyzAdd : GeometryBase
    {
        public XyzAdd()
        {
            InPortData.Add(new PortData("XYZ(a)", "XYZ", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("XYZ(b)", "XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("XYZ(a+b)", "a + b", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ xyza = (XYZ)((FScheme.Value.Container)args[0]).Item;
            XYZ xyzb = (XYZ)((FScheme.Value.Container)args[1]).Item;

            XYZ pt = xyza + xyzb;

            return FScheme.Value.NewContainer(pt);
        }
    }

    [NodeName("Subtract XYZs")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Subtracts the components of two XYZs.")]
    [NodeSearchTags("difference", "xyz")]
    public class XyzSubtract : GeometryBase
    {
        public XyzSubtract()
        {
            InPortData.Add(new PortData("XYZ(a)", "XYZ", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("XYZ(b)", "XYZ", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("XYZ(a-b)", "a - b", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ xyza = (XYZ)((FScheme.Value.Container)args[0]).Item;
            XYZ xyzb = (XYZ)((FScheme.Value.Container)args[1]).Item;

            XYZ pt = xyza - xyzb;

            return FScheme.Value.NewContainer(pt);
        }
    }

    [NodeName("Average XYZs")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_QUERY)]
    [NodeDescription("Averages a list of XYZs.")]
    [NodeSearchTags("mean", "xyz")]
    public class XyzAverage : GeometryBase
    {
        public XyzAverage()
        {
            InPortData.Add(new PortData("XYZs", "The list of XYZs to average.", typeof(FScheme.Value.List)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Longest;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list of XYZs is required to average.");

            var lst = ((FScheme.Value.List)args[0]).Item;
            var average = BestFitLine.MeanXYZ(BestFitLine.AsGenericList<XYZ>(lst));

            return FScheme.Value.NewContainer(average);
        }
    }

    [NodeName("Negate XYZ")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Negate an XYZ.")]
    [NodeSearchTags("reverse", "invert", "negate", "xyz")]
    public class XyzNegate : GeometryBase
    {
        public XyzNegate()
        {
            InPortData.Add(new PortData("XYZ", "The XYZ to negate.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("xyz", "XYZ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ pt = (XYZ)((FScheme.Value.Container)args[0]).Item;

            return FScheme.Value.NewContainer(pt.Negate());
        }
    }

    [NodeName("XYZ Cross Product")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Calculate the cross product of two XYZs.")]
    [NodeSearchTags("cross", "product", "vector", "dot")]
    public class XyzCrossProduct : GeometryBase
    {
        public XyzCrossProduct()
        {
            InPortData.Add(new PortData("a", "XYZ A.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("b", "XYZ B.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("xyz", "The cross product of vectors A and B. ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ a = (XYZ)((FScheme.Value.Container)args[0]).Item;
            XYZ b = (XYZ)((FScheme.Value.Container)args[1]).Item;

            return FScheme.Value.NewContainer(a.CrossProduct(b));
        }
    }

    [NodeName("XYZ Dot Product")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Calculate the dot product of two XYZs.")]
    [NodeSearchTags("inner", "cross", "scalar", "vector")]
    public class XyzDotProduct : GeometryBase
    {
        public XyzDotProduct()
        {
            InPortData.Add(new PortData("a", "XYZ A.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("b", "XYZ B.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("xyz", "The dot product of vectors A and B. ", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ a = (XYZ)((FScheme.Value.Container)args[0]).Item;
            XYZ b = (XYZ)((FScheme.Value.Container)args[1]).Item;

            return FScheme.Value.NewNumber(a.DotProduct(b));
        }
    }

    [NodeName("Direction to XYZ")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_MODIFY)]
    [NodeDescription("Calculate the normalized vector from one xyz to another.")]
    [NodeSearchTags("unitized", "normalized", "vector")]
    public class XyzStartEndVector : GeometryBase
    {
        public XyzStartEndVector()
        {
            InPortData.Add(new PortData("start", "The start of the vector.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("end", "The end of the vector.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("xyz", "The normalized vector from start to end. ", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            XYZ a = (XYZ)((FScheme.Value.Container)args[0]).Item;
            XYZ b = (XYZ)((FScheme.Value.Container)args[1]).Item;

            return FScheme.Value.NewContainer((b - a).Normalize());
        }
    }

    [NodeName("XYZ Grid")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_POINT_GRID)]
    [NodeDescription("Creates a grid of XYZs.")]
    [NodeSearchTags("array", "collection", "field")]
    public class ReferencePtGrid : GeometryBase
    {
        public ReferencePtGrid()
        {
            InPortData.Add(new PortData("x-count", "Number in the X direction.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("y-count", "Number in the Y direction.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("z-count", "Number in the Z direction.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("x0", "Starting X Coordinate", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("y0", "Starting Y Coordinate", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("z0", "Starting Z Coordinate", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("x-space", "The X spacing.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("y-space", "The Y spacing.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("z-space", "The Z spacing.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("XYZs", "List of XYZs in the grid", typeof(FScheme.Value.List)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            double xi, yi, zi, x0, y0, z0, xs, ys, zs;

            xi = ((FScheme.Value.Number)args[0]).Item;
            yi = ((FScheme.Value.Number)args[1]).Item;
            zi = ((FScheme.Value.Number)args[2]).Item;
            x0 = ((FScheme.Value.Number)args[3]).Item;
            y0 = ((FScheme.Value.Number)args[4]).Item;
            z0 = ((FScheme.Value.Number)args[5]).Item;
            xs = ((FScheme.Value.Number)args[6]).Item;
            ys = ((FScheme.Value.Number)args[7]).Item;
            zs = ((FScheme.Value.Number)args[8]).Item;

            FSharpList<FScheme.Value> result = FSharpList<FScheme.Value>.Empty;

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

                        result = FSharpList<FScheme.Value>.Cons(
                           FScheme.Value.NewContainer(pt),
                           result
                        );
                        x += xs;
                    }
                    y += ys;
                }
                z += zs;
            }

            return FScheme.Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("XYZ Array On Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_DIVIDE)]
    [NodeDescription("Creates a list of XYZs along a curve.")]
    [NodeSearchTags("divide", "array", "curve", "repeat")]
    public class XyzArrayAlongCurve : GeometryBase
    {
        public XyzArrayAlongCurve()
        {
            InPortData.Add(new PortData("curve", "Curve", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("count", "Number", typeof(FScheme.Value.Number))); // just divide equally for now, dont worry about spacing and starting point
            OutPortData.Add(new PortData("XYZs", "List of XYZs in the array", typeof(FScheme.Value.List)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {

            double xi;//, x0, xs;
            xi = ((FScheme.Value.Number)args[1]).Item;// Number
            xi = Math.Round(xi);
            if (xi < System.Double.Epsilon)
                throw new Exception("The point count must be larger than 0.");
            xi = xi - 1;

            //x0 = ((Value.Number)args[2]).Item;// Starting Coord
            //xs = ((Value.Number)args[3]).Item;// Spacing


            var result = FSharpList<FScheme.Value>.Empty;

            Curve crvRef = null;

            if (((FScheme.Value.Container)args[0]).Item is CurveElement)
            {
                var c = (CurveElement)((FScheme.Value.Container)args[0]).Item; // Curve 
                crvRef = c.GeometryCurve;
            }
            else
            {
                crvRef = (Curve)((FScheme.Value.Container)args[0]).Item; // Curve 
            }

            double t = 0;

            if (xi < System.Double.Epsilon)
            {
                var pt = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(pt), result);

                return FScheme.Value.NewList(
                  ListModule.Reverse(result)
               );
            }

            for (int xCount = 0; xCount <= xi; xCount++)
            {
                t = xCount / xi; // create normalized curve param by dividing current number by total number
                var pt = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(pt), result);
            }

            return FScheme.Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("Equal Distanced XYZs On Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_DIVIDE)]
    [NodeDescription("Creates a list of equal distanced XYZs along a curve.")]
    [NodeSearchTags("distance", "xyz", "curve", "equal", "chord", "cord")]
    public class EqualDistXyzAlongCurve : GeometryBase
    {
        public EqualDistXyzAlongCurve()
        {
            InPortData.Add(new PortData("curve", "Curve", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("count", "Number", typeof(FScheme.Value.Number))); // just divide equally for now, dont worry about spacing and starting point
            OutPortData.Add(new PortData("XYZs", "List of equal distanced XYZs", typeof(FScheme.Value.List)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {

            double xi;//, x0, xs;
            xi = ((FScheme.Value.Number)args[1]).Item;// Number
            xi = Math.Round(xi);
            if (xi < System.Double.Epsilon)
                throw new Exception("The point count must be larger than 0.");

            //x0 = ((Value.Number)args[2]).Item;// Starting Coord
            //xs = ((Value.Number)args[3]).Item;// Spacing


            var result = FSharpList<FScheme.Value>.Empty;

            Curve crvRef = null;

            if (((FScheme.Value.Container)args[0]).Item is CurveElement)
            {
                var c = (CurveElement)((FScheme.Value.Container)args[0]).Item; // Curve 
                crvRef = c.GeometryCurve;
            }
            else
            {
                crvRef = (Curve)((FScheme.Value.Container)args[0]).Item; // Curve 
            }

            double t = 0.0;

            XYZ startPoint = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);

            result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(startPoint), result);

            t = 1.0;
            XYZ endPoint = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);

            if (xi > 2.0 + System.Double.Epsilon)
            {
                int numParams = Convert.ToInt32(xi - 2.0);

                var curveParams = new List<double>();

                for (int ii = 0; ii < numParams; ii++)
                {
                    curveParams.Add((ii + 1.0) / (xi - 1.0));
                }

                int maxIterNum = 15;
                bool bUnbound = XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef);

                int iterNum = 0;
                for (; iterNum < maxIterNum; iterNum++)
                {
                    XYZ prevPoint = startPoint;
                    XYZ thisXYZ = null;
                    XYZ nextXYZ = null;

                    Vector<double> distValues = DenseVector.Create(numParams, (c) => 0.0);

                    Matrix<double> iterMat = DenseMatrix.Create(numParams, numParams, (r, c) => 0.0);
                    double maxDistVal = -1.0;
                    for (int iParam = 0; iParam < numParams; iParam++)
                    {
                        t = curveParams[iParam];

                        if (nextXYZ != null)
                            thisXYZ = nextXYZ;
                        else
                            thisXYZ = !bUnbound ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);

                        double tNext = (iParam == numParams - 1) ? 1.0 : curveParams[iParam + 1];
                        nextXYZ = (iParam == numParams - 1) ? endPoint :
                                   !bUnbound ? crvRef.Evaluate(tNext, true) : crvRef.Evaluate(tNext * crvRef.Period, false);

                        distValues[iParam] = thisXYZ.DistanceTo(prevPoint) - thisXYZ.DistanceTo(nextXYZ);

                        if (Math.Abs(distValues[iParam]) > maxDistVal)
                            maxDistVal = Math.Abs(distValues[iParam]);
                        Transform thisDerivTrf = !bUnbound ? crvRef.ComputeDerivatives(t, true) : crvRef.ComputeDerivatives(t * crvRef.Period, false);
                        XYZ derivThis = thisDerivTrf.BasisX;
                        if (bUnbound)
                            derivThis = derivThis.Multiply(crvRef.Period);
                        double distPrev = thisXYZ.DistanceTo(prevPoint);
                        if (distPrev > System.Double.Epsilon)
                        {
                            double valDeriv = (thisXYZ - prevPoint).DotProduct(derivThis) / distPrev;
                            iterMat[iParam, iParam] += valDeriv;
                            if (iParam > 0)
                            {
                                iterMat[iParam - 1, iParam] -= valDeriv;
                            }
                        }
                        double distNext = thisXYZ.DistanceTo(nextXYZ);
                        if (distNext > System.Double.Epsilon)
                        {
                            double valDeriv = (thisXYZ - nextXYZ).DotProduct(derivThis) / distNext;

                            iterMat[iParam, iParam] -= valDeriv;
                            if (iParam < numParams - 1)
                            {
                                iterMat[iParam + 1, iParam] += valDeriv;
                            }
                        }
                        prevPoint = thisXYZ;
                    }

                    Matrix<double> iterMatInvert = iterMat.Inverse();
                    Vector<double> changeValues = iterMatInvert.Multiply(distValues);

                    double dampingFactor = 1.0;

                    for (int iParam = 0; iParam < numParams; iParam++)
                    {
                        curveParams[iParam] -= dampingFactor * changeValues[iParam];

                        if (iParam == 0 && curveParams[iParam] < 0.000000001)
                        {
                            double oldValue = dampingFactor * changeValues[iParam] + curveParams[iParam];
                            while (curveParams[iParam] < 0.000000001)
                                curveParams[iParam] = 0.5 * (dampingFactor * changeValues[iParam] + curveParams[iParam]);
                            changeValues[iParam] = (oldValue - curveParams[iParam]) / dampingFactor;
                        }
                        else if (iParam > 0 && curveParams[iParam] < 0.000000001 + curveParams[iParam - 1])
                        {
                            for (; iParam > -1; iParam--)
                                curveParams[iParam] = dampingFactor * changeValues[iParam] + curveParams[iParam];

                            dampingFactor *= 0.5;
                            continue;
                        }
                        else if (iParam == numParams - 1 && curveParams[iParam] > 1.0 - 0.000000001)
                        {
                            double oldValue = dampingFactor * changeValues[iParam] + curveParams[iParam];
                            while (curveParams[iParam] > 1.0 - 0.000000001)
                                curveParams[iParam] = 0.5 * (1.0 + dampingFactor * changeValues[iParam] + curveParams[iParam]);
                            changeValues[iParam] = (oldValue - curveParams[iParam]) / dampingFactor;
                        }
                    }
                    if (maxDistVal < 0.000000001)
                    {
                        for (int iParam = 0; iParam < numParams; iParam++)
                        {
                            t = curveParams[iParam];
                            thisXYZ = !XyzOnCurveOrEdge.curveIsReallyUnbound(crvRef) ? crvRef.Evaluate(t, true) : crvRef.Evaluate(t * crvRef.Period, false);
                            result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(thisXYZ), result);

                        }
                        break;
                    }
                }

                if (iterNum == maxIterNum)
                    throw new Exception("could not solve for equal distances");

            }

            if (xi > 1.0 + System.Double.Epsilon)
            {
                result = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(endPoint), result);
            }
            return FScheme.Value.NewList(
               ListModule.Reverse(result)
            );
        }
    }

    [NodeName("Evaluate Curve")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Evaluates curve or edge at parameter.")]
    [NodeSearchTags("parameter", "1d", "divide")]
    public class XyzOnCurveOrEdge : GeometryBase
    {
        public XyzOnCurveOrEdge()
        {
            InPortData.Add(new PortData("parameter", "The normalized parameter to evaluate at within 0..1 range, any for closed curve.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("curve or edge", "The curve or edge to evaluate.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("XYZ", "XYZ at parameter.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public static bool curveIsReallyUnbound(Curve curve)
        {
            if (!curve.IsBound)
                return true;
            if (!curve.IsCyclic)
                return false;
            double period = curve.Period;
            if (curve.get_EndParameter(1) > curve.get_EndParameter(0) + period - 0.000000001)
                return true;
            return false;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            double parameter = ((FScheme.Value.Number)args[0]).Item;

            Curve thisCurve = null;
            Edge thisEdge = null;
            if (((FScheme.Value.Container)args[1]).Item is Curve)
                thisCurve = ((FScheme.Value.Container)args[1]).Item as Curve;
            else if (((FScheme.Value.Container)args[1]).Item is Edge)
                thisEdge = ((FScheme.Value.Container)args[1]).Item as Edge;
            else if (((FScheme.Value.Container)args[1]).Item is Reference)
            {
                Reference r = (Reference)((FScheme.Value.Container)args[1]).Item;
                if (r != null)
                {
                    Element refElem = DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(r.ElementId);
                    if (refElem != null)
                    {
                        GeometryObject geob = refElem.GetGeometryObjectFromReference(r);
                        thisEdge = geob as Edge;
                        if (thisEdge == null)
                            thisCurve = geob as Curve;
                    }
                    else
                        throw new Exception("Could not accept second in-port for Evaluate curve or edge node");
                }
            }
            else if (((FScheme.Value.Container)args[1]).Item is CurveElement)
            {
                CurveElement cElem = ((FScheme.Value.Container)args[1]).Item as CurveElement;
                if (cElem != null)
                {
                    thisCurve = cElem.GeometryCurve;
                }
                else
                    throw new Exception("Could not accept second in-port for Evaluate curve or edge node");

            }
            else
                throw new Exception("Could not accept second in-port for Evaluate curve or edge node");

            XYZ result = (thisCurve != null) ? (!curveIsReallyUnbound(thisCurve) ? thisCurve.Evaluate(parameter, true) : thisCurve.Evaluate(parameter, false))
                :
                (thisEdge == null ? null : thisEdge.Evaluate(parameter));

            return FScheme.Value.NewContainer(result);
        }
    }

    [NodeName("XYZ By Offset from Origin")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_QUERY)]
    [NodeDescription("Evaluates curve or edge at parameter.")]
    [NodeSearchTags("offset", "xyz", "origin")]
    public class XyzByDistanceOffsetFromOrigin : GeometryBase
    {
        public XyzByDistanceOffsetFromOrigin()
        {
            InPortData.Add(new PortData("origin", "The origin for the offset.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("direction", "The direction to offset.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("distance", "The distance to offset.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("point", "The offset point.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var origin = (XYZ) ((FScheme.Value.Container) args[0]).Item;
            var direction = (XYZ)((FScheme.Value.Container)args[1]).Item;
            var distance = ((FScheme.Value.Number)args[2]).Item;

            var pt = origin + direction.Multiply(distance);
            return FScheme.Value.NewContainer(pt);
        }
    }
}

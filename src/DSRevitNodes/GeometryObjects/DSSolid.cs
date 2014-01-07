using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using DSRevitNodes.Graphics;
using Curve = Autodesk.Revit.DB.Curve;
using Point = Autodesk.DesignScript.Geometry.Point;
using Solid = Autodesk.Revit.DB.Solid;

namespace DSRevitNodes.Elements
{
    public class DSSolid : AbstractGeometryObject
    {
        #region private members

        private const double RevitPI = 3.14159265358979;

        #endregion

        #region internal properties

        internal Autodesk.Revit.DB.Solid InternalSolid
        {
            get; private set;
        }

        protected override GeometryObject InternalGeometryObject
        {
            get { return InternalSolid; }
        }

        #endregion

        #region Internal constructors

        /// <summary>
        /// Internal constructor making a solid by extrusion
        /// </summary>
        /// <param name="loops"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        internal DSSolid(IEnumerable<CurveLoop> loops, XYZ direction, double distance )
        {
            var result = GeometryCreationUtilities.CreateExtrusionGeometry(loops.ToList(), direction, distance);
            this.InternalSolid = result;
        }

        /// <summary>
        /// Internal constructor making a solid by revolve
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="trans"></param>
        /// <param name="start">The start angle</param>
        /// <param name="end">The end angle</param>
        internal DSSolid(CurveLoop loop, Transform trans, double start, double end)
        {
            var loopList = new List<Autodesk.Revit.DB.CurveLoop> { loop };
            var thisFrame = new Autodesk.Revit.DB.Frame();
            thisFrame.Transform(trans);

            var result = GeometryCreationUtilities.CreateRevolvedGeometry(thisFrame, loopList, start, end);
            this.InternalSolid = result;
        }

        /// <summary>
        /// Internal contructor making a solid by blend
        /// </summary>
        /// <param name="loops"></param>
        internal DSSolid(IEnumerable<CurveLoop> loops)
        {
            List<VertexPair> vertPairs = null;
            var result = GeometryCreationUtilities.CreateBlendGeometry(loops.ElementAt(0), loops.ElementAt(1), vertPairs);
            this.InternalSolid = result;
        }

        /// <summary>
        /// Internal contructor to make a solid by blending profiles along a path.
        /// </summary>
        /// <param name="profileLoops"></param>
        /// <param name="pathCurve"></param>
        /// <param name="inputParameters"></param>
        internal DSSolid(IEnumerable<CurveLoop> profileLoops, Autodesk.Revit.DB.Curve pathCurve, List<double> inputParameters)
        {
            var attachParams = new List<double>();

            if (inputParameters.Count < profileLoops.Count())
            {
                //intersect plane of each curve loop with the pathCurve
                double lastParam = pathCurve.ComputeRawParameter(0.0);
                foreach (Autodesk.Revit.DB.CurveLoop cLoop in profileLoops)
                {
                    Autodesk.Revit.DB.Plane planeOfCurveLoop = cLoop.GetPlane();
                    if (planeOfCurveLoop == null)
                        throw new Exception("Profile Curve Loop is not planar");
                    Autodesk.Revit.DB.Face face = BuildFaceOnPlaneByCurveExtensions(pathCurve, planeOfCurveLoop);
                    IntersectionResultArray xsects = null;
                    face.Intersect(pathCurve, out xsects);
                    if (xsects == null)
                        throw new Exception("Could not find attachment point on path curve");
                    if (xsects.Size > 1)
                    {
                        for (int indexInt = 0; indexInt < xsects.Size; indexInt++)
                        {
                            if (xsects.get_Item(indexInt).Parameter < lastParam + 0.0000001)
                                continue;
                            lastParam = xsects.get_Item(indexInt).Parameter;
                            break;
                        }
                    }
                    else
                        lastParam = xsects.get_Item(0).Parameter;
                    attachParams.Add(lastParam);
                }
            }
            else
            {
                foreach (double vPar in inputParameters)
                {
                    attachParams.Add(vPar);
                }
            }
            //check the parameter and set it if not right or not defined
            List<ICollection<Autodesk.Revit.DB.VertexPair>> vertPairs = null;

            var result = GeometryCreationUtilities.CreateSweptBlendGeometry(pathCurve, attachParams, profileLoops.ToList(), vertPairs);
            this.InternalSolid = result;
        }

        internal DSSolid(Autodesk.Revit.DB.Solid x)
        {
            this.InternalSolid = x;
        }

        /// <summary>
        /// Internal constructor to make a solid by boolean operation.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operationType"></param>
        internal DSSolid(Autodesk.Revit.DB.Solid a, Autodesk.Revit.DB.Solid b, BooleanOperationsType operationType)
        {
            Autodesk.Revit.DB.Solid result = null;

            switch (operationType)
            {
                case BooleanOperationsType.Difference:
                    result = BooleanOperationsUtils.ExecuteBooleanOperation(a, b, BooleanOperationsType.Difference);
                    break;
                case BooleanOperationsType.Intersect:
                    result = BooleanOperationsUtils.ExecuteBooleanOperation(a, b, BooleanOperationsType.Intersect);
                    break;
                case BooleanOperationsType.Union:
                    result = BooleanOperationsUtils.ExecuteBooleanOperation(a, b, BooleanOperationsType.Union);
                    break;
            }

            if (result == null)
            {
                throw new Exception("A boolean operation could not be completed with the provided solids.");
            }

            this.InternalSolid = result;
        }

        /// <summary>
        /// Internal constructor to make a solid by extracting solids from an element.
        /// </summary>
        /// <param name="element"></param>
        internal DSSolid(Element element)
        {
            var instanceSolids = new Dictionary<ElementId, List<GeometryObject>>();;
            Solid mySolid = null;

            var thisId = ElementId.InvalidElementId;

            if (element != null)
            {
                thisId = element.Id;
                instanceSolids[thisId] = new List<GeometryObject>();
            }

            bool bNotVisibleOption = false;
            if (element is GenericForm)
            {
                var gF = (GenericForm)element;
                if (!gF.Combinations.IsEmpty)
                    bNotVisibleOption = true;
            }
            int nTry = (bNotVisibleOption) ? 2 : 1;
            for (int iTry = 0; iTry < nTry && (mySolid == null); iTry++)
            {
                var geoOptions = new Autodesk.Revit.DB.Options();
                geoOptions.ComputeReferences = true;
                if (bNotVisibleOption && (iTry == 1))
                    geoOptions.IncludeNonVisibleObjects = true;

                GeometryObject geomObj = element.get_Geometry(geoOptions);
                var geomElement = geomObj as GeometryElement;

                if (geomElement != null)
                {
                    foreach (GeometryObject geob in geomElement)
                    {
                        var ginsta = geob as GeometryInstance;
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

            this.InternalSolid = mySolid;
        }
        


        #endregion

        #region Public properties

        /// <summary>
        /// The internal faces of the solid
        /// </summary>
        public DSFace[] Faces
        {
            get
            {
                return this.InternalSolid.Faces.Cast<Autodesk.Revit.DB.Face>()
                            .Select(DSFace.FromExisting)
                            .ToArray();
            }
        }

        /// <summary>
        /// The edges of the solid
        /// </summary>
        public DSEdge[] Edges
        {
            get
            {
                return this.InternalSolid.Edges.Cast<Autodesk.Revit.DB.Edge>()
                    .Select(DSEdge.FromExisting)
                    .ToArray();
            }
        }

        /// <summary>
        /// The total volume of this solid
        /// </summary>
        public double Volume
        {
            get
            {
                return this.InternalSolid.Volume;
            }
        }

        /// <summary>
        /// The total surface area of the solid
        /// </summary>
        public double SurfaceArea
        {
            get
            {
                return this.InternalSolid.SurfaceArea;
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create geometry by linearly extruding a closed curve
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static DSSolid ByExtrusion(Autodesk.DesignScript.Geometry.Curve[] closedProfileCurves, Vector direction, double distance)
        {
            if (closedProfileCurves == null)
            {
                throw new ArgumentNullException("closedProfileCurves");
            }

            if (direction == null)
            {
                throw new ArgumentNullException("direction");
            }

            var loop = new Autodesk.Revit.DB.CurveLoop();
            closedProfileCurves.ForEach(x => loop.Append( x.ToRevitType()));

            return new DSSolid(new List<CurveLoop>() { loop }, direction.ToXyz(), distance);
        }

        /// <summary>
        /// Create geometry by revolving a closed curve around an axis.
        /// </summary>
        /// <param name="profile">The profile to revolve.</param>
        /// <param name="coordinateSystem">The coordinate system whose Z axis will be the axis of revolution.</param>
        /// <param name="startAngle">The start angle in radians.</param>
        /// <param name="endAngle">The end angle in radians.</param>
        /// <returns></returns>
        public static DSSolid ByRevolve(List<Autodesk.DesignScript.Geometry.Curve> profile,  CoordinateSystem coordinateSystem, double startAngle, double endAngle )
        {
            if (profile == null)
            {
                throw new ArgumentException("profile");
            }

            if (coordinateSystem == null)
            {
                throw new ArgumentException("coordinate system");
            }

            var crvs = CurveLoop.Create(profile.Select(x => x.ToRevitType()).ToList());

            return new DSSolid( crvs, coordinateSystem.ToTransform(), startAngle, endAngle);
        }

        /// <summary>
        /// Create geometry by blending two profiles together.
        /// </summary>
        /// <param name="profiles">A list of lists of curves representing the profiles to blend.</param>
        /// <returns></returns>
        public static DSSolid ByBlend(List<List<Autodesk.DesignScript.Geometry.Curve>> profiles)
        {
            if (profiles == null)
            {
                throw new ArgumentException("profiles");
            }

            if (profiles.Count != 2)
            {
                throw new Exception("You must have two profiles to create a blend.");
            }

            var loops = profiles.Select(x => CurveLoop.Create(x.Select(y => y.ToRevitType()).ToList()));

            return new DSSolid(loops);
        }

        /// <summary>
        /// Create geometry by blending profiles along a path.
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="spine"></param>
        /// <returns></returns>
        public static DSSolid BySweptBlend(List<List<Autodesk.DesignScript.Geometry.Curve>> profiles, Autodesk.DesignScript.Geometry.Curve spine, List<double> attachmentParameters)
        {
            if (profiles == null)
            {
                throw new ArgumentException("profiles");
            }

            if (profiles.Count < 1)
            {
                throw new Exception("You must have more than one profile to create a swept blend.");
            }

            if (profiles.Count != attachmentParameters.Count)
            {
                throw new Exception("You must have the same number of profiles as attachment parameters.");
            }

            var loops = profiles.Select(x => CurveLoop.Create(x.Select(y => y.ToRevitType()).ToList()));

            return new DSSolid(loops, spine.ToRevitType(), attachmentParameters);
        }

        /// <summary>
        /// Create cylinder geometry by extruding a circle of a given radius, by a given height
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="radius"></param>
        /// <param name="direction"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static DSSolid Cylinder(Autodesk.DesignScript.Geometry.Point origin, double radius, Vector direction, double height)
        {
            if (radius <= 0)
            {
                throw new ArgumentException("Radius must be greater than zero.");
            }

            if (direction == null)
            {
                throw new ArgumentException("Direction can not be null.");
            }

            if (height <= 0)
            {
                throw new ArgumentException("Height must be greater than zero.");
            }

            var axis = direction.ToXyz();

            // get axis that is perp to axis by first generating random vector
            var zaxis = axis.Normalize();
            var randXyz = new XYZ(1, 0, 0);
            if (axis.IsAlmostEqualTo(randXyz)) randXyz = new XYZ(0, 1, 0);
            var yaxis = zaxis.CrossProduct(randXyz).Normalize();

            // get second axis that is perp to axis
            var xaxis = yaxis.CrossProduct(zaxis);

            // create circle (this is ridiculous, but curve loop doesn't work with a circle - you need two arcs)
            var arc1 = Ellipse.Create(origin.ToXyz(), radius, radius, xaxis, yaxis, 0, RevitPI);
            var arc2 = Ellipse.Create(origin.ToXyz(), radius, radius, xaxis, yaxis, RevitPI, 2 * RevitPI);

            // create curve loop from cirle
            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { arc1, arc2 });

            return new DSSolid(new List<CurveLoop>{circleLoop}, axis, height);
        }

        /// <summary>
        /// Create a sphere of a given radius at a given center point. 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static DSSolid Sphere(Autodesk.DesignScript.Geometry.Point center, double radius)
        {
            if (center == null)
            {
                throw new ArgumentException("Center point is null.");
            }

            if (radius <= 0)
            {
                throw new ArgumentException("Radius must be greater than zero.");
            }

            var origin = center.ToXyz();

            // create semicircular arc
            var semicircle = Autodesk.Revit.DB.Arc.Create(origin, radius, 0, RevitPI, XYZ.BasisZ, XYZ.BasisX);

            // create axis curve of sphere - running from north to south pole
            var axisCurve = Autodesk.Revit.DB.Line.CreateBound(new XYZ(origin.X, origin.Y, origin.Z-radius),
                new XYZ(origin.X, origin.Y, origin.Z + radius));

            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { semicircle, axisCurve });

            var trans = Transform.Identity;
            trans.Origin = origin;
            trans.BasisX = XYZ.BasisX;
            trans.BasisY = XYZ.BasisY;
            trans.BasisZ = XYZ.BasisZ;

            return new DSSolid(circleLoop, trans, 0, 2*RevitPI);
        }

        /// <summary>
        /// Create a torus aligned to an axis with a radius and a section radius.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="sectionRadius"></param>
        /// <returns></returns>
        public static DSSolid Torus(Vector axis, Point center, double radius, double sectionRadius)
        {
            if (center == null)
            {
                throw new ArgumentException("Center is null");
            }

            if (axis == null || axis.Length < 1e-6)
            {
                throw new ArgumentException("Your axis is null or is 0 length.");
            }

            if (radius <= 0)
            {
                throw new ArgumentException("The radius must be greater than zero.");
            }

            if (sectionRadius <= 0)
            {
                throw new ArgumentException("The section radius must be greater than zero.");
            }

            var revolveAxis = axis.ToXyz();

            // get axis that is perp to axis by first generating random vector
            var zaxis = revolveAxis.Normalize();
            var randXyz = new XYZ(1, 0, 0);
            if (zaxis.IsAlmostEqualTo(randXyz)) randXyz = new XYZ(0, 1, 0);
            var yaxis = zaxis.CrossProduct(randXyz).Normalize();

            // get second axis that is perp to axis
            var xaxis = yaxis.CrossProduct(zaxis);

            // form origin of the arc
            var origin = center.ToXyz() + xaxis * radius;

            // create circle (this is ridiculous but curve loop doesn't work with a circle
            var arc1 = Ellipse.Create(origin, sectionRadius, sectionRadius, xaxis, zaxis, 0, RevitPI);
            var arc2 = Ellipse.Create(origin, sectionRadius, sectionRadius, xaxis, zaxis, RevitPI, 2 * RevitPI);

            // create curve loop from cirle
            var circleLoop = CurveLoop.Create(new List<Curve>() { arc1, arc2 });

            var trans = Transform.Identity;
            trans.Origin = center.ToXyz();
            trans.BasisX = xaxis;
            trans.BasisY = yaxis;
            trans.BasisZ = zaxis;

            return new DSSolid(circleLoop, trans, 0, 2*RevitPI);
        }

        /// <summary>
        /// Create a box by minimum and maximum points.
        /// </summary>
        /// <returns></returns>
        public static DSSolid BoxByTwoCorners(Point minimum, Point maximum)
        {
            if ((maximum.Z-minimum.Z)<1e-6)
            {
                throw new ArgumentException("The minimum and maximum points specify a box with zero height.");
            }

            var bottomInput = minimum.ToXyz();
            var topInput = maximum.ToXyz();

            XYZ top, bottom;
            if (bottomInput.Z > topInput.Z)
            {
                top = bottomInput;
                bottom = topInput;
            }
            else
            {
                top = topInput;
                bottom = bottomInput;
            }

            // obtain coordinates of base rectangle
            var p0 = bottom;
            var p1 = p0 + new XYZ(top.X - bottom.X, 0, 0);
            var p2 = p1 + new XYZ(0, top.Y - bottom.Y, 0);
            var p3 = p2 - new XYZ(top.X - bottom.X, 0, 0);

            // form edges of base rect
            var l1 = Autodesk.Revit.DB.Line.CreateBound(p0, p1);
            var l2 = Autodesk.Revit.DB.Line.CreateBound(p1, p2);
            var l3 = Autodesk.Revit.DB.Line.CreateBound(p2, p3);
            var l4 = Autodesk.Revit.DB.Line.CreateBound(p3, p0);

            // form curve loop from lines of base rect
            var cl = new Autodesk.Revit.DB.CurveLoop();
            cl.Append(l1);
            cl.Append(l2);
            cl.Append(l3);
            cl.Append(l4);

            // get height of box
            var height = top.Z - bottom.Z;

            return new DSSolid(new List<CurveLoop>{ cl },XYZ.BasisZ,height);
        }

        /// <summary>
        /// Create a box by center and dimensions.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static DSSolid BoxByCenterAndDimensions(Point center,double x, double y, double z)
        {
            var bottom = center.ToXyz() - new XYZ(x / 2, y / 2, z / 2);
            var top = center.ToXyz() + new XYZ(x / 2, y / 2, z / 2);

            return BoxByTwoCorners(bottom.ToPoint(), top.ToPoint());
        }

        /// <summary>
        /// Create a solid by the boolean difference of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DSSolid ByBooleanDifference(DSSolid a, DSSolid b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentException();
            }

            return new DSSolid(a.InternalSolid, b.InternalSolid, BooleanOperationsType.Difference);
        }

        /// <summary>
        /// Create a solid by the boolean union of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DSSolid ByBooleanUnion(DSSolid a, DSSolid b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentException();
            }

            return new DSSolid(a.InternalSolid, b.InternalSolid, BooleanOperationsType.Union);
        }

        /// <summary>
        /// Create a solid by the boolean intersection of two solids.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DSSolid ByBooleanIntersection(DSSolid a, DSSolid b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentException();
            }

            return new DSSolid(a.InternalSolid, b.InternalSolid, BooleanOperationsType.Intersect);
        }

        /// <summary>
        /// Create a solid by extracting solids from an element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static DSSolid FromElement(AbstractElement element)
        {
            if (element == null)
            {
                throw new ArgumentException("element");
            }

            return new DSSolid(element.InternalElement);
        }

        #endregion

        #region Tesselation

        public override void Tessellate(IRenderPackage package)
        {
            var meshes = this.InternalSolid.Faces.Cast<Autodesk.Revit.DB.Face>()
                .Select(x => x.Triangulate(GraphicsManager.TesselationLevelOfDetail));

            foreach (var mesh in meshes)
            {
                for (var i = 0; i < mesh.NumTriangles; i++)
                {
                    var triangle = mesh.get_Triangle(i);
                    for (var j = 0; j < 3; j++)
                    {
                        var xyz = triangle.get_Vertex(j);
                        package.PushTriangleVertex(xyz.X, xyz.Y, xyz.Z);
                    }
                }
            }

        }

        #endregion

        #region helper methods

        private static Autodesk.Revit.DB.Face BuildFaceOnPlaneByCurveExtensions(Autodesk.Revit.DB.Curve crv, Autodesk.Revit.DB.Plane thisPlane)
        {
            Autodesk.Revit.DB.Face face = null;
            // tesselate curve and find uv envelope in projection to the plane
            IList<XYZ> tessCurve = crv.Tessellate();
            var curvePointEnum = tessCurve.GetEnumerator();
            var corner1 = new XYZ();
            var corner2 = new XYZ();
            bool cornersSet = false;
            for (; curvePointEnum.MoveNext(); )
            {
                if (!cornersSet)
                {
                    corner1 = curvePointEnum.Current;
                    corner2 = curvePointEnum.Current;
                    cornersSet = true;
                }
                else
                {
                    for (int coord = 0; coord < 3; coord++)
                    {
                        if (corner1[coord] > curvePointEnum.Current[coord])
                            corner1 = new XYZ(coord == 0 ? curvePointEnum.Current[coord] : corner1[coord],
                                            coord == 1 ? curvePointEnum.Current[coord] : corner1[coord],
                                            coord == 2 ? curvePointEnum.Current[coord] : corner1[coord]);
                        if (corner2[coord] < curvePointEnum.Current[coord])
                            corner2 = new XYZ(coord == 0 ? curvePointEnum.Current[coord] : corner2[coord],
                                           coord == 1 ? curvePointEnum.Current[coord] : corner2[coord],
                                           coord == 2 ? curvePointEnum.Current[coord] : corner2[coord]);
                    }
                }
            }

            double dist1 = thisPlane.Origin.DistanceTo(corner1);
            double dist2 = thisPlane.Origin.DistanceTo(corner2);
            double sizeRect = 2.0 * (dist1 + dist2) + 100.0;

            var cLoop = new Autodesk.Revit.DB.CurveLoop();
            for (int index = 0; index < 4; index++)
            {
                double coord0 = (index == 0 || index == 3) ? -sizeRect : sizeRect;
                double coord1 = (index < 2) ? -sizeRect : sizeRect;
                XYZ pnt0 = thisPlane.Origin + coord0 * thisPlane.XVec + coord1 * thisPlane.YVec;

                double coord3 = (index < 2) ? sizeRect : -sizeRect;
                double coord4 = (index == 0 || index == 3) ? -sizeRect : sizeRect;
                XYZ pnt1 = thisPlane.Origin + coord3 * thisPlane.XVec + coord4 * thisPlane.YVec;
                //Line cLine = DocumentManager.GetInstance().CurrentUIApplication.Application.Create.NewLineBound(pnt0, pnt1);
                var cLine = Autodesk.Revit.DB.Line.CreateBound(pnt0, pnt1);
                cLoop.Append(cLine);
            }
            var listCLoops = new List<Autodesk.Revit.DB.CurveLoop> { cLoop };

            var tempSolid = GeometryCreationUtilities.CreateExtrusionGeometry(listCLoops, thisPlane.Normal, 100.0);

            //find right face

            var facesOfExtrusion = tempSolid.Faces;
            for (int indexFace = 0; indexFace < facesOfExtrusion.Size; indexFace++)
            {
                var faceAtIndex = facesOfExtrusion.get_Item(indexFace);
                if (faceAtIndex is PlanarFace)
                {
                    var pFace = faceAtIndex as PlanarFace;
                    if (System.Math.Abs(thisPlane.Normal.DotProduct(pFace.Normal)) < 0.99)
                        continue;
                    if (System.Math.Abs(thisPlane.Normal.DotProduct(thisPlane.Origin - pFace.Origin)) > 0.1)
                        continue;
                    face = faceAtIndex;
                    break;
                }
            }
            if (face == null)
                throw new Exception("Curve Face Intersection could not process supplied Plane.");

            return face;
        }

        #endregion
    }
}

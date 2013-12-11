using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using DSRevitNodes.Graphics;
using Curve = Autodesk.Revit.DB.Curve;

namespace DSRevitNodes.Elements
{
    public class DSSolid : IGeometryObject
    {
        private Autodesk.Revit.DB.Solid x;
        private const double RevitPI = 3.14159265358979;

        internal Autodesk.Revit.DB.Solid InternalSolid
        {
            get; private set;
        }

        #region Internal constructors

        /// <summary>
        /// Internal constructor making a solid by extrusion
        /// </summary>
        /// <param name="loops"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        internal DSSolid(CurveLoop loop, XYZ direction, double distance )
        {
            var result = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>(){loop}, direction, distance);
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
            // TODO: Complete member initialization
            this.x = x;
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
                            .Select(x => new DSFace(x))
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
                    .Select(x => new DSEdge(x))
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
        public static DSSolid ByExtrusion(DSCurveLoop profile, Vector direction, double distance)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (direction == null)
            {
                throw new ArgumentNullException("direction");
            }

            return new DSSolid(profile.InternalCurveLoop, direction.ToXyz(), distance);
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

            return new DSSolid(circleLoop, axis, height);
        }

        /// <summary>
        /// Create sphere geometry of a given radius at a given center point. 
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
            var axisCurve = Autodesk.Revit.DB.Line.CreateBound(new XYZ(0, 0, -radius),
                new XYZ(0, 0, radius));

            var circleLoop = Autodesk.Revit.DB.CurveLoop.Create(new List<Curve>() { semicircle, axisCurve });

            var trans = Transform.Identity;
            trans.Origin = origin;
            trans.BasisX = XYZ.BasisX;
            trans.BasisY = XYZ.BasisY;
            trans.BasisZ = XYZ.BasisZ;

            return new DSSolid(circleLoop, trans, 0, 2*RevitPI);
        }

        public static DSSolid Torus()
        {
            throw new NotImplementedException();
        }

        public static DSSolid BoxByTwoCorners()
        {
            throw new NotImplementedException();
        }

        public static DSSolid BoxByCenterAndDimensions()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Tesselation

        public void Tessellate(IRenderPackage package)
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
                //Line cLine = dynRevitSettings.Revit.Application.Create.NewLineBound(pnt0, pnt1);
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
                    if (Math.Abs(thisPlane.Normal.DotProduct(pFace.Normal)) < 0.99)
                        continue;
                    if (Math.Abs(thisPlane.Normal.DotProduct(thisPlane.Origin - pFace.Origin)) > 0.1)
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

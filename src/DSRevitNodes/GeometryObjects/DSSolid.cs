using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using DSRevitNodes.Graphics;

namespace DSRevitNodes.Elements
{
    public class DSSolid : IGeometryObject
    {
        private Autodesk.Revit.DB.Solid x;

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

        public static DSSolid BySweptBlend(List<List<DSCurve>> profiles, DSCurve spine)
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

    }
}

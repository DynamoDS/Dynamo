using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using DSRevitNodes.Graphics;

namespace DSRevitNodes
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

        static DSSolid ByRevolve(List<DSCurve> profile, Vector axis )
        {
            throw new NotImplementedException();
        }

        static DSSolid ByBlend(List<List<DSCurve>> profiles)
        {
            throw new NotImplementedException();
        }

        static DSSolid BySweptBlend(List<List<DSCurve>> profiles, DSCurve spine)
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
                    for (var j = 0; j < 3; j++)
                    {
                        var xyz = mesh.get_Triangle(i).get_Vertex(i);
                        package.PushTriangleVertex(xyz.X, xyz.Y, xyz.Z);
                    }
                }
            }

        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryObjects;

namespace DSRevitNodes
{
    public class DSSolid
    {
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

        #endregion

        #region Public properties

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

    }
}

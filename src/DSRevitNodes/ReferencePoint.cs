using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Revit
{
    public class ReferencePoint
    {
        /// <summary>
        /// Create a reference point by x,y, and z coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        static ReferencePoint ByCoordinates(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a reference point from a point.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static ReferencePoint ByPoint(Point p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a reference point by UV coordinates on a face.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        static ReferencePoint ByPointOnFace(Face f, Vector v)
        {
            throw new NotImplementedException();
        }
    }
}

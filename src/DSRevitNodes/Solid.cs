using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Revit
{
    class Solid
    {
        static Solid ByExtrusion(List<Curve> profile, Vector direction)
        {
            throw new NotImplementedException();
        }

        static Solid ByRevolve(List<Curve> profile, Vector axis )
        {
            throw new NotImplementedException();
        }

        static Solid ByBlend(List<List<Curve>> profiles)
        {
            throw new NotImplementedException();
        }

        static Solid BySweptBlend(List<List<Curve>> profiles, Curve spine)
        {
            throw new NotImplementedException();
        }
    }
}

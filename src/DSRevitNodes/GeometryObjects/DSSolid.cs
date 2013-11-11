using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace DSRevitNodes
{
    class DSSolid
    {
        static DSSolid ByExtrusion(List<DSCurve> profile, Vector direction)
        {
            throw new NotImplementedException();
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
    }
}

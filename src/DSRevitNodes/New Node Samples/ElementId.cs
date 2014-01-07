using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRevitNodes
{
    class ElementId
    {
        //Convenience wrapper for calling the ElementId constructor.
        //TODO: Replace with direct constructor call.
        public static Autodesk.Revit.DB.ElementId ElementIdByInt(int i)
        {
            return new Autodesk.Revit.DB.ElementId(i);
        }
    }
}

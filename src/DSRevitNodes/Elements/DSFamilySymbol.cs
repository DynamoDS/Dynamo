using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRevitNodes
{
    class DSFamilySymbol
    {
        public Autodesk.Revit.DB.FamilySymbol InternalFamilySymbol
        {
            get;
            private set;
        }

    }
}

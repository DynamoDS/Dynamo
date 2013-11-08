using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRevitNodes.Elements
{
    public class DSFamily : AbstractElement
    {
        internal Autodesk.Revit.DB.Family InternalFamily
        {
            get; private set;
        }

        internal DSFamily(Autodesk.Revit.DB.Family family)
        {
            InternalSetFamily(family);
        }

        private void InternalSetFamily(Autodesk.Revit.DB.Family family)
        {
            this.InternalFamily = family;
            this.InternalId = family.Id;
            this.InternalUniqueId = family.UniqueId;
        }
    }
}

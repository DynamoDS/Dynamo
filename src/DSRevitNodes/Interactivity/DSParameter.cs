using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRevitNodes.Elements
{
    public class DSParameter
    {

        private Autodesk.Revit.DB.Parameter InternalParameter
        { 
            get; set;
        }

        internal DSParameter(Autodesk.Revit.DB.Parameter internalParameter)
        {
            this.InternalParameter = internalParameter;
        }

        public string Name
        {
            get
            {
                return InternalParameter.Definition.Name;
            }
        }

    }
}

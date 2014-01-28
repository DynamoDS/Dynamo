using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revit.Elements
{
    public class Parameter
    {

        private Autodesk.Revit.DB.Parameter InternalParameter
        { 
            get; set;
        }

        internal Parameter(Autodesk.Revit.DB.Parameter internalParameter)
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

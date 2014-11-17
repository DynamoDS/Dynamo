using Autodesk.Revit.DB;

namespace Revit.Elements
{
    public class Parameter
    {
        internal Autodesk.Revit.DB.Parameter InternalParameter
        { 
            get; set;
        }

        internal Parameter(Autodesk.Revit.DB.Parameter internalParameter)
        {
            this.InternalParameter = internalParameter;
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name
        {
            get
            {
                return InternalParameter.Definition.Name;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Name, InternalParameter.AsValueString(new FormatOptions(){}));
        }
    }
}

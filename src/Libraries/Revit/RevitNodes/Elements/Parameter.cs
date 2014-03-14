using System;
using Autodesk.Revit.DB;

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

        /// <summary>
        /// The value of the parameter
        /// </summary>
        public object Value
        {
            get
            {
                switch (InternalParameter.StorageType)
                {
                    case StorageType.ElementId:
                        return InternalParameter.AsElementId();
                    case StorageType.String:
                        return InternalParameter.AsString();
                    case StorageType.Integer:
                    case StorageType.Double:
                        switch (InternalParameter.Definition.ParameterType)
                        {
                            case ParameterType.Length:
                                return Dynamo.Units.Length.FromFeet(InternalParameter.AsDouble());
                            case ParameterType.Area:
                                return Dynamo.Units.Area.FromSquareFeet(InternalParameter.AsDouble());
                            case ParameterType.Volume:
                                return Dynamo.Units.Volume.FromCubicFeet(InternalParameter.AsDouble());
                            default:
                                return InternalParameter.AsDouble();
                        }
                    default:
                        throw new Exception(string.Format("Parameter {0} has no storage type.", InternalParameter));
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Name, Value);
        }
    }
}

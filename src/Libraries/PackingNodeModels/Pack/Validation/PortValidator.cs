using Dynamo.Graph.Nodes;
using PackingNodeModels.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingNodeModels.Pack.Validation
{
    /// <summary>
    /// Used to validate that a given value matches the type defined by the PortModel+PropertyType combination of a Pack node InPort.
    /// Comes with a basic mapping of known types to Dynamo Types. Assumes that an unknown type is coming from another Pack node.
    /// </summary>
    internal class PortValidator
    {
        /// <summary>
        /// "Primitive" known to Pack node.
        /// </summary>
        private static Dictionary<string, List<Type>> CompatibleTypes = new Dictionary<string, List<Type>>
        {
            { "Bool", new List<Type>() { typeof(bool), typeof(Boolean) } },
            { "Int32", new List<Type>() { typeof(int), typeof(Int32) } },
            { "String", new List<Type>() { typeof(string) } },
            { "Float64", new List<Type>() { typeof(float), typeof(double), typeof(Int32), typeof(Int64) } },
            { "Arc", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Arc) } },
            { "Circle", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Circle) } },
            { "CoordinateSystem", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.CoordinateSystem) } },
            { "Cone", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Cone) } },
            { "Curve", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Curve) } },
            { "Cuboid", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Cuboid) } },
            { "Cylinder", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Cylinder) } },
            { "Ellipse", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Ellipse) } },
            { "Line", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Line) } },
            { "Plane", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Plane) } },
            { "Point", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Point) } },
            { "PolyCurve", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.PolyCurve) } },
            { "Polygon", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Polygon) } },
            { "Rectangle", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Rectangle) } },
            { "Sphere", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Sphere) } },
            { "Vector", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Vector) } },
            { "Surface", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Surface) } },
            { "Geometry", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Geometry) } } //TODO SolidDef or Geometry?
        };

        /// <summary>
        /// Validates a value against its port's associated property.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="portModel"></param>
        /// <returns></returns>
        public static string Validate(KeyValuePair<string,PropertyType> property, object value, PortModel portModel)
        {
            if (value == null)
            {
                return string.Format(Resource.PortValidatorNullValueMessage, property.Key, property.Value.Type);
            }

            if (property.Value.IsCollection)
            {
                return ValidateCollection(property, value, portModel);
            }

            return ValidateSingleValue(property, value, portModel);
        }

        private static string ValidateSingleValue(KeyValuePair<string, PropertyType> property, object value, PortModel portModel)
        {
            if (value is ArrayList arrayList)
            {
                return ValidateSingleValueLacing(property, arrayList, portModel);
            }

            return ValidateTypeMatch(property, value, portModel);
        }

        private static string ValidateSingleValueLacing(KeyValuePair<string, PropertyType> property, ArrayList values, PortModel portModel)
        {
            if (ContainsCollections(values))
            {
                return string.Format(Resource.PortValidatorArrayInsteadOfSingleValueMessage, property.Key, property.Value.Type);
            }

            return ValidateValues(property, values, portModel);
        }

        private static string ValidateCollection(KeyValuePair<string, PropertyType> property, object value, PortModel portModel)
        {
            if (!(value is ArrayList))
            {
                return string.Format(Resource.PortValidatorSingleValueInsteadOfArrayMessage, property.Key, property.Value.Type);
            }

            var values = value as ArrayList;

            if (IsMixedCombinationOfCollectionAndSingleValues(values))
            {
                return string.Format(Resource.PortValidatorMixedCombinationMessage, property.Key, property.Value.Type);
            }

            if (ContainsCollections(values))
            {
                return ValidateArrayLacing(property, values, portModel);
            }

            return ValidateValues(property, values, portModel);
        }

        private static string ValidateArrayLacing(KeyValuePair<string, PropertyType> property, ArrayList values, PortModel portModel)
        {
            foreach (var subArray in values)
            {
                foreach (var subValue in subArray as ArrayList)
                {
                    //Deeper arrays not allowed
                    if (subValue is ArrayList)
                    {
                        return string.Format(Resource.PortValidatorNestedArrayMessage, property.Key, property.Value.Type);
                    }
                    else
                    {
                        var validation = ValidateTypeMatch(property, subValue, portModel);
                        if (!string.IsNullOrEmpty(validation)) return validation;
                    }
                        
                }
            }

            return null;
        }

        public static string ValidateTypeMatch(KeyValuePair<string, PropertyType> property, object value, PortModel portModel)
        {
            if (IsKnownType(property.Value.Type))
            {
                if (!IsTypeMatch(value, property.Value.Type))
                {
                    return string.Format(Resource.PortValidatorWrongTypeMessage, property.Key, property.Value.Type, value?.GetType());
                }

                return null;
            }

            return ValidateUnknownType(property, value, portModel);
        }

        private static string ValidateValues(KeyValuePair<string, PropertyType> property, ArrayList values, PortModel portModel)
        {
            foreach (var value in values)
            {
                var validation = ValidateTypeMatch(property, value, portModel);

                if (!string.IsNullOrEmpty(validation)) return validation;
            }

            return null;
        }

        private static string ValidateUnknownType(KeyValuePair<string, PropertyType> property, object value, PortModel portModel)
        {
            //Assume and expect that an unkwown type comes from another Pack
            var owner = portModel.Connectors[0].Start.Owner as Pack;
            if (owner == null || !property.Value.Type.Equals(owner.TypeDefinition.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Format(Resource.PortValidatorWrongTypeMessage, property.Key, property.Value.Type, owner?.TypeDefinition.Name ?? value?.GetType().ToString());
            }

            return null;
        }

        private static bool ContainsCollections(ArrayList values)
        {
            return values.Cast<object>().Any(x => x is ArrayList);
        }

        private static bool IsMixedCombinationOfCollectionAndSingleValues(ArrayList values)
        {
            return values.Cast<object>().Select(x => x is ArrayList).Distinct().Count() > 1;
        }

        private static bool IsKnownType(string type)
        {
            return CompatibleTypes.ContainsKey(type);
        }

        private static bool IsTypeMatch(object value, string expectedType)
        {
            if (!CompatibleTypes.ContainsKey(expectedType)) return true;

            return CompatibleTypes[expectedType].Exists(x => x == value?.GetType());
        }
    }
}

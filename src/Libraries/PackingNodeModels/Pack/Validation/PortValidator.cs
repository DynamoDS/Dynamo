using Dynamo.Graph.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingNodeModels.Pack.Validation
{
    public class PortValidator
    {
        private static Dictionary<string, List<Type>> CompatibleTypes = new Dictionary<string, List<Type>>
        {
            { "String", new List<Type>() { typeof(string) } },
            { "Float64", new List<Type>() { typeof(float), typeof(double), typeof(Int32), typeof(Int64) } },
            { "autodesk.aec.primitive:arc-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Arc) } },
            { "autodesk.aec.primitive:circle-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Circle) } },
            { "autodesk.aec.primitive:coordinatesystem-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.CoordinateSystem) } },
            { "autodesk.aec.primitive:cone-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Cone) } },
            { "autodesk.aec.primitive:curve-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Curve) } },
            { "autodesk.aec.primitive:cuboid-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Cuboid) } },
            { "autodesk.aec.primitive:cylinder-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Cylinder) } },
            { "autodesk.aec.primitive:ellipse-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Ellipse) } },
            { "autodesk.aec.primitive:line-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Line) } },
            { "autodesk.aec.primitive:plane-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Plane) } },
            { "autodesk.aec.primitive:point-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Point) } },
            { "autodesk.aec.primitive:polycurve-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.PolyCurve) } },
            { "autodesk.aec.primitive:polygon-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Polygon) } },
            { "autodesk.aec.primitive:rectangle-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Rectangle) } },
            { "autodesk.aec.primitive:sphere-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Sphere) } },
            { "autodesk.aec.primitive:vector-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Vector) } },
            { "autodesk.aec.primitive:surface-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Surface) } },
            { "autodesk.soliddef:model-1.0.0", new List<Type>() { typeof(Autodesk.DesignScript.Geometry.Geometry) } } //TODO double check soliddef's type.
        };

        public static string Validate(KeyValuePair<string,PropertyType> property, object value, PortModel portModel)
        {
            if (value == null)
                return $"Input {property.Key} expected type {property.Value.Type} but received null.";

            if (property.Value.IsCollection)
                return ValidateCollection(property, value, portModel);

            return ValidateSingleValue(property, value, portModel);
        }

        private static string ValidateSingleValue(KeyValuePair<string, PropertyType> property, object value, PortModel portModel)
        {
            if (value is ArrayList arrayList)
                return ValidateSingleValueLacing(property, arrayList, portModel);

            return ValidateTypeMatch(property, value, portModel);
        }

        private static string ValidateSingleValueLacing(KeyValuePair<string, PropertyType> property, ArrayList values, PortModel portModel)
        {
            if (ContainsCollections(values))
                return $"Input {property.Key} expected a single value of type {property.Value.Type} but received a list of values.";

            return ValidateValues(property, values, portModel);
        }

        private static string ValidateCollection(KeyValuePair<string, PropertyType> property, object value, PortModel portModel)
        {
            if (!(value is ArrayList))
                return $"Input {property.Key} expected an array of type {property.Value.Type} but received a single value.";

            var values = value as ArrayList;

            if (IsMixedCombinationOfCollectionAndSingleValues(values))
                return $"Input {property.Key} expected an array of type {property.Value.Type} but received a mixed combination of single values and arrays.";

            if (ContainsCollections(values))
                return ValidateArrayLacing(property, values, portModel);

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
                        return $"Input {property.Key} expected an array of type {property.Value.Type} but received a nested array.";
                    else
                    {
                        var validation = ValidateTypeMatch(property, subValue, portModel);
                        if (!string.IsNullOrEmpty(validation))
                            return validation;
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
                    return $"Input {property.Key} expected type {property.Value.Type} but received {value?.GetType()}.";

                return null;
            }

            return ValidateUnknownType(property, value, portModel);
        }

        private static string ValidateValues(KeyValuePair<string, PropertyType> property, ArrayList values, PortModel portModel)
        {
            foreach (var value in values)
            {
                var validation = ValidateTypeMatch(property, value, portModel);

                if (!string.IsNullOrEmpty(validation))
                    return validation;
            }

            return null;
        }

        private static string ValidateUnknownType(KeyValuePair<string, PropertyType> property, object value, PortModel portModel)
        {
            //Assume and expect that an unkwown type comes from another Pack
            var owner = portModel.Connectors[0].Start.Owner as Pack;
            if (owner == null || !property.Value.Type.Equals(owner.TypeDefinition.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return $"Input {property.Key} expected type {property.Value.Type} but received {owner?.TypeDefinition.Name ?? value?.GetType().ToString()}.";
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
            if (!CompatibleTypes.ContainsKey(expectedType))
                return true;

            return CompatibleTypes[expectedType].Exists(x => x == value?.GetType());
        }
    }
}

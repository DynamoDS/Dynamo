using System;
using System.Linq;
using Dynamo.Core;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    public static class Utilities
    {
        public static string Ellipsis(string value, int desiredLength)
        {
            if (desiredLength > value.Length)
            {
                return value;
            }
            else
            {
                return value.Remove(desiredLength - 1) + "...";
            }
        }

        /// <summary>
        /// <para>This method patches the fullyQualifiedName of a given type. It 
        /// updates the given name to its newer form (i.e. "Dynamo.Nodes.Xyz")
        /// if it matches the older form (e.g. "Dynamo.Elements.Xyz").</para>
        /// <para>The method also attempts to update "XYZ/UV" convention to 
        /// "Xyz/Uv" to comply with the new Dynamo naming convention.</para>
        /// </summary>
        /// <param name="fullyQualifiedName">A fully qualified name. An example
        /// of this would be "Dynamo.Elements.dynNode".</param>
        /// <returns>The processed fully qualified name. For an example, the 
        /// name "Dynamo.Elements.UV" will be returned as "Dynamo.Nodes.Uv".
        /// </returns>
        public static string PreprocessTypeName(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
                throw new ArgumentNullException("fullyQualifiedName");

            // older files will have nodes in the Dynamo.Elements namespace
            string oldPrefix = "Dynamo.Elements.";
            string newPrefix = "Dynamo.Nodes.";
            string className = string.Empty;

            // Attempt to extract the class name out of the fully qualified 
            // name, regardless of whether it is in the form of the older 
            // "Dynamo.Elements.XxxYyy" or the newer "Dynamo.Nodes.XxxYyy".
            // 
            if (fullyQualifiedName.StartsWith(oldPrefix))
                className = fullyQualifiedName.Substring(oldPrefix.Length);
            else if (fullyQualifiedName.StartsWith(newPrefix))
                className = fullyQualifiedName.Substring(newPrefix.Length);
            else
            {
                // We are only expected to process names of our built-in types,
                // and if we're given any of the system types, then we'll just
                // return them as-is without any patches.
                // 
                return fullyQualifiedName;
            }

            // Remove prefix of 'dyn' from older files.
            if (className.StartsWith("dyn"))
                className = className.Remove(0, 3);

            // Older files will have nodes that use "XYZ" and "UV" 
            // instead of "Xyz" and "Uv". Update these names.
            className = className.Replace("XYZ", "Xyz");
            className = className.Replace("UV", "Uv");
            return newPrefix + className; // Always new prefix from now on.
        }

        /// <summary>
        /// <para>Resolve either a built-in type or a system type, given its fully
        /// qualified name. This method performs the search with the following 
        /// order:</para>
        /// <para>1. Search among the built-in types registered with 
        /// DynamoController.BuiltInTypesByName dictionary</para>
        /// <para>2. Search among the available .NET runtime types</para>
        /// <para>3. Search among built-in types, taking their "also-known-as" 
        /// attributes into consideration when matching the type name</para>
        /// </summary>
        /// <param name="fullyQualifiedName"></param>
        /// <returns></returns>
        public static Type ResolveType(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
                throw new ArgumentNullException("fullyQualifiedName");

            TypeLoadData tData = null;
            var builtInTypes = DynamoSettings.Controller.BuiltInTypesByName;
            if (builtInTypes.TryGetValue(fullyQualifiedName, out tData))
                return tData.Type; // Found among built-in types, return it.

            //try and get a system type by this name
            Type type = Type.GetType(fullyQualifiedName);
            if (null != type)
                return type;

            // If we still can't find the type, try the also known as attributes.
            foreach (var builtInType in DynamoSettings.Controller.BuiltInTypesByName)
            {
                var attribs = builtInType.Value.Type.GetCustomAttributes(
                    typeof(AlsoKnownAsAttribute), false);

                if (!attribs.Any())
                    continue;

                AlsoKnownAsAttribute akaAttrib = attribs[0] as AlsoKnownAsAttribute;
                if (akaAttrib.Values.Contains(fullyQualifiedName))
                {
                    DynamoLogger.Instance.Log(string.Format(
                        "Found matching node for {0} also known as {1}",
                        builtInType.Key, fullyQualifiedName));

                    return builtInType.Value.Type; // Found a matching type.
                }
            }

            DynamoLogger.Instance.Log(string.Format(
                "Could not load node of type: {0}", fullyQualifiedName));

            DynamoLogger.Instance.Log("Loading will continue but nodes " +
                                      "might be missing from your workflow.");

            return null;
        }
    }
}
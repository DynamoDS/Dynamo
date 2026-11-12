using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Dynamo.Engine;
using Newtonsoft.Json;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Contains methods for serializing a WorkspaceModel to json.
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Save a Workspace to json.
        /// </summary>
        /// <returns>A string representing the serialized WorkspaceModel.</returns>
        internal static string ToJson(this WorkspaceModel workspace, EngineController engine)
        {
            var logger = engine != null ? engine.AsLogger() : null;

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;
                    Console.WriteLine(args.ErrorContext.Error);
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Culture = CultureInfo.InvariantCulture,
                Converters = new List<JsonConverter>{
                        new ConnectorConverter(logger),                        
                        new WorkspaceWriteConverter(engine),
                        new DummyNodeWriteConverter(),
                        new TypedParameterConverter(),
                        new NodeLibraryDependencyConverter(logger),
                        new LinterManagerConverter(logger)
                    },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var json = JsonConvert.SerializeObject(workspace, settings);
            var result = ReplaceTypeDeclarations(json);

            result = SerializeIntegerSliderAs32BitType(result);

            return result;
        }

        // Match a type declaration only where it is a JSON property name, that is a quoted name
        // followed by a colon. The quotes may be escaped, so that property names of json stored
        // inside a string value (for example the "$typeid" of cached geometry) keep being swapped
        // exactly as before, which graphs saved by earlier versions rely on.
        // Any other occurrence inside a string value is left alone.
        private const RegexOptions typeDeclarationRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static readonly Regex concreteTypePropertyRegex =
            new Regex(@"(?<!\\)(?<quote>\\*"")ConcreteType(?<suffix>(?:id)?)\k<quote>(?=\s*:)", typeDeclarationRegexOptions);
        private static readonly Regex typePropertyRegex =
            new Regex(@"(?<!\\)(?<quote>\\*"")\$type(?<suffix>(?:id)?)\k<quote>(?=\s*:)", typeDeclarationRegexOptions);

        /// <summary>
        /// Strips $type references from the generated json, replacing them with 
        /// type names matching those expected by the server.
        /// Only property names are replaced, including those of json stored inside a string value.
        /// Other text in string values is preserved.
        /// </summary>
        /// <param name="json">The json to parse.</param>
        /// <param name="fromServer">A flag indicating whether this json is coming from the server, and thus
        /// needs to be converted back to its Json.net friendly format.</param>
        /// <returns></returns>
        internal static string ReplaceTypeDeclarations(string json, bool fromServer = false)
        {
            return fromServer
                ? concreteTypePropertyRegex.Replace(json, "${quote}$$type${suffix}${quote}")
                : typePropertyRegex.Replace(json, "${quote}ConcreteType${suffix}${quote}");
        }

        [Obsolete("Remove method after obsoleting IntegerSlider and replacing it with IntegerSlider64Bit")]
        internal static string DeserializeIntegerSliderTo64BitType(string json)
        {
            var result = json;

            var rgx2 = new Regex(@"\bCoreNodeModels.Input.IntegerSlider\b");

            return rgx2.Replace(result, "CoreNodeModels.Input.IntegerSlider64Bit");
        }

        [Obsolete("Remove method after obsoleting IntegerSlider and replacing it with IntegerSlider64Bit")]
        internal static string SerializeIntegerSliderAs32BitType(string json)
        {
            var result = json;

            var rgx2 = new Regex(@"\bCoreNodeModels.Input.IntegerSlider64Bit\b");

            return rgx2.Replace(result, "CoreNodeModels.Input.IntegerSlider");
        }        
    }
}

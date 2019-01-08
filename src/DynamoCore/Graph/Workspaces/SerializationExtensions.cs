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
                        new TypedParameterConverter()
                    },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var json = JsonConvert.SerializeObject(workspace, settings);
            var result = ReplaceTypeDeclarations(json);

            return result;
        }

        /// <summary>
        /// Strips $type references from the generated json, replacing them with 
        /// type names matching those expected by the server.
        /// </summary>
        /// <param name="json">The json to parse.</param>
        /// <param name="fromServer">A flag indicating whether this json is coming from the server, and thus
        /// needs to be converted back to its Json.net friendly format.</param>
        /// <returns></returns>
        internal static string ReplaceTypeDeclarations(string json, bool fromServer = false)
        {
            var result = json;

            if (fromServer)
            {
                var rgx2 = new Regex(@"ConcreteType");
                result = rgx2.Replace(result, "$type");
            }
            else
            {
                var rgx2 = new Regex(@"\$type");
                result = rgx2.Replace(result, "ConcreteType");
            }

            return result;
        }
    }
}

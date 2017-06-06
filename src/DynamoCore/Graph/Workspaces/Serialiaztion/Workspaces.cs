using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Newtonsoft.Json;

namespace Autodesk.Workspaces
{
    public static class Utilities
    {
        /// <summary>
        /// Load a WorkspaceModel from json. If the WorkspaceModel is a HomeWorkspaceModel
        /// it will be set as the current workspace.
        /// </summary>
        /// <param name="json"></param>
        public static WorkspaceModel LoadWorkspaceFromJson(string json, LibraryServices libraryServices,
            EngineController engineController, DynamoScheduler scheduler, NodeFactory factory,
            bool isTestMode, bool verboseLogging, CustomNodeManager manager)
        {
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
                Converters = new List<JsonConverter>{
                        new ConnectorConverter(),
                        new AnnotationConverter(),
                        new WorkspaceConverter(engineController, scheduler, factory, isTestMode, verboseLogging),
                        new NodeModelConverter(manager, libraryServices),
                    },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var result = ReplaceTypeDeclarations(json, true);
            var ws = JsonConvert.DeserializeObject<WorkspaceModel>(result, settings);

            return ws;
        }

        /// <summary>
        /// Save a Workspace to json.
        /// </summary>
        /// <returns>A string representing the serialized WorkspaceModel.</returns>
        public static string SaveWorkspaceToJson(WorkspaceModel workspace, LibraryServices libraryServices, 
            EngineController engineController, DynamoScheduler scheduler, NodeFactory factory,
            bool isTestMode, bool verboseLogging, CustomNodeManager manager)
        {
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
                Converters = new List<JsonConverter>{
                        new ConnectorConverter(),
                        new AnnotationConverter(),
                        new WorkspaceConverter(engineController, scheduler, factory,
                        isTestMode, verboseLogging),
                        new NodeModelConverter(manager, libraryServices),
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
        private static string ReplaceTypeDeclarations(string json, bool fromServer = false)
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

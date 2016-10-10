using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Workspaces;
using Dynamo.Scheduler;
using Newtonsoft.Json;
using ProtoCore.Namespace;
using System;
using System.Collections.Generic;

namespace Workspaces.Serialization
{
    public static class Workspaces
    {
        /// <summary>
        /// Load a WorkspaceModel from json. If the WorkspaceModel is a HomeWorkspaceModel
        /// it will be set as the current workspace.
        /// </summary>
        /// <param name="json"></param>
        public static WorkspaceModel LoadWorkspaceFromJson(string json, LibraryServices libraryServices,
            EngineController engineController, DynamoScheduler scheduler, NodeFactory factory,
            bool isTestMode, bool verboseLogging, CustomNodeManager manager, ElementResolver elementResolver)
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
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new List<JsonConverter>{
                        new FunctionDescriptorConverter(libraryServices),
                        new ConnectorConverter(),
                        new AnnotationConverter(),
                        new WorkspaceConverter(engineController, scheduler, factory, isTestMode, verboseLogging),
                        new NodeModelConverter(manager, libraryServices)
                    },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            var ws = JsonConvert.DeserializeObject<WorkspaceModel>(json, settings);

            return ws;
        }

        /// <summary>
        /// Save a Workspace to json.
        /// </summary>
        /// <returns>A string representing the serialized WorkspaceModel.</returns>
        public static string SaveWorkspaceToJson(WorkspaceModel workspace, LibraryServices libraryServices, 
            EngineController engineController, DynamoScheduler scheduler, NodeFactory factory,
            bool isTestMode, bool verboseLogging, CustomNodeManager manager, ElementResolver elementResolver)
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
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new List<JsonConverter>{
                        new FunctionDescriptorConverter(libraryServices),
                        new ConnectorConverter(),
                        new AnnotationConverter(),
                        new WorkspaceConverter(engineController, scheduler, factory,
                        isTestMode, verboseLogging),
                        new NodeModelConverter(manager, libraryServices)
                    },
                ReferenceResolverProvider = () => { return new IdReferenceResolver(); }
            };

            return JsonConvert.SerializeObject(workspace, settings);
        }
    }
}

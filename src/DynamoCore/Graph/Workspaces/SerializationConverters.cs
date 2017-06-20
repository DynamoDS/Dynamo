using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ProtoCore;
using ProtoCore.Namespace;
using Type = System.Type;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// The NodeModelConverter is used to serialize and deserialize NodeModels.
    /// These nodes require a CustomNodeDefinition which can only be supplied
    /// by looking it up in the CustomNodeManager.
    /// </summary>
    public class NodeReadConverter : JsonConverter
    {
        private CustomNodeManager manager;
        private LibraryServices libraryServices;
        
        public ElementResolver ElementResolver { get; set; }

        public NodeReadConverter(CustomNodeManager manager, LibraryServices libraryServices)
        {
            this.manager = manager;
            this.libraryServices = libraryServices;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NodeModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            NodeModel node = null;

            var obj = JObject.Load(reader);
            var type = Type.GetType(obj["$type"].Value<string>());

            //if the id is not a guid, makes a guid based on the id of the node
            var guid = GuidUtility.tryParseOrCreateGuid(obj["Id"].Value<string>());
        

           
            var inPorts = obj["Inputs"].ToArray().Select(t => t.ToObject<PortModel>()).ToArray();
            var outPorts = obj["Outputs"].ToArray().Select(t => t.ToObject<PortModel>()).ToArray();

            var resolver = (IdReferenceResolver)serializer.ReferenceResolver;

            if (type == typeof(Function))
            {
                var functionId = Guid.Parse(obj["FunctionUuid"].Value<string>());
                node = manager.CreateCustomNodeInstance(functionId);
                RemapPorts(node, inPorts, outPorts, resolver);
            }
            else if (type == typeof(CodeBlockNodeModel))
            {
                var code = obj["Code"].Value<string>();
                node = new CodeBlockNodeModel(code, guid, 0.0, 0.0, libraryServices, ElementResolver);
                RemapPorts(node, inPorts, outPorts, resolver);
            }
            else if (typeof(DSFunctionBase).IsAssignableFrom(type))
            {
                var mangledName = obj["FunctionSignature"].Value<string>();

                var description = libraryServices.GetFunctionDescriptor(mangledName);

                if (type == typeof(DSVarArgFunction))
                {
                    node = new DSVarArgFunction(description);
                    // The node syncs with the function definition.
                    // Then we need to make the inport count correct
                    var varg = (DSVarArgFunction)node;
                    varg.VarInputController.SetNumInputs(inPorts.Count());
                }
                else if (type == typeof(DSFunction))
                {
                    node = new DSFunction(description);

                }
                RemapPorts(node, inPorts, outPorts, resolver);
            }
            else if (type == typeof(DSVarArgFunction))
            {
                var functionId = Guid.Parse(obj["FunctionUuid"].Value<string>());
                node = manager.CreateCustomNodeInstance(functionId);
                RemapPorts(node, inPorts, outPorts, resolver);
            }
            else if (type.ToString() == "CoreNodeModels.Formula")
            {
                node = (NodeModel)obj.ToObject(type);
                RemapPorts(node, inPorts, outPorts, resolver);
            }
            else
            {
                node = (NodeModel)obj.ToObject(type);
            }

            node.GUID = guid;

            // Add references to the node and the ports to the reference resolver,
            // so that they are available for entities which are deserialized later.
            serializer.ReferenceResolver.AddReference(serializer.Context, node.GUID.ToString(), node);

            foreach (var p in node.InPorts)
            {
                serializer.ReferenceResolver.AddReference(serializer.Context, p.GUID.ToString(), p);
            }
            foreach (var p in node.OutPorts)
            {
                serializer.ReferenceResolver.AddReference(serializer.Context, p.GUID.ToString(), p);
            }
            return node;
        }

        /// <summary>
        /// Map old Guids to new Models in the IdReferenceResolver.
        /// </summary>
        /// <param name="node">The newly created node.</param>
        /// <param name="inPorts">The deserialized input ports.</param>
        /// <param name="outPorts">The deserialized output ports.</param>
        /// <param name="resolver">The IdReferenceResolver used during deserialization.</param>
        private static void RemapPorts(NodeModel node, PortModel[] inPorts, PortModel[] outPorts, IdReferenceResolver resolver)
        {
            foreach (var p in node.InPorts)
            {
                resolver.AddToReferenceMap(inPorts[p.Index].GUID, p);
            }
            foreach (var p in node.OutPorts)
            {
                resolver.AddToReferenceMap(outPorts[p.Index].GUID, p);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
    }

    /// <summary>
    /// The WorkspaceConverter is used to serialize and deserialize WorkspaceModels.
    /// Construction of a WorkspaceModel requires things like an EngineController,
    /// a NodeFactory, and a Scheduler. These must be supplied at the time of 
    /// construction and should not be serialized.
    /// </summary>
    public class WorkspaceReadConverter : JsonConverter
    {
        DynamoScheduler scheduler;
        EngineController engine;
        NodeFactory factory;
        bool isTestMode;
        bool verboseLogging;

        public WorkspaceReadConverter(EngineController engine, 
            DynamoScheduler scheduler, NodeFactory factory, bool isTestMode, bool verboseLogging)
        {
            this.scheduler = scheduler;
            this.engine = engine;
            this.factory = factory;
            this.isTestMode = isTestMode;
            this.verboseLogging = verboseLogging;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(WorkspaceModel).IsAssignableFrom(objectType);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var isCustomNode = obj["IsCustomNode"].Value<bool>();
            var description = obj["Description"].Value<string>();
            var guidStr = obj["Uuid"].Value<string>();
            var guid = Guid.Parse(guidStr);
            var name = obj["Name"].Value<string>();

            var elementResolver = obj["ElementResolver"].ToObject<ElementResolver>(serializer);
            var nmc = (NodeReadConverter)serializer.Converters.First(c => c is NodeReadConverter);
            nmc.ElementResolver = elementResolver;

            // nodes
            var nodes = obj["Nodes"].ToObject<IEnumerable<NodeModel>>(serializer);
            
            // notes
            //TODO: Check this when implementing ReadJSON in ViewModel.
            //var notes = obj["Notes"].ToObject<IEnumerable<NoteModel>>(serializer);
            //if (notes.Any())
            //{
            //    foreach(var n in notes)
            //    {
            //        serializer.ReferenceResolver.AddReference(serializer.Context, n.GUID.ToString(), n);
            //    }
            //}

            // connectors
            // Although connectors are not used in the construction of the workspace
            // we need to deserialize this collection, so that they connect to their
            // relevant ports.
            var connectors = obj["Connectors"].ToObject<IEnumerable<ConnectorModel>>(serializer);

            var info = new WorkspaceInfo(guid.ToString(), name, description, Dynamo.Models.RunType.Automatic);

            //Build an empty annotations. Annotations are defined in the view block. If the file has View block
            //serialize view block first and build the annotations.
            var annotations = new List<AnnotationModel>();

            //Build an empty notes. Notes are defined in the view block. If the file has View block
            //serialize view block first and build the notes.
            var notes = new List<NoteModel>();

            WorkspaceModel ws;
            if (isCustomNode)
            {
                ws = new CustomNodeWorkspaceModel(factory, nodes, notes, annotations, 
                    Enumerable.Empty<PresetModel>(), elementResolver, info);
            }
            else
            {
                ws = new HomeWorkspaceModel(guid, engine, scheduler, factory, 
                    Enumerable.Empty<KeyValuePair<Guid, List<CallSite.RawTraceData>>>(), nodes, notes, annotations, 
                    Enumerable.Empty<PresetModel>(), elementResolver, 
                    info, verboseLogging, isTestMode);
            }

            return ws;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// WorkspaceWriteConverter is used for serializing Workspaces to JSON.
    /// </summary>
    public class WorkspaceWriteConverter : JsonConverter
    {
        private EngineController engine;

        public WorkspaceWriteConverter(EngineController engine = null)
        {
            if (engine != null)
            {
                this.engine = engine;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(WorkspaceModel).IsAssignableFrom(objectType);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ws = (WorkspaceModel)value;
            bool isCustomNode = value is CustomNodeWorkspaceModel;
            writer.WriteStartObject();
            writer.WritePropertyName("Uuid");
            if (isCustomNode)
                writer.WriteValue((ws as CustomNodeWorkspaceModel).CustomNodeId.ToString());
            else
                writer.WriteValue(ws.Guid.ToString());
            // TODO: revisit IsCustomNode during DYN/DYF convergence
            writer.WritePropertyName("IsCustomNode");
            writer.WriteValue(value is CustomNodeWorkspaceModel ? true : false);
            if (isCustomNode)
            {
                writer.WritePropertyName("Category");
                writer.WriteValue(((CustomNodeWorkspaceModel)value).Category);
            }

            // Description
            writer.WritePropertyName("Description");
            if (isCustomNode)
                writer.WriteValue(((CustomNodeWorkspaceModel)ws).Description);
            else
                writer.WriteValue(ws.Description);
            writer.WritePropertyName("Name");
            writer.WriteValue(ws.Name);

            //element resolver
            writer.WritePropertyName("ElementResolver");
            serializer.Serialize(writer, ws.ElementResolver);

            //nodes
            writer.WritePropertyName("Nodes");
            serializer.Serialize(writer, ws.Nodes);

            //connectors
            writer.WritePropertyName("Connectors");
            serializer.Serialize(writer, ws.Connectors);

            // Dependencies
            writer.WritePropertyName("Dependencies");
            writer.WriteStartArray();
            var functions = ws.Nodes.Where(n => n is Function);
            if (functions.Any())
            {
                var deps = functions.Cast<Function>().Select(f => f.Definition.FunctionId).Distinct();
                foreach (var d in deps)
                {
                    writer.WriteValue(d);
                }
            }
            writer.WriteEndArray();

            if (engine != null)
            {
                // Bindings
                writer.WritePropertyName(Configurations.BindingsTag);
                writer.WriteStartArray();

                // Selecting all nodes that are either a DSFunction,
                // a DSVarArgFunction or a CodeBlockNodeModel into a list.
                var nodeGuids =
                    ws.Nodes.Where(
                            n => n is DSFunction || n is DSVarArgFunction || n is CodeBlockNodeModel || n is Function)
                        .Select(n => n.GUID);

                var nodeTraceDataList = engine.LiveRunnerRuntimeCore.RuntimeData.GetTraceDataForNodes(nodeGuids,
                    this.engine.LiveRunnerRuntimeCore.DSExecutable);

                // serialize given node-data-list pairs into an Json.
                if (nodeTraceDataList.Any())
                {
                    foreach (var pair in nodeTraceDataList)
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName(Configurations.NodeIdAttribName);
                        // Set the node ID attribute for this element.
                        var nodeGuid = pair.Key.ToString();
                        writer.WriteValue(nodeGuid);
                        writer.WritePropertyName(Configurations.BingdingTag);
                        // D4R binding
                        writer.WriteStartObject();
                        foreach (var data in pair.Value)
                        {
                            writer.WritePropertyName(data.ID);
                            writer.WriteValue(data.Data);
                        }
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                    }
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// The ConnectorConverter is used to serialize and deserialize ConnectorModels.
    /// The Start and End of a ConnectorModel are references to PortModels, but
    /// we want the serialized representation of a Connector to reference these 
    /// ports by Id. This converter resolves the reference to the correct NodeModel
    /// instance by id, and constructs the ConnectorModel.
    /// </summary>
    public class ConnectorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ConnectorModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var startId = obj["Start"].Value<string>();
            var endId = obj["End"].Value<string>();

            var resolver = (IdReferenceResolver)serializer.ReferenceResolver;

            Guid startIdGuid = GuidUtility.tryParseOrCreateGuid(startId);
            Guid endIdGuid = GuidUtility.tryParseOrCreateGuid(endId);

            var startPort = (PortModel)resolver.ResolveReference(serializer.Context, startIdGuid.ToString());
            var endPort = (PortModel)resolver.ResolveReference(serializer.Context, endIdGuid.ToString());

            // If the start or end ports can't be found in the resolver,
            // try to resolve them from the resolver's map, which maps
            // the persisted port ids to the new port ids.
            if(startPort == null)
            {
                startPort = (PortModel)resolver.ResolveReferenceFromMap(serializer.Context, startIdGuid.ToString());
            }

            if(endPort == null)
            {
                endPort = (PortModel)resolver.ResolveReferenceFromMap(serializer.Context, endIdGuid.ToString());
            }

            //if the id is not a guid, makes a guid based on the id of the model
            Guid connectorId = GuidUtility.tryParseOrCreateGuid(obj["Id"].Value<string>());

            return new ConnectorModel(startPort, endPort, connectorId);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var connector = (ConnectorModel)value;

            writer.WriteStartObject();
            writer.WritePropertyName("Start");
            writer.WriteValue(connector.Start.GUID.ToString());
            writer.WritePropertyName("End");
            writer.WriteValue(connector.End.GUID.ToString());
            writer.WritePropertyName("Id");
            writer.WriteValue(connector.GUID.ToString());
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// This converter is used to attempt to convert an id string to a guid - if the id
    /// is not a guid string, it will create a UUID based on the string.
    /// </summary>
    public class IdToGuidConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JValue.Load(reader);
            Guid deterministicGuid;
            if (!Guid.TryParse(obj.Value<string>(), out deterministicGuid))
            {
                Console.WriteLine("the id was not a guid, converting to a guid");
                deterministicGuid = GuidUtility.Create(GuidUtility.UrlNamespace, obj.Value<string>());
                Console.WriteLine(obj + " becomes " + deterministicGuid);
            }
            return deterministicGuid;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
    }

    /// <summary>
    /// The IdReferenceResolver class allows us to use the Guid of
    /// an object as the reference id during serialization.
    /// </summary>
    public class IdReferenceResolver : IReferenceResolver
    {
        private readonly IDictionary<Guid, object> models = new Dictionary<Guid, object>();
        private readonly IDictionary<Guid, object> modelMap = new Dictionary<Guid, object>();

        /// <summary>
        /// Add a reference to a newly created object, referencing
        /// an old id.
        /// </summary>
        /// <param name="oldid">The old id of the object.</param>
        /// <param name="newObject">The new object which maps to the old id.</param>
        public void AddToReferenceMap(Guid oldId, object newObject)
        {
            if (modelMap.ContainsKey(oldId))
            {
                throw new InvalidOperationException(@"the map already contains a model with this id, the id must
                    be unique for the workspace that is currently being deserialized: "+oldId);
            }
            modelMap.Add(oldId, newObject);
        }

        public void AddReference(object context, string reference, object value)
        {
            Guid id = new Guid(reference);
            if (models.ContainsKey(id))
            {
                throw new InvalidOperationException(@"the map already contains a model with this id, the id must
                    be unique for the workspace that is currently being deserialized :"+id);
            }
            models[id] = value;
        }

        private static Guid GetGuidPropertyValue(object value)
        {
            // Use reflection to find the Guid or the GUID
            // property on the object.

            var pi = value.GetType().GetProperty("Guid");
            if (pi == null)
            {
                pi = value.GetType().GetProperty("GUID");
            }

            var id = pi == null ? Guid.NewGuid() : (Guid)pi.GetValue(value);
            return id;
        }

        public string GetReference(object context, object value)
        {
            models[GetGuidPropertyValue(value)] = value;

            return GetGuidPropertyValue(value).ToString();
        }

        public bool IsReferenced(object context, object value)
        {
            var id = GetGuidPropertyValue(value);
            return models.ContainsKey(id);
        }

        public object ResolveReference(object context, string reference)
        {
            Guid id;
            if (!Guid.TryParse(reference, out id))
            {
                //if this is not a guid, it won't be in the resolver.
                Console.WriteLine("not a guid");
                return null;
            }
            object model;
            models.TryGetValue(id, out model);

            return model;
        }

        /// <summary>
        /// Resolve a reference to a newly created object, given
        /// the original id for the object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public object ResolveReferenceFromMap(object context, string reference)
        {
            Guid id;
            if (!Guid.TryParse(reference, out id))
            {
                //if this is not a guid, it won't be in the resolver.
                Console.WriteLine("not a guid");
                return null;
            }
            object model;
            modelMap.TryGetValue(id, out model);

            return model;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
using Dynamo.Graph.Workspaces;
using Dynamo.Scheduler;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ProtoCore;
using ProtoCore.Namespace;
using Type = System.Type;

namespace Autodesk.Workspaces
{
    /// <summary>
    /// The NodeModelConverter is used to serialize and deserialize NodeModels.
    /// These nodes require a CustomNodeDefinition which can only be supplied
    /// by looking it up in the CustomNodeManager.
    /// </summary>
    public class NodeModelConverter : JsonConverter
    {
        private CustomNodeManager manager;
        private LibraryServices libraryServices;
        
        public ElementResolver ElementResolver { get; set; }

        public NodeModelConverter(CustomNodeManager manager, LibraryServices libraryServices)
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
        
            var displayName = obj["DisplayName"].Value<string>();

           
            var inPorts = obj["InputPorts"].ToArray().Select(t => t.ToObject<PortModel>()).ToArray();
            var outPorts = obj["OutputPorts"].ToArray().Select(t => t.ToObject<PortModel>()).ToArray();

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
            node.NickName = displayName;
            //node.X = x;
            //node.Y = y;

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
    public class WorkspaceConverter : JsonConverter
    {
        DynamoScheduler scheduler;
        EngineController engine;
        NodeFactory factory;
        bool isTestMode;
        bool verboseLogging;

        public WorkspaceConverter(EngineController engine, 
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var isCustomNode = obj["IsCustomNode"].Value<bool>();
            var lastModifiedStr = obj["LastModified"].Value<string>();
            var lastModified = DateTime.Parse(lastModifiedStr);
            var author = obj["LastModifiedBy"].Value<string>();
            var description = obj["Description"].Value<string>();
            var guidStr = obj["Uuid"].Value<string>();
            var guid = Guid.Parse(guidStr);
            var name = obj["Name"].Value<string>();

            var elementResolver = obj["ElementResolver"].ToObject<ElementResolver>(serializer);
            var nmc = (NodeModelConverter)serializer.Converters.First(c => c is NodeModelConverter);
            nmc.ElementResolver = elementResolver;

            // nodes
            var nodes = obj["Nodes"].ToObject<IEnumerable<NodeModel>>(serializer);
            
            // notes
            var notes = obj["Notes"].ToObject<IEnumerable<NoteModel>>(serializer);
            if (notes.Any())
            {
                foreach(var n in notes)
                {
                    serializer.ReferenceResolver.AddReference(serializer.Context, n.GUID.ToString(), n);
                }
            }

            // connectors
            // Although connectors are not used in the construction of the workspace
            // we need to deserialize this collection, so that they connect to their
            // relevant ports.
            var connectors = obj["Connectors"].ToObject<IEnumerable<ConnectorModel>>(serializer);

            // annotations
            var annotations = obj["Annotations"].ToObject<IEnumerable<AnnotationModel>>(serializer);

            var info = new WorkspaceInfo(guid.ToString(), name, description, Dynamo.Models.RunType.Automatic);

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
            var ws = (WorkspaceModel)value;

            writer.WriteStartObject();

            writer.WritePropertyName("Uuid");
            writer.WriteValue(ws.Guid.ToString());
            writer.WritePropertyName("IsCustomNode");
            writer.WriteValue(value is CustomNodeWorkspaceModel ? true : false);
            if(value is CustomNodeWorkspaceModel)
            {
                writer.WritePropertyName("Category");
                writer.WriteValue(((CustomNodeWorkspaceModel)value).Category);
            }
            writer.WritePropertyName("LastModified");
            writer.WriteValue(ws.LastSaved.ToUniversalTime());
            writer.WritePropertyName("LastModifiedBy");
            writer.WriteValue(ws.Author);
            writer.WritePropertyName("Description");
            writer.WriteValue(ws.Description);
            writer.WritePropertyName("Name");
            writer.WriteValue(ws.Name);

            //element resolver
            writer.WritePropertyName("ElementResolver");
            serializer.Serialize(writer, ws.ElementResolver);

            //nodes
            writer.WritePropertyName("Nodes");
            serializer.Serialize(writer, ws.Nodes);

            //notes
            writer.WritePropertyName("Notes");
            serializer.Serialize(writer, ws.Notes);

            //connectors
            writer.WritePropertyName("Connectors");
            serializer.Serialize(writer, ws.Connectors);

            //annotations
            writer.WritePropertyName("Annotations");
            serializer.Serialize(writer, ws.Annotations);

            //cameras
            writer.WritePropertyName("Cameras");
            writer.WriteStartArray();
            writer.WriteEndArray();

            writer.WritePropertyName("Dependencies");
            writer.WriteStartArray();
            var functions = ws.Nodes.Where(n => n is Function);
            if (functions.Any())
            {
                var deps = functions.Cast<Function>().Select(f => f.Definition.FunctionId).Distinct();
                foreach(var d in deps)
                {
                    writer.WriteValue(d);
                }
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// The AnnotationConverter is used to serialize and deserialize AnnotationModels.
    /// The SelectedModels property on AnnotationModel is a list of references
    /// to ModelBase objects. During serialization we want to refer to these objects
    /// by their ids. During deserialization, we use the ReferenceResolver to
    /// find the correct ModelBase instances to reference.
    /// </summary>
    public class AnnotationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AnnotationModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var title = obj["Title"].Value<string>();
            //if the id is not a guid, makes a guid based on the id of the model
            Guid annotationId =  GuidUtility.tryParseOrCreateGuid(obj["Id"].Value<string>());

            // This is a collection of string Guids, which
            // should be accessible in the ReferenceResolver.
            var models = obj["Nodes"].Values<JValue>();

            var existing = models.Select(m => {
                Guid modelId = GuidUtility.tryParseOrCreateGuid(m.Value<string>());
              
                return serializer.ReferenceResolver.ResolveReference(serializer.Context, modelId.ToString());
                });

            var nodes = existing.Where(m => typeof(NodeModel).IsAssignableFrom(m.GetType())).Cast<NodeModel>();
            var notes = existing.Where(m => typeof(NoteModel).IsAssignableFrom(m.GetType())).Cast<NoteModel>();

            var anno = new AnnotationModel(nodes, notes);
            anno.AnnotationText = title;
            anno.GUID = annotationId;

            return anno;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var anno = (AnnotationModel)value;

            writer.WriteStartObject();

            writer.WritePropertyName("Title");
            writer.WriteValue(anno.AnnotationText);
            writer.WritePropertyName("Nodes");
            writer.WriteStartArray();
            foreach (var m in anno.Nodes)
            {
                writer.WriteValue(m.GUID.ToString());
            }
            writer.WriteEndArray();
            writer.WritePropertyName("Id");
            writer.WriteValue(anno.GUID.ToString());

            writer.WriteEndObject();
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

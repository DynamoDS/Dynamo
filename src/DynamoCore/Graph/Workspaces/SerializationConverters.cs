using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Dynamo.Library;
using Dynamo.Logging;
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
        private NodeFactory nodeFactory;
        private bool isTestMode;

        public ElementResolver ElementResolver { get; set; }
        // Map of all loaded assemblies including LoadFrom context assemblies
        private Dictionary<string, List<Assembly>> loadedAssemblies;

        [Obsolete("This constructor will be removed in Dynamo 3.0, please use new NodeReadConverter constructor with additional parameters to support node migration.")]
        public NodeReadConverter(CustomNodeManager manager, LibraryServices libraryServices, bool isTestMode = false)
        {
            this.manager = manager;
            this.libraryServices = libraryServices;
            this.isTestMode = isTestMode;
            // We only do this in test mode because it should not be required-
            // see comment below in NodeReadConverter.ReadJson - and it could be slow.
            if (this.isTestMode)
            {
                this.loadedAssemblies = this.buildMapOfLoadedAssemblies();
            }
        }

        public NodeReadConverter(CustomNodeManager manager, LibraryServices libraryServices, NodeFactory nodeFactory, bool isTestMode = false)
        {
            this.manager = manager;
            this.libraryServices = libraryServices;
            this.nodeFactory = nodeFactory;
            this.isTestMode = isTestMode;
            // We only do this in test mode because it should not be required-
            // see comment below in NodeReadConverter.ReadJson - and it could be slow.
            if (this.isTestMode)
            {
                this.loadedAssemblies = this.buildMapOfLoadedAssemblies();
            }
        }

        private Dictionary<string,List<Assembly>> buildMapOfLoadedAssemblies()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var dict = new Dictionary<string, List<Assembly>>();
            foreach(var assembly in allAssemblies)
            {
                if (!dict.ContainsKey(assembly.GetName().Name))
                {
                    dict[assembly.GetName().Name] = new List<Assembly>() { assembly };
                }
                else{
                    dict[assembly.GetName().Name].Add(assembly);
                }
            }
            return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NodeModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            NodeModel node = null;

            var obj = JObject.Load(reader);
            Type type = null;
            try
            {
               type = Type.GetType(obj["$type"].Value<string>());
            }
            catch(Exception e)
            {
                nodeFactory?.AsLogger().Log(e);
            }
            // If we can't find this type - try to look in our load from assemblies,
            // but only during testing - this is required during testing because some dlls are loaded
            // using Assembly.LoadFrom using the assemblyHelper - which loads dlls into loadFrom context - 
            // dlls loaded with LoadFrom context cannot be found using Type.GetType() - this should
            // not be an issue during normal dynamo use but if it is we can enable this code.
            if(type == null && this.isTestMode == true)
            {
                List<Assembly> resultList;

                var typeName = obj["$type"].Value<string>().Split(',').FirstOrDefault();
                // This assemblyName does not usually contain version information...
                var assemblyName = obj["$type"].Value<string>().Split(',').Skip(1).FirstOrDefault().Trim();
                if (assemblyName != null)
                {
                    if(this.loadedAssemblies.TryGetValue(assemblyName, out resultList))
                    {
                        var matchingTypes = resultList.Select(x => x.GetType(typeName)).ToList();
                        type =  matchingTypes.FirstOrDefault();
                    }
                }
            }

            // Check for and attempt to resolve an unknown type before proceeding
            if (type == null)
            {
                // Attempt to resolve the type using `AlsoKnownAs`
                var unresolvedName = obj["$type"].Value<string>().Split(',').FirstOrDefault();
                Type newType;
                nodeFactory.ResolveType(unresolvedName, out newType);

                // If resolved update the type
                if (newType != null)
                {
                    type = newType;
                }
            }

            // If the id is not a guid, makes a guid based on the id of the node
            var guid = GuidUtility.tryParseOrCreateGuid(obj["Id"].Value<string>());

            var replication = obj["Replication"].Value<string>();
           
            var inPorts = obj["Inputs"].ToArray().Select(t => t.ToObject<PortModel>()).ToArray();
            var outPorts = obj["Outputs"].ToArray().Select(t => t.ToObject<PortModel>()).ToArray();

            var resolver = (IdReferenceResolver)serializer.ReferenceResolver;
            string assemblyLocation = objectType.Assembly.Location;

            bool remapPorts = true;

            // If type is still null at this point return a dummy node
            if (type == null)
            {
                node = CreateDummyNode(obj, assemblyLocation, inPorts, outPorts);
            }
            // Attempt to create a valid node using the type
            else if (type == typeof(Function))
            {
                var functionId = Guid.Parse(obj["FunctionSignature"].Value<string>());

                CustomNodeDefinition def = null;
                CustomNodeInfo info = null;
                bool isUnresolved = !manager.TryGetCustomNodeData(functionId, null, false, out def, out info);
                Function function = manager.CreateCustomNodeInstance(functionId, null, false, def, info);
                node = function;

                if (isUnresolved)
                  function.UpdatePortsForUnresolved(inPorts, outPorts);
            }

            else if (type == typeof(CodeBlockNodeModel))
            {
                var code = obj["Code"].Value<string>();
                CodeBlockNodeModel codeBlockNode = new CodeBlockNodeModel(code, guid, 0.0, 0.0, libraryServices, ElementResolver);
                node = codeBlockNode;

                // If the code block node is in an error state read the extra port data
                // and initialize the input and output ports
                if (node.IsInErrorState)
                {
                    List<string> inPortNames = new List<string>();
                    var inputs = obj["Inputs"];
                    foreach (var input in inputs)
                    {
                        inPortNames.Add(input["Name"].ToString());
                    }

                    // NOTE: This could be done in a simpler way, but is being implemented
                    //       in this manner to allow for possible future port line number
                    //       information being available in the file
                    List<int> outPortLineIndexes = new List<int>();
                    var outputs = obj["Outputs"];
                    int outputLineIndex = 0;
                    foreach (var output in outputs)
                    {
                        outPortLineIndexes.Add(outputLineIndex++);
                    }

                    codeBlockNode.SetErrorStatePortData(inPortNames, outPortLineIndexes);
                }
            }
            else if (typeof(DSFunctionBase).IsAssignableFrom(type))
            {
                var mangledName = obj["FunctionSignature"].Value<string>();
                var priorNames = libraryServices.GetPriorNames();
                var functionDescriptor = libraryServices.GetFunctionDescriptor(mangledName);
                string newName;

                // Update the function descriptor if a newer migrated version of the node exists
                if (priorNames.TryGetValue(mangledName, out newName))
                {
                    functionDescriptor = libraryServices.GetFunctionDescriptor(newName);
                }

                // Use the functionDescriptor to try and restore the proper node if possible
                if (functionDescriptor == null)
                {
                    node = CreateDummyNode(obj, assemblyLocation, inPorts, outPorts);
                }
                else
                {
                    if (type == typeof(DSVarArgFunction))
                    {
                        node = new DSVarArgFunction(functionDescriptor);
                        // The node syncs with the function definition.
                        // Then we need to make the inport count correct
                        var varg = (DSVarArgFunction)node;
                        varg.VarInputController.SetNumInputs(inPorts.Count());
                    }
                    else if (type == typeof(DSFunction))
                    {
                        node = new DSFunction(functionDescriptor);
                    }
                }
            }
            else if (type == typeof(DSVarArgFunction))
            {
                var functionId = Guid.Parse(obj["FunctionSignature"].Value<string>());
                node = manager.CreateCustomNodeInstance(functionId);
            }
            else if (type.ToString() == "CoreNodeModels.Formula")
            {
                node = (NodeModel)obj.ToObject(type);
            }
            else
            {
                node = (NodeModel)obj.ToObject(type);
                
                // if node is an customNode input symbol - assign the element resolver.
                if(node is Nodes.CustomNodes.Symbol)
                {
                    (node as Nodes.CustomNodes.Symbol).ElementResolver = ElementResolver;
                }
                // We don't need to remap ports for any nodes with json constructors which pass ports
                remapPorts = false;
            }

            if (remapPorts)
            {
                RemapPorts(node, inPorts, outPorts, resolver, manager.AsLogger());
            }
               

            // Cannot set Lacing directly as property is protected
            node.UpdateValue(new UpdateValueParams("ArgumentLacing", replication));
            node.GUID = guid;

            // Add references to the node and the ports to the reference resolver,
            // so that they are available for entities which are deserialized later.
            serializer.ReferenceResolver.AddReference(serializer.Context, node.GUID.ToString(), node);

            foreach (var p in node.InPorts)
                serializer.ReferenceResolver.AddReference(serializer.Context, p.GUID.ToString(), p);

            foreach (var p in node.OutPorts)
                serializer.ReferenceResolver.AddReference(serializer.Context, p.GUID.ToString(), p);
            
            return node;
        }

        private DummyNode CreateDummyNode(JObject obj, string assemblyLocation, PortModel[] inPorts, PortModel[] outPorts)
        {
            var inputcount = inPorts.Count();
            var outputcount = outPorts.Count();

            return new DummyNode(
                obj["Id"].ToString(),
                inputcount,
                outputcount,
                assemblyLocation,
                obj);
        }


        /// <summary>
        /// Map old Guids to new Models in the IdReferenceResolver.
        /// This method also sets portData from the deserialized ports onto the 
        /// newly created ports.
        /// </summary>
        /// <param name="node">The newly created node.</param>
        /// <param name="inPorts">The deserialized input ports.</param>
        /// <param name="outPorts">The deserialized output ports.</param>
        /// <param name="resolver">The IdReferenceResolver used during deserialization.</param>
        private static void RemapPorts(NodeModel node, PortModel[] inPorts, PortModel[] outPorts, IdReferenceResolver resolver, ILogger logger)
        {
            foreach (var p in node.InPorts)
            {
                // Check that the port index is not out of range of the loaded ports
                if (p.Index < inPorts.Length)
                {
                    var deserializedPort = inPorts[p.Index];
                    resolver.AddToReferenceMap(deserializedPort.GUID, p);
                    setPortDataOnNewPort(p, deserializedPort);
                }
                else
                {
                    if (logger != null)
                    {
                        logger.Log(
                            string.Format("while loading node {0} we could not find a port for the parameter {1} at index {2}",node.Name,p.Name,p.Index)
                            );
                    }
                }
              
            }
            foreach (var p in node.OutPorts)
            {
                // Check that the port index is not out of range of the loaded ports
                if (p.Index < outPorts.Length)
                {
                    var deserializedPort = outPorts[p.Index];
                    resolver.AddToReferenceMap(deserializedPort.GUID, p);
                    setPortDataOnNewPort(p, deserializedPort);
                }
                else
                {
                    if (logger != null)
                    {
                        logger.Log(
                            string.Format("while loading node {0} we could not find a port for the retrunkey {1} at index {2}", node.Name, p.Name, p.Index)
                            );
                    }
                }
            }
        }

        private static void setPortDataOnNewPort(PortModel newPort, PortModel deserializedPort )
        {
            // Set the appropriate properties on the new port.
            newPort.GUID = deserializedPort.GUID;
            newPort.UseLevels = deserializedPort.UseLevels;
            newPort.Level = deserializedPort.Level;
            newPort.KeepListStructure = deserializedPort.KeepListStructure;
            newPort.UsingDefaultValue = deserializedPort.UsingDefaultValue;
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

    ///<Summary>
    ///  Converter for Description property in the NodeModel class.
    ///</Summary>
    public class DescriptionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(String));
        }

        /// When deserializing, we do not want to read this property from the file
        /// so null is being returned. This is to convert the Description property
        /// to the localized language. 
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        /// Serializing the description property. 
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
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

        internal readonly static string NodeLibraryDependenciesPropString = "NodeLibraryDependencies";

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

            var nodes = obj["Nodes"].ToObject<IEnumerable<NodeModel>>(serializer);

            // Setting Inputs
            // Required in headless mode by Dynamo Player that NodeModel.Name and NodeModel.IsSetAsInput are set
            var inputsToken = obj["Inputs"];
            if (inputsToken != null)
            {
                var inputs = inputsToken.ToArray().Select(x => x.ToObject<NodeInputData>()).ToList();
                // Use the inputs to set the correct properties on the nodes.
                foreach (var inputData in inputs)
                {
                    var matchingNode = nodes.Where(x => x.GUID == inputData.Id).FirstOrDefault();
                    if (matchingNode != null)
                    {
                        matchingNode.IsSetAsInput = true;
                        matchingNode.Name = inputData.Name;
                    }
                }
            }

            // Setting Outputs
            var outputsToken = obj["Outputs"];
            if (outputsToken != null)
            {
                var outputs = outputsToken.ToArray().Select(x => x.ToObject<NodeOutputData>()).ToList();
                // Use the outputs to set the correct properties on the nodes.
                foreach (var outputData in outputs)
                {
                    var matchingNode = nodes.Where(x => x.GUID == outputData.Id).FirstOrDefault();
                    if (matchingNode != null)
                    {
                        matchingNode.IsSetAsOutput = true;
                        matchingNode.Name = outputData.Name;
                    }
                }
            }

            #region Setting Inputs based on view layer info
            // TODO: It is currently duplicating the effort with Input Block parsing which should be cleaned up once
            // Dynamo supports both selection and drop down nodes in Inputs block
            var view = obj["View"];
            if (view != null)
            {
                var nodesView = view["NodeViews"];
                if (nodesView != null)
                {
                    var inputsView = nodesView.ToArray().Select(x => x.ToObject<Dictionary<string, string>>()).ToList();
                    foreach (var inputViewData in inputsView)
                    {
                        string isSetAsInput = "";
                        if (!inputViewData.TryGetValue("IsSetAsInput", out isSetAsInput) || isSetAsInput == bool.FalseString)
                        {
                            continue;
                        }

                        string inputId = "";
                        if (inputViewData.TryGetValue("Id", out inputId))
                        {
                            Guid inputGuid;
                            try
                            {
                                inputGuid = Guid.Parse(inputId);
                            }
                            catch
                            {
                                continue;
                            }

                            var matchingNode = nodes.Where(x => x.GUID == inputGuid).FirstOrDefault();
                            if (matchingNode != null)
                            {
                                matchingNode.IsSetAsInput = true;
                                string inputName = "";
                                if (inputViewData.TryGetValue("Name", out inputName))
                                {
                                    matchingNode.Name = inputName;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

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

            IEnumerable<INodeLibraryDependencyInfo> nodeLibraryDependencies;
            if (obj[NodeLibraryDependenciesPropString] != null)
            {
                nodeLibraryDependencies = obj[NodeLibraryDependenciesPropString].ToObject<IEnumerable<INodeLibraryDependencyInfo>>(serializer);
            }
            else
            {
                nodeLibraryDependencies = new List<PackageDependencyInfo>();
            }

            var info = new WorkspaceInfo(guid.ToString(), name, description, Dynamo.Models.RunType.Automatic);

            // IsVisibleInDynamoLibrary and Category should be set explicitly for custom node workspace
            if (obj["View"] != null && obj["View"]["Dynamo"] !=null && obj["View"]["Dynamo"]["IsVisibleInDynamoLibrary"] != null)
            {
                info.IsVisibleInDynamoLibrary = obj["View"]["Dynamo"]["IsVisibleInDynamoLibrary"].Value<bool>();
            }
            if (obj["Category"] != null)
            {
                info.Category = obj["Category"].Value<string>();
            }

            // Build an empty annotations. Annotations are defined in the view block. If the file has View block
            // serialize view block first and build the annotations.
            var annotations = new List<AnnotationModel>();

            // Build an empty notes. Notes are defined in the view block. If the file has View block
            // serialize view block first and build the notes.
            var notes = new List<NoteModel>();

            #region Restore trace data
            // Trace Data
            Dictionary<Guid, List<CallSite.RawTraceData>> loadedTraceData = new Dictionary<Guid, List<CallSite.RawTraceData>>();
            // Restore trace data if bindings are present in json
            if (obj["Bindings"] != null && obj["Bindings"].Children().Count() > 0)
            {
                JEnumerable<JToken> bindings = obj["Bindings"].Children();

                // Iterate through bindings to extract nodeID's and bindingData (callsiteId & traceData)
                foreach (JToken entity in bindings)
                {
                    Guid nodeId = Guid.Parse(entity["NodeId"].ToString());
                    string bindingString = entity["Binding"].ToString();

                    // Key(callsiteId) : Value(traceData)
                    Dictionary<string, string> bindingData = JsonConvert.DeserializeObject<Dictionary<string, string>>(bindingString);
                    List<CallSite.RawTraceData> callsiteTraceData = new List<CallSite.RawTraceData>();

                    foreach (KeyValuePair<string, string> pair in bindingData)
                    {
                        callsiteTraceData.Add(new CallSite.RawTraceData(pair.Key, pair.Value));
                    }

                    loadedTraceData.Add(nodeId, callsiteTraceData);
                }
            }
            #endregion

            WorkspaceModel ws;
            if (isCustomNode)
            {
                ws = new CustomNodeWorkspaceModel(factory, nodes, notes, annotations, 
                    Enumerable.Empty<PresetModel>(), elementResolver, info);
            }
            else
            {
                ws = new HomeWorkspaceModel(guid, engine, scheduler, factory,
                    loadedTraceData, nodes, notes, annotations, 
                    Enumerable.Empty<PresetModel>(), elementResolver, 
                    info, verboseLogging, isTestMode);
            }

            ws.NodeLibraryDependencies = nodeLibraryDependencies.ToList();

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

            // Element resolver
            writer.WritePropertyName("ElementResolver");
            serializer.Serialize(writer, ws.ElementResolver);

            // Inputs
            writer.WritePropertyName("Inputs");
            // Find nodes which are inputs and get their inputData if its not null.
            var inputNodeDatas = ws.Nodes.Where((node) => node.IsSetAsInput == true && node.InputData != null)
                .Select(inputNode => inputNode.InputData).ToList();
            serializer.Serialize(writer, inputNodeDatas);

            // Outputs
            writer.WritePropertyName("Outputs");
            // Find nodes which are outputs and get their outputData if its not null.
            var outputNodeDatas = ws.Nodes.Where((node) => node.IsSetAsOutput == true && node.OutputData != null)
                .Select(outputNode => outputNode.OutputData).ToList();
            serializer.Serialize(writer, outputNodeDatas);

            // Nodes
            writer.WritePropertyName("Nodes");
            serializer.Serialize(writer, ws.Nodes);

            // Connectors
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

            // NodeLibraryDependencies
            writer.WritePropertyName(WorkspaceReadConverter.NodeLibraryDependenciesPropString);
            serializer.Serialize(writer, ws.NodeLibraryDependencies);

            if (engine != null)
            {
                // Bindings
                writer.WritePropertyName(Configurations.BindingsTag);
                writer.WriteStartArray();

                // Selecting all nodes that are either a DSFunction,
                // a DSVarArgFunction or a CodeBlockNodeModel into a list.
                var nodeGuids =
                    ws.Nodes.Where(
                            n => n is DSFunction || n is DSVarArgFunction || n is CodeBlockNodeModel || n is Function || 
                            n.GetType().GetCustomAttributes(typeof(DynamoServices.RegisterForTraceAttribute),false).Any() )
                        .Select(n => n.GUID);

                var nodeTraceDataList = engine.LiveRunnerRuntimeCore.RuntimeData.GetTraceDataForNodes(nodeGuids,
                    this.engine.LiveRunnerRuntimeCore.DSExecutable);

                // Serialize given node-data-list pairs into an Json.
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
    /// Is used to serialize and deserialize graph workspace node library references
    /// </summary>
    public class NodeLibraryDependencyConverter : JsonConverter
    {
        private Logging.ILogger logger;
        internal static readonly string ReferenceTypePropString = "ReferenceType";
        internal static readonly string NamePropString = "Name";
        internal static readonly string VersionPropString = "Version";
        internal static readonly string NodesPropString = "Nodes";

        /// <summary>
        /// Constructs a WorkspaceNodeReferenceConverter.
        /// </summary>
        /// <param name="logger"></param>
        public NodeLibraryDependencyConverter(Logging.ILogger logger)
        {
            this.logger = logger;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(INodeLibraryDependencyInfo).IsAssignableFrom(objectType);
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            INodeLibraryDependencyInfo p = value as INodeLibraryDependencyInfo;
            if (p != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(NamePropString);
                writer.WriteValue(p.Name);
                writer.WritePropertyName(VersionPropString);
                writer.WriteValue(p.Version.ToString());
                writer.WritePropertyName(ReferenceTypePropString);
                writer.WriteValue(p.ReferenceType.ToString("G"));
                writer.WritePropertyName(NodesPropString);
                writer.WriteStartArray();
                foreach(var node in p.Nodes)
                {
                    writer.WriteValue(node.ToString("N"));
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else
            {
                logger.LogWarning("Unnsuccessful attempt to serialize a INodeLibraryDependencyInfo object.", Logging.WarningLevel.Moderate);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            // Get dependency name
            var name = obj["Name"].Value<string>();

            // Try get dependency version
            var versionString = obj["Version"].Value<string>();
            Version version;
            if (!Version.TryParse(versionString, out version))
            {
                logger.LogWarning(
                    string.Format("The version of Package Dependency {0} could not be deserialized.", name), 
                    Logging.WarningLevel.Moderate);
            }

            //default to package.
            ReferenceType parsedType = ReferenceType.Package;
            JToken referenceTypeToken;
            if (obj.TryGetValue(ReferenceTypePropString, out referenceTypeToken))
            {
                var referenceTypeString = referenceTypeToken.Value<string>();
                if (!Enum.TryParse<ReferenceType>(referenceTypeString, out parsedType))
                {
                    logger.LogWarning(
                        string.Format("The ReferenceType of Dependency {0} could not be deserialized.", name),
                        Logging.WarningLevel.Moderate);
                }

            }
        
            INodeLibraryDependencyInfo depInfo;
            //select correct constructor based on referenceType
            switch (parsedType)
            {
                case ReferenceType.Package:
                    depInfo = new PackageDependencyInfo(name, version);
                    break;
                /*TODO add other cases: for example
                 * case ReferenceType.ZeroTouch
                 * depInfp = new ZeroTouchDependencyInfo(name,version)
                */
                default:
                    depInfo = new PackageDependencyInfo(name, version);
                    break;
            }

           

            // Try get dependent node IDs
            var nodes = obj["Nodes"].Values<string>();
            foreach(var nodeID in nodes)
            {
                Guid guid;
                if (!Guid.TryParse(nodeID, out guid))
                {
                    logger.LogWarning(
                    string.Format("The id ({0}) of a node dependent on {1} could not be parsed as a GUID.", nodeID, name),
                    Logging.WarningLevel.Moderate);
                }
                else
                {
                    depInfo.AddDependent(guid);
                }
            }
            return depInfo;
        }
    }

    /// <summary>
    /// DummyNodeWriteConverter is used for serializing DummyNodes to JSON.
    /// Note that the DummyNode objects serialize as their original content and not as a DummyNode.
    /// </summary>
    public class DummyNodeWriteConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DummyNode).IsAssignableFrom(objectType);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dummyNode = value as DummyNode;
            if (dummyNode == null)
                throw new ArgumentException("value is not a DummyNode.");

            if (dummyNode.OriginalNodeContent == null)
                throw new ArgumentException("There is no content to write for this DummyNode.");

            JObject originalContent = dummyNode.OriginalNodeContent as JObject;
            if (originalContent == null)
                throw new ArgumentException("originalContent is not JSON compatible.");

            originalContent.WriteTo(writer);
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
        private Logging.ILogger logger;
        
        /// <summary>
        /// Constructs a ConnectorConverter.
        /// </summary>
        /// <param name="logger"></param>
        public ConnectorConverter(Logging.ILogger logger)
        {
            this.logger = logger;
        }

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

            // If the id is not a guid, makes a guid based on the id of the model
            Guid connectorId = GuidUtility.tryParseOrCreateGuid(obj["Id"].Value<string>());
            if(startPort != null && endPort != null)
            {
                return new ConnectorModel(startPort, endPort, connectorId);
            }
            else
            {
                if (this.logger!=null)
                {
                    this.logger.LogWarning(
                       string.Format("connector {0} could not be created, start or end port does not exist", connectorId),
                       Logging.WarningLevel.Moderate);

                }
                
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var connector = (ConnectorModel)value;

            writer.WriteStartObject();
            writer.WritePropertyName("Start");
            writer.WriteValue(connector.Start.GUID.ToString("N"));
            writer.WritePropertyName("End");
            writer.WriteValue(connector.End.GUID.ToString("N"));
            writer.WritePropertyName("Id");
            writer.WriteValue(connector.GUID.ToString("N"));
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
            writer.WriteValue(((Guid)value).ToString("N"));
        }
    }

    /// <summary>
    /// This converter is used to attempt to convert TypedParameter into a json object
    /// </summary>
    public class TypedParameterConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TypedParameter);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Name");
            writer.WriteValue(((TypedParameter)value).Name);
            writer.WritePropertyName("TypeName");
            writer.WriteValue(((TypedParameter)value).Type.Name);
            writer.WritePropertyName("TypeRank");
            writer.WriteValue(((TypedParameter)value).Type.rank);
            writer.WritePropertyName("DefaultValue");
            writer.WriteValue(((TypedParameter)value).DefaultValueString);
            writer.WritePropertyName("Description");
            writer.WriteValue(((TypedParameter)value).Summary);
            writer.WriteEndObject();
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
                // If this is not a guid, it won't be in the resolver.
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
                // If this is not a guid, it won't be in the resolver.
                Console.WriteLine("not a guid");
                return null;
            }
            object model;
            modelMap.TryGetValue(id, out model);

            return model;
        }
    }
}

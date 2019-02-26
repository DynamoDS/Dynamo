using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.Exceptions;
using ProtoCore.Lang;
using ProtoCore.Lang.Replication;
using ProtoCore.Utils;
using StackFrame = ProtoCore.DSASM.StackFrame;
using ProtoCore.Properties;
using ProtoCore.Runtime;
using WarningID = ProtoCore.Runtime.WarningID;
using System.Collections.ObjectModel;

namespace ProtoCore
{
    public class CallSite
    {
        #region private classes

        /// <summary>
        /// Trace data for a specific callsite
        /// </summary>
        public class RawTraceData
        {
            public string ID
            {
                get; private set;
            }

            public string Data
            {
                get; private set;
            }

            public RawTraceData(string callSiteID, string data)
            {
                ID = callSiteID;
                Data = data;
            }
        }

        /// <summary>
        /// Data structure used to carry trace data
        /// </summary>
        public class SingleRunTraceData
        {
            internal SingleRunTraceData() { }

            /// <summary>
            /// Does this struct contain any trace data
            /// </summary>
            public bool IsEmpty
            {
                get { return Data == null && NestedData == null; }
            }

            /// <summary>
            /// Is there any data anywhere in this run data, or is it all
            /// empty structure
            /// </summary>
            public bool HasAnyNestedData
            {
                get
                {
                    //Base case
                    if (IsEmpty)
                        return false;

                    if (HasData)
                        return true;
                    else
                    {
                        //Not empty, and doesn't have data so test recursive
                        Validity.Assert(NestedData != null,
                            "Invalid recursion logic, this is a VM bug, please report to the Dynamo Team");

                        return NestedData.Any(srtd => srtd.HasAnyNestedData);
                    }
                }
            }

            public bool HasNestedData
            {
                get { return NestedData != null; }
            }

            public bool HasData
            {
                get { return Data != null; }
            }

            internal static SingleRunTraceData DeserialseFromData(SerializationInfo info, StreamingContext context, int objectID, string marker)
            {
                SingleRunTraceData srtd = new SingleRunTraceData();

                bool hasData = info.GetBoolean(marker + objectID + "_HasData");

                if (hasData)
                {
                    Byte[] data = Convert.FromBase64String(info.GetString(marker + objectID + "_Data"));

                    IFormatter formatter = new SoapFormatter();
                    MemoryStream s = new MemoryStream(data);
                    formatter.Binder = new TraceBinder();
                    srtd.Data = (ISerializable)formatter.Deserialize(s);
                }

                bool hasNestedData = info.GetBoolean(marker + objectID + "_HasNestedData");

                if (hasNestedData)
                {

                    int nestedDataCount = info.GetInt32(marker + objectID + "_NestedDataCount");

                    if (nestedDataCount > 0)
                        srtd.NestedData = new List<SingleRunTraceData>();

                    for (int i = 0; i < nestedDataCount; i++)
                    {
                        srtd.NestedData.Add(
                            DeserialseFromData(info, context, i, marker + objectID + "-")
                            );
                    }

                }

                return srtd;
            }

            internal void GetObjectData(SerializationInfo info, StreamingContext context, int objectID, string marker)
            {
                info.AddValue(marker + objectID + "_HasData", HasData);

                if (HasData)
                {
                    //Serialise the object
                    using (MemoryStream s = new MemoryStream())
                    {
                        IFormatter formatter = new SoapFormatter();
                        formatter.Serialize(s, Data);
                        info.AddValue(marker + objectID + "_Data", Convert.ToBase64String(s.ToArray()));
                    }
                }

                info.AddValue(marker + objectID + "_HasNestedData", HasNestedData);

                if (HasNestedData)
                {
                    //Recursive Serialise
                    info.AddValue(marker + objectID + "_NestedDataCount", NestedData.Count);

                    for (int i = 0; i < NestedData.Count; i++)
                        NestedData[i].GetObjectData(info, context, i, marker + objectID + "-");
                }


            }

            /// <summary>
            /// This gets the zero-most, left most index
            /// null if no data
            /// </summary>
            /// <returns></returns>
            public ISerializable GetLeftMostData()
            {
                if (HasData)
                    return Data;
                else
                {
                    if (!HasNestedData)
                        return null;
                    else
                    {
#if DEBUG

                        Validity.Assert(NestedData != null, "Nested data has changed null status since last check, suspected race");
                        Validity.Assert(NestedData.Count > 0, "Empty subnested array, please file repo data with @lukechurch, relates to MAGN-4059");
#endif

                        //Safety trap to protect against an empty array, need repro test to figure out why this is getting set with empty arrays
                        if (NestedData.Count == 0)
                            return null;

                        SingleRunTraceData nestedTraceData = NestedData[0];
                        return nestedTraceData.GetLeftMostData();
                    }
                }
            }

            public List<SingleRunTraceData> NestedData;
            public ISerializable Data;

            public bool Contains(ISerializable data)
            {
                if (HasData)
                {
                    if (Data.Equals(data))
                    {
                        return true;
                    }
                }

                if (HasNestedData)
                {
                    foreach (SingleRunTraceData srtd in NestedData)
                    {
                        if (srtd.Contains(data))
                            return true;
                    }
                }

                return false;
            }

            public List<ISerializable> RecursiveGetNestedData()
            {
                List<ISerializable> ret = new List<ISerializable>();

                if (HasData)
                    ret.Add(Data);

                if (HasNestedData)
                {
                    foreach (SingleRunTraceData srtd in NestedData)
                        ret.AddRange(srtd.RecursiveGetNestedData());
                }

                return ret;
            }
        }

        /// <summary>
        /// TraceBinder is used to find assemblies to be used for
        /// deserialization in cases where the exact assemlby that was
        /// used in the serialization is not available. 
        /// </summary>
        internal class TraceBinder : SerializationBinder
        {
            // Use a custom serialization binder to make the serializer more permissive
            // http://www.codeproject.com/Articles/11079/NET-XML-and-SOAP-Serialization-Samples-Tips

            public override System.Type BindToType(string assemblyName, string typeName)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
                var assemblyNameObj = new AssemblyName(assemblyName);
                //find matching assemblies by name, version is not used.
                var matchingAssembly = assemblies.FirstOrDefault(x => x.GetName().Name == assemblyNameObj.Name);
                if(matchingAssembly!= null)
                {
                   var matchingType = matchingAssembly.GetType(typeName);
                    if(matchingType != null)
                    {
                        return matchingType;
                    }
                }
                //if there was no matching assembly, or type, try all assemblies and all types.
                //TODO(DYN-1594 - remove this fallback when we can determine if it is required. It is very slow.)
                var types = new List<System.Type>();
                foreach (var a in assemblies)
                {
                    try
                    {
                        types.AddRange(a.GetTypes());
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // We ignore assembly loading exceptions that are thrown here when their dependencies cannot be found
                    }
                }
                var result = types.FirstOrDefault(t => t.FullName == typeName);

                return result;
            }
        }

        /// <summary>
        /// Helper class that complies with the standard serialization contract that
        /// can be used for loading and saving the trace data
        /// Normal usage patten is:
        /// 1. Instantiate
        /// 2. Push Trace data from callsite
        /// 3. Call GetObjectData to serialise it onto a stream
        /// 4. Recreate using the special constructor
        /// </summary>
        [Serializable]
        private class TraceSerialiserHelper : ISerializable
        {
            /// <summary>
            /// Empty defaul
            /// </summary>
            public TraceSerialiserHelper()
            {

            }

            /// <summary>
            /// Load the data out of the serialisation entries
            /// </summary>
            public TraceSerialiserHelper(SerializationInfo info, StreamingContext context)
            {
                TraceData = new List<SingleRunTraceData>();

                int noElements = info.GetInt32("NumberOfElements");
                for (int i = 0; i < noElements; i++)
                {
                    try
                    {
                        SingleRunTraceData srtd = SingleRunTraceData.DeserialseFromData(
                            info, context, i, "Base-");
                        TraceData.Add(srtd);
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // If deserialization fails, continue to the next 
                        // element. Deserialization will throw an exception in
                        // contexts where the assembly used do do the serialization,
                        // or any of its referenced assemblies cannot be resolved.
#if DEBUG
                        Debug.WriteLine("Deserialization of trace data failed.");
#endif
                        continue;
                    }
                }

            }

            /// <summary>
            /// Save the data into the standard serialisation pattern
            /// </summary>
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("NumberOfElements", TraceData.Count);
                for (int i = 0; i < TraceData.Count; i++)
                {
                    TraceData[i].GetObjectData(info, context, i, "Base-");
                }
            }

            /// <summary>
            /// Create a TraceSerialiserHelper from CallSiteData.
            /// </summary>
            /// <param name="callSiteData">A string repsenting the CallSiteData </param>
            /// <returns>A TraceSerialiserHelper or null if deserialization fails.</returns>
            internal static TraceSerialiserHelper FromCallSiteData(string callSiteData)
            {
                try
                {
                    Validity.Assert(!String.IsNullOrEmpty(callSiteData));
                    var data = Convert.FromBase64String(callSiteData);
                    var formatter = new SoapFormatter();
                    formatter.Binder = new TraceBinder();
                    var s = new MemoryStream(data);
                    var helper = (TraceSerialiserHelper)formatter.Deserialize(s);
                    return helper;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("Constructing a TraceSerialiserHelper from CallSiteData failed.");
                    Debug.WriteLine(ex.Message);
#endif
                    return null;
                }

            }

            public List<SingleRunTraceData> TraceData { get; set; }
        }

        #endregion

        #region private members

        private int classScope;
        private string methodName;
        private readonly FunctionTable globalFunctionTable;
        private int invokeCount; //Number of times the callsite has been executed within this run
        private List<ISerializable> beforeFirstRunSerializables = new List<ISerializable>();

        //TODO(Luke): This should be loaded from the attribute
        private string TRACE_KEY = TraceUtils.__TEMP_REVIT_TRACE_ID;

        #endregion

        #region public properties

        /// <summary>
        /// The method group name that is associated with this function
        /// </summary>
        public String MethodName { get { return methodName; } }

        private List<SingleRunTraceData> traceData = new List<SingleRunTraceData>();
        public List<SingleRunTraceData> TraceData
        {
            get
            {
                return traceData;
            }
            private set
            {
                traceData = value;
            }
        }

        private Guid callsiteID = Guid.Empty;
        public Guid CallSiteID
        {
            get
            {
                return callsiteID;
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Constructs an instance of the CallSite object given its scope and 
        /// method information. This constructor optionally takes in a preloaded
        /// trace data information.
        /// </summary>
        /// <param name="classScope"></param>
        /// <param name="methodName"></param>
        /// <param name="globalFunctionTable"></param>
        /// <param name="execMode"></param>
        /// <param name="serializedTraceData">An optional Base64 encoded string
        /// representing the trace data that the callsite could use as part of 
        /// its re-construction.</param>
        /// 
        public CallSite(int classScope, string methodName,
            FunctionTable globalFunctionTable,
            ExecutionMode execMode, string serializedTraceData = null)
        {
            //Set the ID of internal test
            callsiteID = Guid.NewGuid();

            Validity.Assert(methodName != null);
            Validity.Assert(globalFunctionTable != null);

            this.classScope = classScope;
            this.methodName = methodName;
            this.globalFunctionTable = globalFunctionTable;

            if (execMode == ExecutionMode.Parallel)
                throw new CompilerInternalException(
                    "Parrallel Mode is not yet implemented {46F83CBB-9D37-444F-BA43-5E662784B1B3}");

            // Found preloaded trace data, reconstruct the instances from there.
            if (!String.IsNullOrEmpty(serializedTraceData))
            {
                LoadSerializedDataIntoTraceCache(serializedTraceData);

            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Load the serialised data provided into this callsite's trace cache
        /// </summary>
        /// <param name="serializedTraceData">The data to load</param>
        public void LoadSerializedDataIntoTraceCache(string serializedTraceData)
        {
            var helper = TraceSerialiserHelper.FromCallSiteData(serializedTraceData);
            if (helper == null)
            {
                beforeFirstRunSerializables = new List<ISerializable>();
                return;
            }

            this.traceData = helper.TraceData;

            // Cache the historical trace data for comparison
            // when graph update is complete. This data will be cleared
            // after the first reconciliation.
            beforeFirstRunSerializables = traceData.SelectMany(td => td.RecursiveGetNestedData()).ToList();
        }

        public void UpdateCallSite(int classScope, string methodName)
        {
            this.classScope = classScope;
            this.methodName = methodName;
        }

        /// <summary>
        /// Conservative guess as to whether this call will replicate or not
        /// This may give inaccurate answers if the node cluster doesn't actually exist
        /// </summary>
        /// <param name="context"></param>
        /// <param name="arguments"></param>
        /// <param name="stackFrame"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public bool WillCallReplicate(Context context, List<StackValue> arguments,
                                      List<List<ReplicationGuide>> partialReplicationGuides, StackFrame stackFrame, RuntimeCore runtimeCore,
                                      out List<List<ReplicationInstruction>> replicationTrials)
        {
            replicationTrials = new List<List<ReplicationInstruction>>();

            if (partialReplicationGuides.Count > 0)
            {
                // Jun Comment: And at least one of them contains somthing
                for (int n = 0; n < partialReplicationGuides.Count; ++n)
                {
                    if (partialReplicationGuides[n].Count > 0)
                    {
                        return true;
                    }
                }
            }

            #region Get Function Group

            //@PERF: Possible optimisation point here, to deal with static dispatches that don't need replication analysis
            //Handle resolution Pass 1: Name -> Method Group
            FunctionGroup funcGroup;
            try
            {
                funcGroup = globalFunctionTable.GlobalFuncTable[classScope + 1][methodName];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            #endregion

            //Replication Control is an ordered list of the elements that we have to replicate over
            //Ordering implies containment, so element 0 is the outer most forloop, element 1 is nested within it etc.
            //Take the explicit replication guides and build the replication structure
            //Turn the replication guides into a guide -> List args data structure
            var instructions = Replicator.BuildPartialReplicationInstructions(partialReplicationGuides);

            #region First Case: Replicate only according to the replication guides
            {
                FunctionEndPoint fep = GetCompleteMatchFunctionEndPoint(context, arguments, funcGroup, instructions, stackFrame, runtimeCore);
                if (fep != null)
                {
                    //found an exact match
                    return false;
                }
            }

            #endregion

            #region Case 2: Replication with no type cast
            {
                //Build the possible ways in which we might replicate
                replicationTrials = Replicator.BuildReplicationCombinations(instructions, arguments, runtimeCore);

                foreach (List<ReplicationInstruction> replicationOption in replicationTrials)
                {
                    List<List<StackValue>> reducedParams = Replicator.ComputeAllReducedParams(arguments, replicationOption, runtimeCore);
                    HashSet<FunctionEndPoint> lookups;
                    if (funcGroup.CanGetExactMatchStatics(context, reducedParams, stackFrame, runtimeCore, out lookups))
                    {
                        return true; //Replicates against cluster
                    }
                }
            }

            #endregion

            #region Case 3: Match with type conversion, but no array promotion

            {
                Dictionary<FunctionEndPoint, int> candidatesWithDistances =
                    funcGroup.GetConversionDistances(context, arguments, instructions,
                                                     runtimeCore.DSExecutable.classTable, runtimeCore);
                Dictionary<FunctionEndPoint, int> candidatesWithCastDistances =
                    funcGroup.GetCastDistances(context, arguments, instructions, runtimeCore.DSExecutable.classTable,
                                               runtimeCore);

                List<FunctionEndPoint> candidateFunctions = GetCandidateFunctions(stackFrame, candidatesWithDistances);
                FunctionEndPoint compliantTarget = GetCompliantTarget(context, arguments,
                                                                      instructions, stackFrame, runtimeCore,
                                                                      candidatesWithCastDistances, candidateFunctions,
                                                                      candidatesWithDistances);

                if (compliantTarget != null)
                {
                    return false; //Type conversion but no replication
                }
            }

            #endregion

            #region Case 5: Match with type conversion, replication and array promotion

            {
                //Build the possible ways in which we might replicate
                replicationTrials =
                    Replicator.BuildReplicationCombinations(instructions, arguments, runtimeCore);

                //Add as a first attempt a no-replication, but allowing up-promoting
                replicationTrials.Insert(0,
                                         new List<ReplicationInstruction>()
                    );
            }

            #endregion

            return true; //It'll replicate if it suceeds
        }

        /// <summary>
        /// Call this method to obtain the Base64 encoded string that 
        /// represent this instance of CallSite;s trace data
        /// </summary>
        /// <returns>Returns the Base64 encoded string that represents the
        /// trace data of this callsite
        /// </returns>
        /// 
        public string GetTraceDataToSave()
        {
            //Test to see if there is any actual data in the trace cache
            if (!this.traceData.Any(srtd => srtd.HasAnyNestedData))
                return null;

            TraceSerialiserHelper helper = new TraceSerialiserHelper();
            helper.TraceData = this.traceData;

            using (MemoryStream memoryStream = new MemoryStream())
            {

                IFormatter formatter = new SoapFormatter();
                formatter.Serialize(memoryStream, helper);

                return Convert.ToBase64String(memoryStream.ToArray());
            }

        }

        /// <summary>
        /// Returns all serializables that were created historically, but
        /// were not re-created in the most recent graph update.
        /// </summary>
        public IList<ISerializable> GetOrphanedSerializables()
        {
            var result = new List<ISerializable>();

            if (!beforeFirstRunSerializables.Any())
                return result;

            var currentSerializables = traceData.SelectMany(td => td.RecursiveGetNestedData());
            result.AddRange(beforeFirstRunSerializables.Where(hs => !currentSerializables.Contains(hs)).ToList());

            // Clear the historical serializable to avoid 
            // them being used again. 
            beforeFirstRunSerializables.Clear();

            return result;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Report that whole function group couldn't be found
        /// </summary>
        /// <param name="core"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private StackValue ReportFunctionGroupNotFound(RuntimeCore runtimeCore, List<StackValue> arguments)
        {
            runtimeCore.RuntimeStatus.LogFunctionGroupNotFoundWarning(methodName, classScope, arguments);
            return StackValue.Null;
        }

        /// <summary>
        /// Internal support method for reporting a method that can't be located
        /// </summary>
        /// <returns></returns>
        private StackValue ReportMethodNotFoundForArguments(RuntimeCore runtimeCore, FunctionGroup funcGroup, List<StackValue> arguments)
        {
            runtimeCore.RuntimeStatus.LogMethodResolutionWarning(funcGroup, methodName, classScope, arguments);
            return StackValue.Null;
        }

        private StackValue ReportMethodNotAccessible(RuntimeCore runtimeCore)
        {
            runtimeCore.RuntimeStatus.LogMethodNotAccessibleWarning(methodName);
            return StackValue.Null;
        }

        /// <summary>
        /// Minimal sanity check of arugments
        /// </summary>
        /// <param name="arguments"></param>
        private static void ArgumentSanityCheck(List<StackValue> arguments)
        {
            //Minimal sanity check
            foreach (StackValue sv in arguments)
            {
                Validity.Assert(sv.metaData.type != (int)PrimitiveType.InvalidType,
                                "Invalid object passed to JILDispatch");

                Validity.Assert(!sv.IsInvalid,
                                "Invalid object passed to JILDispatch");
            }
        }

        /// <summary>
        ///  This function handles generating a unique callsite ID and serializing the data associated with this callsite
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="core"></param>
        private void UpdateCallsiteExecutionState(Object callsiteData, RuntimeCore runtimeCore)
        {
            invokeCount = 0;

            /*
            if (core.EnableCallsiteExecutionState)
            {
                // Get the uid of this function call
                int exprID = core.RuntimeExpressionUID;
                string callsiteGUID = ProtoCore.CallsiteExecutionState.GetCallsiteGUID(methodName, exprID);

                bool isAutogeneratedFunction = ProtoCore.Utils.CoreUtils.IsCompilerGenerated(methodName);
                if (!isAutogeneratedFunction)
                {
                    // Store the data associated with this callsite
                    runID = core.csExecutionState.StoreAndUpdateRunId(callsiteGUID, callsiteData);
                }
            }*/
        }

        #endregion

        #region Target resolution

        private FunctionEndPoint GetCompliantFEP(
            Context context,
            List<StackValue> arguments,
            FunctionGroup funcGroup,
            List<ReplicationInstruction> replicationInstructions,
            StackFrame stackFrame,
            RuntimeCore runtimeCore,
            bool allowArrayPromotion = false)
        {
            Dictionary<FunctionEndPoint, int> candidatesWithDistances =
                funcGroup.GetConversionDistances(
                    context,
                    arguments,
                    replicationInstructions,
                    runtimeCore.DSExecutable.classTable,
                    runtimeCore,
                    allowArrayPromotion);

            Dictionary<FunctionEndPoint, int> candidatesWithCastDistances =
                funcGroup.GetCastDistances(
                    context,
                    arguments,
                    replicationInstructions,
                    runtimeCore.DSExecutable.classTable,
                    runtimeCore);

            List<FunctionEndPoint> candidateFunctions = GetCandidateFunctions(stackFrame, candidatesWithDistances);

            FunctionEndPoint compliantTarget =
                GetCompliantTarget(
                    context,
                    arguments,
                    replicationInstructions,
                    stackFrame,
                    runtimeCore,
                    candidatesWithCastDistances,
                    candidateFunctions,
                    candidatesWithDistances);

            return compliantTarget;
        }

        private FunctionEndPoint GetLooseCompliantFEP(
           Context context,
           List<StackValue> arguments,
           FunctionGroup funcGroup,
           List<ReplicationInstruction> replicationInstructions,
           StackFrame stackFrame,
           RuntimeCore runtimeCore)
        {
            Dictionary<FunctionEndPoint, int> candidatesWithDistances =
                funcGroup.GetLooseConversionDistances(
                    context,
                    arguments,
                    replicationInstructions,
                    runtimeCore.DSExecutable.classTable,
                    runtimeCore);

            Dictionary<FunctionEndPoint, int> candidatesWithCastDistances =
                funcGroup.GetCastDistances(
                    context,
                    arguments,
                    replicationInstructions,
                    runtimeCore.DSExecutable.classTable,
                    runtimeCore);

            List<FunctionEndPoint> candidateFunctions = GetCandidateFunctions(stackFrame, candidatesWithDistances);

            FunctionEndPoint compliantTarget =
                GetCompliantTarget(
                    context,
                    arguments,
                    replicationInstructions,
                    stackFrame,
                    runtimeCore,
                    candidatesWithCastDistances,
                    candidateFunctions,
                    candidatesWithDistances);

            return compliantTarget;
        }

        private void ComputeFeps(
            Context context,
            List<StackValue> arguments,
            FunctionGroup funcGroup,
            List<ReplicationInstruction> instructions,
            StackFrame stackFrame,
            RuntimeCore runtimeCore,
            out List<FunctionEndPoint> resolvesFeps,
            out List<ReplicationInstruction> replicationInstructions)
        {
            #region Case 1: Replication guide with exact match 
            {
                FunctionEndPoint fep = GetCompleteMatchFunctionEndPoint(context, arguments, funcGroup, instructions, stackFrame, runtimeCore);
                if (fep != null)
                {
                    resolvesFeps = new List<FunctionEndPoint>() { fep };
                    replicationInstructions = instructions;
                    return;
                }
            }
            #endregion

            var replicationTrials = Replicator.BuildReplicationCombinations(instructions, arguments, runtimeCore);

            #region Case 2: Replication and replication guide with exact match
            {
                //Build the possible ways in which we might replicate
                foreach (List<ReplicationInstruction> replicationOption in replicationTrials)
                {
                    List<List<StackValue>> reducedParams = Replicator.ComputeAllReducedParams(arguments, replicationOption, runtimeCore);
                    HashSet<FunctionEndPoint> lookups;
                    if (funcGroup.CanGetExactMatchStatics(context, reducedParams, stackFrame, runtimeCore, out lookups))
                    {
                        //Otherwise we have a cluster of FEPs that can be used to dispatch the array
                        resolvesFeps = new List<FunctionEndPoint>(lookups);
                        replicationInstructions = replicationOption;
                        return;
                    }
                }

            }
            #endregion

            #region Case 3: Replication with type conversion
            {
                FunctionEndPoint compliantTarget = GetCompliantFEP(context, arguments, funcGroup, instructions, stackFrame, runtimeCore);
                if (compliantTarget != null)
                {
                    resolvesFeps = new List<FunctionEndPoint>() { compliantTarget };
                    replicationInstructions = instructions;
                    return;
                }
            }
            #endregion

            #region Case 4: Replication and replication guide with type conversion
            {
                if (arguments.Any(arg => arg.IsArray))
                {
                    foreach (List<ReplicationInstruction> replicationOption in replicationTrials)
                    {
                        FunctionEndPoint compliantTarget = GetCompliantFEP(context, arguments, funcGroup, replicationOption, stackFrame, runtimeCore);
                        if (compliantTarget != null)
                        {
                            resolvesFeps = new List<FunctionEndPoint>() { compliantTarget };
                            replicationInstructions = replicationOption;
                            return;
                        }
                    }
                }
            }

            #endregion

            #region Case 5: Replication and replciation guide with type conversion and array promotion
            {
                //Add as a first attempt a no-replication, but allowing up-promoting
                replicationTrials.Add(new List<ReplicationInstruction>());

                foreach (List<ReplicationInstruction> replicationOption in replicationTrials)
                {
                    FunctionEndPoint compliantTarget = GetCompliantFEP(context, arguments, funcGroup, replicationOption, stackFrame, runtimeCore, true);
                    if (compliantTarget != null)
                    {
                        resolvesFeps = new List<FunctionEndPoint>() { compliantTarget };
                        replicationInstructions = replicationOption;
                        return;
                    }
                }
            }
            #endregion

            #region Case 6: Replication and replication guide with type conversion and array promotion, and OK if not all convertible
            {
                foreach (List<ReplicationInstruction> replicationOption in replicationTrials)
                {
                    FunctionEndPoint compliantTarget = GetLooseCompliantFEP(context, arguments, funcGroup, replicationOption, stackFrame, runtimeCore);
                    if (compliantTarget != null)
                    {
                        resolvesFeps = new List<FunctionEndPoint>() { compliantTarget };
                        replicationInstructions = replicationOption;
                        return;
                    }
                }
            }
            #endregion

            resolvesFeps = new List<FunctionEndPoint>();
            replicationInstructions = instructions;
        }

        private bool IsFunctionGroupAccessible(RuntimeCore runtimeCore, ref FunctionGroup funcGroup)
        {
            bool methodAccessible = true;
            if (classScope != Constants.kGlobalScope)
            {
                // If last stack frame is not member function, then only public 
                // functions are acessible in this context. 
                int callerci, callerfi;
                runtimeCore.CurrentExecutive.CurrentDSASMExec.GetCallerInformation(out callerci, out callerfi);
                if (callerci == Constants.kGlobalScope ||
                    (classScope != callerci && !runtimeCore.DSExecutable.classTable.ClassNodes[classScope].IsMyBase(callerci)))
                {
                    bool hasFEP = funcGroup.FunctionEndPoints.Count > 0;
                    FunctionGroup visibleFuncGroup = new FunctionGroup();
                    visibleFuncGroup.CopyPublic(funcGroup.FunctionEndPoints);
                    funcGroup = visibleFuncGroup;

                    if (hasFEP && funcGroup.FunctionEndPoints.Count == 0)
                    {
                        methodAccessible = false;
                    }
                }
            }
            return methodAccessible;
        }

        /// <summary>
        /// Returns complete match attempts to locate a function endpoint where 1 FEP matches all of the requirements for dispatch
        /// </summary>
        private FunctionEndPoint GetCompleteMatchFunctionEndPoint(
            Context context, List<StackValue> arguments,
            FunctionGroup funcGroup,
            List<ReplicationInstruction> replicationInstructions,
            StackFrame stackFrame,
            RuntimeCore runtimeCore)
        {
            //Exact match
            var exactTypeMatchingCandindates = funcGroup.GetExactTypeMatches(context, arguments, replicationInstructions, stackFrame, runtimeCore);

            if (exactTypeMatchingCandindates.Count == 0)
            {
                return null;
            }

            FunctionEndPoint fep = null;
            if (exactTypeMatchingCandindates.Count == 1)
            {
                //Exact match
                fep = exactTypeMatchingCandindates[0];
            }
            else
            {
                //Exact match with upcast
                fep = SelectFEPFromMultiple(stackFrame, runtimeCore, exactTypeMatchingCandindates, arguments);
            }

            return fep;
        }


        private bool Inherits(ReadOnlyCollection<ClassNode> classNodes, int parentIndex, int childIndex)
        {
            if (parentIndex < 0 || parentIndex >= classNodes.Count || childIndex < 0 || childIndex >= classNodes.Count)
            {
                return false;
            }

            return Inherits(classNodes, classNodes[parentIndex], classNodes[childIndex]);
        }

        private bool Inherits(ReadOnlyCollection<ClassNode> classNodes, ClassNode parent, ClassNode child)
        {
            if (child == parent)
            {
                return true;
            }

            if (child.Base != Constants.kInvalidIndex)
            {
                var baseClassNode = classNodes[child.Base];
                if (Inherits(classNodes, parent, baseClassNode))
                {
                    return true;
                }
            }

            foreach (var interf in child.Interfaces)
            {
                var interfNode = classNodes[interf];
                if (Inherits(classNodes, parent, interfNode))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the function group associated with this callsite
        /// </summary>
        private FunctionGroup GetFuncGroup(RuntimeCore runtimeCore, List<StackValue> arguments)
        {
            // try to use dynamic classScope
            if (arguments.Count > 0)
            {
                var firstArg = arguments.First();

                var firstNonArray = firstArg;
                if (firstArg.IsArray && ArrayUtils.GetFirstNonArrayStackValue(firstArg, ref firstNonArray, runtimeCore))
                {
                    firstArg = firstNonArray;
                }

                if (firstArg.IsPointer && firstArg.metaData.type != classScope)
                {
                    if (Inherits(runtimeCore.DSExecutable.classTable.ClassNodes, classScope, firstArg.metaData.type))
                    {
                        var fg = FirstFunctionGroupInInheritanceChain(runtimeCore, firstArg.metaData.type);
                        if (fg != null)
                        {
                            return fg;
                        }
                    }
                }
            }

            // use static classScope
            return FirstFunctionGroupInInheritanceChain(runtimeCore, classScope);
        }

        private FunctionGroup FirstFunctionGroupInInheritanceChain(RuntimeCore runtimeCore, int cidx)
        {
            FunctionGroup funcGroup = null;
            var classNodes = runtimeCore.DSExecutable.classTable.ClassNodes;
            var globalFuncTable = globalFunctionTable.GlobalFuncTable;

            do
            {
                if (globalFuncTable[cidx + 1].TryGetValue(methodName, out funcGroup))
                {
                    break;
                }
            } while ((cidx = classNodes[cidx].Base) != Constants.kInvalidIndex);

            return funcGroup;
        }

        private FunctionEndPoint SelectFEPFromMultiple(StackFrame stackFrame, RuntimeCore runtimeCore,
                                                       List<FunctionEndPoint> feps, List<StackValue> argumentsList)
        {
            StackValue svThisPtr = stackFrame.ThisPtr;
            Validity.Assert(svThisPtr.IsPointer,
                            "this pointer wasn't a pointer. {89635B06-AD53-4170-ADA5-065EB2AE5858}");

            int typeID = svThisPtr.metaData.type;

            //Test for exact match
            List<FunctionEndPoint> exactFeps = new List<FunctionEndPoint>();

            foreach (FunctionEndPoint fep in feps)
                if (fep.ClassOwnerIndex == typeID)
                    exactFeps.Add(fep);

            if (exactFeps.Count == 1)
            {
                return exactFeps[0];
            }


            //Walk the class tree structure to find the method

            while (runtimeCore.DSExecutable.classTable.ClassNodes[typeID].Base != Constants.kInvalidIndex)
            {
                typeID = runtimeCore.DSExecutable.classTable.ClassNodes[typeID].Base;

                foreach (FunctionEndPoint fep in feps)
                    if (fep.ClassOwnerIndex == typeID)
                        return fep;
            }

            //We weren't able to distinguish based on class hiearchy, try to sepearete based on array ranking
            List<int> numberOfArbitraryRanks = new List<int>();

            foreach (FunctionEndPoint fep in feps)
            {
                int noArbitraries = 0;

                for (int i = 0; i < argumentsList.Count; i++)
                {
                    if (fep.FormalParams[i].rank == Constants.kArbitraryRank)
                        noArbitraries++;

                    numberOfArbitraryRanks.Add(noArbitraries);
                }
            }

            int smallest = Int32.MaxValue;
            List<int> indeciesOfSmallest = new List<int>();

            for (int i = 0; i < feps.Count; i++)
            {
                if (numberOfArbitraryRanks[i] < smallest)
                {
                    smallest = numberOfArbitraryRanks[i];
                    indeciesOfSmallest.Clear();
                    indeciesOfSmallest.Add(i);
                }
                else if (numberOfArbitraryRanks[i] == smallest)
                    indeciesOfSmallest.Add(i);
            }

            Validity.Assert(indeciesOfSmallest.Count > 0,
                            "Couldn't find a fep when there should have been multiple: {EB589F55-F36B-404A-91DC-8D0EDC527E72}");

            if (indeciesOfSmallest.Count == 1)
                return feps[indeciesOfSmallest[0]];


            if (!CoreUtils.IsInternalMethod(feps[0].procedureNode.Name) || CoreUtils.IsGetterSetter(feps[0].procedureNode.Name))
            {
                //If this has failed, we have multiple feps, which can't be distiquished by class hiearchy. Emit a warning and select one
                StringBuilder possibleFuncs = new StringBuilder();
                possibleFuncs.Append(Resources.MultipleFunctionsFound);
                possibleFuncs.AppendLine();
                possibleFuncs.AppendLine();
                foreach (FunctionEndPoint fep in feps)
                    possibleFuncs.AppendLine("    " + fep.ToString());

                runtimeCore.RuntimeStatus.LogWarning(WarningID.AmbiguousMethodDispatch, possibleFuncs.ToString());
            }

            return feps[0];
        }

        private FunctionEndPoint GetCompliantTarget(Context context, List<StackValue> formalParams,
                                                    List<ReplicationInstruction> replicationControl,
                                                    StackFrame stackFrame, RuntimeCore runtimeCore,
                                                    Dictionary<FunctionEndPoint, int> candidatesWithCastDistances,
                                                    List<FunctionEndPoint> candidateFunctions,
                                                    Dictionary<FunctionEndPoint, int> candidatesWithDistances)
        {
            FunctionEndPoint compliantTarget = null;
            //Produce an ordered list of the graph costs
            Dictionary<int, List<FunctionEndPoint>> conversionCostList = new Dictionary<int, List<FunctionEndPoint>>();

            foreach (FunctionEndPoint fep in candidateFunctions)
            {
                int cost = candidatesWithDistances[fep];
                if (conversionCostList.ContainsKey(cost))
                    conversionCostList[cost].Add(fep);
                else
                    conversionCostList.Add(cost, new List<FunctionEndPoint> { fep });
            }

            List<int> conversionCosts = new List<int>(conversionCostList.Keys);
            conversionCosts.Sort();

            List<FunctionEndPoint> fepsToSplit = new List<FunctionEndPoint>();

            foreach (int cost in conversionCosts)
            {
                foreach (FunctionEndPoint funcFep in conversionCostList[cost])
                {
                    if (funcFep.DoesPredicateMatch(context, formalParams, replicationControl))
                    {
                        compliantTarget = funcFep;
                        fepsToSplit.Add(funcFep);
                    }
                }

                if (compliantTarget != null)
                    break;
            }

            if (fepsToSplit.Count > 1)
            {
                int lowestCost = candidatesWithCastDistances[fepsToSplit[0]];
                compliantTarget = fepsToSplit[0];

                List<FunctionEndPoint> lowestCostFeps = new List<FunctionEndPoint>();

                foreach (FunctionEndPoint fep in fepsToSplit)
                {
                    if (candidatesWithCastDistances[fep] < lowestCost)
                    {
                        lowestCost = candidatesWithCastDistances[fep];
                        compliantTarget = fep;
                        lowestCostFeps = new List<FunctionEndPoint>() { fep };
                    }
                    else if (candidatesWithCastDistances[fep] == lowestCost)
                    {
                        lowestCostFeps.Add(fep);
                    }
                }

                //We have multiple feps, e.g. form overriding
                if (lowestCostFeps.Count > 0)
                    compliantTarget = SelectFEPFromMultiple(stackFrame, runtimeCore, lowestCostFeps, formalParams);
            }
            return compliantTarget;
        }

        private List<FunctionEndPoint> GetCandidateFunctions(StackFrame stackFrame,
                                                             Dictionary<FunctionEndPoint, int> candidatesWithDistances)
        {
            List<FunctionEndPoint> candidateFunctions = new List<FunctionEndPoint>();

            foreach (FunctionEndPoint fep in candidatesWithDistances.Keys)
            {
                if ((stackFrame.ThisPtr.IsPointer &&
                     stackFrame.ThisPtr.Pointer == Constants.kInvalidIndex && fep.procedureNode != null
                     && !fep.procedureNode.IsConstructor) && !fep.procedureNode.IsStatic
                    && (fep.procedureNode.ClassID != -1))
                {
                    continue;
                }

                candidateFunctions.Add(fep);
            }
            return candidateFunctions;
        }

        private FunctionEndPoint SelectFinalFep(Context context,
                                                List<FunctionEndPoint> functionEndPoint,
                                                List<StackValue> formalParameters, StackFrame stackFrame, RuntimeCore runtimeCore)
        {
            List<ReplicationInstruction> replicationControl = new List<ReplicationInstruction>();
            //We're never going to replicate so create an empty structure to allow us to use
            //the existing utility methods

            //Filter for exact matches

            List<FunctionEndPoint> exactTypeMatchingCandindates = new List<FunctionEndPoint>();

            foreach (FunctionEndPoint possibleFep in functionEndPoint)
            {
                if (possibleFep.DoesTypeDeepMatch(formalParameters, runtimeCore))
                {
                    exactTypeMatchingCandindates.Add(possibleFep);
                }
            }


            //There was an exact match, so dispath to it
            if (exactTypeMatchingCandindates.Count > 0)
            {
                FunctionEndPoint fep = null;

                if (exactTypeMatchingCandindates.Count == 1)
                {
                    fep = exactTypeMatchingCandindates[0];
                }
                else
                {
                    fep = SelectFEPFromMultiple(stackFrame, runtimeCore,
                                                exactTypeMatchingCandindates, formalParameters);
                }

                return fep;
            }
            else
            {
                Dictionary<FunctionEndPoint, int> candidatesWithDistances = new Dictionary<FunctionEndPoint, int>();
                Dictionary<FunctionEndPoint, int> candidatesWithCastDistances = new Dictionary<FunctionEndPoint, int>();

                foreach (FunctionEndPoint fep in functionEndPoint)
                {
                    //@TODO(Luke): Is this value for allow array promotion correct?
                    int distance = fep.ComputeTypeDistance(formalParameters, runtimeCore.DSExecutable.classTable, runtimeCore, false);
                    if (distance !=
                        (int)ProcedureDistance.InvalidDistance)
                        candidatesWithDistances.Add(fep, distance);
                }

                foreach (FunctionEndPoint fep in functionEndPoint)
                {
                    int dist = fep.ComputeCastDistance(formalParameters, runtimeCore.DSExecutable.classTable, runtimeCore);
                    candidatesWithCastDistances.Add(fep, dist);
                }

                List<FunctionEndPoint> candidateFunctions = GetCandidateFunctions(stackFrame, candidatesWithDistances);

                if (candidateFunctions.Count == 0)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.AmbiguousMethodDispatch,
                                                  Resources.kAmbigousMethodDispatch);
                    return null;
                }


                FunctionEndPoint compliantTarget = GetCompliantTarget(context, formalParameters, replicationControl,
                                                                      stackFrame, runtimeCore, candidatesWithCastDistances,
                                                                      candidateFunctions, candidatesWithDistances);

                return compliantTarget;
            }
        }

        #endregion

        #region Execution methods
        //Inbound methods

        public StackValue JILDispatchViaNewInterpreter(
            Context context,
            List<StackValue> arguments,
            List<List<ReplicationGuide>> replicationGuides,
            DominantListStructure domintListStructure,
            StackFrame stackFrame, RuntimeCore runtimeCore)
        {
#if DEBUG
            ArgumentSanityCheck(arguments);
#endif
            // Dispatch method
            context.IsImplicitCall = true;
            return DispatchNew(context, arguments, replicationGuides, domintListStructure, stackFrame, runtimeCore);
        }

        public StackValue JILDispatch(
            List<StackValue> arguments,
            List<List<ReplicationGuide>> replicationGuides,
            DominantListStructure domintListStructure,
            StackFrame stackFrame,
            RuntimeCore runtimeCore,
            Context context)
        {
#if DEBUG

            ArgumentSanityCheck(arguments);
#endif
            // Dispatch method
            return DispatchNew(context, arguments, replicationGuides, domintListStructure, stackFrame, runtimeCore);
        }

        //Dispatch
        private StackValue DispatchNew(
            Context context,
            List<StackValue> arguments,
            List<List<ReplicationGuide>> partialReplicationGuides,
            DominantListStructure domintListStructure,
            StackFrame stackFrame, RuntimeCore runtimeCore)
        {
            // Update the CallsiteExecutionState with 
            // TODO: Replace this with the real data
            UpdateCallsiteExecutionState(null, runtimeCore);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            StringBuilder log = new StringBuilder();

            log.AppendLine("Method name: " + methodName);

            #region Get Function Group

            //@PERF: Possible optimisation point here, to deal with static dispatches that don't need replication analysis
            //Handle resolution Pass 1: Name -> Method Group
            FunctionGroup funcGroup = GetFuncGroup(runtimeCore, arguments);
            if (funcGroup == null)
            {
                log.AppendLine("Function group not located");
                log.AppendLine("Resolution failed in: " + sw.ElapsedMilliseconds);

                if (runtimeCore.Options.DumpFunctionResolverLogic)
                    runtimeCore.DSExecutable.EventSink.PrintMessage(log.ToString());

                return ReportFunctionGroupNotFound(runtimeCore, arguments);
            }

            //check accesibility of function group
            bool methodAccessible = IsFunctionGroupAccessible(runtimeCore, ref funcGroup);
            if (!methodAccessible)
            {
                return ReportMethodNotAccessible(runtimeCore);
            }

            //If we got here then the function group got resolved
            log.AppendLine("Function group resolved: " + funcGroup);

            // Filter function end point
            List<FunctionEndPoint> candidatesFeps = new List<FunctionEndPoint>();
            int argumentNumber = arguments.Count;
            foreach (var fep in funcGroup.FunctionEndPoints)
            {
                int defaultParamNumber = fep.procedureNode.ArgumentInfos.Count(x => x.IsDefault);
                int parameterNumber = fep.procedureNode.ArgumentTypes.Count;

                if (argumentNumber <= parameterNumber && parameterNumber - argumentNumber <= defaultParamNumber)
                {
                    candidatesFeps.Add(fep);
                }
            }
            funcGroup = new FunctionGroup(candidatesFeps);

            #endregion

            partialReplicationGuides = PerformRepGuideDemotion(arguments, partialReplicationGuides, runtimeCore);

            //Replication Control is an ordered list of the elements that we have to replicate over
            //Ordering implies containment, so element 0 is the outer most forloop, element 1 is nested within it etc.
            //Take the explicit replication guides and build the replication structure
            //Turn the replication guides into a guide -> List args data structure
            var partialInstructions = Replicator.BuildPartialReplicationInstructions(partialReplicationGuides);

            //Get the fep that are resolved
            List<FunctionEndPoint> resolvesFeps;
            List<ReplicationInstruction> replicationInstructions;

            ComputeFeps(context, arguments, funcGroup, partialInstructions, stackFrame, runtimeCore, out resolvesFeps, out replicationInstructions);

            if (resolvesFeps.Count == 0)
            {
                log.AppendLine("Resolution Failed");

                if (runtimeCore.Options.DumpFunctionResolverLogic)
                    runtimeCore.DSExecutable.EventSink.PrintMessage(log.ToString());

                return ReportMethodNotFoundForArguments(runtimeCore, funcGroup, arguments);
            }

            arguments.ForEach(x => runtimeCore.AddCallSiteGCRoot(CallSiteID, x));
            StackValue ret = Execute(resolvesFeps, context, arguments, replicationInstructions, stackFrame, runtimeCore);
            if (!ret.IsExplicitCall)
            {
                ret = AtLevelHandler.RestoreDominantStructure(ret, domintListStructure, replicationInstructions, runtimeCore);
            }
            runtimeCore.RemoveCallSiteGCRoot(CallSiteID);
            return ret;
        }


        private StackValue Execute(
            List<FunctionEndPoint> functionEndPoint,
            Context c,
            List<StackValue> formalParameters,
            List<ReplicationInstruction> replicationInstructions,
            StackFrame stackFrame,
            RuntimeCore runtimeCore)
        {
            SingleRunTraceData singleRunTraceData = (invokeCount < traceData.Count) ? traceData[invokeCount] : new SingleRunTraceData();
            SingleRunTraceData newTraceData = new SingleRunTraceData();
            StackValue ret;

            if (replicationInstructions.Count == 0)
            {
                c.IsReplicating = false;
                ret = ExecWithZeroRI(functionEndPoint, c, formalParameters, stackFrame, runtimeCore, singleRunTraceData, newTraceData);
            }
            else //replicated call
            {
                c.IsReplicating = true;
                ret = ExecWithRISlowPath(functionEndPoint, c, formalParameters, replicationInstructions, stackFrame, runtimeCore, singleRunTraceData, newTraceData);
            }

            //Do a trace save here
            if (invokeCount < traceData.Count)
            {
                traceData[invokeCount] = newTraceData;
            }
            else
            {
                traceData.Add(newTraceData);
            }

            invokeCount++; //We've completed this invocation
            return ret;
        }

        /// <summary>
        /// Execute an arbitrary depth replication using the full slow path algorithm
        /// </summary>
        /// <param name="functionEndPoint"></param>
        /// <param name="c"></param>
        /// <param name="formalParameters"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="stackFrame"></param>
        /// <param name="runtimeCore"></param>
        /// <param name="previousTraceData"></param>
        /// <param name="newTraceData"></param>
        /// <param name="finalFunctionEndPoint"></param>
        /// <returns></returns>
        private StackValue ExecWithRISlowPath(
            List<FunctionEndPoint> functionEndPoint,
            Context c,
            List<StackValue> formalParameters,
            List<ReplicationInstruction> replicationInstructions,
            StackFrame stackFrame,
            RuntimeCore runtimeCore,
            SingleRunTraceData previousTraceData,
            SingleRunTraceData newTraceData,
            FunctionEndPoint finalFunctionEndPoint = null)
        {
            if (runtimeCore.Options.ExecutionMode == ExecutionMode.Parallel)
                throw new NotImplementedException("Parallel mode disabled: {BF417AD5-9EA9-4292-ABBC-3526FC5A149E}");

            //Recursion base case
            if (replicationInstructions.Count == 0)
            {
                return ExecWithZeroRI(functionEndPoint, c, formalParameters, stackFrame, runtimeCore, previousTraceData, newTraceData, finalFunctionEndPoint);
            }

            //Get the replication instruction that this call will deal with
            ReplicationInstruction ri = replicationInstructions[0];

            if (ri.Zipped)
            {
                ZipAlgorithm algorithm = ri.ZipAlgorithm;

                //For each item in this plane, an array of the length of the minimum will be constructed

                //The size of the array will be the minimum size of the passed arrays
                List<int> repIndecies = ri.ZipIndecies;

                //this will hold the heap elements for all the arrays that are going to be replicated over
                List<StackValue[]> parameters = new List<StackValue[]>();

                int retSize;
                switch (algorithm)
                {
                    case ZipAlgorithm.Shortest:
                        retSize = Int32.MaxValue; //Search to find the smallest
                        break;

                    case ZipAlgorithm.Longest:
                        retSize = Int32.MinValue; //Search to find the largest
                        break;

                    default:
                        throw new ReplicationCaseNotCurrentlySupported(Resources.AlgorithmNotSupported);
                }

                //We determine if the input parameters are of homogeneous types to set the final function endpoint once
                var homogeneousReturn = AreParametersHomogeneousTypes(formalParameters, runtimeCore);
                var isHomogeneous = homogeneousReturn.Item1;
                if (isHomogeneous)
                {
                    var finalFormalParameters = homogeneousReturn.Item2;
                    finalFunctionEndPoint = SelectFinalFep(c, functionEndPoint, finalFormalParameters, stackFrame, runtimeCore);
                }

                bool hasEmptyArg = false;
                foreach (int repIndex in repIndecies)
                {

                    StackValue[] subParameters = null;
                    if (formalParameters[repIndex].IsArray)
                    {
                        subParameters = runtimeCore.Heap.ToHeapObject<DSArray>(formalParameters[repIndex]).Values.ToArray();
                    }
                    else
                    {
                        subParameters = new StackValue[] { formalParameters[repIndex] };
                    }
                    parameters.Add(subParameters);

                    if (subParameters.Length == 0)
                        hasEmptyArg = true;

                    switch (algorithm)
                    {
                        case ZipAlgorithm.Shortest:
                            retSize = Math.Min(retSize, subParameters.Length); //We need the smallest array
                            break;
                        case ZipAlgorithm.Longest:
                            retSize = Math.Max(retSize, subParameters.Length); //We need the longest array
                            break;
                    }

                }

                // If we're being asked to replicate across an empty list
                // then it's always going to be zero, as there will never be any
                // data to pass to that parameter.
                if (hasEmptyArg)
                    retSize = 0;

                StackValue[] retSVs = new StackValue[retSize];
                SingleRunTraceData retTrace = newTraceData;
                retTrace.NestedData = new List<SingleRunTraceData>(); //this will shadow the SVs as they are created

                //Populate out the size of the list with default values
                //@TODO:Luke perf optimisation here
                for (int i = 0; i < retSize; i++)
                    retTrace.NestedData.Add(new SingleRunTraceData());

                for (int i = 0; i < retSize; i++)
                {
                    SingleRunTraceData lastExecTrace = new SingleRunTraceData();

                    if (previousTraceData.HasNestedData && i < previousTraceData.NestedData.Count)
                    {
                        //There was previous data that needs loading into the cache
                        lastExecTrace = previousTraceData.NestedData[i];
                    }
                    else
                    {
                        //We're off the edge of the previous trace window
                        //So just pass in an empty block
                        lastExecTrace = new SingleRunTraceData();
                    }

                    //Build the call
                    List<StackValue> newFormalParams = new List<StackValue>();
                    newFormalParams.AddRange(formalParameters);

                    for (int repIi = 0; repIi < repIndecies.Count; repIi++)
                    {
                        switch (algorithm)
                        {
                            case ZipAlgorithm.Shortest:
                                //If the shortest algorithm is selected this would
                                newFormalParams[repIndecies[repIi]] = parameters[repIi][i];
                                break;

                            case ZipAlgorithm.Longest:

                                int length = parameters[repIi].Length;
                                if (i < length)
                                {
                                    newFormalParams[repIndecies[repIi]] = parameters[repIi][i];
                                }
                                else
                                {
                                    newFormalParams[repIndecies[repIi]] = parameters[repIi].Last();
                                }
                                break;
                        }
                    }

                    List<ReplicationInstruction> newRIs = new List<ReplicationInstruction>();
                    newRIs.AddRange(replicationInstructions);
                    newRIs.RemoveAt(0);


                    SingleRunTraceData cleanRetTrace = new SingleRunTraceData();

                    retSVs[i] = ExecWithRISlowPath(functionEndPoint, c, newFormalParams, newRIs, stackFrame, runtimeCore, lastExecTrace, cleanRetTrace, finalFunctionEndPoint);

                    runtimeCore.AddCallSiteGCRoot(CallSiteID, retSVs[i]);

                    retTrace.NestedData[i] = cleanRetTrace;
                }

                try
                {
                    StackValue ret = runtimeCore.RuntimeMemory.Heap.AllocateArray(retSVs);
                    return ret;
                }
                catch (RunOutOfMemoryException)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return StackValue.Null;
                }
            }
            else
            {
                //With a cartesian product over an array, we are going to create an array of n
                //where the n is the product of the next item

                //We will call the subsequent reductions n times
                int cartIndex = ri.CartesianIndex;

                //We determine if the input parameters are of homogeneous types to set the final function endpoint once
                if (cartIndex == 0)
                {
                    var homogeneousReturn = AreParametersHomogeneousTypes(formalParameters, runtimeCore);
                    var isHomogeneous = homogeneousReturn.Item1;
                    if (isHomogeneous)
                    {
                        var finalFormalParameters = homogeneousReturn.Item2;
                        finalFunctionEndPoint = SelectFinalFep(c, functionEndPoint, finalFormalParameters, stackFrame, runtimeCore);
                    }
                }

                //this will hold the heap elements for all the arrays that are going to be replicated over
                bool supressArray = false;
                int retSize;
                StackValue[] parameters = null;

                if (formalParameters[cartIndex].IsArray)
                {
                    DSArray array = runtimeCore.Heap.ToHeapObject<DSArray>(formalParameters[cartIndex]);
                    parameters = array.Values.ToArray();
                    retSize = parameters.Length;
                }
                else
                {
                    retSize = 1;
                    supressArray = true;
                }

                StackValue[] retSVs = new StackValue[retSize];

                SingleRunTraceData retTrace = newTraceData;
                retTrace.NestedData = new List<SingleRunTraceData>(); //this will shadow the SVs as they are created

                //Populate out the size of the list with default values
                //@TODO:Luke perf optimisation here
                for (int i = 0; i < retSize; i++)
                {
                    retTrace.NestedData.Add(new SingleRunTraceData());
                }

                if (supressArray)
                {
                    List<ReplicationInstruction> newRIs = new List<ReplicationInstruction>();
                    newRIs.AddRange(replicationInstructions);
                    newRIs.RemoveAt(0);

                    List<StackValue> newFormalParams = new List<StackValue>();
                    newFormalParams.AddRange(formalParameters);

                    return ExecWithRISlowPath(functionEndPoint, c, newFormalParams, newRIs, stackFrame, runtimeCore, previousTraceData, newTraceData, finalFunctionEndPoint);
                }

                //Now iterate over each of these options
                for (int i = 0; i < retSize; i++)
                {
                    //Build the call
                    List<StackValue> newFormalParams = new List<StackValue>();
                    newFormalParams.AddRange(formalParameters);

                    if (parameters != null)
                    {
                        //It was an array pack the arg with the current value
                        newFormalParams[cartIndex] = parameters[i];
                    }

                    List<ReplicationInstruction> newRIs = new List<ReplicationInstruction>();
                    newRIs.AddRange(replicationInstructions);
                    newRIs.RemoveAt(0);


                    SingleRunTraceData lastExecTrace;

                    if (previousTraceData.HasNestedData && i < previousTraceData.NestedData.Count)
                    {
                        //There was previous data that needs loading into the cache
                        lastExecTrace = previousTraceData.NestedData[i];
                    }
                    else if (previousTraceData.HasData && i == 0)
                    {
                        //We've moved up one dimension, and there was a previous run
                        lastExecTrace = new SingleRunTraceData();
                        lastExecTrace.Data = previousTraceData.GetLeftMostData();

                    }
                    else
                    {
                        //We're off the edge of the previous trace window
                        //So just pass in an empty block
                        lastExecTrace = new SingleRunTraceData();
                    }

                    //previousTraceData = lastExecTrace;
                    SingleRunTraceData cleanRetTrace = new SingleRunTraceData();

                    retSVs[i] = ExecWithRISlowPath(functionEndPoint, c, newFormalParams, newRIs, stackFrame, runtimeCore, lastExecTrace, cleanRetTrace, finalFunctionEndPoint);

                    runtimeCore.AddCallSiteGCRoot(CallSiteID, retSVs[i]);

                    retTrace.NestedData[i] = cleanRetTrace;
                }

                try
                {
                    StackValue ret = runtimeCore.RuntimeMemory.Heap.AllocateArray(retSVs);
                    return ret;
                }
                catch (RunOutOfMemoryException)
                {
                    runtimeCore.RuntimeStatus.LogWarning(WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return StackValue.Null;
                }
            }
        }

        //Single function call
        /// <summary>
        /// Dispatch without replication
        /// </summary>
        private StackValue ExecWithZeroRI(List<FunctionEndPoint> functionEndPoint, Context c,
                                          List<StackValue> formalParameters, StackFrame stackFrame, RuntimeCore runtimeCore,
                                          SingleRunTraceData previousTraceData, SingleRunTraceData newTraceData, FunctionEndPoint finalFep = null)
        {
            if (runtimeCore.CancellationPending)
            {
                throw new ExecutionCancelledException();
            }

            if (finalFep == null)
            {
                finalFep = SelectFinalFep(c, functionEndPoint, formalParameters, stackFrame, runtimeCore);
            }

            if (functionEndPoint == null)
            {
                runtimeCore.RuntimeStatus.LogWarning(WarningID.MethodResolutionFailure,
                    string.Format(Resources.FunctionDispatchFailed, "{2EB39E1B-557C-4819-94D8-CF7C9F933E8A}"));
                return StackValue.Null;
            }

            if (runtimeCore.Options.IDEDebugMode && runtimeCore.Options.RunMode != InterpreterMode.Expression)
            {
                DebugFrame debugFrame = runtimeCore.DebugProps.DebugStackFrame.Peek();
                debugFrame.FinalFepChosen = finalFep;
            }

            List<StackValue> coercedParameters = finalFep.CoerceParameters(formalParameters, runtimeCore);

            // Correct block id where the function is defined. 
            stackFrame.FunctionBlock = finalFep.BlockScope;

            //TraceCache -> TLS
            //Extract left most high-D pack
            ISerializable traceD = previousTraceData.GetLeftMostData();

            if (traceD != null)
            {
                //There was data associated with the previous execution, push this into the TLS

                Dictionary<string, ISerializable> dataDict = new Dictionary<string, ISerializable>();
                dataDict.Add(TRACE_KEY, traceD);

                TraceUtils.SetObjectToTLS(dataDict);
            }
            else
            {
                //There was no trace data for this run
                TraceUtils.ClearAllKnownTLSKeys();
            }

            //EXECUTE
            StackValue ret = finalFep.Execute(c, coercedParameters, stackFrame, runtimeCore);

            if (ret.IsNull)
            {
                //wipe the trace cache
                TraceUtils.ClearTLSKey(TRACE_KEY);
            }

            //TLS -> TraceCache
            Dictionary<string, ISerializable> traceRet = TraceUtils.GetObjectFromTLS();

            if (traceRet.ContainsKey(TRACE_KEY))
            {
                var val = traceRet[TRACE_KEY];
                newTraceData.Data = val;
            }

            // An explicit call requires return coercion at the return instruction
            if (!ret.IsExplicitCall)
            {
                ret = PerformReturnTypeCoerce(finalFep, runtimeCore, ret);
            }
            return ret;
        }

        /// <summary>
        /// Determine if the formalParameters are homogeneous types and initialize a flat list of final formalParameters
        /// </summary>
        /// <param name="formalParameters"></param>
        /// <param name="runtimeCore"></param>
        /// <returns>item1: true if the formalParameters are homogeneous, item2: finalformalParameters</returns>
        private static Tuple<bool, List<StackValue>> AreParametersHomogeneousTypes(List<StackValue> formalParameters, RuntimeCore runtimeCore)
        {
            var finalFormalParameters = new List<StackValue>();

            foreach (var formalParameter in formalParameters)
            { 
                //expand array if required to compare inputs
                if (formalParameter.IsArray)
                {
                    var array = runtimeCore.Heap.ToHeapObject<DSArray>(formalParameter);
                    var flatParameters = array.Values.ToArray();

                    switch (flatParameters.Length)
                    {
                        case 0:
                            //set function result false and exit due to empty list
                            return new Tuple<bool, List<StackValue>> (false, null);
                        case 1:
                            //Add single sample parameter to pass for evaluation in SelectFinalFep
                            finalFormalParameters.Add(flatParameters[0]);
                            break;
                        default:
                            for (int j = 0; j < flatParameters.Length - 1; j++)
                            {
                                //Compare the type data for subsequent items
                                if (flatParameters[j].optype != flatParameters[j + 1].optype ||
                                    flatParameters[j].metaData.type != flatParameters[j + 1].metaData.type)
                                {
                                    //set function result false and exit due to dissimilar function parameters
                                    return new Tuple<bool, List<StackValue>>(false, null);
                                }
                            }

                            //Add single sample parameter to pass for evaluation in SelectFinalFep
                            finalFormalParameters.Add(flatParameters[0]);
                            break;
                    }
                }
                else
                {
                    //For single parameter add it to pass for evaluation in SelectFinalFep
                    finalFormalParameters.Add(formalParameter);
                }
            }

            //formalParameteres evaluated as homogeneous
            return new Tuple<bool, List<StackValue>>(true, finalFormalParameters);
        }


        /// <summary>
        /// If all the arguments that have rep guides are single values, then strip the rep guides
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="partialReplicationGuides"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        private static List<List<ReplicationGuide>> PerformRepGuideDemotion(List<StackValue> arguments, List<List<ReplicationGuide>> providedReplicationGuides, RuntimeCore runtimeCore)
        {
            if (providedReplicationGuides.Count == 0)
                return providedReplicationGuides;

            //Check if rep guide demotion needed (each time there is a rep guide, the value is a single)
            for (int i = 0; i < arguments.Count; i++)
            {
                if (providedReplicationGuides[i].Count == 0)
                {
                    continue; //Ignore this case
                }

                //We have rep guides
                if (arguments[i].IsArray)
                {
                    //Rep guides on array, use guides as provided
                    return providedReplicationGuides;
                }
            }

            //Everwhere where we have replication guides, we have single values
            //drop the guides
            return new List<List<ReplicationGuide>>();
        }

        public static StackValue PerformReturnTypeCoerce(ProcedureNode procNode, RuntimeCore runtimeCore, StackValue ret)
        {
            Validity.Assert(procNode != null, "Proc Node was null.... {976C039E-6FE4-4482-80BA-31850E708E79}");

            Type retType = procNode.ReturnType;

            if (retType.UID == (int)PrimitiveType.Var)
            {
                if (retType.rank < 0)
                {
                    return ret;
                }
                else
                {
                    StackValue coercedRet = TypeSystem.Coerce(ret, procNode.ReturnType, runtimeCore);
                    return coercedRet;
                }
            }

            if (ret.IsNull)
            {
                return ret;
            }

            if (ret.metaData.type == retType.UID)
            {
                if (!ret.IsArray && retType.IsIndexable)
                {
                    StackValue coercedRet = TypeSystem.Coerce(ret, retType, runtimeCore);
                    return coercedRet;
                }
                else
                {
                    return ret;
                }
            }

            if (ret.IsArray && retType.IsIndexable)
            {
                StackValue coercedRet = TypeSystem.Coerce(ret, retType, runtimeCore);
                return coercedRet;
            }

            if (runtimeCore.DSExecutable.classTable.ClassNodes[ret.metaData.type].ConvertibleTo(retType.UID))
            {
                StackValue coercedRet = TypeSystem.Coerce(ret, retType, runtimeCore);
                return coercedRet;
            }
            else
            {
                //@TODO(Luke): log no-type coercion possible warning
                runtimeCore.RuntimeStatus.LogWarning(WarningID.ConversionNotPossible,
                                              Resources.kConvertNonConvertibleTypes);
                return StackValue.Null;
            }
        }

        public static StackValue PerformReturnTypeCoerce(FunctionEndPoint functionEndPoint, RuntimeCore runtimeCore, StackValue ret)
        {
            return PerformReturnTypeCoerce(functionEndPoint.procedureNode, runtimeCore, ret);
        }

        #endregion

        /// <summary>
        /// Returns a flat collection of ISerializable objects from a serialized representation of a SingleRunTraceData object.
        /// </summary>
        /// <param name="callSiteData">The serialized representation of a SingleRunTraceData object.</param>
        /// <returns>A flat collection of ISerializable objects.</returns>
        public static IList<ISerializable> GetAllSerializablesFromSingleRunTraceData(
            RawTraceData callSiteData)
        {
            var helper = TraceSerialiserHelper.FromCallSiteData(callSiteData.Data);
            if (helper == null)
            {
                return new List<ISerializable>();
            }

            var serializables = helper.TraceData.SelectMany(std => std.RecursiveGetNestedData()).ToList();
            return serializables;
        }
    }
}
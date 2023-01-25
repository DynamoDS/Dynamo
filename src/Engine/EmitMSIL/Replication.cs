using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoCore;
using ProtoCore.Lang.Replication;
using ProtoCore.Exceptions;
using ProtoCore.Properties;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoCore.Runtime;
using System.Text;

namespace EmitMSIL
{
    [Obsolete("This is an internal class, do not use it.")]
    public class Replication
    {
        internal static List<CLRStackValue> MarshalFunctionArguments(IList args, MSILRuntimeCore runtimeCore)
        {
            var marshaller = ProtoFFI.CLRDLLModule.GetMarshaler(runtimeCore);
            List<CLRStackValue> stackValues = new List<CLRStackValue>();
            foreach (var arg in args)
            {
                CLRStackValue dsObj;
                if (arg != null)
                {
                    //if it's already a wrapper - just return, no need to find type or marshal.
                    if (arg is CLRStackValue clrArg)
                    {
                        dsObj = clrArg;
                    }
                    else
                    {
                        var protoType = ProtoFFI.CLRObjectMarshaler.GetProtoCoreType(arg.GetType());
                        dsObj = marshaller.Marshal(arg, protoType, runtimeCore);
                    }
                }
                else
                {
                    dsObj = CLRStackValue.Null;
                }

                stackValues.Add(dsObj);
            }
            return stackValues;
        }

        public static IList<object> UnMarshalFunctionArguments2(IEnumerable<CLRFunctionEndPoint> feps, IEnumerable args, MSILRuntimeCore runtimeCore)
        {
            //TODO_MSIL figure out how to handle multiple feps here.
            return UnmarshalFunctionArguments(feps.FirstOrDefault().FormalParams, (IList)args, runtimeCore).ToArray();
        }

        internal static List<object> UnmarshalFunctionArguments(List<CLRFunctionEndPoint.ParamInfo> formalParams, IList args, MSILRuntimeCore runtimeCore)
        {
            var marshaller = ProtoFFI.CLRDLLModule.GetMarshaler(runtimeCore);
            List<object> values = new List<object>();

            Validity.Assert(formalParams.Count == args.Count);

            for (int i = 0; i < args.Count; ++i)
            {
                if (!(args[i] is CLRStackValue))
                {
                    values.Add(args[i]);
                }
                else
                {
                    CLRStackValue arg = (CLRStackValue)args[i];
                    System.Type paramType = formalParams[i].CLRType;
                    try
                    {
                        object param = null;
                        if (arg.IsDefaultArgument)
                            param = System.Type.Missing;
                        else
                        {
                            param = marshaller.UnMarshal(arg, paramType, runtimeCore);
                        }

                        /*TODO_MSIL: Figure out how to set/use these flags
                        if (paraminfos[i].KeepReference && opArg.IsReferenceType)
                        {
                            referencedParameters.Add(opArg);
                        }*/

                        //null is passed for a value type, so we must return null 
                        //rather than interpreting any value from null. fix defect 1462014 
                        if (!paramType.IsGenericType && paramType.IsValueType && param == null)
                        {
                            //This is going to cause a cast exception. This is a very frequently called problem, so we want to short-cut the execution

                            runtimeCore.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation,
                                string.Format(Resources.FailedToCastFromNull, paramType.Name));

                            return null;
                            //throw new System.InvalidCastException(string.Format("Null value cannot be cast to {0}", paraminfos[i].ParameterType.Name));

                        }

                        values.Add(param);
                    }
                    catch (System.InvalidCastException ex)
                    {
                        runtimeCore.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, ex.Message);
                        return null;
                    }
                    catch (InvalidOperationException)
                    {
                        string message = String.Format(Resources.kFFIFailedToObtainObject, paramType.Name, formalParams[i].CLRType.DeclaringType.Name, formalParams[i].CLRType.Name);
                        runtimeCore.LogWarning(ProtoCore.Runtime.WarningID.AccessViolation, message);
                        return null;
                    }

                }

            }
            return values;
        }

        /// <summary>
        /// Returns a single function endpoint chosen from the input functionEndPoints.
        /// Filtering is done based on coercion cost between the call arguments 'formalParameters' and each function's parameters.
        /// </summary>
        /// <param name="functionEndPoints"></param>
        /// <param name="formalParameters"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        private static CLRFunctionEndPoint SelectFinalFep(
            List<CLRFunctionEndPoint> functionEndPoints,
            List<CLRStackValue> formalParameters,
            MSILRuntimeCore runtimeCore)
        {
            //Filter for exact matches

            var exactTypeMatchingCandidates = new List<CLRFunctionEndPoint>();
            foreach (CLRFunctionEndPoint possibleFep in functionEndPoints)
            {
                if (possibleFep.DoesTypeDeepMatch(formalParameters, runtimeCore))
                {
                    exactTypeMatchingCandidates.Add(possibleFep);
                }
            }

            //There was an exact match, so dispath to it
            if (exactTypeMatchingCandidates.Count > 0)
            {
                CLRFunctionEndPoint fep;
                if (exactTypeMatchingCandidates.Count == 1)
                {
                    fep = exactTypeMatchingCandidates[0];
                }
                else
                {
                    fep = SelectFEPFromMultiple(runtimeCore, exactTypeMatchingCandidates, formalParameters);
                }

                return fep;
            }
            else
            {
                Dictionary<CLRFunctionEndPoint, int> candidatesWithDistances = new Dictionary<CLRFunctionEndPoint, int>();
                Dictionary<CLRFunctionEndPoint, int> candidatesWithCastDistances = new Dictionary<CLRFunctionEndPoint, int>();

                foreach (CLRFunctionEndPoint fep in functionEndPoints)
                {
                    //@TODO(Luke): Is this value for allow array promotion correct?
                    int distance = fep.ComputeTypeDistance(formalParameters, runtimeCore, false);
                    if (distance !=
                        (int)ProcedureDistance.InvalidDistance)
                        candidatesWithDistances.Add(fep, distance);
                }

                foreach (CLRFunctionEndPoint fep in functionEndPoints)
                {
                    int dist = fep.ComputeCastDistance(formalParameters);
                    candidatesWithCastDistances.Add(fep, dist);
                }

                var candidateFunctions = GetCandidateFunctions(candidatesWithDistances);

                if (candidateFunctions.Count == 0)
                {
                    runtimeCore.LogWarning(ProtoCore.Runtime.WarningID.AmbiguousMethodDispatch,
                                                  Resources.kAmbigousMethodDispatch);
                    return null;
                }


                CLRFunctionEndPoint compliantTarget = GetCompliantTarget(formalParameters,
                                                                      runtimeCore, candidatesWithCastDistances,
                                                                      candidateFunctions, candidatesWithDistances);

                return compliantTarget;
            }
        }


        /// <summary>
        /// Invoke method with replication.
        /// </summary>
        /// <param name="className">fully qualified name parsed from function call AST</param>
        /// <param name="methodName">parsed from function call AST</param>
        /// <param name="args"></param>
        /// <param name="replicationAttrs"></param>
        /// <returns></returns>
        [Obsolete("This is an internal function, do not use it.")]
        public static CLRStackValue ReplicationLogic(List<CLRFunctionEndPoint> feps, IList args, string[][] replicationAttrs, MSILRuntimeCore runtimeCore)
        {
            // TODO_MSIL: Emit these CLRStackValue's from the CodeGen stage.
            var stackValues = MarshalFunctionArguments(args, runtimeCore);

            // Construct replicationGuides from replicationAttrs
            var replicationGuides = ConstructRepGuides(replicationAttrs);

            var partialReplicationGuides = PerformRepGuideDemotion(stackValues, replicationGuides);

            //Replication Control is an ordered list of the elements that we have to replicate over
            //Ordering implies containment, so element 0 is the outer most forloop, element 1 is nested within it etc.
            //Take the explicit replication guides and build the replication structure
            //Turn the replication guides into a guide -> List args data structure
            var partialInstructions = Replicator.BuildPartialReplicationInstructions(partialReplicationGuides);

            List<CLRFunctionEndPoint> resolvedFeps;
            List<ReplicationInstruction> replicationInstructions;
            ComputeFeps(stackValues, feps, partialInstructions, runtimeCore, out resolvedFeps, out replicationInstructions);
            Validity.Assert(resolvedFeps.Count > 0, "Expected to resolve at least a function endpoint");

            var finalFep = SelectFinalFep(resolvedFeps, stackValues, runtimeCore);
            Validity.Assert(finalFep != null, "Expected to find a function endpoint");

            CLRStackValue result;
            if (replicationInstructions.Count == 0)
            {
                // TODO: Ideally, we should not reach here as no-replication cases should ideally be all handled
                // at compile-time. Uncomment the following Validity.Assert to debug these remaining cases.
                //Validity.Assert(replicationInstructions.Count != 0,
                //    "No-replication case, not expected to be handled inside ReplicationLogic function.");
                result = ExecWithZeroRI(finalFep, stackValues, runtimeCore);
            }
            else //replicated call
            {
                result = ExecWithRISlowPath(finalFep, stackValues, replicationInstructions, runtimeCore);
            }
            return result;
        }

        private static List<List<ReplicationGuide>> ConstructRepGuides(string[][] replicationAttrs)
        {
            var repGuides = new List<List<ReplicationGuide>>();
            foreach (var argGuides in replicationAttrs)
            {
                var argRepGuides = new List<ReplicationGuide>();
                foreach (var guide in argGuides)
                {
                    bool longest = false;
                    int guideNum = 0;
                    if (!string.IsNullOrEmpty(guide))
                    {
                        int len = guide.Length - 1;
                        if (guide[len] == 'L')
                        {
                            longest = true;
                            guideNum = int.Parse(guide.Substring(0, len));
                        }
                        else
                            guideNum = int.Parse(guide);
                    }
                    argRepGuides.Add(new ReplicationGuide(guideNum, longest));
                }
                repGuides.Add(argRepGuides);
            }
            return repGuides;
        }

        /// <summary>
        /// If all the arguments that have rep guides are single values, then strip the rep guides
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="partialReplicationGuides"></param>
        /// <returns></returns>
        private static List<List<ReplicationGuide>> PerformRepGuideDemotion(List<CLRStackValue> arguments,
            List<List<ReplicationGuide>> providedReplicationGuides)
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
                if (arguments[i].IsEnumerable)
                {
                    //Rep guides on array, use guides as provided
                    return providedReplicationGuides;
                }
            }

            //Everwhere where we have replication guides, we have single values
            //drop the guides
            return new List<List<ReplicationGuide>>();
        }

        /// <summary>
        /// Filters out invalid scenarios (ex global function context and instance method candidates)
        /// </summary>
        /// <param name="candidatesWithDistances"></param>
        /// <returns></returns>
        private static List<CLRFunctionEndPoint> GetCandidateFunctions(
            Dictionary<CLRFunctionEndPoint, int> candidatesWithDistances)
        {
            var candidateFunctions = new List<CLRFunctionEndPoint>();
            foreach (var fep in candidatesWithDistances.Keys)
            {
                bool isInstanceFEP = fep.procedureNode != null &&
                  fep.procedureNode.ClassID != Constants.kInvalidIndex &&//valid class
                  !fep.procedureNode.IsConstructor && !fep.procedureNode.IsStatic;//not static and not constructor  

                // TODO_MSIL: figure out global functions
                bool IsGlobalFunc = false;// is global function
                if (IsGlobalFunc && isInstanceFEP)
                {
                    // Filter out class instance methods when dealing with global function scope
                    continue;
                }

                candidateFunctions.Add(fep);
            }
            return candidateFunctions;
        }

        /// <summary>
        /// Returns a single FEP based on coercion cost between arguments and function parameters.
        /// The function with the lowest coercion cost will be returned.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="funcGroup"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="runtimeCore"></param>
        /// <param name="allowArrayPromotion"></param>
        /// <returns></returns>
        private static CLRFunctionEndPoint GetCompliantFEP(
            List<CLRStackValue> arguments,
            List<CLRFunctionEndPoint> funcGroup,
            List<ReplicationInstruction> replicationInstructions,
            MSILRuntimeCore runtimeCore,
            bool allowArrayPromotion = false)
        {
            Dictionary<CLRFunctionEndPoint, int> candidatesWithDistances =
                FunctionGroup.GetConversionDistances(
                    funcGroup,
                    arguments,
                    replicationInstructions,
                    runtimeCore,
                    allowArrayPromotion);

            var candidatesWithCastDistances =
                FunctionGroup.GetCastDistances(
                    funcGroup,
                    arguments,
                    replicationInstructions);

            var candidateFunctions = GetCandidateFunctions(candidatesWithDistances);

            CLRFunctionEndPoint compliantTarget = GetCompliantTarget(
                    arguments,
                    runtimeCore,
                    candidatesWithCastDistances,
                    candidateFunctions,
                    candidatesWithDistances);

            return compliantTarget;
        }

        /// <summary>
        /// Filters FEPs based on class hiearchy and array ranking.
        /// Ex.1 Overidden functions class B{ method; } class D : B { method; }
        /// D d; d.method(); => D::method will be chosen because it is closes to the caller's type.
        /// Ex.2 Functions that differ in array rank
        /// method(var[][]), method(var[]..[]); method([1..2]); => method(var[][]) will be chosen because it has the lowest array rank.
        /// </summary>
        /// <param name="runtimeCore"></param>
        /// <param name="feps"></param>
        /// <param name="argumentsList"></param>
        /// <returns></returns>
        private static CLRFunctionEndPoint SelectFEPFromMultiple(
            MSILRuntimeCore runtimeCore,
            List<CLRFunctionEndPoint> feps,
            List<CLRStackValue> argumentsList)
        {
            /*TODO_MSIL: figure out the this ptr context
            Validity.Assert(thisPtr.IsNull || thisPtr.IsPointer,
                            "this pointer wasn't null or a pointer. {89635B06-AD53-4170-ADA5-065EB2AE5858}");
            */

            var classId = feps.FirstOrDefault().procedureNode.ClassID;

            // We have multiple possible scopes for the function call:
            // 1. Static method call - no this pointer
            // ex: ClassA.Method();
            //    Hidden static methods generate multiple feps.
            //    We do not need to check actually if the method has the "IsHideBySig" (https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase.ishidebysig)
            //    because static methods can only be hidden.
            // 2. Method call from an instance of a class - valid this pointer.
            // ex: classAInstance.method();
            // 3. Function from the global scope - no this pointer and no class scope.
            // ex: SomeGlobalFunction();
            //
            // All 3 cases will run through the same matching steps.

            // A static function call has an invalid this pointer and a valid class scope;
            // TODO_MSIL: figure out the this ptr context
            bool isValidStaticFuncCall = /*!thisPtr.IsNull && !thisPtr.IsPointer &&*/ classId != Constants.kInvalidIndex;
            if (isValidStaticFuncCall)
            {
                return feps.FirstOrDefault();
            }

            // TODO_MSIL: figure out the this ptr context
            int typeID = /*thisPtr.TypeUID;*/Constants.kInvalidIndex;

            // Try to match with feps belonging to the class scope (most derived class should have priority).
            // In this case we simply select the function that belongs to the calling class.
            // The assumption here is that all function end points in "feps" have already been checked that they have the same signature.
            var exactFeps = feps.Where(x => x.procedureNode.ClassID == typeID);
            if (exactFeps.Count() == 1)
            {
                return exactFeps.First();
            }

            //Walk the class tree structure to find the method
            while (runtimeCore.ClassTable.ClassNodes[typeID].Base != Constants.kInvalidIndex)
            {
                typeID = runtimeCore.ClassTable.ClassNodes[typeID].Base;

                foreach (var fep in feps)
                    if (fep.procedureNode.ClassID == typeID)
                        return fep;
            }

            //We weren't able to distinguish based on class hiearchy, try to separate based on array ranking
            var numberOfArbitraryRanks = new List<int>();

            foreach (var fep in feps)
            {
                int numArbitraryRanks = 0;

                for (int i = 0; i < argumentsList.Count; i++)
                {
                    if (fep.FormalParams[i].Rank == Constants.kArbitraryRank)
                        numArbitraryRanks++;
                }

                numberOfArbitraryRanks.Add(numArbitraryRanks);
            }

            int smallest = Int32.MaxValue;
            List<int> indicesOfSmallest = new List<int>();

            for (int i = 0; i < feps.Count; i++)
            {
                if (numberOfArbitraryRanks[i] < smallest)
                {
                    smallest = numberOfArbitraryRanks[i];
                    indicesOfSmallest.Clear();
                    indicesOfSmallest.Add(i);
                }
                else if (numberOfArbitraryRanks[i] == smallest)
                    indicesOfSmallest.Add(i);
            }

            Validity.Assert(indicesOfSmallest.Count > 0,
                            "Couldn't find a fep when there should have been multiple: {EB589F55-F36B-404A-91DC-8D0EDC527E72}");

            if (indicesOfSmallest.Count == 1)
                return feps[indicesOfSmallest[0]];


            if (!CoreUtils.IsInternalMethod(feps[0].procedureNode.Name) || CoreUtils.IsGetterSetter(feps[0].procedureNode.Name))
            {
                //If this has failed, we have multiple feps, which can't be distiquished by class hiearchy. Emit a warning and select one
                StringBuilder possibleFuncs = new StringBuilder();
                possibleFuncs.Append(Resources.MultipleFunctionsFound);
                possibleFuncs.AppendLine();
                possibleFuncs.AppendLine();
                foreach (var fep in feps)
                    possibleFuncs.AppendLine("    " + fep.ToString());

                runtimeCore.LogWarning(WarningID.AmbiguousMethodDispatch, possibleFuncs.ToString());
            }

            return feps[0];
        }

        /// <summary>
        /// Tries to find a FEP that exactly matches the type and rank of the input arguments.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="funcGroup"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        private static CLRFunctionEndPoint GetCompleteMatchFunctionEndPoint(
            List<CLRStackValue> arguments,
            List<CLRFunctionEndPoint> funcGroup,
            List<ReplicationInstruction> replicationInstructions,
            MSILRuntimeCore runtimeCore)
        {
            //Exact match
            var exactTypeMatchingCandidates = FunctionGroup.GetExactTypeMatches(arguments, funcGroup, replicationInstructions, runtimeCore);

            if (exactTypeMatchingCandidates.Count == 0)
            {
                return null;
            }

            CLRFunctionEndPoint fep = null;
            if (exactTypeMatchingCandidates.Count == 1)
            {
                //Exact match
                fep = exactTypeMatchingCandidates[0];
            }
            else
            {
                //Exact match with upcast
                fep = SelectFEPFromMultiple(runtimeCore, exactTypeMatchingCandidates, arguments);
                fep = exactTypeMatchingCandidates[0];
            }

            return fep;
        }


        /// <summary>
        /// Returns the FEP that has the lowest cast distance.
        /// </summary>
        /// <param name="formalParams"></param>
        /// <param name="runtimeCore"></param>
        /// <param name="candidatesWithCastDistances"></param>
        /// <param name="candidateFunctions"></param>
        /// <param name="candidatesWithDistances"></param>
        /// <returns></returns>
        private static CLRFunctionEndPoint GetCompliantTarget(
            List<CLRStackValue> formalParams,
            MSILRuntimeCore runtimeCore,
            Dictionary<CLRFunctionEndPoint, int> candidatesWithCastDistances,
            List<CLRFunctionEndPoint> candidateFunctions,
            Dictionary<CLRFunctionEndPoint, int> candidatesWithDistances)
        {
            CLRFunctionEndPoint compliantTarget = null;
            //Produce an ordered list of the graph costs
            Dictionary<int, List<CLRFunctionEndPoint>> conversionCostList = new Dictionary<int, List<CLRFunctionEndPoint>>();

            foreach (CLRFunctionEndPoint fep in candidateFunctions)
            {
                int cost = candidatesWithDistances[fep];
                if (conversionCostList.ContainsKey(cost))
                    conversionCostList[cost].Add(fep);
                else
                    conversionCostList.Add(cost, new List<CLRFunctionEndPoint> { fep });
            }

            List<int> conversionCosts = new List<int>(conversionCostList.Keys);
            conversionCosts.Sort();

            List<CLRFunctionEndPoint> fepsToSplit = new List<CLRFunctionEndPoint>();
            foreach (int cost in conversionCosts)
            {
                fepsToSplit = conversionCostList[cost];
                compliantTarget = fepsToSplit.FirstOrDefault(x => x != null);
                if (compliantTarget != null)
                    break;
            }

            if (fepsToSplit.Count > 1)
            {
                int lowestCost = candidatesWithCastDistances[fepsToSplit[0]];
                compliantTarget = fepsToSplit[0];

                List<CLRFunctionEndPoint> lowestCostFeps = new List<CLRFunctionEndPoint>();

                foreach (CLRFunctionEndPoint fep in fepsToSplit)
                {
                    if (candidatesWithCastDistances[fep] < lowestCost)
                    {
                        lowestCost = candidatesWithCastDistances[fep];
                        compliantTarget = fep;
                        lowestCostFeps = new List<CLRFunctionEndPoint>() { fep };
                    }
                    else if (candidatesWithCastDistances[fep] == lowestCost)
                    {
                        lowestCostFeps.Add(fep);
                    }
                }

                //We have multiple feps, e.g. form overriding
                if (lowestCostFeps.Count > 0)
                    compliantTarget = SelectFEPFromMultiple(runtimeCore, lowestCostFeps, formalParams);
            }
            return compliantTarget;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="funcGroup"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        private static CLRFunctionEndPoint GetLooseCompliantFEP(
           List<CLRStackValue> arguments,
           List<CLRFunctionEndPoint> funcGroup,
           List<ReplicationInstruction> replicationInstructions,
           MSILRuntimeCore runtimeCore)
        {
            Dictionary<CLRFunctionEndPoint, int> candidatesWithDistances =
                FunctionGroup.GetLooseConversionDistances(
                    funcGroup,
                    arguments,
                    replicationInstructions,
                    runtimeCore);

            var candidatesWithCastDistances =
                FunctionGroup.GetCastDistances(
                    funcGroup,
                    arguments,
                    replicationInstructions);

            var candidateFunctions = GetCandidateFunctions(candidatesWithDistances);

            CLRFunctionEndPoint compliantTarget =
                GetCompliantTarget(
                    arguments,
                    runtimeCore,
                    candidatesWithCastDistances,
                    candidateFunctions,
                    candidatesWithDistances);

            return compliantTarget;
        }

        /// <summary>
        /// Generates a list of function endpoints and replication instructions
        /// based on arguments to parameter matching, coercion cost and initial replication instructions.
        /// </summary>
        /// <param name="arguments">The input arguments for the function call</param>
        /// <param name="funcGroup">A list of possible function endpoints</param>
        /// <param name="instructions">Replication instructions</param>
        /// <param name="runtimeCore"></param>
        /// <param name="resolvedFeps">A list of resolved function endpoints</param>
        /// <param name="replicationInstructions">A list of resolved replication instructions</param>
        private static void ComputeFeps(
            List<CLRStackValue> arguments,
            List<CLRFunctionEndPoint> funcGroup,
            List<ReplicationInstruction> instructions,
            MSILRuntimeCore runtimeCore,
            out List<CLRFunctionEndPoint> resolvedFeps,
            out List<ReplicationInstruction> replicationInstructions)
        {
            replicationInstructions = null;
            resolvedFeps = null;
            var matchFound = false;

            #region Case 1: Replication guide with exact match 
            {
                CLRFunctionEndPoint fep = GetCompleteMatchFunctionEndPoint(arguments, funcGroup, instructions, runtimeCore);
                if (fep != null)
                {
                    resolvedFeps = new List<CLRFunctionEndPoint>() { fep };
                    replicationInstructions = instructions;
                    return;
                }
            }
            #endregion

            var replicationTrials = Replicator.BuildReplicationCombinations(instructions, arguments);

            #region Case 2: Replication and replication guide with exact match
            {
                //Build the possible ways in which we might replicate
                foreach (List<ReplicationInstruction> replicationOption in replicationTrials)
                {
                    List<List<CLRStackValue>> reducedParams = Replicator.ComputeAllReducedParams(arguments, replicationOption, runtimeCore);
                    HashSet<CLRFunctionEndPoint> lookups;
                    if (FunctionGroup.CanGetExactMatchStatics(reducedParams, funcGroup, runtimeCore, out lookups))
                    {
                        if (replicationInstructions == null || CallSite.IsSimilarOptionButOfHigherRank(replicationInstructions, replicationOption))
                        {
                            // We have a cluster of FEPs that can be used to dispatch the array
                            resolvedFeps = new List<CLRFunctionEndPoint>(lookups);
                            replicationInstructions = replicationOption;
                            matchFound = true;
                        }
                    }
                }
                if (matchFound)
                    return;
            }
            #endregion

            #region Case 3: Replication with type conversion
            {
                CLRFunctionEndPoint compliantTarget = GetCompliantFEP(arguments, funcGroup, instructions, runtimeCore);
                if (compliantTarget != null)
                {
                    resolvedFeps = new List<CLRFunctionEndPoint>() { compliantTarget };
                    replicationInstructions = instructions;
                    return;
                }
            }
            #endregion

            #region Case 4: Replication and replication guide with type conversion
            {
                if (arguments.Any(arg => arg.IsEnumerable))
                {
                    foreach (var replicationOption in replicationTrials)
                    {
                        CLRFunctionEndPoint compliantTarget = GetCompliantFEP(arguments, funcGroup, replicationOption, runtimeCore);
                        if (compliantTarget != null)
                        {
                            if (replicationInstructions == null ||
                                CallSite.IsSimilarOptionButOfHigherRank(replicationInstructions, replicationOption))
                            {
                                resolvedFeps = new List<CLRFunctionEndPoint>() { compliantTarget };
                                replicationInstructions = replicationOption;
                                matchFound = true;
                            }
                        }
                    }
                    if (matchFound)
                        return;
                }
            }
            #endregion

            #region Case 5: Replication and replication guide with type conversion and array promotion
            {
                //Add as a first attempt a no-replication, but allowing up-promoting
                replicationTrials.Add(new List<ReplicationInstruction>());

                foreach (List<ReplicationInstruction> replicationOption in replicationTrials)
                {
                    CLRFunctionEndPoint compliantTarget = GetCompliantFEP(arguments, funcGroup, replicationOption, runtimeCore, true);
                    if (compliantTarget != null)
                    {
                        resolvedFeps = new List<CLRFunctionEndPoint>() { compliantTarget };
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
                    var compliantTarget = GetLooseCompliantFEP(arguments, funcGroup, replicationOption, runtimeCore);
                    if (compliantTarget != null)
                    {
                        if (replicationInstructions == null ||
                            CallSite.IsSimilarOptionButOfHigherRank(replicationInstructions, replicationOption))
                        {
                            resolvedFeps = new List<CLRFunctionEndPoint>() { compliantTarget };
                            replicationInstructions = replicationOption;
                            matchFound = true;
                        }
                    }
                }
                if (matchFound)
                    return;
            }
            #endregion

            resolvedFeps = new List<CLRFunctionEndPoint>();
            replicationInstructions = instructions;
        }

        /// <summary>
        /// Executes a function endpoint without replication.
        /// </summary>
        /// <param name="finalFep"></param>
        /// <param name="formalParameters"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        private static CLRStackValue ExecWithZeroRI(CLRFunctionEndPoint finalFep, List<CLRStackValue> formalParameters, MSILRuntimeCore runtimeCore)
        {
            List<CLRStackValue> coercedParameters = finalFep.CoerceParameters(formalParameters, runtimeCore);

            List<object> args = UnmarshalFunctionArguments(finalFep.FormalParams, coercedParameters, runtimeCore);

            // Testing invoking method without replication
            object result = finalFep.Invoke(args);
            
            var marshaller = ProtoFFI.CLRDLLModule.GetMarshaler(runtimeCore);
            CLRStackValue dsRetValue = marshaller.Marshal(result, finalFep.ProtoCoreReturnType, runtimeCore);

            //TODO_MSIL this is always false, and the comment below does not make sense.
            // An explicit call requires return coercion at the return instruction
            if (!dsRetValue.IsExplicitCall)
            {
                dsRetValue = CallSite.PerformReturnTypeCoerce(finalFep.ProtoCoreReturnType, dsRetValue, runtimeCore);
            }

            dsRetValue.CLRFEPReturnType = finalFep.CLRReturnType;
            return dsRetValue;
        }

        private static IList<CLRStackValue> getSubParameters(CLRStackValue o)
        {

            if (o.IsEnumerable)
            {
                return o.Value as IList<CLRStackValue>;
            }
            else
            {
                return new List<CLRStackValue>() { o };
            }
        }

        /// <summary>
        /// Executes a selected function endpoint with replication.
        /// </summary>
        /// <param name="finalFep"></param>
        /// <param name="formalParameters"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        /// <exception cref="ReplicationCaseNotCurrentlySupported"></exception>
        private static CLRStackValue ExecWithRISlowPath(CLRFunctionEndPoint finalFep, List<CLRStackValue> formalParameters,
            List<ReplicationInstruction> replicationInstructions, MSILRuntimeCore runtimeCore)
        {
            //Recursion base case
            if (replicationInstructions.Count == 0)
            {
                return ExecWithZeroRI(finalFep, formalParameters, runtimeCore);
            }

            //Get the replication instruction that this call will deal with
            ReplicationInstruction ri = replicationInstructions[0];

            if (ri.Zipped)
            {
                ZipAlgorithm algorithm = ri.ZipAlgorithm;

                //For each item in this plane, an array of the length of the minimum will be constructed

                //The size of the array will be the minimum size of the passed arrays
                List<int> repIndecies = ri.ZipIndecies;

                List<CLRStackValue[]> parameters = new List<CLRStackValue[]>();

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

                bool hasEmptyArg = false;
                foreach (int repIndex in repIndecies)
                {
                    // TODO: Investigate convertToArray performance
                    var subParameters = getSubParameters(formalParameters[repIndex]).ToArray();
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

                var retSVs = new CLRStackValue[retSize];
                for (int i = 0; i < retSize; i++)
                {
                    //Build the call
                    List<CLRStackValue> newFormalParams = formalParameters.ToList();
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

                    List<ReplicationInstruction> newRIs = replicationInstructions.GetRange(1, replicationInstructions.Count - 1);
                    retSVs[i] = ExecWithRISlowPath(finalFep, newFormalParams, newRIs, runtimeCore);
                }

                //TODO will this always be array?
                //TODO can we avoid calling toList() - would be nice to avoid iterating and copying..
                //span?
                return new CLRStackValue(retSVs.ToList(), (int)ProtoCore.PrimitiveType.Array, retSVs[0].CLRFEPReturnType.MakeArrayType());
            }
            else
            {
                //With a cartesian product over an array, we are going to create an array of n
                //where the n is the product of the next item

                //We will call the subsequent reductions n times
                int cartIndex = ri.CartesianIndex;

                //this will hold the heap elements for all the arrays that are going to be replicated over
                bool suppressArray = false;
                int retSize;
                IList<CLRStackValue> array = null;

                if (formalParameters[cartIndex].IsEnumerable)
                {
                    array = formalParameters[cartIndex].Value as IList<CLRStackValue>;
                    retSize = array.Count;
                }
                else
                {
                    retSize = 1;
                    suppressArray = true;
                }

                CLRStackValue[] retSVs = new CLRStackValue[retSize];

                //Build the call
                List<CLRStackValue> newFormalParams = formalParameters.ToList();
                if (suppressArray)
                {
                    List<ReplicationInstruction> newRIs = replicationInstructions.GetRange(1, replicationInstructions.Count - 1);
                    return ExecWithRISlowPath(finalFep, newFormalParams, newRIs, runtimeCore);
                }

                //Now iterate over each of these options
                for (int i = 0; i < retSize; i++)
                {
                    //It was an array pack the arg with the current value
                    newFormalParams[cartIndex] = array[i];

                    List<ReplicationInstruction> newRIs = replicationInstructions.GetRange(1, replicationInstructions.Count - 1);
                    retSVs[i] = ExecWithRISlowPath(finalFep, newFormalParams, newRIs, runtimeCore);
                }

                //TODO will this always be array?
                //TODO can we avoid calling toList() - would be nice to avoid iterating and copying..
                //span?
                return new CLRStackValue(retSVs.ToList(), (int)ProtoCore.PrimitiveType.Array, retSVs[0].CLRFEPReturnType.MakeArrayType());
            }
        }
    }
}

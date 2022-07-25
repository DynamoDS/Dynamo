using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoCore;
using ProtoCore.Lang.Replication;
using ProtoCore.Exceptions;
using ProtoCore.Properties;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace EmitMSIL
{
    public class Replication
    {
        internal static CLRStackValue ConvertToStackValue(object obj)
        {
            if (obj == null) return CLRStackValue.Null;

            if (obj is IEnumerable arr)
            {
                List<CLRStackValue> list = new List<CLRStackValue>();
                int maxRank = 0;
                foreach (object item in arr)
                {
                    CLRStackValue mObj = ConvertToStackValue(item);
                    maxRank = mObj.Rank > maxRank ? mObj.Rank : maxRank;

                    list.Add(mObj);
                }
                return new CLRStackValue(list, ProtoFFI.CLRObjectMarshaler.GetProtoCoreType(arr.GetType()), maxRank + 1);
            }
            return new CLRStackValue(obj, ProtoFFI.CLRObjectMarshaler.GetProtoCoreType(obj.GetType()));
        }

        internal static object ConvertFromStackValue(CLRStackValue sv)
        {
            if (sv.IsNull) return null;

            if (sv.IsEnumerable)
            {
                var list = new List<object>();
                foreach (var item in sv.Value as IEnumerable<CLRStackValue>)
                {
                    var mObj = ConvertFromStackValue(item);
                    list.Add(mObj);
                }
                return list;
            }
            else
            {
                return sv.Value;
            }
        }

        internal static List<CLRStackValue> MarshalArgumentsToStackValues(IList args)
        {
            List<CLRStackValue> stackValues = new List<CLRStackValue>();
            foreach (var arg in args)
            {
                CLRStackValue mArg = ConvertToStackValue(arg);
                stackValues.Add(mArg);
            }
            return stackValues;
        }

        internal static List<CLRFunctionEndPoint> MarshalMethodsToFEPs(IEnumerable<MethodBase> methods)
        {
            List<CLRFunctionEndPoint> feps = new List<CLRFunctionEndPoint>();
            foreach (var method in methods)
            {
                List<CLRFunctionEndPoint.ParamInfo> formalParams = new List<CLRFunctionEndPoint.ParamInfo>();
                foreach (var param in method.GetParameters())
                {
                    var dsType = ProtoFFI.CLRObjectMarshaler.GetProtoCoreType(param.ParameterType);
                    formalParams.Add(new CLRFunctionEndPoint.ParamInfo() { CLRInfo = param, ProtoInfo = dsType }); ;
                }
                CLRFunctionEndPoint fep = new CLRFunctionEndPoint() { method = method, Parameters = formalParams };
                feps.Add(fep);
            }
            return feps;
        }

        /// <summary>
        /// Invoke method with replication.
        /// </summary>
        /// <param name="className">fully qualified name parsed from function call AST</param>
        /// <param name="methodName">parsed from function call AST</param>
        /// <param name="args"></param>
        /// <param name="replicationAttrs"></param>
        /// <returns></returns>
        public static IList ReplicationLogic(IEnumerable<MethodBase> mInfos, IList args, string[][] replicationAttrs)
        {
            var stackValues = MarshalArgumentsToStackValues(args);

            var reducedArgs = ReduceArgs(stackValues);

            // Construct replicationGuides from replicationAttrs
            var replicationGuides = ConstructRepGuides(replicationAttrs);

            var partialReplicationGuides = PerformRepGuideDemotion(reducedArgs, replicationGuides);

            //Replication Control is an ordered list of the elements that we have to replicate over
            //Ordering implies containment, so element 0 is the outer most forloop, element 1 is nested within it etc.
            //Take the explicit replication guides and build the replication structure
            //Turn the replication guides into a guide -> List args data structure
            var partialInstructions = Replicator.BuildPartialReplicationInstructions(partialReplicationGuides);

            var feps = MarshalMethodsToFEPs(mInfos);

            List<CLRFunctionEndPoint> resolvedFeps;
            List<ReplicationInstruction> replicationInstructions;
            ComputeFeps(reducedArgs, feps, partialInstructions, out resolvedFeps, out replicationInstructions);

            var finalFep = SelectFinalFep(resolvedFeps, reducedArgs);

            object result;
            if (replicationInstructions.Count == 0)
            {
                result = ExecWithZeroRI(finalFep, reducedArgs);
            }
            else //replicated call
            {
                result = ExecWithRISlowPath(finalFep, reducedArgs, replicationInstructions);
            }

            var stackVal = ConvertToStackValue(result);
            if (!stackVal.IsExplicitCall)
            {
                // An explicit call requires return coercion at the return instruction
                stackVal = CallSite.PerformReturnTypeCoerce(finalFep, stackVal);
            }

            return new[] { ConvertFromStackValue(stackVal) };
        }

        private static List<CLRStackValue> ReduceArgs(List<CLRStackValue> args)
        {
            var reducedArgs = new List<CLRStackValue>();
            foreach (var arg in args)
            {
                if (arg.IsEnumerable && (arg.Value as IList).Count == 1)
                {
                    reducedArgs.Add((arg.Value as IList<CLRStackValue>)[0]);
                }
                else
                {
                    reducedArgs.Add(arg);
                }
            }
            return reducedArgs;
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
        private static List<List<ReplicationGuide>> PerformRepGuideDemotion(IList arguments,
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
                if (arguments[i] is IList)
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
        /// Returns complete match attempts to locate a function endpoint where 1 FEP matches all of the requirements for dispatch
        /// </summary>
        private static MethodBase GetCompleteMatchFunctionEndPoint(List<object> arguments,
            IEnumerable<MethodBase> funcGroup,
            List<ReplicationInstruction> replicationInstructions)
        {
            //Exact match
            var exactTypeMatchingCandindates = new List<MethodBase>();// funcGroup.GetExactTypeMatches(context, arguments, replicationInstructions, stackFrame, runtimeCore);

            if (exactTypeMatchingCandindates.Count == 0)
            {
                return null;
            }

            MethodBase fep = null;
            if (exactTypeMatchingCandindates.Count == 1)
            {
                //Exact match
                fep = exactTypeMatchingCandindates[0];
            }
            else
            {
                fep = exactTypeMatchingCandindates[0];
                //Exact match with upcast
                //fep = SelectFEPFromMultiple(stackFrame, runtimeCore, exactTypeMatchingCandindates, arguments);
            }

            return fep;
        }

        private static CLRFunctionEndPoint GetCompliantFEP(
            List<CLRStackValue> arguments,
            IEnumerable<CLRFunctionEndPoint> funcGroup,
            List<ReplicationInstruction> replicationInstructions,
            bool allowArrayPromotion = false)
        {
            Dictionary<CLRFunctionEndPoint, int> candidatesWithDistances =
                FunctionGroup.GetConversionDistances(
                    funcGroup,
                    arguments,
                    replicationInstructions,
                    allowArrayPromotion);

            Dictionary<CLRFunctionEndPoint, int> candidatesWithCastDistances =
                FunctionGroup.GetCastDistances(
                    funcGroup,
                    arguments,
                    replicationInstructions);

            // TODO: implement GetCandidateFunctions;
            List<CLRFunctionEndPoint> candidateFunctions = candidatesWithDistances.Keys.ToList();//CallSite.GetCandidateFunctions(candidatesWithDistances);

            CLRFunctionEndPoint compliantTarget = candidateFunctions.Count > 0 ? candidateFunctions[0] : null;
                /*GetCompliantTarget(
                    arguments,
                    replicationInstructions,
                    candidatesWithCastDistances,
                    candidateFunctions,
                    candidatesWithDistances);*/

            return compliantTarget;
        }

        private static void ComputeFeps(List<CLRStackValue> arguments,
            IEnumerable<CLRFunctionEndPoint> funcGroup,
            List<ReplicationInstruction> instructions,
            out List<CLRFunctionEndPoint> resolvedFeps,
            out List<ReplicationInstruction> replicationInstructions)
        {
            replicationInstructions = null;
            resolvedFeps = null;
            var matchFound = false;

            #region Case 1: Replication guide with exact match 
            #endregion

            var replicationTrials = Replicator.BuildReplicationCombinations(instructions, arguments);

            #region Case 2: Replication and replication guide with exact match
            #endregion

            #region Case 3: Replication with type conversion
            {
                CLRFunctionEndPoint compliantTarget = GetCompliantFEP(arguments, funcGroup, instructions);
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
                        CLRFunctionEndPoint compliantTarget = GetCompliantFEP(arguments, funcGroup, replicationOption);
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
                    CLRFunctionEndPoint compliantTarget = GetCompliantFEP(arguments, funcGroup, replicationOption, true);
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
            #endregion

            resolvedFeps = new List<CLRFunctionEndPoint>();
            replicationInstructions = instructions;
        }

        private static CLRFunctionEndPoint SelectFinalFep(IEnumerable<CLRFunctionEndPoint> functionEndPoints, List<CLRStackValue> formalParameters)
        {
            // TODO: Determine final function endpoint here based on fitting runtime args to function parameters
            return functionEndPoints.FirstOrDefault();
        }

        private static object ExecWithZeroRI(CLRFunctionEndPoint finalFep, List<CLRStackValue> formalParameters)
        {
            // TODO: CoerceParameters
            List<CLRStackValue> coercedParameters = FunctionEndPoint.CoerceParameters(finalFep, formalParameters);

            List<object> args = new List<object>();
            foreach(var item in coercedParameters)
            {
                var arg = ConvertFromStackValue(item);
                args.Add(arg);
            }
            
            // Testing invoking method without replication
            object result;
            if (finalFep.method.IsStatic)
            {
                result = finalFep.method.Invoke(null, args.ToArray());
            }
            else
            {
                result = finalFep.method.Invoke(args[0], args.Skip(1).ToArray());
            }

            return result;
        }

        private static IEnumerable<CLRStackValue> getSubParameters(CLRStackValue o)
        {

            if (ArrayUtils.IsEnumerable(o.GetType()))
            {
                return o.Value as IEnumerable<CLRStackValue>;
            }
            else
            {
                return new List<CLRStackValue>() { o };
            }
        }

        private static object ExecWithRISlowPath(CLRFunctionEndPoint finalFep, List<CLRStackValue> formalParameters,
            List<ReplicationInstruction> replicationInstructions)
        {
            //Recursion base case
            if (replicationInstructions.Count == 0)
            {
                return ExecWithZeroRI(finalFep, formalParameters);
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
                    parameters.Add(subParameters.ToArray());

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

                object[] retSVs = new object[retSize];
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
                    retSVs[i] = ExecWithRISlowPath(finalFep, newFormalParams, newRIs);
                }

                return retSVs;
            }
            else
            {
                //With a cartesian product over an array, we are going to create an array of n
                //where the n is the product of the next item

                //We will call the subsequent reductions n times
                int cartIndex = ri.CartesianIndex;

                //this will hold the heap elements for all the arrays that are going to be replicated over
                bool supressArray = false;
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
                    supressArray = true;
                }

                object[] retSVs = new object[retSize];

                //Build the call
                List<CLRStackValue> newFormalParams = formalParameters.ToList();
                if (supressArray)
                {
                    List<ReplicationInstruction> newRIs = replicationInstructions.GetRange(1, replicationInstructions.Count - 1);
                    return ExecWithRISlowPath(finalFep, newFormalParams, newRIs);
                }

                //Now iterate over each of these options
                for (int i = 0; i < retSize; i++)
                {
                    //It was an array pack the arg with the current value
                    newFormalParams[cartIndex] = array[i];

                    List<ReplicationInstruction> newRIs = replicationInstructions.GetRange(1, replicationInstructions.Count - 1);
                    retSVs[i] = ExecWithRISlowPath(finalFep, newFormalParams, newRIs);
                }

                return retSVs;
            }
        }
    }
}

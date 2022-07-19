using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProtoCore;
using ProtoCore.Lang.Replication;
using ProtoCore.Exceptions;
using ProtoCore.Properties;

namespace EmitMSIL
{
    public class Replication
    {
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
            var reducedArgs = ReduceArgs(args);

            // Construct replicationGuides from replicationAttrs
            var replicationGuides = ConstructRepGuides(replicationAttrs);

            var partialReplicationGuides = PerformRepGuideDemotion(reducedArgs, replicationGuides);

            //Replication Control is an ordered list of the elements that we have to replicate over
            //Ordering implies containment, so element 0 is the outer most forloop, element 1 is nested within it etc.
            //Take the explicit replication guides and build the replication structure
            //Turn the replication guides into a guide -> List args data structure
            var partialInstructions = Replicator.BuildPartialReplicationInstructions(partialReplicationGuides);

            // TODO: implement auto replication
            //ComputeFeps(reducedArgs, mInfos, partialInstructions, out resolvesFeps, out replicationInstructions);
            
            var finalFep = SelectFinalFep(mInfos, reducedArgs);

            object result;
            if (partialInstructions.Count == 0)
            {
                result = ExecWithZeroRI(finalFep, reducedArgs);
            }
            else //replicated call
            {
                result = ExecWithRISlowPath(finalFep, reducedArgs, partialInstructions);
            }
            return new[] { result };
        }

        private static List<object> ReduceArgs(IList args)
        {
            var reducedArgs = new List<object>();
            foreach (var arg in args)
            {
                if (arg is IList argList && argList.Count == 1)
                {
                    reducedArgs.Add(argList[0]);
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

        private static MethodBase SelectFinalFep(IEnumerable<MethodBase> functionEndPoints, List<object> formalParameters)
        {
            // TODO: Determine final function endpoint here based on fitting runtime args to function parameters
            return functionEndPoints.FirstOrDefault();
        }

        private static object ExecWithZeroRI(MethodBase finalFep, List<object> formalParameters)
        {
            // TODO: CoerceParameters
            List<object> coercedParameters = formalParameters;//finalFep.CoerceParameters(formalParameters);

            // Testing invoking method without replication
            object result;
            if (finalFep.IsStatic)
            {
                result = finalFep.Invoke(null, coercedParameters.ToArray());
            }
            else
            {
                result = finalFep.Invoke(coercedParameters[0], coercedParameters.Skip(1).ToArray());
            }

            // TODO: PerformReturnTypeCoerce
            // An explicit call requires return coercion at the return instruction
            //result = PerformReturnTypeCoerce(finalFep, ret);

            return result;
        }

        // TODO: Look into array conversion performance 
        private static object[] convertToArray(object o)
        {
            System.Type type = o.GetType();
            if (o is Array oldArr)
            {
                Array newArr = Array.CreateInstance(typeof(object), oldArr.Length);
                Array.Copy(oldArr, newArr, oldArr.Length);
                return newArr as object[];
            }
            return new object[] { o };
        }

        private static bool isIndexable(System.Type type) => type.IsArray || type.IsAssignableFrom(typeof(IEnumerable));

        private static bool isIndexable(object o) => isIndexable(o.GetType());

        private static object ExecWithRISlowPath(MethodBase finalFep, List<object> formalParameters,
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
                List<object[]> parameters = new List<object[]>();

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
                    object[] subParameters = convertToArray(formalParameters[repIndex]);
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

                object[] retSVs = new object[retSize];
                for (int i = 0; i < retSize; i++)
                {
                    //Build the call
                    List<object> newFormalParams = formalParameters.ToList();
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
                object[] array = null;

                if (isIndexable(formalParameters[cartIndex]))
                {
                    array = formalParameters[cartIndex] as object[];
                    retSize = array.Length;
                }
                else
                {
                    retSize = 1;
                    supressArray = true;
                }

                object[] retSVs = new object[retSize];

                //Build the call
                List<object> newFormalParams = formalParameters.ToList();
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

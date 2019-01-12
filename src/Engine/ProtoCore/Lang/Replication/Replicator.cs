using System;
using System.Linq;
using System.Collections.Generic;
using ProtoCore.DSASM;
using ProtoCore.Exceptions;
using ProtoCore.Properties;
using ProtoCore.Utils;

namespace ProtoCore.Lang.Replication
{
    public class Replicator
    {
        /// <summary>
        /// Calculate partial replication instruciton based on replication guide level.
        /// For example, for foo(xs<1><2><3>, ys<1><1><2>, zs<1><1><3>), the guides
        /// are:
        /// 
        ///     level |  0  |  1  |  2  |
        ///     ------+-----+-----+-----+
        ///       xs  |  1  |  2  |  3  |
        ///     ------+-----+-----+-----+
        ///       ys  |  1  |  1  |  2  |
        ///     ------+-----+-----+-----+
        ///       zs  |  1  |  1  |  3  |
        ///
        /// This function goes through each level and calculate replication instructions.
        /// 
        /// replication instructions on level 0:
        ///     Zip replication on (0, 1, 2) (i.e., zip on xs, ys, zs)
        ///
        /// replication instructions on level 1:
        ///     Zip replication on (1, 2)    (i.e., zip on ys, zs)
        ///     Cartesian replication on 0   (i.e., on xs)
        ///
        /// replication instructions on level 2:
        ///     Cartesian replication on 1   (i.e., on ys)
        ///     Zip replication on (0, 2)    (i.e., zip on xs, zs)
        /// </summary>
        /// <param name="partialRepGuides"></param>
        /// <returns></returns>
        public static List<ReplicationInstruction> BuildPartialReplicationInstructions(List<List<ReplicationGuide>> partialRepGuides)
        {
            if (!partialRepGuides.Any())
            {
                return new List<ReplicationInstruction>();
            }

            int maxGuideLevel= partialRepGuides.Max(gs => gs.Count);

            var instructions = new List<ReplicationInstruction>();
            for (int level = 0; level < maxGuideLevel; ++level)
            {
                var positions = new Dictionary<int, List<int>>();
                var algorithms = new Dictionary<int, ZipAlgorithm>();
                var guides = new HashSet<int>();

                for (int i = 0; i < partialRepGuides.Count; ++i)
                {
                    var replicationGuides = partialRepGuides[i];
                    if (replicationGuides == null || replicationGuides.Count <= level)
                        continue;

                    // If it is negative or 0, treat it as a stub
                    var guide = replicationGuides[level].GuideNumber;
                    if (guide <= 0)
                        continue;

                    var algorithm = replicationGuides[level].IsLongest ? ZipAlgorithm.Longest : ZipAlgorithm.Shortest;

                    guides.Add(guide);

                    List<int> positionList = null;
                    if (!positions.TryGetValue(guide, out positionList))
                    {
                        positionList = new List<int>();
                        positions[guide] = positionList;
                    }
                    positionList.Add(i);

                    ZipAlgorithm zipAlgorithm;
                    if (algorithms.TryGetValue(guide, out zipAlgorithm))
                    {
                        if (algorithm == ZipAlgorithm.Longest)
                            algorithms[guide] = ZipAlgorithm.Longest;
                    }
                    else
                    {
                        algorithms[guide] = algorithm;
                    }
                }

                var sortedGuides = guides.ToList();
                sortedGuides.Sort();

                foreach (var guide in sortedGuides)
                {
                    ReplicationInstruction repInstruction = new ReplicationInstruction();

                    var positionList = positions[guide];
                    if (positionList.Count == 1)
                    {
                        repInstruction.Zipped = false;
                        repInstruction.CartesianIndex = positionList[0];
                    }
                    else
                    {
                        repInstruction.Zipped = true;
                        repInstruction.ZipIndecies = positionList;
                        repInstruction.ZipAlgorithm = algorithms[guide];
                    }

                    instructions.Add(repInstruction);
                }
            }
            return instructions;
        }

        /// <summary>
        /// Convert reduction to instruction. Using zip-first strategy.
        /// 
        /// For example,
        ///     0 2 4   > Zip on 1,2
        ///     0 1 3   > Zip on 1,2
        ///     0 0 2   > Cartesian on 2
        ///     0 0 1   > Cartesian on 2
        /// </summary>
        /// <param name="reductions"></param>
        /// <param name="providedControl"></param>
        /// <returns></returns>
        public static List<ReplicationInstruction> ReductionToInstructions(List<int> reductions, List<ReplicationInstruction> providedControl)
        {
            List<ReplicationInstruction> ret = new List<ReplicationInstruction>(providedControl);

            while (true)
            {
                int zippableItemCount = reductions.Count(x => x > 0);
                if (zippableItemCount <= 1)
                    break;

                List<int> locations = new List<int>();
                for (int i = 0; i < reductions.Count; i++)
                {
                    if (reductions[i] >= 1)
                    {
                        locations.Add(i);
                        reductions[i] -= 1;
                    }
                }

                ReplicationInstruction ri = new ReplicationInstruction()
                {
                    CartesianIndex = -1,
                    ZipIndecies = locations,
                    Zipped = true
                };
                ret.Add(ri);
            }

            for (int i = 0; i < reductions.Count; i++)
            {
                int reduction = reductions[i];
                while (reduction > 0) 
                {
                    ReplicationInstruction ri = new ReplicationInstruction()
                    {
                        CartesianIndex = i, ZipIndecies = null, Zipped = false
                    };
                    ret.Add(ri);
                    reduction--;
                }
            }

            return ret;
        }

        /// <summary>
        /// For each parameter, if there is a replication instruction for it, and
        /// if it is an array, expand parameter list based on the types of elements
        /// in that array. For example, for parameters
        /// 
        ///     {p1, p2, ..., pk, ..., pn} where pk is an array 
        ///     
        ///     {a1:int, a2:string, a3:double, ...} 
        /// 
        /// and there is a Cartesian replication on pk, the parameter list will be
        /// expanded to
        /// 
        ///     {p1, p2, ..., a1, ..., pn}
        ///     {p1, p2, ..., a2, ..., pn}
        ///     {p1, p2, ..., a3, ..., pn}
        ///     ...
        /// 
        /// </summary>
        /// <param name="formalParams"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        public static List<List<StackValue>> ComputeAllReducedParams(
            List<StackValue> formalParams, 
            List<ReplicationInstruction> replicationInstructions, 
            RuntimeCore runtimeCore)
        {
            //Copy the types so unaffected ones get copied back directly
            var basicList = new List<StackValue>(formalParams);

            //Compute the reduced Type args
            var reducedParams = new List<List<StackValue>>();
            reducedParams.Add(basicList);

            foreach (var ri in replicationInstructions)
            {
                var indices = ri.Zipped ? ri.ZipIndecies : new List<int> { ri.CartesianIndex };
                foreach (int index in indices)
                {
                    //This should generally be a collection, so we need to do a one phase unboxing
                    var targets = reducedParams.Select(r => r[index]).ToList();
                    var target = basicList[index];

                    if (!target.IsArray)
                    {
                        System.Console.WriteLine("WARNING: Replication unbox requested on Singleton. Trap: 437AD20D-9422-40A3-BFFD-DA4BAD7F3E5F");
                        continue;
                    }

                    var array = runtimeCore.Heap.ToHeapObject<DSArray>(target);
                    if (array.Count == 0)
                    {
                        continue;
                    }

                    var arrayStats = new HashSet<StackValue>();
                    foreach (var targetTemp in targets)
                    {
                        var temp = ArrayUtils.GetTypeExamplesForLayer2(targetTemp, runtimeCore).ToList();
                        arrayStats.UnionWith(temp);
                    }

                    var clonedList = new List<List<StackValue>>(reducedParams);
                    reducedParams.Clear();

                    foreach (var sv in arrayStats)
                    {
                        foreach (var lst in clonedList)
                        {
                            var newArgs = new List<StackValue>(lst);
                            newArgs[index] = sv;
                            reducedParams.Add(newArgs);
                        }
                    }
                }
            }

            return reducedParams;
        }

        /// <summary>
        /// Compute the effects of the replication guides on the formal parameter lists
        /// The results of this loose data, and will not be correct on jagged arrays of hetrogenius types
        /// </summary>
        /// <param name="formalParams"></param>
        /// <param name="replicationInstructions"></param>
        /// <returns></returns>
        public static List<StackValue> EstimateReducedParams(List<StackValue> formalParams, List<ReplicationInstruction> replicationInstructions, RuntimeCore runtimeCore)
        {
            //Compute the reduced Type args
            List<StackValue> reducedParamTypes = new List<StackValue>(formalParams);

            foreach (ReplicationInstruction ri in replicationInstructions)
            {
                var indices = ri.Zipped ? ri.ZipIndecies : new List<int> { ri.CartesianIndex };
                foreach (int index in indices)
                {
                    //This should generally be a collection, so we need to do a one phase unboxing
                    StackValue target = reducedParamTypes[index];
                    StackValue reducedSV = StackValue.Null;

                    if (target.IsArray)
                    {
                        var array = runtimeCore.Heap.ToHeapObject<DSArray>(reducedParamTypes[index]);

                        //It is a collection, so cast it to an array and pull the type of the first element
                        //The elements of the array are still type structures
                        if (array.Count == 0)
                            reducedSV = StackValue.Null;
                        else
                            reducedSV = array.GetValueFromIndex(0, runtimeCore);
                    }
                    else
                    {
                        System.Console.WriteLine("WARNING: Replication unbox requested on Singleton. Trap: 437AD20D-9422-40A3-BFFD-DA4BAD7F3E5F");
                        reducedSV = target;
                    }

                    reducedParamTypes[index] = reducedSV;
                }
            }

            return reducedParamTypes;
        }

        public static List<List<int>> BuildReductions(List<int> reductionDepths)
        {
            int argumentCount = reductionDepths.Count;
            if (argumentCount == 0)
                return new List<List<int>>();

            int count = reductionDepths.Aggregate(1, (acc, x) => acc * (x + 1));
            List<List<int>> retList = new List<List<int>>(count);
            for (int r = 0; r <= reductionDepths[0]; r++)
            {
                List<int> reductions = new List<int>(argumentCount);
                reductions.Add(r);
                retList.Add(reductions);
            }

            for (int i = 1; i < argumentCount; i++)
            {
                List<List<int>> tempRetList = new List<List<int>>(retList);
                retList.Clear();

                for (int r = 0; r <= reductionDepths[i]; r++)
                {
                    foreach (var reductions in tempRetList)
                    //reducedType.IsIndexable = false;
                    {
                        List<int> newReductions = new List<int>(reductions);
                        newReductions.Add(r);
                        retList.Add(newReductions);
                    }
                }
            }

            return retList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count">Count is the number of elements left for allocation</param>
        /// <param name="maxAlloc">Max alloc is the maximum potential reductions that can yet be applied</param>
        /// <returns></returns>
        public static List<List<int>> BuildAllocation(int count, int maxAlloc)
        {
            //@PERF: this algorithm is not optimal, e.g. short-cutting can be used
            //it's also needlessly recursive
            List<List<int>> retList = new List<List<int>>();

            if (count == 1)
            {
                //Base case
                for (int currVal = 0; currVal <= maxAlloc; currVal++)
                {
                    List<int> list = new List<int> { currVal };
                    retList.Add(list);
                }
            }
            else
            {
                if (count == 0) throw new CompilerInternalException("Internal error: Recursion past base case {158ECB5E-2139-4DD7-9470-F64A42CE0D6D}");

                for (int currVal = 0; currVal <= maxAlloc; currVal++)
                {
                    List<List<int>> subLists = BuildAllocation(count - 1, maxAlloc - currVal);

                    //prepend all of these lists with currVal
                    foreach (List<int> list in subLists)
                    {
                        list.Insert(0, currVal);
                        retList.Add(list);
                    }
                }
            }

            return retList;
        }

        /// <summary>
        /// Build all possible replications based on the rank of parameters and
        /// the provided replicatoin guide.
        /// </summary>
        /// <param name="providedControl"></param>
        /// <param name="formalParams"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        public static List<List<ReplicationInstruction>> BuildReplicationCombinations(List<ReplicationInstruction> providedControl, List<StackValue> formalParams, RuntimeCore runtimeCore)
        {
            List<int> maxReductionDepths = formalParams.Select(p => GetMaxReductionDepth(p, runtimeCore)).ToList();

            int maxDepth = maxReductionDepths.Sum();
            if (maxDepth == 0)
                return new List<List<ReplicationInstruction>>();

            // Reduce reduction level on parameter if the parameter has replication guides 
            if (providedControl.Count > 0)
            {
                var reversedControl = new List<ReplicationInstruction>(providedControl);
                reversedControl.Reverse();

                foreach (ReplicationInstruction ri in reversedControl)
                {
                    if (ri.Zipped)
                    {
                        foreach (int idx in ri.ZipIndecies)
                        {
                            if(maxReductionDepths[idx] > 0)
                                maxReductionDepths[idx] = maxReductionDepths[idx] - 1;
                        }
                    }
                    else
                    {
                        if (maxReductionDepths[ri.CartesianIndex] > 0)
                        {
                            maxReductionDepths[ri.CartesianIndex] = maxReductionDepths[ri.CartesianIndex] - 1;
                        }
                    }
                }
            }

            // Generate reduction lists (x1, x2, ..., xn) 
            if (maxReductionDepths.Any(r => r < 0) || maxReductionDepths.All(r => r == 0))
                return new List<List<ReplicationInstruction>> { providedControl };

            List<List<int>> reductions = BuildReductions(maxReductionDepths);
            var ret = reductions.Select(rs => ReductionToInstructions(rs, providedControl)).ToList();
            return ret;
        }

        /// <summary>
        /// Returns the maximum depth to which an element can be reduced
        /// This will include cases where only partial reduction can be performed on jagged arrays
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static int GetMaxReductionDepth(StackValue sv, RuntimeCore runtimeCore)
        {
            return RecursiveProtectGetMaxReductionDepth(sv, runtimeCore, 0);
        }

        /// <summary>
        /// This computes the max depth to which the element can be reduced
        /// It contains a protected envelope 
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <param name="depthCount"></param>
        /// <returns></returns>
        private static int RecursiveProtectGetMaxReductionDepth(StackValue sv, RuntimeCore runtimeCore, int depthCount)
        {
            Validity.Assert(depthCount < 1000, 
                "StackOverflow protection trap. This is almost certainly a VM cycle-in-array bug. {0B530165-2E38-431D-88D9-56B0636364CD}");

            //PERF(Luke): Could be non-recursive
            if (!sv.IsArray)
                return 0;

            int maxReduction = 0;

            //De-ref the sv
            var array = runtimeCore.Heap.ToHeapObject<DSArray>(sv);
            foreach (var subSv in array.Values)
            {
                maxReduction = Math.Max(maxReduction, RecursiveProtectGetMaxReductionDepth(subSv, runtimeCore, depthCount + 1));
            }

            return 1 + maxReduction;   
        }
    }
}

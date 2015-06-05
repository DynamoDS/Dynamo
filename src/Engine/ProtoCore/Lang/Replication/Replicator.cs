using System;
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
        /// Build partial replication instructions for guides
        /// </summary>
        /// <param name="partialGuides">The guides, empty sub list if no guides for an argument</param>
        /// <returns>The replication instructions</returns>
        [Obsolete]
        public static ReplicationControl Old_ConvertGuidesToInstructions(List<List<ProtoCore.ReplicationGuide>> partialGuides)
        {
            /*
            //Test to ensure that we're within the known limitations, supporting at most 1 guide per argument
            //Munge the format to use the legacy mechansi,
            //This is temporary code
            //@TODO(Luke)
            List<int?> singlePartialGuides = new List<int?>();
            foreach (List<int> guideList in partialGuides)
            {
                if (guideList.Count == 0)
                    singlePartialGuides.Add(null);
                else if (guideList.Count > 1)
                    throw new NotImplementedException("Multiple guides on an argument not yet supported: {93AF9B93-7EDC-4EE9-8E20-1A5FF029871C}");
                else
                    singlePartialGuides.Add(guideList[0]);
            }*/

            ReplicationControl rc = new ReplicationControl()
            {
                //Instructions = Old_BuildPartialReplicationInstructions(singlePartialGuides)
                Instructions = BuildPartialReplicationInstructions(partialGuides)

            };
            

            //Now push the result to the legacy code to do the computation
            return rc;

        }


        private static List<ReplicationInstruction> BuildPartialReplicationInstructions(List<List<ProtoCore.ReplicationGuide>> partialRepGuides)
        {
            //DS code:          foo(a<1><2><3>, b<2>, c)
            //partialGuides     {1,2,3}, {2}, {}
            //Instructions

            //Check for out of order unboxing


            // Comment Jun: Convert from new replication guide data struct to the old format where guides are only a list of ints
            // TODO Luke: Remove this temporary marshalling and use the replicationguide data structure directly
            List<List<int>> partialGuides = new List<List<int>>();
            List<List<ZipAlgorithm>> partialGuidesLace = new List<List<ZipAlgorithm>>();

            foreach (List<ProtoCore.ReplicationGuide> guidesOnParam in partialRepGuides)
            {
                List<int> tempGuide = new List<int>();
                List<ZipAlgorithm> tempGuideLaceStrategy = new List<ZipAlgorithm>();
                foreach (ProtoCore.ReplicationGuide guide in guidesOnParam)
                {
                    tempGuide.Add(guide.guideNumber);
                    tempGuideLaceStrategy.Add(guide.isLongest ? ZipAlgorithm.Longest : ZipAlgorithm.Shortest);
                }
                partialGuides.Add(tempGuide);
                partialGuidesLace.Add(tempGuideLaceStrategy);
            }

            //@TODO: remove this limitation
            VerifyIncreasingGuideOrdering(partialGuides);

            //Guide -> args
            Dictionary<int, List<int>> index = new Dictionary<int, List<int>>();
            Dictionary<int, ZipAlgorithm> indexLace = new Dictionary<int, ZipAlgorithm>();

            int maxGuide = int.MinValue;

            foreach (List<int> guidesOnParam in partialGuides)
            {
                foreach (int guide in guidesOnParam)
                    maxGuide = Math.Max(maxGuide, guide);
            }

            //There were no guides, exit with no instructions
            if (maxGuide == int.MinValue)
                return new List<ReplicationInstruction>();

            //Iterate over all of the guides
            for (int guideCounter = 1; guideCounter <= maxGuide; guideCounter++)
            {
                index.Add(guideCounter, new List<int>());
                indexLace.Add(guideCounter, ZipAlgorithm.Shortest);

                for (int i = 0; i < partialGuides.Count; i++)
                    if (partialGuides[i].Contains(guideCounter))
                    {
                        index[guideCounter].Add(i);

                        int indexOfGuide = partialGuides[i].IndexOf(guideCounter);

                        if (partialGuidesLace[i][indexOfGuide] == ZipAlgorithm.Longest)
                        {

                            //If we've previous seen a shortest node with this guide
                            if (i > 0 && indexLace[guideCounter] == ZipAlgorithm.Shortest)
                            {
                                throw new ReplicationCaseNotCurrentlySupported(Resources.ZipAlgorithmError);
                            }

                            //Overwrite the default behaviour
                            indexLace[guideCounter] = ZipAlgorithm.Longest;
                        }
                        else
                        {
                            //it's shortest, if we had something previous saying it should be longest
                            //then we've created a violation foo(a<1>, b1<1L>) is not allowed
                            if (indexLace[guideCounter] == ZipAlgorithm.Longest)
                            {
                                throw new ReplicationCaseNotCurrentlySupported(Resources.ZipAlgorithmError);
                            }
                            else
                            {
                                //Shortest lacing is actually the default, this call should be redundant
                                indexLace[guideCounter] = ZipAlgorithm.Shortest;
                            }
                        }
                    }

                // Validity.Assert(index[guideCounter].Count > 0, "Sorry, non-contiguous replication guides are not yet supported, please try again without leaving any gaps, err code {3E080694}");
            }




            //Now walk over the guides in order creating the replication 
            int[] uniqueGuides = new int[index.Keys.Count];
            index.Keys.CopyTo(uniqueGuides, 0);
            Array.Sort(uniqueGuides);

            List<ReplicationInstruction> ret = new List<ReplicationInstruction>();
            foreach (int i in uniqueGuides)
            {
                //Create a new replication instruction
                ReplicationInstruction ri = new ReplicationInstruction();

                if (index[i].Count == 0)
                {
                    continue;
                }
                else if (index[i].Count == 1)
                {
                    ri.CartesianIndex = index[i][0];
                    ri.Zipped = false;
                }
                else
                {
                    ri.Zipped = true;
                    ri.ZipIndecies = index[i];
                    ri.ZipAlgorithm = indexLace[i];
                }

                ret.Add(ri);
            }

            return ret;
        }

        /// <summary>
        /// Verify that the guides are in increasing order
        /// </summary>
        /// <param name="partialGuides"></param>
        private static void VerifyIncreasingGuideOrdering(List<List<int>> partialGuides)
        {
            foreach (List<int> guidesOnParam in partialGuides)
            {
                List<int> sorted = new List<int>();
                sorted.AddRange(guidesOnParam);
                sorted.Sort();

                for (int i = 0; i < sorted.Count; i++)
                {
                    if (guidesOnParam[i] != sorted[i])
                    {
                        throw new ReplicationCaseNotCurrentlySupported(Resources.MultipleGuidesNotSupported + 
                            string.Format(Resources.ErrorCode, "{3C5360D1}"));
                    }
                }
            }
        }


        /// <summary>
        /// Legacy method that converts guides with at most one guide per argument
        /// To be factored into ConvertGuidesToInstructions and removed
        /// </summary>
        /// <param name="partialGuides"></param>
        /// <returns></returns>
        [Obsolete]
        private static List<ReplicationInstruction> Old_BuildPartialReplicationInstructions(List<int?> partialGuides)
        {



            Dictionary<int, List<int>> index = new Dictionary<int, List<int>>();

            for (int i = 0; i < partialGuides.Count; i++)
            {
                if (partialGuides[i].HasValue)
                {
                    int value = partialGuides[i].Value;

                    if (!index.ContainsKey(value))
                        index.Add(value, new List<int>());

                    index[value].Add(i);
                }

            }


            //Now walk over the guides in order creating the replication 
            int[] uniqueGuides = new int[index.Keys.Count];
            index.Keys.CopyTo(uniqueGuides, 0);
            Array.Sort(uniqueGuides);

            List<ReplicationInstruction> ret = new List<ReplicationInstruction>();
            foreach (int i in uniqueGuides)
            {
                //Create a new replication instruction
                ReplicationInstruction ri = new ReplicationInstruction();

                if (index[i].Count == 1)
                {
                    ri.CartesianIndex = index[i][0];
                    ri.Zipped = false;
                }
                else
                {
                    ri.Zipped = true;
                    ri.ZipIndecies = index[i];
                }

                ret.Add(ri);
            }

            return ret;
        }


        public static List<ReplicationInstruction> ReductionToInstructions(List<int> reductionList)
        {

            List<ReplicationInstruction> ret = new List<ReplicationInstruction>();

            //Basic sanity check on the reductions
            foreach (int reduction in reductionList)
                Validity.Assert(reduction >= 0, "Negative reductions aren't supported {991CBD60-8B2B-438B-BDEB-734D18B4FE68}");

            int maxReductionCount = int.MinValue;
            foreach (int reduction in reductionList)
                maxReductionCount = Math.Max(maxReductionCount, reduction);

            //@PERF This is going to do a O(n^2) cost scan, when we could just build a reverse index once....

            int lastSeenReduction = 0; //This is the last instruction that we have seen and don't need to inject further
            //reductions from

            //Walk over all the reductions that we're going to do
            for (int i = 1; i <= maxReductionCount; i++)
            {
                if (!reductionList.Contains(i))
                    continue; //We didn't have a reduction of this phase, we'll insert the padding reductions in the next line

                //Otherwise look at how many times the reduction was contained. If it was more than once zip, otherwise cartesian
                List<int> locations = new List<int>();

                for (int j = 0; j < reductionList.Count; j++ )
                    if (reductionList[j] == i)
                        locations.Add(j);
                    // reductionList.f .FindAll((int x) => x == i);

                    Validity.Assert(locations.Count > 0, "We should have trapped this case and short-cut the loop, {DF3D67B8-F1B5-4C61-BF3B-930D4C860FA9}");

                if (locations.Count == 1)
                {
                    //There was one location, this is a cartesian expansion

                    ReplicationInstruction ri = new ReplicationInstruction()
                                                    {CartesianIndex = locations[0], ZipIndecies = null, Zipped = false};
                    ret.Add(ri);

                    if (i - lastSeenReduction > 1)
                    {
                        //Add padding instructions
                        //@TODO(Luke): Suspect cartesian padding is incorrect

                        for (int padding = 0; padding < ((i - lastSeenReduction) -1); padding++)
                        {
                            ReplicationInstruction riPad = new ReplicationInstruction() { CartesianIndex = locations[0], ZipIndecies = null, Zipped = false };
                            ret.Add(riPad);

                        }

                    }

                }
                else
                {
                    ReplicationInstruction ri = new ReplicationInstruction()
                                                    {CartesianIndex = -1, ZipIndecies = locations, Zipped = true};
                    ret.Add(ri);

                    if (i - lastSeenReduction > 1)
                    {
                        //Add padding instructions
                        //@TODO(Luke): Suspect cartesian padding is incorrect

                        for (int padding = 0; padding < ((i - lastSeenReduction)-1); padding++)
                        {
                            ReplicationInstruction riPad = new ReplicationInstruction() { CartesianIndex = -1, ZipIndecies = locations, Zipped = true }; 
                            ret.Add(riPad);

                        }

                    }

                }

                lastSeenReduction = i;


            }

            return ret;


        }

        public static List<ReplicationInstruction> ReductionToInstructions(List<int> reductionList, List<ReplicationInstruction> providedControl)
        {
            
            List<ReplicationInstruction> ret = new List<ReplicationInstruction>();

            ret.AddRange(providedControl);

            //if (providedControl.Count > 0)
            //    throw new NotImplementedException();


            //Basic sanity check on the reductions
            foreach (int reduction in reductionList)
                Validity.Assert(reduction >= 0, "Negative reductions aren't supported {991CBD60-8B2B-438B-BDEB-734D18B4FE68}");

            int maxReductionCount = int.MinValue;
            foreach (int reduction in reductionList)
                maxReductionCount = Math.Max(maxReductionCount, reduction);

            //@PERF This is going to do a O(n^2) cost scan, when we could just build a reverse index once....

            int lastSeenReduction = 0; //This is the last instruction that we have seen and don't need to inject further
            //reductions from

            //Walk over all the reductions that we're going to do
            for (int i = 1; i <= maxReductionCount; i++)
            {
                if (!reductionList.Contains(i))
                    continue; //We didn't have a reduction of this phase, we'll insert the padding reductions in the next line

                //Otherwise look at how many times the reduction was contained. If it was more than once zip, otherwise cartesian
                List<int> locations = new List<int>();

                for (int j = 0; j < reductionList.Count; j++)
                    if (reductionList[j] == i)
                        locations.Add(j);
                // reductionList.f .FindAll((int x) => x == i);

                Validity.Assert(locations.Count > 0, "We should have trapped this case and short-cut the loop, {DF3D67B8-F1B5-4C61-BF3B-930D4C860FA9}");

                if (locations.Count == 1)
                {
                    //There was one location, this is a cartesian expansion

                    ReplicationInstruction ri = new ReplicationInstruction() { CartesianIndex = locations[0], ZipIndecies = null, Zipped = false };
                    ret.Add(ri);

                    if (i - lastSeenReduction > 1)
                    {
                        //Add padding instructions
                        //@TODO(Luke): Suspect cartesian padding is incorrect

                        for (int padding = 0; padding < ((i - lastSeenReduction) - 1); padding++)
                        {
                            ReplicationInstruction riPad = new ReplicationInstruction() { CartesianIndex = locations[0], ZipIndecies = null, Zipped = false };
                            ret.Add(riPad);

                        }

                    }

                }
                else
                {
                    ReplicationInstruction ri = new ReplicationInstruction() { CartesianIndex = -1, ZipIndecies = locations, Zipped = true };
                    ret.Add(ri);

                    if (i - lastSeenReduction > 1)
                    {
                        //Add padding instructions
                        //@TODO(Luke): Suspect cartesian padding is incorrect

                        for (int padding = 0; padding < ((i - lastSeenReduction) - 1); padding++)
                        {
                            ReplicationInstruction riPad = new ReplicationInstruction() { CartesianIndex = -1, ZipIndecies = locations, Zipped = true };
                            ret.Add(riPad);

                        }

                    }

                }

                lastSeenReduction = i;


            }

            return ret;


        }


        /// <summary>
        /// This function takes a replication instruction set and uses it to compute all of the types that would be called
        /// </summary>
        /// <param name="formalParams"></param>
        /// <param name="replicationInstructions"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static List<List<StackValue>> ComputeAllReducedParams(List<StackValue> formalParams, List<ReplicationInstruction> replicationInstructions, RuntimeCore runtimeCore)
        {
            List<List<StackValue>> ret; //= new List<List<StackValue>>();

            //First approximation generates possibilities that may never actually exist, due to 

            ret = ComputeReducedParamsSuperset(formalParams, replicationInstructions, runtimeCore);

            return ret;




        }

        public static List<List<StackValue>> ComputeReducedParamsSuperset(List<StackValue> formalParams, List<ReplicationInstruction> replicationInstructions, RuntimeCore runtimeCore)
        {
            //Compute the reduced Type args
            List<List<StackValue>> reducedParams = new List<List<StackValue>>();

            List<StackValue> basicList = new List<StackValue>();

            //Copy the types so unaffected ones get copied back directly
            foreach (StackValue sv in formalParams)
                basicList.Add(sv);

            reducedParams.Add(basicList);


            foreach (ReplicationInstruction ri in replicationInstructions)
                if (ri.Zipped)
                {
                    foreach (int index in ri.ZipIndecies)
                    {
                        //This should generally be a collection, so we need to do a one phase unboxing
                        StackValue target = basicList[index];
                        StackValue reducedSV = StackValue.Null;

                        if (target.IsArray)
                        {

                            //Array arr = formalParams[index].Payload as Array;
                            HeapElement he = ArrayUtils.GetHeapElement(basicList[index], runtimeCore);

                            Validity.Assert(he.Stack != null);

                            //The elements of the array are still type structures
                            if (he.VisibleSize == 0)
                                reducedSV = StackValue.Null;
                            else
                            {
                                var arrayStats = ArrayUtils.GetTypeExamplesForLayer(basicList[index], runtimeCore).Values;

                                List<List<StackValue>> clonedList = new List<List<StackValue>>();

                                foreach (List<StackValue> list in reducedParams)
                                    clonedList.Add(list);

                                reducedParams.Clear();

                                foreach (StackValue sv in arrayStats)
                                {
                                    foreach (List<StackValue> lst in clonedList)
                                    {
                                        List<StackValue> newArgs = new List<StackValue>();
                                        
                                        newArgs.AddRange(lst);
                                        newArgs[index] = sv;

                                        reducedParams.Add(newArgs);
                                    }
                                    
                                }

                            }
                        }
                        else
                        {
                            System.Console.WriteLine("WARNING: Replication unbox requested on Singleton. Trap: 437AD20D-9422-40A3-BFFD-DA4BAD7F3E5F");
                            reducedSV = target;
                        }

                        //reducedType.IsIndexable = false;
                        //reducedParamTypes[index] = reducedSV;
                    }
                }
                else
                {
                    //This should generally be a collection, so we need to do a one phase unboxing
                    int index = ri.CartesianIndex;
                    //This should generally be a collection, so we need to do a one phase unboxing
                    StackValue target = basicList[index];
                    StackValue reducedSV = StackValue.Null;

                    if (target.IsArray)
                    {

                        //Array arr = formalParams[index].Payload as Array;
                        HeapElement he = ArrayUtils.GetHeapElement(basicList[index], runtimeCore);



                        //It is a collection, so cast it to an array and pull the type of the first element
                        //@TODO(luke): Deal with sparse arrays, if the first element is null this will explode

                        Validity.Assert(he.Stack != null);

                        //The elements of the array are still type structures
                        if (he.VisibleSize == 0)
                            reducedSV = StackValue.Null;
                        else
                        {
                            var arrayStats = ArrayUtils.GetTypeExamplesForLayer(basicList[index], runtimeCore).Values;

                            List<List<StackValue>> clonedList = new List<List<StackValue>>();

                            foreach (List<StackValue> list in reducedParams)
                                clonedList.Add(list);

                            reducedParams.Clear();
                        

                            foreach (StackValue sv in arrayStats)
                            {
                                foreach (List<StackValue> lst in clonedList)
                                {
                                    List<StackValue> newArgs = new List<StackValue>();

                                    newArgs.AddRange(lst);
                                    newArgs[index] = sv;

                                    reducedParams.Add(newArgs);
                                }

                            }

                        }
                    }
                    else
                    {
                        System.Console.WriteLine("WARNING: Replication unbox requested on Singleton. Trap: 437AD20D-9422-40A3-BFFD-DA4BAD7F3E5F");
                        reducedSV = target;
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
            List<StackValue> reducedParamTypes = new List<StackValue>();

            //Copy the types so unaffected ones get copied back directly
            foreach (StackValue sv in formalParams)
                reducedParamTypes.Add(sv);


            foreach (ReplicationInstruction ri in replicationInstructions)
                if (ri.Zipped)
                {
                    foreach (int index in ri.ZipIndecies)
                    {
                        //This should generally be a collection, so we need to do a one phase unboxing
                        StackValue target = reducedParamTypes[index];
                        StackValue reducedSV = StackValue.Null;

                        if (target.IsArray)
                        {

                            //Array arr = formalParams[index].Payload as Array;
                            HeapElement he = ArrayUtils.GetHeapElement(reducedParamTypes[index], runtimeCore);



                            //It is a collection, so cast it to an array and pull the type of the first element
                            //@TODO(luke): Deal with sparse arrays, if the first element is null this will explode

                            Validity.Assert(he.Stack != null);

                            //The elements of the array are still type structures
                            if (he.VisibleSize == 0)
                                reducedSV = StackValue.Null;
                            else
                                reducedSV = he.Stack[0];
                        }
                        else
                        {
                            System.Console.WriteLine("WARNING: Replication unbox requested on Singleton. Trap: 437AD20D-9422-40A3-BFFD-DA4BAD7F3E5F");
                            reducedSV = target;
                        }

                        //reducedType.IsIndexable = false;
                        reducedParamTypes[index] = reducedSV;
                    }
                }
                else
                {
                    //This should generally be a collection, so we need to do a one phase unboxing
                    int index = ri.CartesianIndex;
                    StackValue target = reducedParamTypes[index];
                    StackValue reducedSV;

                    if (target.IsArray)
                    {
                        //ProtoCore.DSASM.Mirror.DsasmArray arr = formalParams[index].Payload as ProtoCore.DSASM.Mirror.DsasmArray;
                        HeapElement he = ArrayUtils.GetHeapElement(reducedParamTypes[index], runtimeCore);

                        //It is a collection, so cast it to an array and pull the type of the first element
                        //@TODO(luke): Deal with sparse arrays, if the first element is null this will explode
                        //Validity.Assert(arr != null);
                        //Validity.Assert(arr.members[0] != null);
                        Validity.Assert(he.Stack != null);



                        //The elements of the array are still type structures
                        //reducedType = arr.members[0].Type;
                        if (he.VisibleSize == 0)
                            reducedSV = StackValue.Null;
                        else
                            reducedSV = he.Stack[0];

                    }
                    else
                    {
                        System.Console.WriteLine("WARNING: Replication unbox requested on Singleton. Trap: 437AD20D-9422-40A3-BFFD-DA4BAD7F3E5F");
                        reducedSV = target;
                    }

                    //reducedType.IsIndexable = false;
                    reducedParamTypes[index] = reducedSV;
                }

            return reducedParamTypes;

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

       


        public static List<List<ReplicationInstruction>> BuildReplicationCombinations(List<ReplicationInstruction> providedControl, List<StackValue> formalParams, RuntimeCore runtimeCore)
        {

            
            //@TODO: Performance hint - this should really be done with a yield-generator unless the parrallelism is useful
            //@ROSO: This is not generating a minimal set


            //First build a list of reducible parameters

            List<int> reducibles = new List<int>();

            int maxDepth = 0;

            for (int i = 0; i < formalParams.Count; i++)
            {
                int itemMaxDepth = GetMaxReductionDepth(formalParams[i], runtimeCore);

                if (itemMaxDepth > 0)
                    reducibles.Add(i);

                maxDepth = maxDepth + itemMaxDepth;
            }

            if (providedControl.Count > maxDepth)
            {
                throw new ReplicationCaseNotCurrentlySupported(
                    string.Format(Resources.MaxDimensionExceeded, "{1EC8AF3C-48D6-4582-999E-ADBCBF9155D1}"));
            }
            else
            {
                //Reduce the available reducions by the amount that we've been instructed to
                maxDepth -= providedControl.Count;
            }


            List<List<int>> cleanedReductions = new List<List<int>>();

            if (maxDepth > 0)
            {
                List<List<int>> reductions = new List<List<int>>();

                for (int i = 0; i <= maxDepth; i++)
                    reductions.AddRange(BuildAllocation(formalParams.Count, maxDepth));

                if (providedControl.Count > 0)
                {
                    //The add in the reductions associated with the provided controls.
                    //The silly copy-ctoring here is to avoid the issues of modifying a collection with an iterator on it
                    List<List<int>> completedReductions = new List<List<int>>();
                    
                    List<ReplicationInstruction> reversedControl = new List<ReplicationInstruction>();
                    reversedControl.AddRange(providedControl);
                    reversedControl.Reverse();

                    foreach (List<int> reduction in reductions)
                    {
                        List<int> reducitonList = new List<int>();
                        reducitonList.AddRange(reduction);

                        foreach (ReplicationInstruction ri in reversedControl)
                        {
                            if (!ri.Zipped)
                                reducitonList[ri.CartesianIndex] = reducitonList[ri.CartesianIndex] + 1;
                            else
                            {
                                foreach (int i in ri.ZipIndecies)
                                    reducitonList[i] = reducitonList[i] + 1;
                            }

                        }

                        completedReductions.Add(reducitonList);
                    }

                    reductions = completedReductions;

                }
                




                foreach (List<int> list in reductions)
                {
                    bool append = true;
                    for (int i = 0; i < list.Count; i++)
                        if (list[i] > GetMaxReductionDepth(formalParams[i], runtimeCore))
                        {
                            append = false;
                            break;
                        }

                    int acc = 0;
                    for (int i = 0; i < list.Count; i++)
                        acc += list[i];

                    //We must be reducing something
                    if (acc == 0)
                        append = false;

                    if (append)
                        cleanedReductions.Add(list);
                }



            }

            //if (providedControl.Count > 0)
            //    throw new NotImplementedException("begone.");


            List<List<ReplicationInstruction>> ret = new List<List<ReplicationInstruction>>();

            //At this stage cleanedReductions holds the number of times to try and reduce each argument
            //All options being suggested should be possible

            foreach (List<int> reduction in cleanedReductions)
            {

                ////@PERF - unify data formats so we don't have to do this conversion again
                //List<List<int>> partial = new List<List<int>>();
                //foreach (int i in reduction)
                //{
                //    if (i == 0)
                //        partial.Add(new List<int> { });
                //    else
                //        partial.Add(new List<int> { i });

                //}

                //ret.Add(ReductionToInstructions(reduction));//Replicator.Old_ConvertGuidesToInstructions(partial));
                ret.Add(ReductionToInstructions(reduction, providedControl));//Replicator.Old_ConvertGuidesToInstructions(partial));

            }

            return ret;


        }

        ///// <summary>
        ///// Get the maximum depth to which an object can be reduced
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //[Obsolete]
        //public static int GetMaxReductionDepth(ProtoCore.Lang.Obj obj)
        //{
        //    if (!obj.Type.IsIndexable)
        //        return 0;
        //    else
        //    {
        //        return 1 + GetMaxReductionDepth((Obj)((DSASM.Mirror.DsasmArray)obj.Payload).members[0]);
        //    }
        //}
        
        /// <summary>
        /// Get the maximum depth to which an element can be reduced
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
            HeapElement he = ProtoCore.Utils.ArrayUtils.GetHeapElement(sv, runtimeCore);
            foreach (var subSv in he.VisibleItems)
            {
                maxReduction = Math.Max(maxReduction, RecursiveProtectGetMaxReductionDepth(subSv, runtimeCore, depthCount + 1));
            }

            return 1 + maxReduction;   
        }

        
    }
}

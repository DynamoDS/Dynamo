using ProtoCore.DSASM;
using ProtoCore.Exceptions;
using ProtoCore.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtoCore.Lang.Replication
{
    internal class ElementAtLevel
    {
        public List<int> Indices { get; private set; }
        public StackValue Element { get; private set; }

        public ElementAtLevel(StackValue element)
        {
            Indices = new List<int>();
            Element = element;
        }

        public ElementAtLevel(StackValue element, List<int> indices)
        {
            Indices = indices;
            Element = element;
        }
    }

    internal class ArgumentAtLevel
    {
        public List<List<int>> Indices;
        public bool IsDominant { get; private set; }
        public StackValue Argument { get; private set; }

        public ArgumentAtLevel(StackValue argument)
        {
            Indices = new List<List<int>>();
            IsDominant = false;
            Argument = argument;
        }

        public ArgumentAtLevel(StackValue argument, List<List<int>> indices, bool isDominant)
        {
            Indices = indices;
            IsDominant = isDominant;
            Argument = argument;
        }
    }

    /// <summary>
    /// The positions of items at dominant list.
    /// </summary>
    public class DominantListStructure
    {
        public int ArgumentIndex { get; private set; }
        public List<List<int>> Indices { get; private set; }
        public DominantListStructure(List<List<int>> indices, int argumentIndex)
        {
            Indices = indices;
            ArgumentIndex = argumentIndex;
        }
    }

    public class ArgumentAtLevelStructure
    {
        public List<StackValue> Arguments { get; private set; }
        public DominantListStructure DominantStructure { get; private set; }
        public ArgumentAtLevelStructure(List<StackValue> arguments, DominantListStructure dominantStructure)
        {
            Arguments = arguments;
            DominantStructure = dominantStructure;
        }
    }

    public class AtLevelHandler
    {
        private static List<ElementAtLevel> GetElementsAtLevel(StackValue argument, int level, List<int> indices, bool recordIndices, RuntimeCore runtimeCore)
        {
            var array = runtimeCore.Heap.ToHeapObject<DSArray>(argument);
            if (array == null)
            {
                return new List<ElementAtLevel>();
            }

            int count = array.Values.Count();
            if (level == 0)
            {
                return array.Values.Zip(Enumerable.Range(0, count), (v, i) =>
                {
                    if (recordIndices)
                    {
                        var newIndices = new List<int>(indices);
                        newIndices.Add(i);
                        return new ElementAtLevel(v, newIndices);
                    }
                    else
                    { 
                        return new ElementAtLevel(v);
                    }
               }).ToList();
            }
            else
            {
                return array.Values.Zip(Enumerable.Range(0, count), (v, i) =>
                {
                    if (recordIndices)
                    {
                        var newIndices = new List<int>(indices);
                        newIndices.Add(i);
                        return GetElementsAtLevel(v, level - 1, newIndices, recordIndices, runtimeCore);
                    }
                    else
                    {
                        return GetElementsAtLevel(v, level - 1, new List<int>(), recordIndices, runtimeCore);
                    }
                }).SelectMany(vs => vs).ToList();
            }
        }

        private static ArgumentAtLevel GetArgumentAtLevel(StackValue argument, AtLevel atLevel, RuntimeCore runtimeCore)
        {
            if (atLevel.Level >= 0)
            {
                return new ArgumentAtLevel(argument);
            }

            int maxDepth = Replicator.GetMaxReductionDepth(argument, runtimeCore);
            int nestedLevel = maxDepth + atLevel.Level;

            // Promote the array
            while (nestedLevel < 0)
            {
                try
                {
                    argument = runtimeCore.RuntimeMemory.Heap.AllocateArray(new StackValue[1] { argument });
                }
                catch (RunOutOfMemoryException)
                {
                    runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return null;
                }
                nestedLevel++;
            }

            if (nestedLevel == 0)
            {
                return new ArgumentAtLevel(argument);
            }
            else
            {
                var elements = GetElementsAtLevel(argument, nestedLevel, new List<int>(), atLevel.IsDominant, runtimeCore);
                try
                {
                    argument = runtimeCore.RuntimeMemory.Heap.AllocateArray(elements.Select(e => e.Element).ToArray());
                }
                catch (RunOutOfMemoryException)
                {
                    runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                    return null;
                }
                var indices = elements.Select(e => e.Indices).ToList();
                return new ArgumentAtLevel(argument, indices, atLevel.IsDominant);
            }
        }

        private static List<ArgumentAtLevel> GetArgumentsAtLevels(List<StackValue> arguments, List<AtLevel> atLevels, RuntimeCore runtimeCore)
        {
            if (atLevels.All(x => x.Level >= 0))
                return arguments.Select(a => new ArgumentAtLevel(a)).ToList();

            List<ArgumentAtLevel> argumentAtLevels = new List<ArgumentAtLevel>();
            for (int i = 0; i < arguments.Count; i++)
            {
                var arg = GetArgumentAtLevel(arguments[i], atLevels[i], runtimeCore);
                argumentAtLevels.Add(arg);
            }
            return argumentAtLevels;

        }

        /// <summary>
        /// Returns arguments at the corresponding levles and dominant list structure.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="atLevels"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        public static ArgumentAtLevelStructure GetArgumentAtLevelStructure(List<StackValue> arguments, List<AtLevel> atLevels, RuntimeCore runtimeCore)
        {
            var argumentAtLevels = GetArgumentsAtLevels(arguments, atLevels, runtimeCore);
            arguments = argumentAtLevels.Select(a => a.Argument).ToList();

            int domListIndex = argumentAtLevels.FindIndex(x => x.IsDominant);
            if (domListIndex < 0)
            {
                return new ArgumentAtLevelStructure(arguments, null);
            }

            if (runtimeCore != null && argumentAtLevels.Count(x => x.IsDominant) > 1)
            {
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.MoreThanOneDominantList, Resources.MoreThanOneDominantList);
                return new ArgumentAtLevelStructure(arguments, null);
            }

            var indices = argumentAtLevels[domListIndex].Indices;
            var dominantStructure = new DominantListStructure(indices, domListIndex);
            return new ArgumentAtLevelStructure(arguments, dominantStructure);
        }

        /// <summary>
        /// If an input is a dominant list, restructure the result based on the
        /// structure of dominant list. 
        /// 
        /// Note the dominant structure will be restored only if the dominant
        /// list is zipped with other arguments, or the replication is applied
        /// to the dominant list firstly.
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="domStructure"></param>
        /// <param name="instructions"></param>
        /// <param name="runtimeCore"></param>
        /// <returns></returns>
        public static StackValue RestoreDominantStructure(
            StackValue ret,
            DominantListStructure domStructure,
            List<ReplicationInstruction> instructions,
            RuntimeCore runtimeCore)
        {
            if (domStructure == null)
            {
                return ret;
            }

            var domListIndex = domStructure.ArgumentIndex;
            var indicesList = domStructure.Indices;

            // If there is replication on the dominant list, it should be the
            // topest replicaiton. 
            if (instructions != null && instructions.Any())
            {
                var firstInstruciton = instructions.First();
                if (firstInstruciton.Zipped)
                {
                    if (!firstInstruciton.ZipIndecies.Contains(domListIndex))
                    {
                        return ret;
                    }
                }
                else
                {
                    if (firstInstruciton.CartesianIndex != domListIndex)
                    {
                        return ret;
                    }
                }
            }

            // Allocate an empty array to hold the value
            StackValue newRet;
            try
            {
                newRet = runtimeCore.RuntimeMemory.Heap.AllocateArray(new StackValue[] { });
            }
            catch (RunOutOfMemoryException)
            {
                runtimeCore.RuntimeStatus.LogWarning(Runtime.WarningID.RunOutOfMemory, Resources.RunOutOfMemory);
                return StackValue.Null;
            }
            var array = runtimeCore.Heap.ToHeapObject<DSArray>(newRet);

            // Write the result back
            var values = ret.IsArray ? runtimeCore.Heap.ToHeapObject<DSArray>(ret).Values : Enumerable.Repeat(ret, 1);
            var valueIndicePairs = values.Zip(indicesList, (val, idx) => new { Value = val, Indices = idx });
            foreach (var item in valueIndicePairs)
            {
                var value = item.Value;
                var indices = item.Indices.Select(x => StackValue.BuildInt(x)).ToArray();
                array.SetValueForIndices(indices, value, runtimeCore);
            }

            return newRet;
        }
    }
}

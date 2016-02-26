using ProtoCore.DSASM;
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

    public class ArgumentAtLevel
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

    public class AtLevelExtractor
    {
        private static List<ElementAtLevel> GetElementsAtLevel(StackValue argument, int level, List<int> indices, RuntimeCore runtimeCore)
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
                    var newIndices = new List<int>(indices);
                    newIndices.Add(i);
                    return new ElementAtLevel(v, newIndices);
                }).ToList();
            }
            else
            {
                return array.Values.Zip(Enumerable.Range(0, count), (v, i) =>
                {
                    var newIndices = new List<int>(indices);
                    newIndices.Add(i);
                    return GetElementsAtLevel(v, level - 1, newIndices, runtimeCore);
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
                argument = runtimeCore.RuntimeMemory.Heap.AllocateArray(new StackValue[1] { argument });
                nestedLevel++;
            }

            if (nestedLevel == 0)
            {
                return new ArgumentAtLevel(argument);
            }
            else
            {
                var elements = GetElementsAtLevel(argument, nestedLevel, new List<int>(), runtimeCore);
                argument = runtimeCore.RuntimeMemory.Heap.AllocateArray(elements.Select(e => e.Element).ToArray());
                var indices = elements.Select(e => e.Indices).ToList();
                return new ArgumentAtLevel(argument, indices, atLevel.IsDominant);
            }
        }

        public static List<ArgumentAtLevel> GetArgumentsAtLevels(List<StackValue> arguments, List<AtLevel> atLevels, RuntimeCore runtimeCore)
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

        public static DominantListStructure GetDominantStructure(List<ArgumentAtLevel> arguments)
        {
            int domListIndex = arguments.FindIndex(x => x.IsDominant);
            if (domListIndex < 0)
            {
                return null;
            }

            var indices = arguments[domListIndex].Indices;
            return new DominantListStructure(indices, domListIndex);
        }

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
            var newRet = runtimeCore.RuntimeMemory.Heap.AllocateArray(new StackValue[] { });
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

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Dynamo;
using NUnit.Framework;
using static DSPythonTests.PythonEvalTests;

namespace DynamoPythonTests
{
    public class PythonEvalTestsWithLibraries : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FFITarget.dll");
            libraries.Add("DSCoreNodes.dll");
        }

        public IEnumerable<PythonEvaluatorDelegate> Evaluators = new List<PythonEvaluatorDelegate> {
            DSCPython.CPythonEvaluator.EvaluatePythonScript
        };

        [Test]
        public void TestBigIntegerEncodingDecoding()
        {
            string code = @"
import sys
import clr
clr.AddReference('FFITarget')
from FFITarget import DummyMath

# Provide Python int as arguments: Python => .NET
sum = DummyMath.Sum(11111111111111111111, 11111111111111111111)
# sum contains a BigInteger and we use it in Python + operation: .NET => Python
sum = sum + 1

OUT = sum
";
            var empty = new ArrayList();
            var expected = BigInteger.Parse("22222222222222222223");
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void TestListDecoding()
        {
            string code = @"
import sys
import clr
clr.AddReference('DSCoreNodes')
clr.AddReference('FFITarget')
from FFITarget import DummyCollection
from DSCore import List

l = ['a']
# Python list => .NET IList
untypedList = List.AddItemToEnd('b', l)
untypedList.Add('c')

l2 = ['a','b']
# Python list => .NET IList<>
typedList = List.SetDifference(l2, l)
typedList.Add('b')

l3 = [[1,2],[3,4]]
# Python list (nested) => .NET IList<IList<>>
flattenedList = List.Flatten(l3)

l4 = []
# Python list (empty) => .NET IList
elementCount = List.Count(l4)

sum = 0
# Python-wrapped .NET List can be iterated over
for i in flattenedList:
  sum = sum + i

l5 = [1,2,3,4]
# Python list => .NET IEnumerable<>
max = List.MaximumItem(l5)

# Python list => .NET IEnumerable
enumerable = DummyCollection.ReturnIEnumerable(l2)

OUT = untypedList, typedList, flattenedList, elementCount, sum, max, enumerable
";
            var empty = new ArrayList();
            var expected = new ArrayList { new ArrayList { "a", "b", "c" }, new ArrayList { "b", "b" }, new ArrayList { 1, 2, 3, 4 }, 0, 10, 4
                , new ArrayList { "a", "b" } };
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                CollectionAssert.AreEqual(expected, result as IEnumerable);
            }
        }

        [Test]
        public void TestArrayEncoding()
        {
            string code = @"
import sys
import clr
clr.AddReference('FFITarget')
clr.AddReference('DSCoreNodes')
from FFITarget import DummyCollection
from DSCore import List
from array import array

# .NET array => Python list
a = DummyCollection.MakeArray(1,2)
a[0] = a[1] + 1
b = len(a)
a[1] = b
a = List.AddItemToEnd(1, a)

OUT = a
";
            var empty = new ArrayList();
            var expected = new ArrayList { 3, 2, 1 };
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                CollectionAssert.AreEqual(expected, result as IEnumerable);
            }
        }

        [Test]
        public void TestTupleDecoding()
        {
            string code = @"
import sys
import clr
clr.AddReference('FFITarget')
clr.AddReference('DSCoreNodes')
from FFITarget import DummyCollection
from DSCore import List

t = (1,2,3)
# Python tuple => .NET array
a = DummyCollection.MakeArray(t)
# Python tuple => .NET IList
l = List.AddItemToEnd(4, t)

OUT = a, l
";
            var empty = new ArrayList();
            var expected = new ArrayList { new ArrayList { 1, 2, 3 }, new ArrayList { 1, 2, 3, 4 } };
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                CollectionAssert.AreEqual(expected, result as IEnumerable);
            }
        }

        [Test]
        public void TestRangeDecodingCPython()
        {
            string code = @"
import sys
import clr
clr.AddReference('FFITarget')
clr.AddReference('DSCoreNodes')
from FFITarget import DummyCollection
from DSCore import List

r = range(0, 10, 2)
# Python range => .NET array - Works in both
a = DummyCollection.MakeArray(r)
# Python range => .NET IList - Does not work in IronPython
l = List.AddItemToEnd(10, range(0, 10, 2))

OUT = a, l
";
            var empty = new ArrayList();
            var expected = new ArrayList { new ArrayList { 0, 2, 4, 6, 8 }, new ArrayList { 0, 2, 4, 6, 8, 10 } };
            var result = DSCPython.CPythonEvaluator.EvaluatePythonScript(code, empty, empty);
            Assert.IsTrue(result is IEnumerable);
            CollectionAssert.AreEqual(expected, result as IEnumerable);
        }

        [Test]
        public void TestDictionaryDecodingCPython()
        {
            string code = @"
import sys
import clr
clr.AddReference('FFITarget')
clr.AddReference('DSCoreNodes')
from DSCore import List
from FFITarget import DummyCollection

d = {'one': 1, 'two': 2, 'three': 3}

# Python dict => .NET IDictionary
untypedDictionary = DummyCollection.AcceptIDictionary(d)
untypedDictionary['four'] = 4

# Python dict => .NET IDictionary<> - Does not work in IronPython
typedDictionary = DummyCollection.AcceptDictionary(d)
typedDictionary['four'] = 4

# Python dict => .NET IEnumerable - Returns keys in both engines
sortedKeys = List.Sort(d)

OUT = untypedDictionary, typedDictionary, sortedKeys
";
            var empty = new ArrayList();
            var expectedDictionary = new Dictionary<string, int> {
                { "one", 1 }, { "two", 2 }, { "three", 3 }, { "four", 4 }
            };
            var expectedKeys = new List<string> { "one", "three", "two" };
            var result = DSCPython.CPythonEvaluator.EvaluatePythonScript(code, empty, empty);
            Assert.IsTrue(result is IList);
            var resultList = result as IList;
            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(resultList[i] is IDictionary);
                DictionaryAssert(expectedDictionary, resultList[i] as IDictionary);
            }
            Assert.IsTrue(resultList[2] is IList);
            CollectionAssert.AreEqual(expectedKeys, resultList[2] as IList);
        }

        [Test]
        public void TestDictionaryViewsDecoding()
        {
            var code = @"
import clr
clr.AddReference('DSCoreNodes')
from DSCore import List

d = {'one': 1, 'two': 2, 'three': 3}

dk = List.AddItemToEnd('four', d.keys())
dv = List.AddItemToEnd(4, d.values())
di = List.AddItemToEnd(('four', 4), d.items())

OUT = dk, dv, di
";
            var empty = new ArrayList();
            var expected = new ArrayList[] {
                new ArrayList { "one", "two", "three", "four" },
                new ArrayList { 1, 2, 3, 4 },
                new ArrayList { new ArrayList { "one", 1 }, new ArrayList { "two", 2 }, new ArrayList { "three", 3 }, new ArrayList { "four", 4 } }
            };
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IEnumerable);
                var i = 0;
                foreach (var item in result as IEnumerable)
                {
                    Assert.IsTrue(item is IEnumerable);
                    CollectionAssert.AreEquivalent(expected[i], item as IEnumerable);
                    i++;
                }
            }
        }

        [Test]
        public void TestSetDecodingCPython()
        {
            var code = @"
import clr
clr.AddReference('DSCoreNodes')
from DSCore import List

s = { 'hello' }
fs = frozenset(s)
# Python set => .NET IList - Does not work in IronPython
s2 = List.AddItemToEnd('world', s)
fs2 = List.AddItemToEnd('world', fs)
OUT = s2, fs2
";
            var empty = new ArrayList();
            var expected = new string[] { "hello", "world" };
            var result = DSCPython.CPythonEvaluator.EvaluatePythonScript(code, empty, empty);
            Assert.IsTrue(result is IEnumerable);
            foreach (var item in result as IEnumerable)
            {
                Assert.IsTrue(item is IEnumerable);
                CollectionAssert.AreEquivalent(expected, item as IEnumerable);
            }
        }

        /// <summary>
        /// Tests that the results from doing dir(DSCore) are somewhat equivalent between engines.
        /// They are known not to be exactly the same. Namely the following differences have been found:
        /// - Private (double underscore) attributes are different across engines
        /// - IronPython does not define private attributes for namespaces at all
        /// - PythonNET includes additional methods to classes (Overloads and Finalize)
        /// - PythonNET also includes results from assemblies not explicitly added as references (DSCore.File)
        /// </summary>
        [Test]
        public void TestDir()
        {
            var code = @"
import clr
clr.AddReference('DSCoreNodes')
import DSCore
ref = DSCore
dirAll = []
dic = {}
for key in dir(ref) :
    dir_str = getattr(ref, key)
    dirAll.append(dir(dir_str))
    for value in dirAll :
        dic[key] = value
OUT = dic
";
            var empty = new ArrayList();
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IDictionary);
                var dsCore = result as IDictionary;
                // Assertions for a Class
                CollectionAssert.Contains(dsCore.Keys, "Color", "public class not found");
                Assert.IsTrue(dsCore["Color"] is IList);
                var color = dsCore["Color"] as IList;
                CollectionAssert.Contains(color, "ByARGB", "public static method not found");
                CollectionAssert.Contains(color, "Equals", "public instance method not found");
                CollectionAssert.Contains(color, "Alpha", "public property not found");
                CollectionAssert.Contains(color, "IndexedColor1D", "nested class not found");
                // Assertions for a Namespace
                CollectionAssert.Contains(dsCore.Keys, "IO", "nested namespace not found");
                Assert.IsTrue(dsCore["IO"] is IList);
                var dsCoreIO = dsCore["IO"] as IList;
                CollectionAssert.Contains(dsCoreIO, "Image", "class in nested namespace not found");
            }
        }

        private void DictionaryAssert(IDictionary expected, IDictionary actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            foreach (var key in expected.Keys)
            {
                Assert.AreEqual(expected[key], actual[key]);
            }
        }
    }
}

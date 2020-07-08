using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Dynamo;
using NUnit.Framework;
using static DSIronPythonTests.IronPythonTests;

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
            DSCPython.CPythonEvaluator.EvaluatePythonScript,
            DSIronPython.IronPythonEvaluator.EvaluateIronPythonScript
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
from DSCore import List

l = ['a']
# Python list => .NET IList
untypedList = List.AddItemToEnd('b', l)
untypedList.Add('c')

l2 = ['a','b']
# Python list => .NET IList<>
typedList = List.SetDifference(l2, l)
typedList.Add('b')

OUT = untypedList, typedList
";
            var empty = new ArrayList();
            var expected = new ArrayList { new ArrayList { "a", "b", "c" }, new ArrayList { "b", "b" } } ;
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
        [Category("Failure")]
        public void TestDictionaryEncoding()
        {
            string code = @"
import sys
import clr
clr.AddReference('FFITarget')
clr.AddReference('DesignScriptBuiltin')
from DesignScript.Builtin import Dictionary
from FFITarget import DummyCollection

d = {'one': 1, 'two': 2, 'three': 3}
# Python dict => DS Dictionary - Does not work in either engine
# d2 = Dictionary.SetValueAtKeys(d, ['four'], [4])

# Python dict => .NET IDictionary - Does not work in CPython
d = DummyCollection.AcceptIDictionary(d)
# .NET IDictionary => Python dict - Works in IronPython. Could not test in CPython due to previous bug
d['five'] = 5

OUT = d
";
            var empty = new ArrayList();
            var expected = new Dictionary<string, int> {
                { "one", 1 }, { "two", 2 }, { "three", 3 }, { "five", 5 }
            };
            foreach (var pythonEvaluator in Evaluators)
            {
                var result = pythonEvaluator(code, empty, empty);
                Assert.IsTrue(result is IDictionary);
                CollectionAssert.AreEquivalent(expected, result as IDictionary);
            }
        }
    }
}

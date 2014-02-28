using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DSIronPythonTests
{
    public class IronPythonTests
    {
        [Test]
        public void EvaluatorWorks()
        {
            var empty = new ArrayList();
            var output = DSIronPython.IronPythonEvaluator.EvaluateIronPythonScript("OUT = 0", empty, empty);
            Assert.AreEqual(0, output);
        }

        [Test]
        public void BindingsWork()
        {
            const string expected = "Hi!";
            
            var names = new ArrayList { "test" };
            var vals = new ArrayList { expected };

            var output = DSIronPython.IronPythonEvaluator.EvaluateIronPythonScript(
                "OUT = test",
                names,
                vals);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void FirstClassFunctions()
        {
            Func<string, string> func = s => s + " rule!";

            var names = new ArrayList { "f" };
            var vals = new ArrayList { func };

            dynamic output =
                DSIronPython.IronPythonEvaluator.EvaluateIronPythonScript(
                    "g = lambda x: f(x); OUT = g",
                    names,
                    vals);

            Assert.AreEqual("functions rule!", output("functions"));
        }
    }
}

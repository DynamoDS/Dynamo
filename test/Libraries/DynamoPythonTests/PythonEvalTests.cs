using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace DSIronPythonTests
{
    public class IronPythonTests
    {
        public delegate object PythonEvaluatorDelegate(string code, IList bindingNames, IList bindingValues);

        public IEnumerable<PythonEvaluatorDelegate> Evaluators = new List<PythonEvaluatorDelegate> {
            DSCPython.CPythonEvaluator.EvaluatePythonScript,
            DSIronPython.IronPythonEvaluator.EvaluateIronPythonScript
        };

      
        [Test]
        [Category("UnitTests")]
        public void EvaluatorWorks()
        {
            foreach (var pythonEvaluator in Evaluators)
            {
                var empty = new ArrayList();
                var output = pythonEvaluator("OUT = 0", empty, empty);
                Assert.AreEqual(0, output);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void BindingsWork()
        {
            const string expected = "Hi!";
            
            var names = new ArrayList { "test" };
            var vals = new ArrayList { expected };

            foreach (var pythonEvaluator in Evaluators)
            {
                var output = pythonEvaluator(
                    "OUT = test",
                    names,
                    vals
                );

                Assert.AreEqual(expected, output);
            }
        }
        
        [Test]
        [Category("UnitTests")]
        public void DataMarshaling_Output()
        {
            var marshaler = DSIronPython.IronPythonEvaluator.OutputMarshaler;
            marshaler.RegisterMarshaler((string s) => s.Length);

            const string script = "OUT = ['', ' ', '  ']";

            object output = DSIronPython.IronPythonEvaluator.EvaluateIronPythonScript(
                script,
                new ArrayList(),
                new ArrayList());

            Assert.AreEqual(new[] { 0, 1, 2 }, output);

            marshaler.UnregisterMarshalerOfType<string>();

            // Using the CPython Evaluator
            marshaler = DSCPython.CPythonEvaluator.OutputMarshaler;
            marshaler.RegisterMarshaler((string s) => s.Length);

            output = DSCPython.CPythonEvaluator.EvaluatePythonScript(
                script,
                new ArrayList(),
                new ArrayList());

            Assert.AreEqual(new[] { 0, 1, 2 }, output);

            marshaler.UnregisterMarshalerOfType<string>();


        }

        [Test]
        [Category("UnitTests")]
        public void DataMarshaling_Input()
        {
            var marshaler = DSIronPython.IronPythonEvaluator.InputMarshaler;
            marshaler.RegisterMarshaler((string s) => s.Length);

            const string script = "OUT = sum(IN)";

            object output = DSIronPython.IronPythonEvaluator.EvaluateIronPythonScript(
                script,
                new ArrayList { "IN" },
                new ArrayList { new ArrayList { " ", "  " } });

            Assert.AreEqual(3, output);

            marshaler.UnregisterMarshalerOfType<string>();

            marshaler = DSCPython.CPythonEvaluator.InputMarshaler;
            marshaler.RegisterMarshaler((string s) => s.Length);

            output = DSCPython.CPythonEvaluator.EvaluatePythonScript(
                script,
                new ArrayList { "IN" },
                new ArrayList { new ArrayList { " ", "  " } });

            Assert.AreEqual(3, output);

            marshaler.UnregisterMarshalerOfType<string>();
        }
        

        [Test]
        public void SliceOperator_Output()
        {
            var names = new ArrayList { "indx" };
            var vals = new ArrayList { 3 };

            foreach (var pythonEvaluator in Evaluators)
            {
                var output = pythonEvaluator(
                "OUT = [1,2,3,4,5,6,7][indx:indx+2]",
                names,
                vals);

                var expected = new ArrayList { 4, 5 };

                Assert.AreEqual(expected, output);
            }
        }
    }
}

using DSPythonNet3;
using Dynamo.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using System.IO;
using Dynamo;
using Dynamo.PythonServices;
using Dynamo.PythonServices.EventHandlers;

 namespace DSPythonTests
{
    public class PythonEvalTests : UnitTestBase
    {
        public delegate object PythonEvaluatorDelegate(string code, IList bindingNames, IList bindingValues);

        public PythonEvaluatorDelegate evaluator;
        private System.Reflection.Assembly pyAsm;
        private Type evalType;
        private System.Reflection.MethodInfo evalMethod;

        [OneTimeSetUp]
        public void LoadPythonNet3()
        {
            var enginePath = Path.Combine(PathManager.BuiltinPackagesDirectory, @"PythonNet3Engine\extra\DSPythonNet3.dll");
            pyAsm = System.Reflection.Assembly.LoadFrom(enginePath);
            evalType = pyAsm.GetType("DSPythonNet3.DSPythonNet3Evaluator", throwOnError: true);
            evalMethod = evalType.GetMethod("EvaluatePythonScript",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            Assert.IsNotNull(evalMethod, "EvaluatePythonScript not found in DSPythonNet3Evaluator.");

            evaluator = (string code, IList names, IList values) => evalMethod.Invoke(null, new object[] { code, names, values });
        }

        [Test]
        [Category("UnitTests")]
        public void EvaluatorWorks()
        {
            var emptyNames = new ArrayList();
            var emptyValues = new ArrayList();

            var output = evaluator("OUT = 0", emptyNames, emptyValues);
            Assert.AreEqual(0, output);
        }

        [Test]
        [Category("UnitTests")]
        public void BindingsWork()
        {
            const string expected = "Hi!";

            var names = new ArrayList { "test" };
            var vals = new ArrayList { expected };

            var output = evaluator("OUT = test", names, vals);
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public void DataMarshaling_Output()
        {
            const string script = "OUT = ['', ' ', '  ']";

            // Using the PythonNet3 Evaluator
            var marshaler = DSPythonNet3Evaluator.OutputMarshaler;
            marshaler.RegisterMarshaler((string s) => s.Length);

            var output = DSPythonNet3Evaluator.EvaluatePythonScript(
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
            const string script = "OUT = sum(IN)";

            var marshaler = DSPythonNet3.DSPythonNet3Evaluator.InputMarshaler;
            marshaler.RegisterMarshaler((string s) => s.Length);

            var output = DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(
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

            var output = evaluator("OUT = [1,2,3,4,5,6,7][indx:indx+2]", names, vals);
            var expected = new ArrayList { 4, 5 };
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void PythonEngineIncludesTraceBack()
        {
            var code = @"
# extra line
1/0
# extra line
";
            try
            {
                DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(code, new ArrayList(), new ArrayList());
                Assert.Fail("An exception was expected");
            }
            catch (Exception exc)
            {
                // Trace back is expected here. Line is 3 due to the line break from the code variable declaration
                Assert.AreEqual("division by zero\n  File \"<string>\", line 3, in <module>\n", exc.Message);
            }

            code = @"
# extra line
print 'hello'
# extra line
";
            try
            {
                DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(code, new ArrayList(), new ArrayList());
                Assert.Fail("An exception was expected");
            }
            catch (Exception exc)
            {
                // Trace back is not available for this call, but we still get a reasonable message back
                Assert.AreEqual("Missing parentheses in call to 'print'. Did you mean print(...)? (<string>, line 3)", exc.Message);
            }
        }

        [Test]
        public void PythonEngineWithErrorRaisesCorrectEvent()
        {

            var count = 0;
            EvaluationFinishedEventHandler handler = (state, scope, codeString, bindings) =>
            {
                count = count + 1;
                if (count == 1)
                {
                    Assert.AreEqual(EvaluationState.Success, state);
                }
                else if (count == 2)
                {
                    Assert.AreEqual(EvaluationState.Failed, state);
                }
            };

            DSPythonNet3Evaluator.Instance.EvaluationFinished += handler;

            var code = @"1";
            try
            {
                DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(code, new ArrayList(), new ArrayList());
            }
            finally
            {
                Assert.AreEqual(1, count);
            }

            code = @"1/a";
            try
            {
                DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(code, new ArrayList(), new ArrayList());
            }
            catch
            {
                //we anticipate an undefined var error.
            }
            finally
            {
                DSPythonNet3Evaluator.Instance.EvaluationFinished -= handler;
                Assert.AreEqual(2, count);
            }
        }

        [Test]
        public void OutputPythonObjectDoesNotThrow()
        {
            var code = @"
import weakref

class myobj:
    def __str__(self):
        return 'I am a myobj'

o = myobj()
wr = weakref.ref(o)

OUT = wr
";
            var empty = new ArrayList();
            Assert.DoesNotThrow(() =>
            {
                Assert.IsTrue(DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(code, empty, empty).ToString().Contains("weakref at"));

            });
        }

        [Test]
        public void OutputPythonObjectHasProperToString()
        {
            var code = @"

class myobj:
    def __str__(self):
        return 'I am a myobj'

o = myobj()
OUT = o
";
            var empty = new ArrayList();
            Assert.DoesNotThrow(() =>
            {
                Assert.AreEqual("I am a myobj", DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(code, empty, empty).ToString());

            });
        }

        [Test]
        public void PythonObjectWithDynamoSkipisNotMarshaled()
        {
            var code = @"

class iterable:
    def __str__(self):
        return 'I want to participate in conversion'
    def __iter__(self):
        return iter([0,1,2,3])
    def __getitem__(self,key):
        return key

o = iterable()
OUT = o
";

            var code2 = @"

class notiterable:
    def __dynamoskipconversion__(self):
        pass
    def __str__(self):
        return 'I want to skip in conversion'
    def __iter__(self):
        return iter([0,1,2,3])
    def __getitem__(self,key):
        return key

o = notiterable()
OUT = o
";
            var empty = new ArrayList();
            var result1 = DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(code, empty, empty);
            var result2 = DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(code2, empty, empty);
            Assert.IsInstanceOf(typeof(IList), result1);
            Assert.IsTrue(new List<object>() { 0L, 1L, 2L, 3L }
                .SequenceEqual((IEnumerable<Object>)result1));
            Assert.IsInstanceOf(typeof(DSPythonNet3.DynamoCPythonHandle), result2);

        }

        [Test]
        public void NonListIterablesCanBeOutput()
        {
            var code = @"
s = { 'hello' }
fs = frozenset({ 'world' })
d = { 'one': 1 }
dk = d.keys()
dv = d.values()
di = d.items()

OUT = s,fs,dk,dv,di
";
            var expected = new ArrayList
            {
                new ArrayList { "hello" },
                new ArrayList { "world" },
                new ArrayList { "one" },
                new ArrayList { 1 },
                new ArrayList { new ArrayList { "one", 1 } }
            };
            var empty = new ArrayList();
            var output = evaluator(code, empty, empty);
            Assert.AreEqual(expected, output);
        }
        [Test]
        public void ImportedLibrariesReloadedOnNewEvaluation()
        {

            var modName = "reload_test1";

            var tempPath = Path.Combine(TempFolder, $"{modName}.py");

            //clear file.
            File.WriteAllText(tempPath, "value ='Hello World!'\n");
            try
            {
                var script = $@"import sys
sys.path.append(r'{Path.GetDirectoryName(tempPath)}')
import {modName}
OUT = {modName}.value";

                var output = DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(
                   script,
                   new ArrayList { "IN" },
                   new ArrayList { new ArrayList { " ", "  " } });

                Assert.AreEqual("Hello World!", output);

                //now modify the file.
                File.AppendAllLines(tempPath, new string[] { "value ='bye'" });

                //mock raise event
                DSPythonNet3Evaluator.RequestPythonResetHandler(PythonEngineManager.PythonNet3EngineName);

                output = DSPythonNet3Evaluator.EvaluatePythonScript(
                 script,
                 new ArrayList { "IN" },
                 new ArrayList { new ArrayList { " ", "  " } });
                Assert.AreEqual("bye", output);

            }
            finally
            {
                File.Delete(tempPath);
            }
        }


    }
}

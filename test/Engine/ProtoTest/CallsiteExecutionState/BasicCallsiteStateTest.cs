using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;

namespace ProtoTest.CallsiteState
{

    public class BasicTests
    {
        public TestFrameWork thisTest = new TestFrameWork();

        private ProtoCore.Core SetupTestCore(string csStateName)
        {
            ProtoCore.CallsiteExecutionState.filename = csStateName;
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
            ProtoCore.Core testCore = new ProtoCore.Core(new ProtoCore.Options());
            testCore.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(testCore));
            testCore.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(testCore));

            // this setting is to fix the random failure of replication test case
            testCore.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            testCore.Options.Verbose = true;
            testCore.EnableCallsiteExecutionState = true;

            return testCore;
        }

        private void RemoveTestCallsiteStateFile(string csStateName)
        {
            // TODO implement the path
            File.Delete(csStateName);
        }

        /// <summary>
        /// TestGetEntries will test only for the number of VMstate entries serialized in the current run
        /// Entries in the VMState must equal the number of lines of executable code with at least one function call
        /// </summary>
        /// 
        [Test]
        public void TestGetEntries_GlobalStatements01()
        {

            String code =
@"
import(""ProtoGeometry.dll"");
p = Point.ByCoordinates(1, 1, 1);
";

            ProtoCore.Core core = SetupTestCore("TestGetEntries_GlobalStatements01");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ExecutionMirror mirror = runner.Execute(code, core);

            ProtoCore.CallsiteExecutionState csState = core.csExecutionState;
            int entries = csState.GetCSStateCount();
            Assert.IsTrue(entries == 1);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetEntries_GlobalStatements02()
        {

            String code =
@"
import(""ProtoGeometry.dll"");
x = 1;
p = Point.ByCoordinates(x, 1, 1);
";

            ProtoCore.Core core = SetupTestCore("TestGetEntries_GlobalStatements02");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ExecutionMirror mirror = runner.Execute(code, core);

            ProtoCore.CallsiteExecutionState csState = core.csExecutionState;
            int entries = csState.GetCSStateCount();
            Assert.IsTrue(entries == 1);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetEntries_GlobalStatements03()
        {

            String code =
@"
import(""ProtoGeometry.dll"");
x = 1;
y = 1;
z = 1;
p1 = Point.ByCoordinates(x, y, z);
p2 = Point.ByCoordinates(x, y, z);
";

            ProtoCore.Core core = SetupTestCore("TestGetEntries_GlobalStatements03");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ExecutionMirror mirror = runner.Execute(code, core);

            ProtoCore.CallsiteExecutionState csState = core.csExecutionState;
            int entries = csState.GetCSStateCount();
            Assert.IsTrue(entries == 2);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetEntries_GlobalStatements04()
        {

            String code =
@"
import(""ProtoGeometry.dll"");
x = {1,2,3};
p = Point.ByCoordinates(x, 1, 1);
";

            ProtoCore.Core core = SetupTestCore("TestGetEntries_GlobalStatements04");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ExecutionMirror mirror = runner.Execute(code, core);

            // Verify entries in the callsite map
            ProtoCore.CallsiteExecutionState csState = core.csExecutionState;
            int entries = csState.GetCSStateCount();
            Assert.IsTrue(entries == 1);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetRunId01()
        {

            String code =
@"
def f(i : int)
{
	return = i + 1;
}

x = 0;
y = f(x);

";

            ProtoCore.Core core = SetupTestCore("TestGetRunId01");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ExecutionMirror mirror = runner.Execute(code, core);


            // Verify entries in the callsite map
            ProtoCore.CallsiteExecutionState csState = core.csExecutionState;
            int entries = csState.GetCSStateCount();
            Assert.IsTrue(entries == 1);

            // Verify run id of each callsite
            Assert.IsTrue(core.csExecutionState.CallsiteDataMap[0].RunID == 0);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }   
        
        [Test]
        public void TestGetRunId02()
        {

            String code =
@"
def f(i : int)
{
	return = i + 1;
}

x = 0;
y = f(x);
x = 1;

";

            ProtoCore.Core core = SetupTestCore("TestGetRunId02");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ExecutionMirror mirror = runner.Execute(code, core);


            // Verify entries in the callsite map
            ProtoCore.CallsiteExecutionState csState = core.csExecutionState;
            int entries = csState.GetCSStateCount();
            Assert.IsTrue(entries == 1);

            // Verify run id of each callsite
            Assert.IsTrue(core.csExecutionState.CallsiteDataMap[0].RunID == 1);
            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetRunId03()
        {

            String code =
@"
def f(i : int)
{
	return = i + 1;
}

x = 0;
y = f(x);
x = {1,2,3}; // Replicated call to 'f' should still yield runId of 1

";

            ProtoCore.Core core = SetupTestCore("TestGetRunId03");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ExecutionMirror mirror = runner.Execute(code, core);


            // Verify entries in the callsite map
            ProtoCore.CallsiteExecutionState csState = core.csExecutionState;
            int entries = csState.GetCSStateCount();
            Assert.IsTrue(entries == 1);

            // Verify run id of each callsite
            Assert.IsTrue(core.csExecutionState.CallsiteDataMap[0].RunID == 1);

            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }

        [Test]
        public void TestGetRunId04()
        {

            String code =
@"
    def f(i : int)
    {
          return = i + 1;
    } 

    def g(i : int)
    {
          return = i + 2;
    }


    x = 0;
    y = f(x);   // 1st run: f.runID = 0
                // 2nd run: f.runID = 1
    x = 10;     // update ‘x’ to trigger call to f

    a = 0;
    y = g(a);       // 1st run: g.runID = 0
                    // 2nd run: g.runID = 1
    a = {10,11,12}; // update ‘a’ to an array to trigger replicated call to g


";

            ProtoCore.Core core = SetupTestCore("TestGetRunId04");
            ProtoScript.Runners.ProtoScriptTestRunner runner = new ProtoScript.Runners.ProtoScriptTestRunner();

            ExecutionMirror mirror = runner.Execute(code, core);


            // Verify entries in the callsite map
            ProtoCore.CallsiteExecutionState csState = core.csExecutionState;
            int entries = csState.GetCSStateCount();
            Assert.IsTrue(entries == 2);

            // Verify run id of each callsite
            Assert.IsTrue(core.csExecutionState.CallsiteDataMap[0].RunID == 1);
            Assert.IsTrue(core.csExecutionState.CallsiteDataMap[1].RunID == 1);

            RemoveTestCallsiteStateFile(ProtoCore.CallsiteExecutionState.GetThisSessionFileName());
        }
    }
}

    


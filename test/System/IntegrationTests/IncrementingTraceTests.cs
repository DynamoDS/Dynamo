using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FFITarget;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using ProtoScript.Runners;
using ProtoTestFx.TD;

namespace IntegrationTests
{
    class IncrementingTraceTests
    {
        private const string __TEMP_REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";
        public TestFrameWork testFx = new TestFrameWork();
        private ILiveRunner astLiveRunner = null;



        [SetUp]
        public void Setup()
        {
            testFx = new TestFrameWork();
            astLiveRunner = new ProtoScript.Runners.LiveRunner();
            FFITarget.IncrementerTracedClass.ResetForNextTest();

        }

        [TearDown]
        public static void TLSCleanup()
        {
            Thread.FreeNamedDataSlot(__TEMP_REVIT_TRACE_ID);
        }

        [Test]
        [Category("Trace")]
        public void MultipleCallsToFEPFromDifferentCallsites()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment

            var mirror = testFx.RunScriptSource(
@"import(""FFITarget.dll"");
mtcA = IncrementerTracedClass.IncrementerTracedClass(1);
cleanA = mtcA.WasCreatedWithTrace();

mtcB =  IncrementerTracedClass.IncrementerTracedClass(1);
cleanB = mtcB.WasCreatedWithTrace();

"
);
            Assert.IsTrue((bool)mirror.GetFirstValue("cleanA").Payload == false);
            Assert.IsTrue((bool)mirror.GetFirstValue("cleanB").Payload == false);

        }


        [Test]
        [Category("Trace")]
        public void ReplicatedCallsToFEPFromSameCallsite()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment

            var mirror = testFx.RunScriptSource(
@"import(""FFITarget.dll"");
mtcA = IncrementerTracedClass.IncrementerTracedClass(0..3);
cleanA = mtcA.WasCreatedWithTrace();
//cleanA == {false, false, false, false}


mtcB =  IncrementerTracedClass.IncrementerTracedClass(0..2);
cleanB = mtcB.WasCreatedWithTrace();
//cleanB == {false, false, false}

"
);

            List<Object> allFalse4 = new List<Object> {false, false, false, false};
            Assert.IsTrue(mirror.CompareArrays("cleanA", allFalse4, typeof(bool)));

            List<Object> allFalse3 = new List<Object> { false, false, false };
            Assert.IsTrue(mirror.CompareArrays("cleanB", allFalse3, typeof(bool)));

        }

        [Test]
        [Category("Trace")]
        public void ReplicatedCallsToFEPWithIDs()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment

            var mirror = testFx.RunScriptSource(
@"import(""FFITarget.dll"");
x = 0..3;
mtcA = IncrementerTracedClass.IncrementerTracedClass(x);
cleanA = mtcA.WasCreatedWithTrace();
ids = mtcA.ID;
//cleanA == {false, false, false, false}

x = 1..4;

"
);

            List<Object> allFalse4 = new List<Object> { true, true, true, true };
            Assert.IsTrue(mirror.CompareArrays("cleanA", allFalse4, typeof(bool)));
             Assert.IsTrue(mirror.CompareArrays("ids", new List<Object>{ 0L, 1L, 2L, 3L}, typeof(Int64)));

        }



        [Test]
        [Category("Trace")]
        public void IncrementerIDTest()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment

            var mirror = testFx.RunScriptSource(
@"import(""FFITarget.dll"");
mtcA = IncrementerTracedClass.IncrementerTracedClass(0);
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace();

mtcB = IncrementerTracedClass.IncrementerTracedClass(0);
mtcBID = mtcB.ID;
mtcBWasTraced = mtcB.WasCreatedWithTrace();
"
);
            Assert.IsTrue((Int64)mirror.GetFirstValue("mtcAID").Payload == 0);
            Assert.IsTrue((Boolean)mirror.GetFirstValue("mtcAWasTraced").Payload == false);

            Assert.IsTrue((Int64)mirror.GetFirstValue("mtcBID").Payload == 1);
            Assert.IsTrue((Boolean)mirror.GetFirstValue("mtcBWasTraced").Payload == false);
        }


        [Test]
        [Category("Trace")]
        public void IncrementerIDTestUpdate()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment


            var mirror = testFx.RunScriptSource(
@"import(""FFITarget.dll"");
x = 0;
mtcA = IncrementerTracedClass.IncrementerTracedClass(x);
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace();

x = 1;
"
);
  //          Assert.IsTrue((Int64)mirror.GetFirstValue("mtcAID").Payload == 0);
            Assert.IsTrue((Boolean)mirror.GetFirstValue("mtcAWasTraced").Payload == true);
        }


        

        [Test]
        public void SanityCheckArgChangeCallsiteReuse()
        {
            List<string> codes = new List<string>()
            {
                "def f(i : int) { return = i;} x = 1; a = f(x);",
                "x = 2;",
                "x = 3;",
            };

            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(CreateSubTreeFromCode(guid1, codes[0]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("a", 1, astLiveRunner);



            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(CreateSubTreeFromCode(guid2, codes[1]));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("a", 2, astLiveRunner);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            modified.Add(CreateSubTreeFromCode(guid2, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed
            TestFrameWork.AssertValue("a", 3, astLiveRunner);

        }



        [Test]
        [Category("Trace")]
        public void SingleToSingle()
        {

            //Test to ensure that the first time the code is executed the wasTraced attribute is marked as false
            //and the secodn time it is marked as true


            string setupCode = 
            @"import(""FFITarget.dll""); 
x = 0; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";
            

            
            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", false, astLiveRunner);



            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(CreateSubTreeFromCode(guid2, "x = 1;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);
        }

        
        [Test]
        [Category("Trace")]
        public void ReplicatedToReplicated()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0..2; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";


            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", new List<int>()
                {
                    0,
                    1,
                    2
                }, astLiveRunner);

            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    false,
                    false,
                    false
                }, astLiveRunner);



            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(CreateSubTreeFromCode(guid2, "x = 1..3;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1 ,
                2
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true,
                    true
                }, astLiveRunner);
        }

        [Test]
        [Category("Trace")]
        public void SingleToReplicated()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";

            ExecuteMoreCode(setupCode);

            TestFrameWork.AssertValue("mtcAID", 
                    0, astLiveRunner);

            TestFrameWork.AssertValue("mtcAWasTraced", 
                    false, astLiveRunner);



            ExecuteMoreCode("x = 1..3;");

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1,
                2
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    false,
                    false
                }, astLiveRunner);
        }


        [Test]
        [Category("Trace")]
        public void SingleToReplicatedToSingle()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";

            ExecuteMoreCode(setupCode);

            TestFrameWork.AssertValue("mtcAID",
                    0, astLiveRunner);

            TestFrameWork.AssertValue("mtcAWasTraced",
                    false, astLiveRunner);



            ExecuteMoreCode("x = 1..3;");

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1,
                2
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    false,
                    false
                }, astLiveRunner);


            ExecuteMoreCode("x = 2;");

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID",0 , astLiveRunner);

            TestFrameWork.AssertValue("mtcAWasTraced",
                    true, astLiveRunner);


        }




        //Migrate this code into the test framework
        private void ExecuteMoreCode(string newCode)
        {
            Guid guid2 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(CreateSubTreeFromCode(guid2, newCode));


            GraphSyncData syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
        }

        private Subtree CreateSubTreeFromCode(Guid guid, string code)
        {
            CodeBlockNode commentCode;
            var cbn = GraphToDSCompiler.GraphUtilities.Parse(code, out commentCode) as CodeBlockNode;
            var subtree = null == cbn ? new Subtree(null, guid) : new Subtree(cbn.Body, guid);
            return subtree;
        }





    }
}

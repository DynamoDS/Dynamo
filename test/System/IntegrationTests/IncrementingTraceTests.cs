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
    public class IncrementingTraceTests
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
        public void TestFixup()
        {
            astLiveRunner.ReInitializeLiveRunner();
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
        public void TestCallsiteReuseOnModifiedFunctionArgVariable01()
        {
            List<string> codes = new List<string>()
            {
                @"import(""FFITarget.dll"");",
                "x = 10;",
                "y = 20;",
                "p = IncrementerTracedClass.IncrementerTracedClass(x);",
                "p = IncrementerTracedClass.IncrementerTracedClass(y);",
                "i = p.ID;"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();
            Guid guid5 = System.Guid.NewGuid();

            // Create import CBN and call test class
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));  // import
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));  // x
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));  // y
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));  // call function
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid5, codes[5]));  // i

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Check trace ID
            TestFrameWork.AssertValue("i", 0, astLiveRunner);

           
            // Modify the variable input to the function
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[4]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Check that the same callsite was retrieved by checking that the ID matches the previous
            TestFrameWork.AssertValue("i", 0, astLiveRunner);
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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("a", 1, astLiveRunner);



            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("a", 2, astLiveRunner);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed
            TestFrameWork.AssertValue("a", 3, astLiveRunner);

        }


        #region 1 arg Tests

        [Test]
        [Category("Trace")]
        public void S2S()
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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", false, astLiveRunner);



            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);
        }

        
        [Test]
        [Category("Trace")]
        public void R2R()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
dump = IncrementerTracedClass.IncrementerTracedClass(0); 
x = 0..2; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";


            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", new List<int>()
                {
                    1,
                    2,
                    3
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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..3;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                1,
                2 ,
                3
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
        public void R2R_2()
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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..2;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true
                }, astLiveRunner);
        }

        [Test]
        [Category("Trace")]
        public void R2R_3()
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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..2;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true
                }, astLiveRunner);




            // Simulate a new new CBN
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "x = 1..3;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1,
                3
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true,
                    false
                }, astLiveRunner);
        }

        [Test]
        [Category("Trace")]
        public void R2R_4()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
d = IncrementerTracedClass.IncrementerTracedClass(0);
x = 0..2; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";


            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", new List<int>()
                {
                    1,
                    2,
                    3
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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..4;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                1,
                2,
                3,
                4
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true,
                    true,
                    false
                }, astLiveRunner);




            // Simulate a new new CBN
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "x = 1..3;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                1,
                2,
                3
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
        public void S2R()
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
        public void S2R2S()
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

        [Test]
        [Category("Trace")]
        public void S2R2S2R()
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
            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);

            TestFrameWork.AssertValue("mtcAWasTraced",
                    true, astLiveRunner);

            ExecuteMoreCode("x = 1..3;");

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                3,
                4
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    false,
                    false
                }, astLiveRunner);

        }

        #endregion

        #region 2 arg tests


        [Test]
        [Category("Trace")]
        public void S2S_2Arg()
        {

            //Test to ensure that the first time the code is executed the wasTraced attribute is marked as false
            //and the secodn time it is marked as true


            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, false); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";



            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", false, astLiveRunner);



            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);
        }


        [Test]
        [Category("Trace")]
        public void R2R_2Arg()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0..2; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, false); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";


            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

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


            Assert.IsTrue(
                astLiveRunner.Core.DSExecutable.RuntimeData.CallsiteCache.First().Value.TraceData[0].HasNestedData);

            Assert.IsTrue(
                astLiveRunner.Core.DSExecutable.RuntimeData.CallsiteCache.First().Value.TraceData[0].NestedData.Count == 3);


            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..3;"));


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
        public void R2R_2_2Arg()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0..2; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, false); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";


            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..2;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true
                }, astLiveRunner);
        }

        [Test]
        [Category("Trace")]
        public void R2R_3_2Arg()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0..2; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, false); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";


            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..2;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true
                }, astLiveRunner);




            // Simulate a new new CBN
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "x = 1..3;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                1,
                3
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true,
                    false
                }, astLiveRunner);
        }

        [Test]
        [Category("Trace")]
        public void R2R_4_2Arg()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
d = IncrementerTracedClass.IncrementerTracedClass(0, false);
x = 0..2; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, false); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";


            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", new List<int>()
                {
                    1,
                    2,
                    3
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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..4;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                1,
                2,
                3,
                4
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    true,
                    true,
                    false
                }, astLiveRunner);




            // Simulate a new new CBN
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "x = 1..3;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                1,
                2,
                3
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
        public void S2R_2Arg()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, false);; 
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
        public void S2R2S_2Arg()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, false); 
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
            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);

            TestFrameWork.AssertValue("mtcAWasTraced",
                    true, astLiveRunner);


        }

        [Test]
        [Category("Trace")]
        public void S2R2S2R_2Arg()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0; 
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, false); 
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
            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);

            TestFrameWork.AssertValue("mtcAWasTraced",
                    true, astLiveRunner);

            ExecuteMoreCode("x = 1..3;");

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>()
            {
                0,
                3,
                4
            }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", new List<bool>()
                {
                    true,
                    false,
                    false
                }, astLiveRunner);

        }



#endregion


        #region 2 Argument Replicated


        [Test]
        [Category("Trace")]
        public void R2R_2Arg_Rep()
        {
            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0..2; 
failz = x < 0;
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, failz); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";


            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

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

            Assert.IsTrue(
                astLiveRunner.Core.DSExecutable.RuntimeData.CallsiteCache.First().Value.TraceData[0].HasNestedData);


            Assert.IsTrue(
                astLiveRunner.Core.DSExecutable.RuntimeData.CallsiteCache.First().Value.TraceData[0].NestedData.Count == 3);

            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1..3;"));


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


        #endregion




        [Test]
        [Category("Trace")]
        public void SingleToExceptionToSingle()
        {

            //Test to ensure that the first time the code is executed the wasTraced attribute is marked as false
            //and the secodn time it is marked as true


            string setupCode =
            @"import(""FFITarget.dll""); 
x = 0; 
fail = false;
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, fail); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";



            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", false, astLiveRunner);



            // Simulate a new new CBN
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);



            // Simulate a new new CBN
            //Cause an exception to be thrown
            //Force exception to be thrown
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "fail = true;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", null, astLiveRunner);




            // Simulate a new  CBN
            Guid guid4 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, "fail = false;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            //This should cause a new entity
            TestFrameWork.AssertValue("mtcAID", 1, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", false, astLiveRunner);


            // Simulate a new  CBN
            Guid guid5 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid5, "x = 10;"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            //This should cause a new entity
            TestFrameWork.AssertValue("mtcAID", 1, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);



        }

        [Test]
        [Category("Failure")]
        //[Category("Trace")] // Uncommnet this after removing Failure category.
        public void ReplicatedToAllExceptionToReplicated()
        {

            //Test to ensure that the first time the code is executed the wasTraced attribute is marked as false
            //and the secodn time it is marked as true


            string setupCode =
            @"import(""FFITarget.dll""); 
x = {90, 91, 92}; 
fail = {false, false, false};
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, fail); 
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace(); ";



            // Create 2 CBNs

            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", new List<int>() { 0, 1 ,2} , astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", 
                new List<bool>() { false, false, false }, astLiveRunner);





            // Simulate a new new CBN
            //Cause an exception to be thrown
            //Force exception to be thrown
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "fail = {true, true, true};"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<Object>() { null, null, null} , astLiveRunner);




            // Simulate a new  CBN
            Guid guid4 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, "fail = {false, false, false};"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            //This should cause a new entity
            TestFrameWork.AssertValue("mtcAID", new List<int>() { 3, 4, 5 }, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced",
                new List<bool>() { false, false, false }, astLiveRunner);


        }

        [Test]
        [Category("Trace")]
        public void ReplicatedToSomeExceptionToReplicated()
        {

            //Test to ensure that the first time the code is executed the wasTraced attribute is marked as false
            //and the secodn time it is marked as true


            string setupCode =
            @"import(""FFITarget.dll""); 
x = {90, 91}; 
fail = {false, false};
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, fail); 
mtcAID = mtcA.ID;";

            Console.WriteLine("==============START===================");


            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", new List<int>() { 0, 1}, astLiveRunner);

            Assert.IsTrue(astLiveRunner.Core.DSExecutable.RuntimeData.CallsiteCache.Count == 1);


            Console.WriteLine("==============Completed first verification ===================");


            // Simulate a new new CBN
            //Cause an exception to be thrown
            //Force exception to be thrown
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "x = {44, 54}; fail = {false, true};"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<object>() { 0, null}, astLiveRunner);

            Console.WriteLine("==============Completed second verification ===================");


            // Simulate a new  CBN
            Guid guid4 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, "x = {47, 67}; fail = {false, false};"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            //This should cause a new entity
            TestFrameWork.AssertValue("mtcAID", new List<int>() { 0, 2}, astLiveRunner);

            Console.WriteLine("==============Completed third verification ===================");


            //Cause an exception to be thrown
            //Force exception to be thrown
            Guid guid5 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid5, "x = {4, 5, 6};")); //shortest lacing will cap this to 2 values


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>() { 0, 2}, astLiveRunner);

        }






        //Reproductions

        [Test]
        [Category("Failure")]
        //[Category("Trace")] // Uncommnet this after removing Failure category.
        public void Repo_01()
        {
            //Test to ensure that the first time the code is executed the wasTraced attribute is marked as false
            //and the secodn time it is marked as true


            string setupCode =
            @"import(""FFITarget.dll""); 
x = {90, 91}; 
fail = {false, false};
mtcA = IncrementerTracedClass.IncrementerTracedClass(x, fail); 
mtcAID = mtcA.ID;";

            Console.WriteLine("==============START===================");


            List<Subtree> added = new List<Subtree>();


            // Simulate a new new CBN
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", new List<int>() { 0, 1 }, astLiveRunner);

            Assert.IsTrue(astLiveRunner.Core.DSExecutable.RuntimeData.CallsiteCache.Count == 1);
            //astLiveRunner.Core.CallsiteCache.First().Value.

            Console.WriteLine("==============Completed first verification ===================");


            // Simulate a new new CBN
            //Cause an exception to be thrown
            //Force exception to be thrown
            Guid guid3 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, "x = {44, 54};"));// fail = {false, false};"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<int>() { 0, 1 }, astLiveRunner);

            Console.WriteLine("==============Completed second verification ===================");


            // Simulate a new  CBN
            Guid guid4 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, "x = {47, 67};"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            //This should cause a new entity
            TestFrameWork.AssertValue("mtcAID", new List<int>() { 0, 1 }, astLiveRunner);

            Console.WriteLine("==============Completed third verification ===================");


            //Cause an exception to be thrown
            //Force exception to be thrown
            Guid guid5 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid5, "x = {4, 5, 6};"));


            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", new List<Object>() { 0, 1, 3 }, astLiveRunner);


        }


        //Multithread

        [Test]
        [Category("Trace")]
        public void S2SMultiThread()
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
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, setupCode));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", false, astLiveRunner);


            Thread thread = new Thread(() =>
                {

                    

                    // Simulate a new new CBN
                    Guid guid2 = System.Guid.NewGuid();
                    added = new List<Subtree>();
                    added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, "x = 1;"));


                    syncData = new GraphSyncData(null, added, null);
                    astLiveRunner.UpdateGraph(syncData);



                });

            thread.Start();
            thread.Join();

            // Verify that a is re-executed
            TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
            TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);

        
        }






        //Migrate this code into the test framework
        private void ExecuteMoreCode(string newCode)
        {
            Guid guid2 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, newCode));


            GraphSyncData syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
        }
    }
}

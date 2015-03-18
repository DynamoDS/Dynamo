using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoScript.Runners;
using ProtoTestFx.TD;

namespace IntegrationTests
{
    public class CallsiteRegen
    {
        [TestFixture]
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
            public static void TLSCleanup()
            {
                Thread.FreeNamedDataSlot(__TEMP_REVIT_TRACE_ID);
            }


            [Test]
            [Category("Trace")]
            public void TestGuidStability()
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


                var core = astLiveRunner.Core;
                var ctorCallsites = core.DSExecutable.RuntimeData.CallsiteCache.Values.Where(c => c.MethodName == "IncrementerTracedClass");

                Assert.IsTrue(ctorCallsites.Count() == 1);
                Guid guid = ctorCallsites.First().CallSiteID;
          


                ExecuteMoreCode("x = 1;");

                // Verify that a is re-executed
                TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
                TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);

                //Verify that the GUID has been adjusted
                var ctorCallsites2 = core.DSExecutable.RuntimeData.CallsiteCache.Values.Where(c => c.MethodName == "IncrementerTracedClass");

                Assert.IsTrue(ctorCallsites2.Count() == 1);
                Guid guid2 = ctorCallsites2.First().CallSiteID;

                Assert.AreEqual(guid, guid2);
            }

            [Test]
            [Category("Trace")]
            public void EnsureSerialisationDataNonNull()
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

                //Get the callsite for the ctor
                var core = astLiveRunner.Core;
                var ctorCallsites = core.DSExecutable.RuntimeData.CallsiteCache.Values.Where(c => c.MethodName == "IncrementerTracedClass");

                Assert.IsTrue(ctorCallsites.Count() == 1);
                ProtoCore.CallSite cs = ctorCallsites.First();

                //Request serialisation
                string serialisationData = cs.GetTraceDataToSave();

                //It shouldn't be empty
                Assert.IsTrue(!String.IsNullOrEmpty(serialisationData));
            }

            [Test]
            [Category("Trace")]
            public void SerialisationDataLoadSave()
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

                //Get the callsite for the ctor
                var core = astLiveRunner.Core;
                var ctorCallsites = core.DSExecutable.RuntimeData.CallsiteCache.Values.Where(c => c.MethodName == "IncrementerTracedClass");

                Assert.IsTrue(ctorCallsites.Count() == 1);
                ProtoCore.CallSite cs = ctorCallsites.First();

                //Request serialisation
                string serialisationData = cs.GetTraceDataToSave();

                //It shouldn't be empty
                Assert.IsTrue(!String.IsNullOrEmpty(serialisationData));


                //Wipe the trace data
                cs.TraceData.Clear();

                //Re-inject the data into the trace cache
                cs.LoadSerializedDataIntoTraceCache(serialisationData);

                ExecuteMoreCode("x = 1;");


                // Verify that a is re-executed
                TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
                TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);

            }

            [Test]
            [Category("Trace")]
            public void SerialisationDataLoadSave_Negative()
            {
                //This test is used to ensure that the SerialisationDataLoadSave test is correct
                //If this test fails, and the SerialisationDataLoadSave passes, then the infrastructure
                //has regressed. 


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

                //Get the callsite for the ctor
                var core = astLiveRunner.Core;
                var ctorCallsites = core.DSExecutable.RuntimeData.CallsiteCache.Values.Where(c => c.MethodName == "IncrementerTracedClass");

                Assert.IsTrue(ctorCallsites.Count() == 1);
                ProtoCore.CallSite cs = ctorCallsites.First();

                //Request serialisation
                string serialisationData = cs.GetTraceDataToSave();

                //It shouldn't be empty
                Assert.IsTrue(!String.IsNullOrEmpty(serialisationData));


                //Wipe the trace data
                cs.TraceData.Clear();

                //Don't re-inject the trace data. This should cause the next execution to increment
                //the ID
                //Re-inject the data into the trace cache
                //cs.LoadSerializedDataIntoTraceCache(serialisationData);

                ExecuteMoreCode("x = 1;");

                
                // Verify that a is re-executed, with new trace data
                TestFrameWork.AssertValue("mtcAID", 1, astLiveRunner);
                TestFrameWork.AssertValue("mtcAWasTraced", false, astLiveRunner);

            }


            [Test]
            [Category("Trace")]
            public void LoadSave_R2R_2()
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

                //Get the callsite for the ctor
                var core = astLiveRunner.Core;
                var ctorCallsites = core.DSExecutable.RuntimeData.CallsiteCache.Values.Where(c => c.MethodName == "IncrementerTracedClass");

                Assert.IsTrue(ctorCallsites.Count() == 1);
                ProtoCore.CallSite cs = ctorCallsites.First();

                //Request serialisation
                string serialisationData = cs.GetTraceDataToSave();

                //It shouldn't be empty
                Assert.IsTrue(!String.IsNullOrEmpty(serialisationData));


                //Wipe the trace data
                cs.TraceData.Clear();

                //Re-inject the data into the trace cache
                cs.LoadSerializedDataIntoTraceCache(serialisationData);


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
}

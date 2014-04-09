using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
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
            public void TestGuidIDStability()
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


                var core = astLiveRunner.Core;
                var ctorCallsites = core.CallsiteCache.Values.Where(c => c.MethodName == "IncrementerTracedClass");

                Assert.IsTrue(ctorCallsites.Count() == 1);
                Guid guid = ctorCallsites.First().CallSiteID;
          


                ExecuteMoreCode("x = 1;");

                // Verify that a is re-executed
                TestFrameWork.AssertValue("mtcAID", 0, astLiveRunner);
                TestFrameWork.AssertValue("mtcAWasTraced", true, astLiveRunner);

                //Verify that the GUID has been adjusted
                var ctorCallsites2 = core.CallsiteCache.Values.Where(c => c.MethodName == "IncrementerTracedClass");

                Assert.IsTrue(ctorCallsites2.Count() == 1);
                Guid guid2 = ctorCallsites2.First().CallSiteID;

                Assert.AreEqual(guid, guid2);
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
}

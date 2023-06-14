using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Microsoft.Diagnostics.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace Dynamo.Tests.Loggings
{
    [TestFixture]
    class AnalyticsServiceTest : DynamoModelTestBase
    {
        //We need to override this function because the one in DynamoModelTestBase is setting StartInTestMode = true
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = false,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                Preferences = settings,
                ProcessMode = TaskProcessMode.Synchronous
            };
        }

        /// <summary>
        /// This test method will validate that the AnalyticsService.OnWorkspaceAdded (CustomNodeWorkspaceModel) is executed
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnWorkspaceAdded()
        {
            //Arrange
            // Open/Run XML test graph
            string openPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            RunModel(openPath);
            int InitialNodesCount = CurrentDynamoModel.CurrentWorkspace.Nodes.Count();

            // Convert a DSFunction node Line.ByPointDirectionLength to custom node.
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var node = workspace.Nodes.OfType<DSFunction>().First();

            List<NodeModel> selectionSet = new List<NodeModel>() { node };
            var customWorkspace = CurrentDynamoModel.CustomNodeManager.Collapse(
                selectionSet.AsEnumerable(),
                Enumerable.Empty<Dynamo.Graph.Notes.NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__AnalyticsServiceTest__",
                    Success = true
                }) as CustomNodeWorkspaceModel;

            //Act
            //This will execute the custom workspace assigment and trigger the added workspace assigment event
            CurrentDynamoModel.OpenCustomNodeWorkspace(customWorkspace.CustomNodeId);

            //This will add a new custom node to the workspace
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var ws = CurrentDynamoModel.CustomNodeManager.CreateCustomNode("someNode", "someCategory", "");
            var csid = (ws as CustomNodeWorkspaceModel).CustomNodeId;
            var customNode = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(csid);

            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //Assert
            //At the begining the CurrentWorkspace.Nodes has 4 nodes but two new nodes were added, then verify we have 5 nodes.
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), InitialNodesCount + 2);
        }
    }

    public class DynamoAnalyticsDisableTest
    {
        [Test,Category("FailureNET6")]//TODO this test requires finding ASM using the registry, will not run on linux.
        public void DisableAnalytics()
        {
            var versions = new List<Version>(){

                    new Version(229, 0,0),
                    new Version(228, 6, 0)
            };

            var directory = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            var testDirectory = Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
            string openPath = Path.Combine(testDirectory, @"core\Angle.dyn");
            //go get a valid asm path.
            var locatedPath = string.Empty;
            var coreDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Process dynamoCLI = null;

            DynamoShapeManager.Utilities.GetInstalledAsmVersion2(versions, ref locatedPath, coreDirectory);
            try
            {
                Assert.DoesNotThrow(() =>
                {

                    dynamoCLI = Process.Start(Path.Combine(coreDirectory, "DynamoCLI.exe"), $"--GeometryPath \"{locatedPath}\" -k --DisableAnalytics -o \"{openPath}\" ");

                    Thread.Sleep(5000);// Wait 5 seconds to open the dyn
                    Assert.IsFalse(dynamoCLI.HasExited);
                    var dt = DataTarget.AttachToProcess(dynamoCLI.Id, false);
                    var assemblies = dt
                          .ClrVersions
                          .Select(dtClrVersion => dtClrVersion.CreateRuntime())
                          .SelectMany(runtime => runtime.AppDomains.SelectMany(runtimeAppDomain => runtimeAppDomain.Modules))
                          .Select(clrModule => clrModule.AssemblyName)
                          .Distinct()
                          .Where(x => x != null)
                          .ToList();

                    var firstASMmodulePath = string.Empty;
                    foreach (string module in assemblies)
                    {
                        if (module.IndexOf("Analytics", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            Assert.Fail("Analytics module was loaded");
                        }
                        if (module.IndexOf("AdpSDKCSharpWrapper", StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            Assert.Fail("ADP module was loaded");
                        }
                    }
                });
            }
            finally
            {

                dynamoCLI?.Kill();
            }
        }
    }
}

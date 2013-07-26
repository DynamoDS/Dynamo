using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Search;
using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class PackageDependencyTests
    {
        #region startup and shutdown

        [SetUp]
        public void Init()
        {
            StartDynamo();
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                DynamoLogger.Instance.FinishLogging();
                controller.ShutDown();

                EmptyTempFolder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static DynamoController controller;
        private static string TempFolder;
        private static string ExecutingDirectory { get; set; }

        private static void StartDynamo()
        {
            try
            {
                ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string tempPath = Path.GetTempPath();

                TempFolder = Path.Combine(tempPath, "dynamoTmp");

                if (!Directory.Exists(TempFolder))
                {
                    Directory.CreateDirectory(TempFolder);
                }
                else
                {
                    EmptyTempFolder();
                }

                DynamoLogger.Instance.StartLogging();

                //create a new instance of the ViewModel
                controller = new DynamoController(new Dynamo.FSchemeInterop.ExecutionEnvironment(), typeof(DynamoViewModel), Context.NONE);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void EmptyTempFolder()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        #endregion

        #region utility methods

        public dynNodeModel NodeFromCurrentSpace(DynamoViewModel vm, string guidString)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(guidString, out guid);
            return NodeFromCurrentSpace(vm, guid);
        }

        public dynNodeModel NodeFromCurrentSpace(DynamoViewModel vm, Guid guid)
        {
            return vm.CurrentSpace.Nodes.FirstOrDefault((node) => node.GUID == guid);
        }

        public dynWatch GetWatchNodeFromCurrentSpace(DynamoViewModel vm, string guidString)
        {
            var nodeToWatch = NodeFromCurrentSpace(vm, guidString);
            Assert.NotNull(nodeToWatch);
            Assert.IsAssignableFrom(typeof(dynWatch), nodeToWatch);
            return (dynWatch)nodeToWatch;
        }

        public double GetDoubleFromFSchemeValue(FScheme.Value value)
        {
            var doubleWatchVal = 0.0;
            Assert.AreEqual(true, Dynamo.FSchemeInterop.Utils.Convert(value, ref doubleWatchVal));
            return doubleWatchVal;
        }

        public FSharpList<FScheme.Value> GetListFromFSchemeValue(FScheme.Value value)
        {
            FSharpList<FScheme.Value> listWatchVal = null;
            Assert.AreEqual(true, Dynamo.FSchemeInterop.Utils.Convert(value, ref listWatchVal));
            return listWatchVal;
        }

        #endregion

        [Test]
        public void CanDiscoverDependenciesForFunctionDefinitionOpenFromFile()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\custom_node_dep_test\");

            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "RootNode.dyf")) != null);
            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "SecondLevelNode1.dyf")) != null);
            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "SecondLevelNode2.dyf")) != null);
            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "ThirdLevelCustomNodeB1.dyf")) != null);
            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "ThirdLevelCustomNodeB2.dyf")) != null);
            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "ThirdLevelCustomNodeB3.dyf")) != null);
            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "ThirdLevelCustomNodeA1.dyf")) != null);
            Assert.IsTrue(controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "ThirdLevelCustomNodeA2.dyf")) != null);

            string openPath = Path.Combine(examplePath, "custom_node_dep_test.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            var rootNode = NodeFromCurrentSpace(vm, "333ed3ad-c786-4064-8203-e79ce7cb109f");

            Assert.NotNull(rootNode);
            Assert.IsAssignableFrom(typeof(dynFunction), rootNode);

            var funcRootNode = rootNode as dynFunction;

            var dirDeps = funcRootNode.Definition.DirectDependencies;
            Assert.AreEqual(2, dirDeps.Count() );

            var allDeps = funcRootNode.Definition.Dependencies;
            Assert.AreEqual(7, allDeps.Count());

            var packageRoot = new PackageItemRootViewModel(funcRootNode.Definition);
            packageRoot.BuildDependencies(new HashSet<object>());

            Assert.AreEqual(2, packageRoot.Items.Count);

            Assert.AreEqual(2, packageRoot.Items[0].Items.Count);
            Assert.AreEqual(3, packageRoot.Items[1].Items.Count);
        }

    }
}

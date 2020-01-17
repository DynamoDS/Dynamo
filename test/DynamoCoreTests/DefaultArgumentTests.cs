using System;
using System.Collections.Generic;
using System.IO;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using FFITarget.DesignScript;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class DefaultArgumentTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestDefaultArgumentExpressionResolution()
        {
            // Simulate loading of external libraries in Dynamo by importing
            // FFITarget AFTER preloading the standard set of libraries above
            // By this time the default expressions using ProtoGeometry classes have already been resolved
            string libraryPath = "FFITarget.dll";
            
            CurrentDynamoModel.LibraryServices.ImportLibrary(libraryPath);

            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestDefaultArgumentExpressionResolution.dyn");
            OpenModel(openPath);

            BeginRun();

            // Assert that all nodes that use default argument expressions evaluate 
            var result = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(
                Guid.Parse("3039c01a-a6cc-45db-a8c7-5b5369538456"));

            Assert.IsTrue(result.CachedValue.Data != null);

            result = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(
                Guid.Parse("d15314f9-0903-4709-b183-0f3c6b40a95f"));

            Assert.IsTrue(result.CachedValue.Data != null);

            result = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(
                Guid.Parse("f7620e76-3177-415b-9de5-939c47a727d7"));

            Assert.IsTrue(result.CachedValue.Data != null);
            
            result = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(
                Guid.Parse("3b92db37-3646-429a-9ec0-2fc09d0d5643"));

            Assert.IsTrue(result.CachedValue.Data != null);
            
        }

        [Test]
        public void TestDefaultArgumentExpressionResolutionWithConflict()
        {
            // Simulate loading of external libraries in Dynamo by importing
            // FFITarget AFTER preloading the standard set of libraries above
            // By this time the default expressions using ProtoGeometry classes have already been resolved
            string libraryPath = "FFITarget.dll";

            CurrentDynamoModel.LibraryServices.ImportLibrary(libraryPath);

            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestDefaultArgumentExpressionWithConflict.dyn");
            OpenModel(openPath);

            BeginRun();

            // Assert that node using default argument expression with fully qualified Point class evaluates
            var result = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(
                Guid.Parse("7335190b-f50b-4e99-8dc6-de9ff45426fe"));

            Assert.IsTrue(result.CachedValue.Data != null);

            // Assert that node using default argument expression with unqualified Point class throws warning
            // due to conflicts in Point class
            result = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(
                Guid.Parse("40623ca5-8bbb-427a-b014-a9ac40378b51"));

            Assert.IsTrue(result.State == ElementState.Warning);
        }

        [Test]
        public void TestDefaultArgumentExpressionWithReplicationGuides()
        {
            // Simulate loading of external libraries in Dynamo by importing
            // FFITarget after preloading the standard set of libraries above
            string libraryPath = "FFITarget.dll";

            CurrentDynamoModel.LibraryServices.ImportLibrary(libraryPath);

            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestDefaultArgumentExpressionWithReplicationGuides.dyn");
            OpenModel(openPath);

            BeginRun();

            // Assert that node using default argument expression with fully qualified Point class evaluates
            var pt00 = Point.XYZ(1, 2, 0);
            var pt01 = Point.XYZ(1, 3, 0);
            var pt10 = Point.XYZ(2, 2, 0);
            var pt11 = Point.XYZ(2, 3, 0);

           AssertPreviewValue("cc365216-df02-41ce-9ea4-e477c06b7d55",
                new object[] {new[] {pt00, pt01}, new[] {pt10, pt11}});
        }
    }
}

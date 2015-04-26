using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;

using DynamoShapeManager;

using NUnit.Framework;

using TestServices;

namespace Dynamo.Tests
{
    [TestFixture]
    public class CallsiteTests : DynamoModelTestBase
    {
        private readonly string callsiteDir = @"core\callsite";

        [Test]
        public void Callsite_MultiDimensionDecreaseDimensionOnOpenAndRun()
        {
            CurrentDynamoModel.EngineController.TraceReconcliationComplete += MultiDimensionCheck;

            var ws = Open<HomeWorkspaceModel>(TestDirectory, callsiteDir, "RebindingSingleDimension.dyn");
            var numNode = ws.FirstNodeFromWorkspace<DoubleInput>();

            // File is saved with a value of 5 (elements)
            numNode.Value = "3";

            BeginRun();
        }

        private static void MultiDimensionCheck(TraceReconciliationEventArgs args)
        {
            Assert.AreEqual(args.OrphanedSerializables.Count, 2);
        }

        [Test]
        public void CallSite_MultiDimensionIncreaseDimensionOnOpenAndRun()
        {
            
        }

        [Test]
        public void Callsite_SingleDimensionDecreaseDimensionOnOpenAndRun()
        {
        }

        [Test]
        public void Callsite_SingleDimensionIncreaseDimensionOnOpenAndRun()
        {
            
        }

        [Test]
        public void Callsite_DeleteNodeBeforeRun()
        {
            
        }
    }
}

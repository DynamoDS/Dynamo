using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using Dynamo.Engine.CodeCompletion;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using NUnit.Framework;

using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using ProtoCore.Utils;

using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.Models;
using Dynamo.Graph.Workspaces;
using Dynamo.Properties;

namespace Dynamo.Tests
{
    class InheritanceTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("Builtin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FFITarget.dll");

            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestInheritanceNotDerivedFromClassA()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceA.dyn");
            RunModel(openPath);
            AssertPreviewValue("1115abe3-eddb-4f18-9aae-01f34a8fe0e9", 2);
            AssertPreviewValue("c8b71784-3570-40fb-86d4-19437963d263", null);
        }

        [Test]
        public void TestInheritanceDerivedFromInterfaceA()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceB.dyn");
            RunModel(openPath);
            AssertPreviewValue("976be3c4-96c7-49eb-b309-d2c606b7a0ac", 1);
            AssertPreviewValue("db85e13a-ff8a-493a-a77f-04b0319e68cc", 1);
        }

        [Test]
        public void TestInheritanceDerivedFromInterfaceAWithReplication()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceC.dyn");
            RunModel(openPath);
            AssertPreviewValue("21ba383c-cbb9-426c-a688-703325d6c96f", new int[]{ 1 });
        }

        [Test]
        public void TestInheritanceHidesMethodFromClassA()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceD.dyn");
            RunModel(openPath);
            AssertPreviewValue("68d8ddfe-4e39-4450-b872-58fc30a78997", 3);
            AssertPreviewValue("0089e5a4-ed09-414c-9657-4312d9ad0ed5", 3);
        }

        [Test]
        public void TestInheritanceHidesMethodFromClassAInverse()
        {
            string openPath = Path.Combine(TestDirectory, @"core\inheritance\inheritanceE.dyn");
            RunModel(openPath);
            AssertPreviewValue("968031ae-975f-4095-a513-64d95d65e16a", 0);
            AssertPreviewValue("c09cde58-3e62-4adc-821d-3885ef769410", 0);
        }
    }
}

using CoreNodeModels.Input;
using CoreNodeModels.Logic;
using Dynamo;
using Dynamo.Graph.Workspaces;
using Dynamo.Tests;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    internal class GateTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("BuiltIn.ds");
            libraries.Add("DSCPython.dll");
            libraries.Add("FFITarget.dll");
        }

        [Test]
        public void SettingNodeAsOpenPassesData()
        {
            string openPath = Path.Combine(TestDirectory, @"UI\UIGateNode.dyn");
            RunModel(openPath);

            Guid gateNodeGuid = Guid.Parse("0a4e291d93a84260bd9f37fde3158d83");
            var gateNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(gateNodeGuid) as Gate;

            Assert.AreEqual(false, gateNode.Value);

            //Test that the downstream connected node IsNull is true
            AssertPreviewValue("448ad21ed4af42b4ae71c45961a173ff", true);

            //Open the gate node
            gateNode.Value = true;

            BeginRun();

            //Test that the downstream connected node IsNull is now not null
            AssertPreviewValue("448ad21ed4af42b4ae71c45961a173ff", false);
        }

        [Test]
        public void SettingNodeAsClosedBlocksData()
        {
            string openPath = Path.Combine(TestDirectory, @"UI\UIGateNode.dyn");
            RunModel(openPath);

            Guid gateNodeGuid = Guid.Parse("e323a525bab546199c3635ce33c8c46b");
            var gateNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(gateNodeGuid) as Gate;

            Assert.AreEqual(true, gateNode.Value);

            //Test that the downstream connected node IsNull is false
            AssertPreviewValue("414438cdd24f4d74b08e462e0b17ddff", false);

            //Open the gate node
            gateNode.Value = false;

            BeginRun();

            //Test that the downstream connected node IsNull is now not null
            AssertPreviewValue("414438cdd24f4d74b08e462e0b17ddff", true);
        }
    }
}

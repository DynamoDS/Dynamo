using System;
using System.Collections.Generic;
using Dynamo.Interfaces;
using NUnit.Framework;
using PythonNodeModels;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    public class DefaultPythonEngineTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCPython.dll");
            libraries.Add("DSIronPython.dll");
        }

        protected override IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            var config = (DefaultStartConfiguration)base.CreateStartConfiguration(settings);
            config.DefaultPythonEngine = PythonEngineVersion.CPython3.ToString();
            return config;
        }

        [Test]
        public void NewPythonNodeUsingSystemDefaultEngine()
        {
            CurrentDynamoModel.PreferenceSettings.DefaultPythonEngine = string.Empty;
            var node = new PythonNode();
            node.GUID = Guid.NewGuid();
            CurrentDynamoModel.ExecuteCommand(new CreateNodeCommand(node, 0, 0, true, false));
            node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<PythonNode>();
            AssertPreviewValue(node.AstIdentifierGuid, 0);
            Assert.AreEqual(PythonEngineVersion.CPython3, node.Engine);
        }

        [Test]
        public void NewPythonNodeUsingUserDefaultEngine()
        {
            CurrentDynamoModel.PreferenceSettings.DefaultPythonEngine = PythonEngineVersion.IronPython2.ToString();
            var node = new PythonNode();
            node.GUID = Guid.NewGuid();
            CurrentDynamoModel.ExecuteCommand(new CreateNodeCommand(node, 0, 0, true, false));
            node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<PythonNode>();
            AssertPreviewValue(node.AstIdentifierGuid, 0);
            Assert.AreEqual(PythonEngineVersion.IronPython2, node.Engine);
        }
    }
}

using System;
using System.Collections.Generic;
using Dynamo.Configuration;
using Dynamo.Interfaces;
using Dynamo.PythonServices;
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
        }

        protected override IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            var config = (DefaultStartConfiguration)base.CreateStartConfiguration(settings);
            config.DefaultPythonEngine = PythonEngineManager.CPython3EngineName;
            return config;
        }

        [Test]
        public void NewPythonNodeUsingSystemDefaultEngine()
        {
            PreferenceSettings.Instance.DefaultPythonEngine = string.Empty;
            var node = new PythonNode();
            node.GUID = Guid.NewGuid();
            CurrentDynamoModel.ExecuteCommand(new CreateNodeCommand(node, 0, 0, true, false));
            node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<PythonNode>();
            AssertPreviewValue(node.AstIdentifierGuid, 0);
            Assert.AreEqual(PythonEngineManager.CPython3EngineName, node.EngineName);
        }

        [Test]
        public void NewPythonNodeUsingUserDefaultEngine()
        {
            PreferenceSettings.Instance.DefaultPythonEngine = PythonEngineManager.IronPython2EngineName;
            var node = new PythonNode();
            node.GUID = Guid.NewGuid();
            CurrentDynamoModel.ExecuteCommand(new CreateNodeCommand(node, 0, 0, true, false));
            node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<PythonNode>();
            AssertPreviewValue(node.AstIdentifierGuid, 0);
            Assert.AreEqual(PythonEngineManager.IronPython2EngineName, node.EngineName);
        }
    }
}

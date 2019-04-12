using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Tests;
using NUnit.Framework;
using DateTime = CoreNodeModels.Input.DateTime;

namespace DynamoCoreWpfTests
{
    class DateTimeTests : DynamoModelTestBase
    {
        private string tempPath = Path.GetTempFileName();

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void DateTimeNode_Open_DeserializesCorrectly()
        {
            var path = Path.Combine(TestDirectory, "core", "dateTime", "DateTime.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTime>();
            var dt = (System.DateTime)GetPreviewValue(node.GUID.ToString());

            Assert.AreEqual(string.Format("{0:" + PreferenceSettings.DefaultDateFormat + "}", dt), "January 01, 2150 3:33 PM");
        }

        [Test]
        public void DateTimeNode_Open_MangledDateTime_ResetsToDynamoDefault()
        {
            var path = Path.Combine(TestDirectory, "core", "dateTime", "DateTime_BadDateTime.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTime>();
            var dt = (System.DateTime)GetPreviewValue(node.GUID.ToString());

            Assert.AreEqual(string.Format("{0:" + PreferenceSettings.DefaultDateFormat + "}", dt), PreferenceSettings.DynamoDefaultTime.ToString(PreferenceSettings.DefaultDateFormat));
        }

        [Test]
        public void DateTimeNode_Open_Edit_SerializesCorrectly()
        {
            var path = Path.Combine(TestDirectory, "core", "dateTime", "DateTime.dyn");
            CurrentDynamoModel.OpenFileFromPath(path);

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTime>();
            var testDate = new System.DateTime(2150, 1, 1, 12, 0, 0).ToUniversalTime();
            node.Value = testDate;
            Assert.IsTrue(node.Value.Kind == System.DateTimeKind.Utc, " DateTime Value should be UTC kind");

            CurrentDynamoModel.CurrentWorkspace.Save(tempPath);

            var fileContents = File.ReadAllText(tempPath);
            fileContents = fileContents.Trim();
            if ((fileContents.StartsWith("{") && fileContents.EndsWith("}")) || //For object
                (fileContents.StartsWith("[") && fileContents.EndsWith("]"))) //For array
            {
                var obj = Newtonsoft.Json.Linq.JToken.Parse(fileContents);
                Assert.IsTrue(obj["Inputs"][0].ToString().Contains("Z"),
                    "DateTime Nodes serialization incorrectly in Inputs Block: " + obj["Inputs"][0].ToString());
                Assert.IsTrue(obj["Nodes"][0].ToString().Contains("Z"),
                    "DateTime Nodes serialization incorrectly in Nodes Block: " + obj["Nodes"][0].ToString());
            }

            CurrentDynamoModel.OpenFileFromPath(tempPath);

            node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTime>();
            Assert.IsTrue(node.Value.Kind == System.DateTimeKind.Utc, " DateTime Value should be UTC kind");
            var dt = (System.DateTime)GetPreviewValue(node.GUID.ToString());

            Assert.AreEqual(string.Format("{0:" + PreferenceSettings.DefaultDateFormat + "}", dt), testDate.ToString(PreferenceSettings.DefaultDateFormat));
        }

        [Test]

        public void DateTimeNodeDeprecated_Test()
        {
            var path = Path.Combine(TestDirectory, "core", "dateTime", "DateTimeDeprecated.dyn");
            CurrentDynamoModel.OpenFileFromPath(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            Assert.IsNotNull(node);
            Assert.AreEqual(ElementState.PersistentWarning, node.State);
            Assert.AreEqual("This node is deprecated", node.ToolTipText);
        }

        [TearDown]
        public void Teardown()
        {
            if (tempPath == null)
            {
                return;
            }

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}

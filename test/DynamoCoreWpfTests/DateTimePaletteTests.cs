using System.Collections.Generic;
using System.IO;
using CoreNodeModels.Input;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.Tests;
using Dynamo.Graph.Workspaces;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class DateTimePaletteTests : DynamoModelTestBase
    {
        private string tempPath = Path.GetTempFileName();

        [Test]
        public void DateTimePalette_AddToHomespaceAndRun_NoException()
        {
            var homespace = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homespace, "The current workspace is not a HomeWorkspaceModel");
            var dateTimePalette = new DateTimePalette();
            CurrentDynamoModel.AddNodeToCurrentWorkspace(dateTimePalette, true);
            homespace.Run();
            Assert.DoesNotThrow(DispatcherUtil.DoEvents);
            Assert.Pass();
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void DateTimeNode_Open_DeserializesCorrectly()
        {
            var path = Path.Combine(TestDirectory, "core", "dateTime", "DateTimePalette.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTimePalette>();
            var dt = (System.DateTime)GetPreviewValue(node.GUID.ToString());
            string dateformat = dt.ToString(PreferenceSettings.DefaultDateFormat);

            Assert.AreEqual(string.Format("{0:" + PreferenceSettings.DefaultDateFormat + "}", dt), "January 01, 2150 3:33 PM");
        }

        [Test]
        public void DateTimeNode_Open_MangledDateTime_ResetsToDynamoDefault()
        {
            var path = Path.Combine(TestDirectory, "core", "dateTime", "DateTimePalette_BadDateTime.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTimePalette>();
            var dt = (System.DateTime)GetPreviewValue(node.GUID.ToString());

            Assert.AreEqual(string.Format("{0:" + PreferenceSettings.DefaultDateFormat + "}", dt), PreferenceSettings.DynamoDefaultTime.ToString(PreferenceSettings.DefaultDateFormat));
        }

        [Test]
        public void DateTimeNode_Open_Edit_SerializesCorrectly()
        {
            var path = Path.Combine(TestDirectory, "core", "dateTime", "DateTimePalette.dyn");
            CurrentDynamoModel.OpenFileFromPath(path);

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTimePalette>();
            var testDate = new System.DateTime(2150, 1, 1, 12, 0, 0);
            node.SysDateTime = testDate;

            CurrentDynamoModel.CurrentWorkspace.SaveAs(tempPath,
                CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore);

            CurrentDynamoModel.OpenFileFromPath(tempPath);

            node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTimePalette>();
            var dt = (System.DateTime)GetPreviewValue(node.GUID.ToString());

            Assert.AreEqual(string.Format("{0:" + PreferenceSettings.DefaultDateFormat + "}", dt), testDate.ToString(PreferenceSettings.DefaultDateFormat));
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

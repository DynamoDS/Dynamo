using System.Collections.Generic;
using System.IO;
using Dynamo;
using Dynamo.Configuration;
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

        // TODO: Enable when Open() is expanded to open Json
        [Test, Ignore]
        public void DateTimeNode_Open_Edit_SerializesCorrectly()
        {
            var path = Path.Combine(TestDirectory, "core", "dateTime", "DateTime.dyn");
            CurrentDynamoModel.OpenFileFromPath(path);

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTime>();
            var testDate = new System.DateTime(2150, 1, 1, 12, 0, 0);
            node.Value = testDate;

            CurrentDynamoModel.CurrentWorkspace.Save(tempPath);

            CurrentDynamoModel.OpenFileFromPath(tempPath);

            node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTime>();
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

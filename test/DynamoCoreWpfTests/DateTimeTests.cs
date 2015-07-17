using System;
using System.Collections.Generic;
using System.IO;
using Dynamo;
using Dynamo.Tests;
using NUnit.Framework;
using DateTime = DSCoreNodesUI.DateTime;

namespace DynamoCoreWpfTests
{
    class DateTimeTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void DateTimeNode_Open_DeserializesCorrectly()
        {
            string path = Path.Combine(TestDirectory, "core", "dateTime", "DateTime.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DateTime>();

            var dt = (System.DateTime)GetPreviewValue(node.GUID.ToString());

            Assert.AreEqual(string.Format("{0:" + PreferenceSettings.DefaultDateFormat + "}", dt), "January 01, 2150 3:33 PM");
        }
    }
}

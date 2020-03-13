using Dynamo.Configuration;
using NUnit.Framework;
using System.IO;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class PreferenceSettingsTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void TestLoad()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-PythonTemplate-initial.xml");
            string initialPyFilePath = Path.Combine(settingDirectory, @"PythonTemplate-initial.py");

            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));

            var settings = PreferenceSettings.Load(settingsFilePath);

            Assert.IsNotNull(settings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetPythonTemplateFilePath()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-PythonTemplate-initial.xml");
            string initialPyFilePath = Path.Combine(settingDirectory, @"PythonTemplate-initial.py");

            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));
            Assert.IsTrue(File.Exists(initialPyFilePath));

            PreferenceSettings.Load(settingsFilePath);

            string pythonTemplateFilePath = Path.Combine(settingDirectory, PreferenceSettings.GetPythonTemplateFilePath());

            Assert.AreEqual(pythonTemplateFilePath, initialPyFilePath);
        }

        [Test]
        [Category("UnitTests")]
        public void TestSetIsBackgroundPreviewActive()
        {
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            // Force inital state
            PreferenceSettings initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;
            
            initalSetting.SetIsBackgroundPreviewActive("IsBackgroundPreviewActive", true);

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.GetIsBackgroundPreviewActive("IsBackgroundPreviewActive"),
                initalSetting.GetIsBackgroundPreviewActive("IsBackgroundPreviewActive"));
        }

        [Test]
        [Category("UnitTests")]
        public void TestMaxNumRecentFiles()
        {
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            // Force inital state
            PreferenceSettings initalSetting = new PreferenceSettings();
            PreferenceSettings resultSetting;

            initalSetting.SetIsBackgroundPreviewActive("IsBackgroundPreviewActive", true);

            initalSetting.Save(tempPath);
            resultSetting = PreferenceSettings.Load(tempPath);

            Assert.AreEqual(resultSetting.MaxNumRecentFiles,
                initalSetting.MaxNumRecentFiles);
        }
    }
}

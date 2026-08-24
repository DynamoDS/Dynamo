using System.IO;
using Dynamo.Configuration;
using Dynamo.Core;
using NUnit.Framework;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class PathManagerTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void WhenUpdatePreferenceItemPathTemplatesAndDirectoryDoesNotExistThenReturnsFalseAndDoesNotCreateDirectory()
        {
            var pathManager = new PathManager(new PathManagerParams());
            var stalePath = Path.Combine(TempFolder, "stale-templates", "en-US");

            var updated = pathManager.UpdatePreferenceItemPath(PathManager.PreferenceItem.Templates, stalePath);

            Assert.IsFalse(updated);
            Assert.IsFalse(Directory.Exists(stalePath));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPersistedTemplatePathDoesNotExistThenTemplatePathResetsToDefault()
        {
            var missingPath = Path.Combine(TempFolder, "missing-templates");
            RestartDynamoWithTemplatePath(missingPath);

            var defaultPath = ((PathManager)CurrentDynamoModel.PathManager).DefaultTemplatesDirectory;
            Assert.AreEqual(defaultPath, CurrentDynamoModel.PreferenceSettings.TemplateFilePath);
            Assert.AreEqual(defaultPath, CurrentDynamoModel.PathManager.TemplatesDirectory);
            Assert.IsFalse(Directory.Exists(missingPath));
            AssertSavedTemplatePath(defaultPath);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPersistedTemplatePathHasNoGraphsThenTemplatePathResetsToDefault()
        {
            var emptyPath = Path.Combine(TempFolder, "empty-templates");
            Directory.CreateDirectory(emptyPath);
            RestartDynamoWithTemplatePath(emptyPath);

            var defaultPath = ((PathManager)CurrentDynamoModel.PathManager).DefaultTemplatesDirectory;
            Assert.AreEqual(defaultPath, CurrentDynamoModel.PreferenceSettings.TemplateFilePath);
            Assert.AreEqual(defaultPath, CurrentDynamoModel.PathManager.TemplatesDirectory);
            Assert.IsTrue(Directory.Exists(emptyPath));
            AssertSavedTemplatePath(defaultPath);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPersistedTemplatePathIsValidCustomFolderThenTemplatePathIsPreserved()
        {
            var customPath = Path.Combine(TempFolder, "custom-templates");
            Directory.CreateDirectory(customPath);
            File.WriteAllText(Path.Combine(customPath, "Custom.dyn"), "{}");
            RestartDynamoWithTemplatePath(customPath);

            Assert.AreEqual(customPath, CurrentDynamoModel.PreferenceSettings.TemplateFilePath);
            Assert.AreEqual(customPath, CurrentDynamoModel.PathManager.TemplatesDirectory);
            Assert.IsTrue(CurrentDynamoModel.PreferenceSettings.IsTrustedLocation(customPath));
            CurrentDynamoModel.PreferenceSettings.SaveInternal(CurrentDynamoModel.PathManager.PreferenceFilePath);
            AssertSavedTemplatePath(customPath);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPersistedTemplatePathIsPreviousInstallThenTemplatePathResetsToDefault()
        {
            var oldInstallPath = Path.Combine(TempFolder, "old-install", "templates", "en-US");
            Directory.CreateDirectory(oldInstallPath);
            File.WriteAllText(Path.Combine(oldInstallPath, "Old.dyn"), "{}");
            RestartDynamoWithTemplatePath(oldInstallPath);

            var defaultPath = ((PathManager)CurrentDynamoModel.PathManager).DefaultTemplatesDirectory;
            Assert.AreEqual(defaultPath, CurrentDynamoModel.PreferenceSettings.TemplateFilePath);
            Assert.AreNotEqual(oldInstallPath, CurrentDynamoModel.PreferenceSettings.TemplateFilePath);
            AssertSavedTemplatePath(defaultPath);
        }

        private void RestartDynamoWithTemplatePath(string templatePath)
        {
            CurrentDynamoModel.ShutDown(false);
            CurrentDynamoModel = null;
            StartDynamo(new PreferenceSettings { TemplateFilePath = templatePath });
        }

        private void AssertSavedTemplatePath(string expectedPath)
        {
            var saved = PreferenceSettings.Load(PreferenceSettings.DynamoTestPath);
            Assert.AreEqual(expectedPath, saved.TemplateFilePath);
        }
    }
}

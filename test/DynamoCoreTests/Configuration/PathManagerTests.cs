using System.Globalization;
using System.IO;
using System.Threading;
using Dynamo.Configuration;
using Dynamo.Core;
using NUnit.Framework;
using TestServices;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class PathManagerTests : DynamoModelTestBase
    {
        private static PathManager MakePathManager()
        {
            return new PathManager(new PathManagerParams
            {
                CorePath = Path.GetDirectoryName(typeof(PathManager).Assembly.Location)
            });
        }

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

        /// <summary>
        /// DYN-10661: TemplatesDirectory has no default value. It is only ever assigned by
        /// UpdatePreferenceItemPath, so a PathManager that has not had that call succeed
        /// reports a null templates directory even though DefaultTemplatesDirectory is valid.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenPathManagerIsConstructedThenTemplatesDirectoryIsNotNull()
        {
            var pathManager = MakePathManager();

            Assert.That(pathManager.DefaultTemplatesDirectory, Is.Not.Null.And.Not.Empty,
                "DefaultTemplatesDirectory is built by the constructor and should always be set.");
            Assert.That(pathManager.TemplatesDirectory, Is.Not.Null.And.Not.Empty,
                "TemplatesDirectory must never be null - callers use it in string and path operations.");
        }

        /// <summary>
        /// DYN-10661: when the preferred templates location cannot be created (locked-down
        /// %ProgramData%, unavailable network share, invalid path), UpdatePreferenceItemPath
        /// returns false without assigning templatesDirectory, permanently leaving the
        /// property null for the rest of the session. It should fall back to the default.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenTemplateLocationCannotBeCreatedThenTemplatesDirectoryFallsBackToDefault()
        {
            var pathManager = MakePathManager();

            // A file occupying the target path makes Directory.CreateDirectory throw
            // IOException, which is how PathHelper.CreateFolderIfNotExist reports an
            // unusable location without surfacing an error to the user.
            var blockedTemplateLocation = Path.Combine(TempFolder, "blockedTemplates");
            File.WriteAllText(blockedTemplateLocation, string.Empty);

            var updated = pathManager.UpdatePreferenceItemPath(
                PathManager.PreferenceItem.Templates, blockedTemplateLocation);

            Assert.That(updated, Is.False, "An unusable location should be rejected.");
            Assert.That(pathManager.TemplatesDirectory, Is.EqualTo(pathManager.DefaultTemplatesDirectory),
                "A rejected templates location must leave the default in effect, not null.");
        }

        [Test]
        [Category("UnitTests")]
        public void WhenLocaleTemplateFolderHasNoSubdirectoriesThenThatFolderIsUsed()
        {
            var root = Path.Combine(TempFolder, "shipped-templates");
            var germanTemplates = Path.Combine(root, "templates", "de-DE");
            var englishTemplates = Path.Combine(root, "templates", "en-US");
            Directory.CreateDirectory(germanTemplates);
            Directory.CreateDirectory(englishTemplates);
            File.WriteAllText(Path.Combine(germanTemplates, "Grafik erstellen.dyn"), "{}");
            File.WriteAllText(Path.Combine(englishTemplates, "Create a Graph.dyn"), "{}");

            var pathManager = MakeHostPathManager(root, new CultureInfo("de-DE"));

            Assert.AreEqual(
                Path.GetFullPath(germanTemplates),
                Path.GetFullPath(pathManager.DefaultTemplatesDirectory));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenLocaleTemplateFolderIsMissingThenTemplatesFallBackToEnUs()
        {
            var root = Path.Combine(TempFolder, "english-only-templates");
            var englishTemplates = Path.Combine(root, "templates", "en-US");
            Directory.CreateDirectory(englishTemplates);
            File.WriteAllText(Path.Combine(englishTemplates, "Create a Graph.dyn"), "{}");

            var pathManager = MakeHostPathManager(root, new CultureInfo("fr-FR"));

            Assert.AreEqual(
                Path.GetFullPath(englishTemplates),
                Path.GetFullPath(pathManager.DefaultTemplatesDirectory));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenRecentTemplatePathIsAnotherLocaleThenItMapsToTheActiveTemplatesFolder()
        {
            var root = Path.Combine(TempFolder, "recent-templates");
            var germanTemplates = Path.Combine(root, "templates", "de-DE");
            var englishTemplates = Path.Combine(root, "templates", "en-US");
            Directory.CreateDirectory(germanTemplates);
            Directory.CreateDirectory(englishTemplates);
            File.WriteAllText(Path.Combine(germanTemplates, "Create a Graph.dyn"), "{\"Description\":\"German\"}");
            File.WriteAllText(Path.Combine(englishTemplates, "Create a Graph.dyn"), "{\"Description\":\"English\"}");

            var pathManager = MakeHostPathManager(root, new CultureInfo("de-DE"));
            var englishFile = Path.Combine(englishTemplates, "Create a Graph.dyn");

            var localized = pathManager.LocalizeTemplateFilePath(englishFile);

            Assert.AreEqual(Path.GetFullPath(Path.Combine(germanTemplates, "Create a Graph.dyn")), Path.GetFullPath(localized));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenRecentPathIsNotAShippedTemplateThenItIsLeftUnchanged()
        {
            var root = Path.Combine(TempFolder, "recent-custom");
            var germanTemplates = Path.Combine(root, "templates", "de-DE");
            Directory.CreateDirectory(germanTemplates);
            File.WriteAllText(Path.Combine(germanTemplates, "Create a Graph.dyn"), "{}");
            var customFile = Path.Combine(TempFolder, "MyGraph.dyn");
            File.WriteAllText(customFile, "{}");

            var pathManager = MakeHostPathManager(root, new CultureInfo("de-DE"));

            Assert.AreEqual(customFile, pathManager.LocalizeTemplateFilePath(customFile));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenUiCultureChangesAfterConstructionThenDefaultTemplatesDirectoryFollowsNewCulture()
        {
            var root = Path.Combine(TempFolder, "refresh-templates");
            var germanTemplates = Path.Combine(root, "templates", "de-DE");
            var englishTemplates = Path.Combine(root, "templates", "en-US");
            Directory.CreateDirectory(germanTemplates);
            Directory.CreateDirectory(englishTemplates);
            File.WriteAllText(Path.Combine(germanTemplates, "Grafik erstellen.dyn"), "{}");
            File.WriteAllText(Path.Combine(englishTemplates, "Create a Graph.dyn"), "{}");

            var pathManager = MakeHostPathManager(root, new CultureInfo("en-US"));
            Assert.AreEqual(
                Path.GetFullPath(englishTemplates),
                Path.GetFullPath(pathManager.DefaultTemplatesDirectory));

            WithCulture(new CultureInfo("de-DE"), () => pathManager.RefreshDefaultTemplatesDirectory());

            Assert.AreEqual(
                Path.GetFullPath(germanTemplates),
                Path.GetFullPath(pathManager.DefaultTemplatesDirectory));
            Assert.AreEqual(pathManager.DefaultTemplatesDirectory, pathManager.TemplatesDirectory);
        }

        private static PathManager MakeHostPathManager(string commonDataRoot, CultureInfo culture)
        {
            PathManager pathManager = null;
            WithCulture(culture, () =>
            {
                pathManager = new PathManager(new PathManagerParams
                {
                    CorePath = Path.GetDirectoryName(typeof(PathManager).Assembly.Location),
                    HostPath = Path.GetDirectoryName(typeof(PathManager).Assembly.Location),
                    PathResolver = new TestPathResolver(new TestPathResolverParams
                    {
                        CommonDataRootFolder = commonDataRoot
                    }),
                    // Unversioned templates live next to this root. A version folder that
                    // does not exist makes GetTemplateFolder walk up to commonDataRoot.
                    MajorFileVersion = 9,
                    MinorFileVersion = 9
                });
            });
            return pathManager;
        }

        private static void WithCulture(CultureInfo culture, System.Action action)
        {
            var previousUiCulture = Thread.CurrentThread.CurrentUICulture;
            var previousCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                action();
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = previousUiCulture;
                Thread.CurrentThread.CurrentCulture = previousCulture;
            }
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

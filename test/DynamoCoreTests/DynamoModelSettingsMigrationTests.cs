using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Scheduler;
using NUnit.Framework;
using TestServices;

namespace Dynamo.Tests
{
    /// <summary>
    /// Tests when DynamoModel replaces the user's settings with those migrated from an older
    /// Dynamo version. Each test starts a DynamoModel outside test mode against a temporary user
    /// data root, because settings migration is skipped in test mode.
    /// </summary>
    [TestFixture]
    class DynamoModelSettingsMigrationTests : UnitTestBase
    {
        // Distinct values so the source of the loaded settings is visible in the assertions.
        private const string PreviousVersionNumberFormat = "f1";
        private const string CurrentVersionNumberFormat = "f2";
        private const string OlderVersionNumberFormat = "f5";

        // Version folder names only need to sort below the running Dynamo version, so they are
        // fixed values that do not change when the build version is bumped.
        private const string PreviousVersionFolderName = "1.3";
        private const string OlderVersionFolderName = "1.1";

        private bool originalIsTestMode;
        private bool originalDisableAnalytics;
        private string originalDynamoTestPath;
        private string userDataRoot;
        private TestPathResolver pathResolver;
        private DynamoModel model;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            originalIsTestMode = DynamoModel.IsTestMode;

            // NoNetworkMode sets this process-wide flag and never clears it, which would silently
            // disable analytics for tests that run after this fixture.
            originalDisableAnalytics = Analytics.DisableAnalytics;

            // UnitTestBase points this at a nonexistent file, which DynamoModel prefers over the real
            // settings file; the app never sets it.
            originalDynamoTestPath = PreferenceSettings.DynamoTestPath;
            PreferenceSettings.DynamoTestPath = null;

            userDataRoot = Path.Combine(TempFolder, "UserData");
            Directory.CreateDirectory(userDataRoot);

            pathResolver = new TestPathResolver(new TestPathResolverParams
            {
                UserDataRootFolder = userDataRoot,
                CommonDataRootFolder = Path.Combine(TempFolder, "CommonData")
            });

            // The non-test-mode DynamoModel uses the PathManager singleton and keeps whichever
            // resolver it was first given, so bind it to the temp root up front.
            ResetPathManagerSingleton();
            PathManager.Initialize(new PathManagerParams { PathResolver = pathResolver });

            // Saving on shutdown must never touch the real %AppData% settings.
            Assert.That(PathManager.Instance.PreferenceFilePath, Does.StartWith(userDataRoot),
                "PathManager is not bound to the temp user data root; aborting before Dynamo can write real settings.");
        }

        [TearDown]
        public override void Cleanup()
        {
            try
            {
                model?.ShutDown(false);
                model = null;
            }
            finally
            {
                ResetPathManagerSingleton();
                DynamoModel.IsTestMode = originalIsTestMode;
                SetDisableAnalytics(originalDisableAnalytics);
                PreferenceSettings.DynamoTestPath = originalDynamoTestPath;
                base.Cleanup();
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenNoSettingsFileExistsThenSettingsAreMigratedFromPreviousVersion()
        {
            // Arrange
            CreatePreviousVersionSettings();
            Assert.IsFalse(File.Exists(PathManager.Instance.PreferenceFilePath));

            // Act: same shape as the app on a fresh profile (default preferences, no file).
            StartModel(new PreferenceSettings());

            // Assert
            Assert.AreEqual(PreviousVersionNumberFormat, model.PreferenceSettings.NumberFormat);
            Assert.IsTrue(model.PreferenceSettings.IsFirstRun);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSeveralOlderVersionsExistThenSettingsAreMigratedFromTheNearestOne()
        {
            // Arrange: neither folder is the version right before the current one.
            CreatePreviousVersionSettings(OlderVersionFolderName, OlderVersionNumberFormat);
            CreatePreviousVersionSettings(PreviousVersionFolderName, PreviousVersionNumberFormat);
            Assert.IsFalse(File.Exists(PathManager.Instance.PreferenceFilePath));

            // Act
            StartModel(new PreferenceSettings());

            // Assert: the nearest older version wins over the oldest one.
            Assert.AreEqual(PreviousVersionNumberFormat, model.PreferenceSettings.NumberFormat);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenOnlyAnOlderNonAdjacentVersionExistsThenSettingsAreMigratedFromIt()
        {
            // Arrange: the version right before the current one is missing.
            CreatePreviousVersionSettings(OlderVersionFolderName, OlderVersionNumberFormat);
            Assert.IsFalse(File.Exists(PathManager.Instance.PreferenceFilePath));

            // Act
            StartModel(new PreferenceSettings());

            // Assert
            Assert.AreEqual(OlderVersionNumberFormat, model.PreferenceSettings.NumberFormat);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileExistsAndPreferencesAreNullThenMigrationDoesNotReplaceSettings()
        {
            // Arrange
            CreatePreviousVersionSettings();
            CreateCurrentVersionSettings(isFirstRun: true);

            // Act: DynamoModel loads the file itself.
            StartModel(preferences: null);

            // Assert
            Assert.AreEqual(CurrentVersionNumberFormat, model.PreferenceSettings.NumberFormat);
            Assert.IsTrue(model.PreferenceSettings.IsFirstRun);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileExistsAndPreferencesWereLoadedFromItThenMigrationDoesNotReplaceSettings()
        {
            // Arrange
            CreatePreviousVersionSettings();
            CreateCurrentVersionSettings(isFirstRun: true);

            // Act: same shape as StartupUtils, which passes PreferenceSettings.Instance (already loaded from the file).
            StartModel(PreferenceSettings.Load(PathManager.Instance.PreferenceFilePath));

            // Assert
            Assert.AreEqual(CurrentVersionNumberFormat, model.PreferenceSettings.NumberFormat);
            Assert.IsTrue(model.PreferenceSettings.IsFirstRun);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileExistsAndIsFirstRunIsFalseThenSettingsAreNotMigrated()
        {
            // Arrange
            CreatePreviousVersionSettings();
            CreateCurrentVersionSettings(isFirstRun: false);

            // Act
            StartModel(PreferenceSettings.Load(PathManager.Instance.PreferenceFilePath));

            // Assert
            Assert.AreEqual(CurrentVersionNumberFormat, model.PreferenceSettings.NumberFormat);
            Assert.IsFalse(model.PreferenceSettings.IsFirstRun);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileIsEmptyThenDefaultsAreUsedAndMigrationIsSkipped()
        {
            // Arrange: an empty file still counts as existing settings, so migration is skipped.
            // This records the current behavior.
            CreatePreviousVersionSettings();
            var settingsPath = PathManager.Instance.PreferenceFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            File.WriteAllText(settingsPath, string.Empty);

            // Act: PreferenceSettings.Load falls back to defaults on an unreadable file.
            StartModel(PreferenceSettings.Load(settingsPath));

            // Assert
            Assert.AreEqual(new PreferenceSettings().NumberFormat, model.PreferenceSettings.NumberFormat);
            Assert.IsTrue(model.PreferenceSettings.IsFirstRun);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileExistsAndHostSuppliesDifferentPreferencesThenHostPreferencesAreKept()
        {
            // Arrange
            const string hostNumberFormat = "f4";
            CreatePreviousVersionSettings();
            CreateCurrentVersionSettings(isFirstRun: true);

            // Act: host-supplied preferences that did not come from the file.
            StartModel(new PreferenceSettings { NumberFormat = hostNumberFormat });

            // Assert
            Assert.AreEqual(hostNumberFormat, model.PreferenceSettings.NumberFormat);
            Assert.IsTrue(model.PreferenceSettings.IsFirstRun);
        }

        private void StartModel(PreferenceSettings preferences)
        {
            model = DynamoModel.Start(new DynamoModel.DefaultStartConfiguration
            {
                PathResolver = pathResolver,
                StartInTestMode = false,
                NoNetworkMode = true,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = preferences
            });

            // Fail loudly if the model did not run in the mode these tests depend on.
            Assert.IsFalse(DynamoModel.IsTestMode, "Migration is skipped in test mode.");
        }

        private void CreatePreviousVersionSettings(string folderName = PreviousVersionFolderName,
            string numberFormat = PreviousVersionNumberFormat)
        {
            var previousDir = Path.Combine(userDataRoot, folderName);
            Directory.CreateDirectory(previousDir);

            new PreferenceSettings
            {
                NumberFormat = numberFormat,
                CustomPackageFolders = new List<string> { previousDir },
                // Set explicitly: its default is resolved through a PathManager event.
                SelectedPackagePathForInstall = previousDir,
                IsFirstRun = false
            }.Save(Path.Combine(previousDir, "DynamoSettings.xml"));
        }

        private void CreateCurrentVersionSettings(bool isFirstRun)
        {
            var settingsPath = PathManager.Instance.PreferenceFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));

            new PreferenceSettings
            {
                NumberFormat = CurrentVersionNumberFormat,
                IsFirstRun = isFirstRun
            }.Save(settingsPath);
        }

        private static void SetDisableAnalytics(bool value)
        {
            // The setter is internal to DynamoServices and not visible to the tests.
            var property = typeof(Analytics).GetProperty(nameof(Analytics.DisableAnalytics));
            property.GetSetMethod(nonPublic: true).Invoke(null, new object[] { value });
            Assert.AreEqual(value, Analytics.DisableAnalytics);
        }

        private static void ResetPathManagerSingleton()
        {
            var lazyField = typeof(PathManager).GetField("lazy", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(lazyField, "PathManager no longer has a private static 'lazy' field; update this reset.");
            lazyField.SetValue(null, null);
        }
    }
}

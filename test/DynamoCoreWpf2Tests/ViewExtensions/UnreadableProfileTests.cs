using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Logging;
using Dynamo.Notifications;
using Dynamo.Wpf.Views;
using NUnit.Framework;

namespace DynamoCoreWpfTests.ViewExtensions
{
    /// <summary>
    /// Tests for what the user is told when the profile (DynamoSettings.xml) could not be read.
    /// </summary>
    [TestFixture]
    public class UnreadableProfileTests : DynamoTestUIBase
    {
        private const string SettingsSender = "Preference Settings";

        [Test]
        [Category("UnitTests")]
        public void WhenProfileCouldNotBeReadThenNotificationCenterShowsIt()
        {
            MarkProfileAsUnreadable();

            var extension = LoadNotificationsExtension();

            var notifications = extension.Notifications
                .Where(n => n.Sender == SettingsSender &&
                            n.ShortMessage == Dynamo.Notifications.Properties.Resources.InvalidSettingsFileTitle)
                .ToList();
            Assert.AreEqual(1, notifications.Count);
            StringAssert.Contains(Model.PathManager.PreferenceFilePath, notifications[0].DetailedMessage);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenProfileWasReadThenNotificationCenterDoesNotShowIt()
        {
            Assert.IsTrue(Model.PreferenceSettings.IsCreatedFromValidFile);

            var extension = LoadNotificationsExtension();

            Assert.IsFalse(extension.Notifications.Any(n =>
                n.ShortMessage == Dynamo.Notifications.Properties.Resources.InvalidSettingsFileTitle));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenProfileCouldNotBeReadThenExportStopsAndNotifies()
        {
            MarkProfileAsUnreadable();
            var notifications = new List<NotificationMessage>();
            Model.Logger.NotificationLogged += notifications.Add;
            var preferencesWindow = new PreferencesView(View);
            try
            {
                var stopped = preferencesWindow.StopExportIfProfileIsUnreadable();

                Assert.IsTrue(stopped);
                Assert.AreEqual(1, notifications.Count);
                Assert.AreEqual(SettingsSender, notifications[0].Sender);
                StringAssert.Contains(Model.PathManager.PreferenceFilePath, notifications[0].DetailedMessage);
            }
            finally
            {
                Model.Logger.NotificationLogged -= notifications.Add;
                preferencesWindow.Close();
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenProfileWasReadThenExportIsNotStopped()
        {
            Assert.IsTrue(Model.PreferenceSettings.IsCreatedFromValidFile);
            var notifications = new List<NotificationMessage>();
            Model.Logger.NotificationLogged += notifications.Add;
            var preferencesWindow = new PreferencesView(View);
            try
            {
                var stopped = preferencesWindow.StopExportIfProfileIsUnreadable();

                Assert.IsFalse(stopped);
                Assert.IsEmpty(notifications);
            }
            finally
            {
                Model.Logger.NotificationLogged -= notifications.Add;
                preferencesWindow.Close();
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenUnreadableProfileWasReplacedThenExportIsNotStopped()
        {
            MarkProfileAsUnreadable();
            var settings = Model.PreferenceSettings;
            var originalBackupLocation = settings.BackupLocation;
            var backupRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var notifications = new List<NotificationMessage>();
            Model.Logger.NotificationLogged += notifications.Add;
            var preferencesWindow = new PreferencesView(View);
            try
            {
                // Keep the backup of the test profile out of the real backup folder.
                settings.BackupLocation = backupRoot;
                Assert.IsTrue(settings.SaveInternal(Model.PathManager.PreferenceFilePath));

                var stopped = preferencesWindow.StopExportIfProfileIsUnreadable();

                Assert.IsFalse(stopped);
                Assert.IsEmpty(notifications);
            }
            finally
            {
                settings.BackupLocation = originalBackupLocation;
                Model.Logger.NotificationLogged -= notifications.Add;
                preferencesWindow.Close();
                if (Directory.Exists(backupRoot))
                {
                    Directory.Delete(backupRoot, true);
                }
            }
        }

        private NotificationsViewExtension LoadNotificationsExtension()
        {
            var extension = new NotificationsViewExtension();
            extension.Startup(new Dynamo.Wpf.Extensions.ViewStartupParams(ViewModel));
            extension.Loaded(new Dynamo.Wpf.Extensions.ViewLoadedParams(View, ViewModel));
            return extension;
        }

        // The model is created from a valid profile; flip the flag Load sets when it falls back to defaults.
        private void MarkProfileAsUnreadable()
        {
            var field = typeof(Dynamo.Configuration.PreferenceSettings)
                .GetField("isCreatedFromValidFile", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "PreferenceSettings no longer has a private 'isCreatedFromValidFile' field; update this helper.");
            field.SetValue(Model.PreferenceSettings, false);
            Assert.IsFalse(Model.PreferenceSettings.IsCreatedFromValidFile);
        }
    }
}

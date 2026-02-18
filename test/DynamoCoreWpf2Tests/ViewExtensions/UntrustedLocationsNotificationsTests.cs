using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Models;
using Dynamo.Notifications;
using NUnit.Framework;

namespace DynamoCoreWpfTests.ViewExtensions
{
    /// <summary>
    /// Tests for the EnableUnTrustedLocationsNotifications feature
    /// </summary>
    [TestFixture]
    public class UntrustedLocationsNotificationsTests : DynamoTestUIBase
    {
        private string programDataPath;

        [SetUp]
        public void Setup()
        {
            // Get the ProgramData path
            programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        }

        [Test]
        [Category("UnitTests")]
        public void TestEnableUnTrustedLocationsNotifications_DisabledPreventsNotifications()
        {
            // Arrange - Add unsafe trusted location to the existing base model
            var unsafePath = Path.Combine(programDataPath, "TestUnsafeLocation");
            Model.PreferenceSettings.SetTrustedLocationsUnsafe(new List<string> { unsafePath });
            Model.PreferenceSettings.EnableUnTrustedLocationsNotifications = false;

            // Act - Create and load NotificationsViewExtension
            var notificationExtension = new NotificationsViewExtension();
            var startupParams = new Dynamo.Wpf.Extensions.ViewStartupParams(ViewModel);
            notificationExtension.Startup(startupParams);
            notificationExtension.Loaded(new Dynamo.Wpf.Extensions.ViewLoadedParams(View, ViewModel));

            // Assert
            Assert.IsFalse(Model.PreferenceSettings.EnableUnTrustedLocationsNotifications, 
                "EnableUnTrustedLocationsNotifications should be false");

            // Check that no unsafe path notifications were created
            var unsafePathNotifications = notificationExtension.Notifications
                .Where(n => n.Sender.Contains("Preference Settings") &&
                           n.ShortMessage.Contains("Unsafe"))
                .ToList();

            Assert.IsEmpty(unsafePathNotifications,
                "No unsafe path notifications should be created when EnableUnTrustedLocationsNotifications is false");
        }

        [Test]
        [Category("UnitTests")]
        public void TestEnableUnTrustedLocationsNotifications_EnabledCreatesNotifications()
        {
            // Arrange - Add unsafe trusted location to the existing base model
            var unsafePath = Path.Combine(programDataPath, "TestUnsafeLocation");
            Model.PreferenceSettings.SetTrustedLocationsUnsafe(new List<string> { unsafePath });
            Model.PreferenceSettings.EnableUnTrustedLocationsNotifications = true;

            // Act - Create and load NotificationsViewExtension
            var notificationExtension = new NotificationsViewExtension();
            var startupParams = new Dynamo.Wpf.Extensions.ViewStartupParams(ViewModel);
            notificationExtension.Startup(startupParams);
            notificationExtension.Loaded(new Dynamo.Wpf.Extensions.ViewLoadedParams(View, ViewModel));

            // Assert
            Assert.IsTrue(Model.PreferenceSettings.EnableUnTrustedLocationsNotifications, 
                "EnableUnTrustedLocationsNotifications should be true");

            // Check that unsafe path notifications were created
            var unsafePathNotifications = notificationExtension.Notifications
                .Where(n => n.Sender.Contains("Preference Settings") &&
                           n.DetailedMessage.Contains(unsafePath))
                .ToList();

            Assert.IsNotEmpty(unsafePathNotifications, 
                "Unsafe path notifications should be created when EnableUnTrustedLocationsNotifications is true");
            
            Assert.That(unsafePathNotifications.Count, Is.EqualTo(1), 
                "Exactly one notification should be created for the unsafe path");
        }

        [Test]
        [Category("UnitTests")]
        public void TestEnableUnTrustedLocationsNotifications_DefaultValueIsTrue()
        {
            // The base test class's Model is created with default configuration
            // which should have EnableUnTrustedLocationsNotifications = true by default
            
            // Assert - Should default to true
            Assert.IsTrue(Model.PreferenceSettings.EnableUnTrustedLocationsNotifications,
                "EnableUnTrustedLocationsNotifications should default to true for backward compatibility");
        }
    }
}

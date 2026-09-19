using System.IO;
using Dynamo.Configuration;
using NUnit.Framework;

namespace Dynamo.Tests.Configuration
{
    /// <summary>
    /// Tests for the theme preference added by DYN-6228. The theme is read once during startup
    /// to select a resource dictionary folder, so the only behaviour to guarantee here is that it
    /// defaults to Dark and survives a save/load round trip.
    /// </summary>
    [TestFixture]
    public class DynamoThemeTests
    {
        private string tempSettingsPath;

        [SetUp]
        public void SetUp()
        {
            tempSettingsPath = Path.Combine(Path.GetTempPath(),
                Path.GetRandomFileName() + ".xml");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(tempSettingsPath))
            {
                File.Delete(tempSettingsPath);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenThemeIsNotSetThenDefaultsToDark()
        {
            var settings = new PreferenceSettings();

            Assert.AreEqual(DynamoTheme.Dark, settings.Theme);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenThemeIsLightThenRoundTripsThroughSaveAndLoad()
        {
            var settings = new PreferenceSettings { Theme = DynamoTheme.Light };

            settings.Save(tempSettingsPath);
            var loaded = PreferenceSettings.Load(tempSettingsPath);

            Assert.AreEqual(DynamoTheme.Light, loaded.Theme);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenThemeIsDarkThenRoundTripsThroughSaveAndLoad()
        {
            var settings = new PreferenceSettings { Theme = DynamoTheme.Dark };

            settings.Save(tempSettingsPath);
            var loaded = PreferenceSettings.Load(tempSettingsPath);

            Assert.AreEqual(DynamoTheme.Dark, loaded.Theme);
        }

        /// <summary>
        /// Settings files written before DYN-6228 have no Theme element. Those users must keep
        /// the dark theme they already had rather than getting an unrequested light UI.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenSettingsFileHasNoThemeElementThenThemeIsDark()
        {
            File.WriteAllText(tempSettingsPath,
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<PreferenceSettings xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<ShowEdges>false</ShowEdges>" +
                "</PreferenceSettings>");

            var loaded = PreferenceSettings.Load(tempSettingsPath);

            Assert.AreEqual(DynamoTheme.Dark, loaded.Theme);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenThemeChangesThenPropertyChangedIsRaised()
        {
            var settings = new PreferenceSettings();
            var raised = 0;
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PreferenceSettings.Theme)) raised++;
            };

            settings.Theme = DynamoTheme.Light;

            Assert.AreEqual(1, raised);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenThemeIsSetToSameValueThenPropertyChangedIsNotRaised()
        {
            var settings = new PreferenceSettings { Theme = DynamoTheme.Light };
            var raised = 0;
            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PreferenceSettings.Theme)) raised++;
            };

            settings.Theme = DynamoTheme.Light;

            Assert.AreEqual(0, raised);
        }

        /// <summary>
        /// The light theme selects a separate syntax highlighting definition, so the two resource
        /// names must not collide.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenHighlightingFilesAreComparedThenLightDiffersFromDefault()
        {
            Assert.AreNotEqual(Configurations.HighlightingFile, Configurations.LightHighlightingFile);
            Assert.IsNotEmpty(Configurations.LightHighlightingFile);
        }
    }
}

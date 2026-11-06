using Dynamo.Configuration;
using Dynamo.UI;
using NUnit.Framework;
using Res = Dynamo.Wpf.Properties.Resources;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// The theme is fixed once per process. Inside a host such as Revit, closing and reopening
    /// Dynamo starts a new session in the same process while static resource caches still hold
    /// the theme the process started with, so a later session must not switch themes.
    /// </summary>
    /// <remarks>
    /// The latch is process-wide state shared with every other fixture, so it is saved before
    /// each test and restored afterwards.
    /// </remarks>
    [TestFixture]
    public class ThemeLatchTests
    {
        private DynamoTheme originalTheme;
        private bool wasLatched;

        [SetUp]
        public void SetUp()
        {
            originalTheme = SharedDictionaryManager.CurrentTheme;
            wasLatched = SharedDictionaryManager.IsThemeLatched;
            SharedDictionaryManager.ResetThemeLatchForTesting();
        }

        [TearDown]
        public void TearDown()
        {
            SharedDictionaryManager.ResetThemeLatchForTesting();
            SharedDictionaryManager.CurrentTheme = originalTheme;
            if (wasLatched)
            {
                SharedDictionaryManager.ApplyStartupTheme(originalTheme);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void WhenTheFirstSessionAppliesAThemeThenItTakesEffect()
        {
            var applied = SharedDictionaryManager.ApplyStartupTheme(DynamoTheme.Light);

            Assert.IsTrue(applied);
            Assert.AreEqual(DynamoTheme.Light, SharedDictionaryManager.CurrentTheme);
            Assert.IsTrue(SharedDictionaryManager.IsThemeLatched);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenALaterSessionRequestsADifferentThemeThenTheFirstThemeIsKept()
        {
            SharedDictionaryManager.ApplyStartupTheme(DynamoTheme.Dark);

            var applied = SharedDictionaryManager.ApplyStartupTheme(DynamoTheme.Light);

            Assert.IsFalse(applied);
            Assert.AreEqual(DynamoTheme.Dark, SharedDictionaryManager.CurrentTheme);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenALaterSessionRequestsTheSameThemeThenItIsReportedAsInEffect()
        {
            SharedDictionaryManager.ApplyStartupTheme(DynamoTheme.Light);

            var applied = SharedDictionaryManager.ApplyStartupTheme(DynamoTheme.Light);

            Assert.IsTrue(applied);
            Assert.AreEqual(DynamoTheme.Light, SharedDictionaryManager.CurrentTheme);
        }

        /// <summary>
        /// The message is formatted inside the DynamoViewModel constructor, so a malformed format
        /// string would fail Dynamo's startup rather than just the log line.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenThePendingThemeMessageIsFormattedThenItNamesBothThemes()
        {
            string message = null;

            Assert.DoesNotThrow(() => message = string.Format(
                Res.PreferencesViewThemeChangePendingRestart, DynamoTheme.Light, DynamoTheme.Dark));

            StringAssert.Contains(nameof(DynamoTheme.Light), message);
            StringAssert.Contains(nameof(DynamoTheme.Dark), message);
        }
    }
}

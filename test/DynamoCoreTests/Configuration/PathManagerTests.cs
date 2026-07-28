using System.IO;
using Dynamo.Core;
using NUnit.Framework;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class PathManagerTests : UnitTestBase
    {
        private static PathManager MakePathManager()
        {
            return new PathManager(new PathManagerParams
            {
                CorePath = Path.GetDirectoryName(typeof(PathManager).Assembly.Location)
            });
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
            Assert.That(pathManager.TemplatesDirectory, Is.Not.Null.And.Not.Empty,
                "A rejected templates location must leave a usable fallback, not null.");
        }
    }
}

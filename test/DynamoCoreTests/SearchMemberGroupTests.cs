using Dynamo.Search;
using Dynamo.UI;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class SearchMemberGroupTests
    {
        [Test]
        [Category("UnitTests")]
        public void PrefixTest()
        {
            var memberGroup = new SearchMemberGroup(null);
            Assert.AreEqual(string.Empty, memberGroup.Prefix);

            memberGroup = new SearchMemberGroup(string.Empty);
            Assert.AreEqual(string.Empty, memberGroup.Prefix);

            var delimiter = Configurations.ShortenedCategoryDelimiter;
            var fullyQualifiedName = "Builtin Functions" + delimiter + "Actions";

            memberGroup = new SearchMemberGroup(fullyQualifiedName);
            Assert.AreEqual(string.Empty, memberGroup.Prefix);

            fullyQualifiedName = "1stCategory" + delimiter +
                                 "2ndCategory" + delimiter +
                                 "3rdCategory" + delimiter + "Create";

            memberGroup = new SearchMemberGroup(fullyQualifiedName);
            Assert.AreEqual("2ndCategory" + delimiter +
                            "3rdCategory" + delimiter, memberGroup.Prefix);
        }
    }
}

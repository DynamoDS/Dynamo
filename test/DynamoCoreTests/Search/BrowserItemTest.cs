using Dynamo.Search;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Tests.Search
{
    /// <summary>
    /// In this class we are including the test cases missing from the abstract class BrowserItem,I mean, the ones without coverage
    /// </summary>
    [TestFixture]
    class BrowserItemTest
    {
        [Test]
        [Category("UnitTests")]
        public void TestInternalMigration()
        {
            var browserItem = new BrowserInternalElement();
        }
    }
}

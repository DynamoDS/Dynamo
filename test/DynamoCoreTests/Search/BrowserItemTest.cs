using Dynamo.Search;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public void TestAddChild()
        {
            var browserItem = new BrowserInternalElement();

            var rootItem = new BrowserInternalElement("Root Node", null);
            browserItem.AddChild(rootItem);

            for (int i =0;i<10;i++)
            {
                var childItem = new BrowserInternalElement("Item"+i.ToString(),rootItem);
                browserItem.AddChild(childItem);
            }
        }
    }
}

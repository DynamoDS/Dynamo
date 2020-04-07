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
        /// <summary>
        /// This test method will execute the BrowserElement.AddChild method.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestAddChild()
        {
            var browserItem = new BrowserInternalElement();

            var rootItem = new BrowserInternalElement("Root Node", null);
            rootItem.Height = 100;
            browserItem.AddChild(rootItem);

            for (int i =0;i<10;i++)
            {
                var childItem = new BrowserInternalElement("Item"+i.ToString(),rootItem);
                childItem.Height = 100;
                browserItem.AddChild(childItem);
            }

            //Checks that the height has a value greater than 0
            Assert.Greater(rootItem.Height,0);
        }
    }
}

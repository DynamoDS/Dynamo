using Dynamo.Search;
using NUnit.Framework;

namespace Dynamo.Tests.Search
{
    [TestFixture]
    class BrowserInternalElementTest
    {
        /// <summary>
        /// This test method will execute the next functions in BrowserInternalElement class
        /// Execute() method
        /// Name Get property
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestItemCollections()
        {
            //This is the root element
            BrowserInternalElement elemRoot = new BrowserInternalElement();

            //This is the default element child of root
            BrowserInternalElement elemDefault = new BrowserInternalElement("Default", elemRoot);

            //This are the child elements whose father is elemDefault
            BrowserInternalElement elemChild = new BrowserInternalElement("Child", elemDefault);
            BrowserInternalElement elemChild2 = new BrowserInternalElement("Child2", elemDefault);

            BrowserInternalElement elemSon = new BrowserInternalElement("Son1", null);
            elemChild.AddChild(elemSon);

            BrowserInternalElement elemGrandSon = new BrowserInternalElement("GranSon1", null);
            elemSon.AddChild(elemGrandSon);

            //The elemDefault will have two childs
            elemDefault.AddChild(elemChild);
            elemDefault.AddChild(elemChild2);

            //This will allow to enter the section "if (this.Items.Count == 1)"
            elemChild.Execute();

            //This will enable the section where BrowserInternalElement.Siblings Get method is executed
            elemChild2.Execute();

            elemGrandSon.ExpandToRoot();
            elemChild.ReturnToOldParent();
            elemGrandSon.ReturnToOldParent();

            elemRoot.Items = null; //This will execute the Set method of the Items property

            //Just checking the Get method of the BrowserInternalElement.Name property
            Assert.AreEqual(elemGrandSon.Name, "GranSon1");

            //This will check that was expanded correctly by the ExpandToRoot() method
            Assert.IsTrue(elemRoot.IsExpanded);

            //This will check that is Visible (ExpandToRoot() method make it visible)
            Assert.IsTrue(elemRoot.Visibility);
        }
    }
}

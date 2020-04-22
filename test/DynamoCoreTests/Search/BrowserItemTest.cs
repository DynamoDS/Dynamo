using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Search;
using NUnit.Framework;

namespace Dynamo.Tests.Search
{
    /// <summary>
    /// Due that the child BrowserInternalItem class was not calling OnExecute, I had to create the BrowserItemDerived class and call OnExecute method
    /// </summary>
    class BrowserItemDerived : BrowserItem
    {
        #region Private Members
        private ObservableCollection<BrowserItem> _items = new ObservableCollection<BrowserItem>();

        private string _name;

        /// <summary>
        ///     Returns name of the node
        /// </summary>
        public override string Name
        {
            get { return _name; }
        }

        #endregion

        #region Public Members
        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        public override ObservableCollection<BrowserItem> Items { get { return _items; } set { _items = value; } }

        /// <summary>
        ///     Returns browser item representing category which this element belongs to
        /// </summary>
        public BrowserItem Parent { get; set; }

        /// <summary>
        ///     Returns items which are in the same category as the browser item
        /// </summary>
        public ObservableCollection<BrowserItem> Siblings { get { return this.Parent.Items; } }

        #endregion

        //Default constructor
        public BrowserItemDerived()
        {
            this._name = "Default";
            this.Parent = null;
        }

        //Constructor for when the BrowserItem parent is passed
        public BrowserItemDerived(string name, BrowserItem parent)
        {
            this._name = name;
            this.Parent = parent;
        }

        //This overriden method will call the OnExecuted() event method (this will send the notification to all event subscribers).
        internal override void Execute()
        {
            base.OnExecuted();

            var endState = !this.IsExpanded;

            foreach (var ele in this.Siblings)
                ele.IsExpanded = false;
          
        }
    }
    /// <summary>
    /// In this class we are including the test cases missing from the abstract class BrowserItem,I mean, the ones without coverage
    /// </summary>
    [TestFixture]
    class BrowserItemTest
    {
        //Private Browser elements that will be used in several methods
        private BrowserInternalElement browserItem;
        private BrowserInternalElement rootItem;

        [SetUp]
        public void SetUp()
        {
            browserItem = new BrowserInternalElement();
            rootItem = new BrowserInternalElement("Root Node", null);
        }

        /// <summary>
        /// This test method will execute the next methods:
        /// BrowserElement.AddChild.
        /// BrowserElement.GetVisibleLeaves
        /// BrowserElement.Height property
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestAddChild()
        {
            //Arrange       
            rootItem.Height = 100;
            rootItem.Visibility = true;

            //Act
            browserItem.AddChild(rootItem);

            //In this part we are adding 2 childs BrowserInternalElement under rootItem
   
            var childItem1 = new BrowserInternalElement("Item 1", rootItem);
            childItem1.Height = 100; //This will execute the Set method of the Height property
            childItem1.Visibility = true;
            rootItem.AddChild(childItem1);

            var childItem2= new BrowserInternalElement("Item 2", rootItem);
            childItem2.Height = 100; //This will execute the Set method of the Height property
            childItem2.Visibility = false;
            rootItem.AddChild(childItem2);


            List<BrowserItem> itemsList = new List<BrowserItem>();
            rootItem.GetVisibleLeaves(ref itemsList);

            //Assert
            //Checks that the height has a value greater than 0
            Assert.Greater(rootItem.Height, 0);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCollapseToLeaves()
        {
            //Act
            browserItem.AddChild(rootItem);
            //We are adding five Browser elements more
            for (int i = 0; i < 5; i++)
            {
                var childItem = new BrowserInternalElement("Item" + i.ToString(), rootItem);
                childItem.Visibility = false;
                rootItem.AddChild(childItem);
            }
            List<BrowserItem> itemsList = new List<BrowserItem>();
            rootItem.GetVisibleLeaves(ref itemsList);

            rootItem.CollapseToLeaves();

            //Asert
            //When we called CollapseToLeaves the IsExpanded property should be false
            Assert.IsFalse(rootItem.IsExpanded);

            //Then we set the visibility to true 
            rootItem.SetVisibilityToLeaves(true);

            //Asert
            //When we called SetVisibilityToLeaves the Visibility property is true
            Assert.IsTrue(rootItem.Visibility);
        }

        /// <summary>
        /// This test method will execute the next:
        /// BrowserItem.OnExecute() method
        /// BrowserItem.IsSelected (Get, Set) property
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestOnExecute()
        {
            //Arrange
            var derivedItem = new BrowserItemDerived("DerivedItem", rootItem);

            //Act
            //We subscribe your method to the BrowserItemHandler event
            derivedItem.Executed += OldParent_Executed;
            //This will execute the OnExecute method in BrowserItem abstract class.
            derivedItem.Execute();
            //This will raise the Set method of the property
            derivedItem.IsSelected = true;

            //Assert
            //Check that the value was assigned correctly inside the subscribed method
            Assert.AreEqual(derivedItem.Height, 100);
            //This will raise the Get method of the property
            Assert.AreEqual(derivedItem.IsSelected, true);
        }

        //Subscribed method to the BrowserItemHandler event 
        private void OldParent_Executed(BrowserItem ele)
        {
            ele.Height = 100;
        }
    }
}

using System;
using Dynamo.PackageManager;
using Dynamo.Search.SearchElements;
using Greg.Responses;
using NUnit.Framework;

namespace Dynamo.Tests.Search.SearchElements
{
    [TestFixture]
    class SearchElementBaseTest
    {
        bool bExecuted = false;//This flag will be set to true when the event is raised

        /// <summary>
        /// This test method will execute the next methods in SearchElementBase class
        /// public virtual void Execute()
        /// protected void OnExecuted()
        /// internal event SearchElementHandler Executed;
        /// public virtual string CreationName
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestNodeModelSearchElement()
        {
            //Arrange
            var searchElement = new PackageManagerSearchElement(new PackageHeader() { name = "Foo" });
            searchElement.Executed += SearchElement_Executed; //Subscribe the function to the Executed event

            //Act
            searchElement.Execute();

            //Assert
            Assert.AreEqual(searchElement.CreationName,"Foo");//This will execute the Get method of the CreationName property
            Assert.IsTrue(bExecuted);//This just validates that the Execute event was raised
        }

        //This method will be execute then the OnExecute() event is called.
        private void SearchElement_Executed(SearchElementBase ele)
        {
            bExecuted = true;
        }
    }
}

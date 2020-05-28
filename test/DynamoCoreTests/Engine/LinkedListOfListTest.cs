using System;
using System.Collections;
using System.Reflection;
using Dynamo.Engine;
using NUnit.Framework;

namespace Dynamo.Tests.Engine
{
    [TestFixture]
    class LinkedListOfListTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods from the LinkedListOfList class:
        /// IEnumerator IEnumerable.GetEnumerator()
        /// IEnumerator<List<T>> IEnumerable<List<T>>.GetEnumerator()
        /// public List<TKey> GetKeys()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void LinkedListOfListEnumeratorsTest()
        {
            //Arrange
            LinkedListOfList<Guid, string> nodes = new LinkedListOfList<Guid, string>();
            nodes.AddItem(Guid.NewGuid(), "test1");
            nodes.AddItem(Guid.NewGuid(), "test2");
            nodes.AddItem(Guid.NewGuid(), "test3");

            //Act
            var nodesKeys = nodes.GetKeys();

            //We need to use reflection since the GetEnumerator methods are not used (no references) and are private to the LinkedListOfList class
            MethodInfo dynMethod = nodes.GetType().GetMethod("System.Collections.IEnumerable.GetEnumerator", BindingFlags.NonPublic | BindingFlags.Instance);
            var enumerator1 = (IEnumerator)dynMethod.Invoke(nodes, null);

            MethodInfo dynMethod2 = nodes.GetType().GetMethod("System.Collections.Generic.IEnumerable<System.Collections.Generic.List<T>>.GetEnumerator", BindingFlags.NonPublic | BindingFlags.Instance);
            var enumerator2 = (IEnumerator)dynMethod2.Invoke(nodes, null);

            //Assert
            //Checking that the enumerators and the keys were successfully created
            Assert.AreEqual(nodesKeys.Count, 3);
            Assert.IsNotNull(enumerator1);
            Assert.IsNotNull(enumerator2);
        }
    }
}

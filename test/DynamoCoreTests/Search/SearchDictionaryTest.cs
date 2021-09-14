using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo.Search;
using NUnit.Framework;

namespace Dynamo.Tests.Search
{
    [TestFixture]
    class SearchDictionaryTest
    {
        /// <summary>
        /// This test method will execute several Add methods located in the SearchDictionary class
        /// void Add(IEnumerable<V> values, string tag, double weight = 1)
        /// void Add(V value, IEnumerable<string> tags, IEnumerable<double> weights)
        /// void Add(IEnumerable<V> values, IEnumerable<string> tags, double weight = 1)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestAddItems()
        {
            //Arrange
            var searchDictionary = new SearchDictionary<string>();
            var tag1 = Guid.NewGuid().ToString();

            //It will generate the numbers from 1 to 10, starting from 1
            IEnumerable<string> ranges1 = from value in Enumerable.Range(1, 10)
                                          select value.ToString();

            //This linq expression will generate 1 tags
            IEnumerable<string> tags = from value in Enumerable.Range(1, 1)
                                       select Guid.NewGuid().ToString();

            //This linq expression will generate 20 weights
            IEnumerable<double> wrongWeights = from value in Enumerable.Range(1, 20)
                                               select double.Parse(value.ToString());

            //This linq expression will generate 1 value starting from 21
            IEnumerable<string> values = from value in Enumerable.Range(21, 1)
                                         select value.ToString();

            //Act
            //This will execute the method void Add(IEnumerable<V> values, string tag, double weight = 1)
            searchDictionary.Add(ranges1, tag1);//Add 10 strings

            //This will execute the method internal void Add(IEnumerable<V> values, IEnumerable<string> tags, double weight = 1)
            searchDictionary.Add(values, tags);//Add 1 string

            //Assert
            //This will execute the method void Add(V value, IEnumerable<string> tags, IEnumerable<double> weights)
            //It will fail due that number of tags is different than number of weights
            Assert.Throws<ArgumentException>(() => searchDictionary.Add("Test", tags, wrongWeights));


            //Check that we have 11 items in the dictionary
            Assert.AreEqual(searchDictionary.NumTags, 11);
        }

        /// <summary>
        /// This test method will execute several Remove methods located in the SearchDictionary class
        /// int Remove(Func<V, bool> removeCondition)
        /// int Remove(Func<V, bool> valueCondition, Func<string, bool> removeTagCondition)
        /// bool Remove(V value, string tag)
        /// int Remove(V value, IEnumerable<string> tags)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestRemoveItems()
        {
            // Arrange
            var searchDictionary = new SearchDictionary<string>();
            var tag1 = Guid.NewGuid().ToString();

            //It will generate the numbers from 1 to 10, starting from 1
            IEnumerable<string> ranges1 = from value in Enumerable.Range(1, 10)
                                          select value.ToString();

            //It will generate the numbers from 1 to 10, starting from 1
            IEnumerable<string> tags = from value in Enumerable.Range(1, 2)
                                       select tag1;

            //Delegate that uses a lamda expression to delete the tag "9" inside the searchDictionary
            Func<string, bool> selector = str => str.Equals("9");

            //Delegate that uses a lamda expression to delete the tag "8" inside the searchDictionary
            Func<string, bool> selectorValue = str => str.Equals("8");

            //Delegate that uses a lamda expression to delete the tag "8" inside the searchDictionary
            Func<string, bool> selectorTag = str => str.Equals(tag1);

            //Act
            searchDictionary.Add(ranges1, tag1);//Add 10 strings

            //This will execute the method  internal bool Remove(V value, string tag)
            searchDictionary.Remove("10", tag1);//Remove the value "10"         

            //This will execute the method int Remove(Func<V, bool> removeCondition)
            searchDictionary.Remove(selector);//Remove the value "9"

            //This will execute the method  internal int Remove(Func<V, bool> valueCondition, Func<string, bool> removeTagCondition)
            searchDictionary.Remove(selectorValue, selectorTag);//Remove the value 8

            //This will execute the method internal int Remove(V value, IEnumerable<string> tags)
            searchDictionary.Remove("7", tags);//Remove the value 7
            int valuesRemoved = searchDictionary.Remove("16", tags);//Value won't be found so it will retung 0
         
            //Assert
            //Check that we have only 7 strings in the dictionary, at the beginning we had 10 but 3 were removed
            Assert.AreEqual(searchDictionary.NumTags, 6);          
            Assert.AreEqual(valuesRemoved, 0);//Means that value was't found, the Remove(V value, IEnumerable<string> tags) method returned 0
        }

        /// <summary>
        /// This test method will execute several methods for getting info from the SearchDictionary
        /// IEnumerable<V> ByTag(string tag)
        /// bool Contains(V a)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TesGetItems()
        {
            // Arrange
            var searchDictionary = new SearchDictionary<string>();
            var tag1 = Guid.NewGuid().ToString();
            var tag2 = Guid.NewGuid().ToString();

            //It will generate the numbers from 1 to 10, starting from 1
            IEnumerable<string> ranges1 = from value in Enumerable.Range(1, 10)
                                          select value.ToString();

            //It will generate the numbers from 1 to 10, starting from 1
            IEnumerable<string> ranges2 = from value in Enumerable.Range(1, 10)
                                          select value.ToString();

            //Act
            searchDictionary.Add(ranges1, tag1);//Add 10 strings using the tag1
            searchDictionary.Add(ranges2, tag2);//Add 10 strings using the tag2

            //This will execute the method internal IEnumerable<V> ByTag(string tag)
            var ienumTags = searchDictionary.ByTag(tag1);

            var tags = searchDictionary.GetTags("5");//This will return 2 tags

            var weights = searchDictionary.GetWeights("5");//This will return 2 weights

            //Assert
            //This will validate that the method ByTag returns the 10 strings inserted
            Assert.AreEqual(ienumTags.Count(), 10);

            //This will validate that the dictionary contains the value "6"
            Assert.IsTrue(searchDictionary.Contains("6"));//This will execute the method bool Contains(V a)

            //Check that the two different tags are returned for the "5" value
            Assert.AreEqual(tags.Count(), 2);

            //Check that the two different weights are returned for the "5" value
            Assert.AreEqual(weights.Count(), 2); 
        }

        /// <summary>
        ///  This test method will execute the private static SearchDictionary.ContainsSpecialCharacters method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestContainsSpecialCharacters()
        {
            //Arrange
            string strSpecial = "test*+-test";//Create a string that has special characters
            bool containsSpecial = false;

            //Act
            //Using reflection we execute the private ContainsSpecialCharacters static method 
            MethodInfo dynMethod = typeof(SearchDictionary<string>).GetMethod("ContainsSpecialCharacters", BindingFlags.NonPublic | BindingFlags.Static);
            containsSpecial = (bool)dynMethod.Invoke(null, new object[] { strSpecial });

            //Arrange
            Assert.IsTrue(containsSpecial);//Validate that the string has special characters
        }
    }
}

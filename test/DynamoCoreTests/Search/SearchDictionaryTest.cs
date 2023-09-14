using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Search;
using NUnit.Framework;

namespace Dynamo.Tests.Search
{
    [TestFixture]
    class SearchDictionaryTest : UnitTestBase
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
    }   
}

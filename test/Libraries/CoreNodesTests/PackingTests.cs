using DesignScript.Builtin;
using DSCore;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Tests
{
    public class PackingTests
    {
        public class UnPackOutputByKeyMethod
        {
            [Test]
            public void WhenDictionaryIsNull_ReturnsNull()
            {
                var result = UnPackFunctions.UnPackOutputByKey(null, "property1");

                Assert.Null(result);
            }

            [Test]
            public void WhenDictionaryIsNotNull_ReturnsValueAtKey()
            {
                var dictionary = Dictionary.ByKeysValues(new List<string> { "property1" }, new List<object> { "value" });

                var result = UnPackFunctions.UnPackOutputByKey(dictionary, "property1");

                Assert.AreEqual("value", result);
            }
        }

        public class PackOutputAsDictionaryMethod
        {
            [Test]
            public void WhenKeysAreNull_ShouldReturnNull()
            {
                var result = PackFunctions.PackOutputAsDictionary(null, new List<bool> { true }, 1);

                Assert.Null(result);
            }

            [Test]
            public void WhenKeysIsEmptyList_ShouldReturnNull()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string>(), new List<bool> { true }, 1);

                Assert.Null(result);
            }

            [Test]
            public void WhenIsCollectionIsNull_ShouldReturnNull()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1" }, null, 1);

                Assert.Null(result);
            }

            [Test]
            public void WhenIsCollectionIsEmpty_ShouldReturnNull()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1" }, new List<bool>(), 1);

                Assert.Null(result);
            }

            [Test]
            public void WhenDataIsNull_ShouldReturnNull()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1" }, new List<bool> { true }, null);

                Assert.Null(result);
            }

            [Test]
            public void WhenNoCollections_ReturnsDictionaryMatchingData()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1", "property2" }, new List<bool> { false, false }, new ArrayList { 1,2 }) as List<Dictionary<string, object>>;

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Count, Is.EqualTo(2));
                Assert.That(result[0]["property1"], Is.EqualTo(1));
                Assert.That(result[0]["property2"], Is.EqualTo(2));
            }

            [Test]
            public void WhenNoCollectionsWithLacingDataOfSameSize_ReturnsDictionaryMatchingData()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1", "property2" }, new List<bool> { false, false }, new ArrayList { new ArrayList { 1, 3 }, new ArrayList { 2, 4 } }) as List<Dictionary<string, object>>;

                Assert.That(result.Count, Is.EqualTo(2));
                Assert.That(result[0].Count, Is.EqualTo(2));
                Assert.That(result[0]["property1"], Is.EqualTo(1));
                Assert.That(result[0]["property2"], Is.EqualTo(2));
                Assert.That(result[1].Count, Is.EqualTo(2));
                Assert.That(result[1]["property1"], Is.EqualTo(3));
                Assert.That(result[1]["property2"], Is.EqualTo(4));
            }

            [Test]
            public void WhenNoCollectionsWithLacingOfDifferentSize_ReturnsDictionaryWithDuplicateDataForShortestArray()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1", "property2" }, new List<bool> { false, false }, new ArrayList { new ArrayList { 1, 3, 5 }, new ArrayList { 2, 4 } }) as List<Dictionary<string, object>>;

                Assert.That(result.Count, Is.EqualTo(3));
                Assert.That(result[0].Count, Is.EqualTo(2));
                Assert.That(result[0]["property1"], Is.EqualTo(1));
                Assert.That(result[0]["property2"], Is.EqualTo(2));
                Assert.That(result[1].Count, Is.EqualTo(2));
                Assert.That(result[1]["property1"], Is.EqualTo(3));
                Assert.That(result[1]["property2"], Is.EqualTo(4));
                Assert.That(result[2].Count, Is.EqualTo(2));
                Assert.That(result[2]["property1"], Is.EqualTo(5));
                Assert.That(result[2]["property2"], Is.EqualTo(4));
            }

            [Test]
            public void WhenDataHasCollections_ReturnsDictionaryMatchingData()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1", "property2" }, new List<bool> { true, false }, new ArrayList { new ArrayList { 1, 3, 5 }, 10 }) as List<Dictionary<string, object>>;

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0]["property1"], Is.EqualTo(new ArrayList { 1, 3, 5 }));
                Assert.That(result[0]["property2"], Is.EqualTo(10));
            }

            [Test]
            public void WhenDataHasCollectionsWithLacingOfSameSize_ReturnsDictionaryMatchingData()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1", "property2" }, new List<bool> { true, false }, new ArrayList { new ArrayList { new ArrayList { 1, 3, 5 }, new ArrayList { 2, 4, 6 } }, new ArrayList { 10, 15 } }) as List<Dictionary<string, object>>;

                Assert.That(result.Count, Is.EqualTo(2));
                Assert.That(result[0]["property1"], Is.EqualTo(new ArrayList { 1, 3, 5 }));
                Assert.That(result[0]["property2"], Is.EqualTo(10));
                Assert.That(result[1]["property1"], Is.EqualTo(new ArrayList { 2, 4, 6 }));
                Assert.That(result[1]["property2"], Is.EqualTo(15));
            }

            [Test]
            public void WhenDataHasCollectionsWithLacingOfDifferntSize_ReturnsDictionaryWithDuplicatedShortestValue()
            {
                var result = PackFunctions.PackOutputAsDictionary(new List<string> { "property1", "property2" }, new List<bool> { true, false }, new ArrayList { new ArrayList { new ArrayList { 1, 3, 5 }, new ArrayList { 2, 4, 6 } }, new ArrayList { 10, 15, 20 } }) as List<Dictionary<string, object>>;

                Assert.That(result.Count, Is.EqualTo(3));
                Assert.That(result[0]["property1"], Is.EqualTo(new ArrayList { 1, 3, 5 }));
                Assert.That(result[0]["property2"], Is.EqualTo(10));
                Assert.That(result[1]["property1"], Is.EqualTo(new ArrayList { 2, 4, 6 }));
                Assert.That(result[1]["property2"], Is.EqualTo(15));
                Assert.That(result[2]["property1"], Is.EqualTo(new ArrayList { 2, 4, 6 }));
                Assert.That(result[2]["property2"], Is.EqualTo(20));
            }
        }
    }
}

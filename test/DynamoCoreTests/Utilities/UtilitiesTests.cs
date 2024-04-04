using System;
using System.Collections;
using System.Collections.Generic;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
    /// <summary>
    /// Test class to test the classes in the Utilities folder
    /// </summary>
    [TestFixture]
    public class UtilitiesTests : DynamoModelTestBase
    {
        private class TestResource
        {
            public static string TestString { get; set; }
            public static string TestSecondString { get; set; }
            public static int TestInt { get; set; }
        }

        /// <summary>
        /// Tests the "string Load(Type, string)" method of the ResourceLoader class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ResourceLoaderLoadOneTest()
        {
            //Happy path (null is the positive outcome)
            Assert.IsNull(ResourceLoader.Load(typeof(TestResource), "TestString"));

            //Empty string when one of the parameters is null (returns string.Empty for one or both parameters)
            Assert.AreEqual(string.Empty, ResourceLoader.Load(null, ""));

            //  Exceptions
            //Asking for a property that does not exist
            Assert.Throws<InvalidOperationException>(() => ResourceLoader.Load(typeof(TestResource), "Test"));
            
            //Passing a non-string property
            Assert.Throws<InvalidOperationException>(() => ResourceLoader.Load(typeof(TestResource), "TestInt"));
        }

        /// <summary>
        /// Tests the static "IEnumerable<string> Load(Type, IEnumerable<string>)" method of the ResourceLoader class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ResourceLoaderLoadManyTest()
        {
            //Happy path
            string[] resourceNames = { "TestString", "TestSecondString" };
            Assert.IsNotEmpty(ResourceLoader.Load(typeof(TestResource), resourceNames));

            //Empty string when one of the parameters is null
            Assert.AreEqual(new List<string>(), ResourceLoader.Load(null, resourceNames));

            //Asking for a property that does not exist
            resourceNames = new string[] { "Test", "Test2" };
            Assert.Throws<InvalidOperationException>(() => ResourceLoader.Load(typeof(TestResource), "Test"));

            //Passing a non-string property
            resourceNames = new string[] { "TestInt" };
            Assert.Throws<InvalidOperationException>(() => ResourceLoader.Load(typeof(TestResource), "TestInt"));
        }
        /// <summary>
        /// Test the Hash class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void HashTest()
        {
            var testStr = "Test";

            var filename = Hash.GetHashFilenameFromString(testStr);

            Assert.AreEqual("KMXKVPMVOSEA3P3WXG4MYAEDFQQKN3ARHVUCFGKVBV5G4DZULYSQ", filename);
        }
    }
}

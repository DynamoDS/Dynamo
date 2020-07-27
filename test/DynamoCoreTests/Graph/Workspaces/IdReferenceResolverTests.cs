using System;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Class containing tests for the IdReferenceResolver class
    /// </summary>
    [TestFixture]
    class IdReferenceResolverTests : DynamoModelTestBase
    {
        private IdReferenceResolver idRefResolver;
        private object testingObject;

        [SetUp]
        public void Init()
        {
            idRefResolver = new IdReferenceResolver();
            testingObject = new object();
        }

        #region ReferenceModelMap Tests

        [Test]
        [Category("UnitTests")]
        public void AddAndResolveReferenceFromMapTest()
        {
            //Guid to use as reference
            Guid guid = Guid.NewGuid();
            var reference = guid.ToString();

            //Adds new reference to the map
            idRefResolver.AddToReferenceMap(guid, testingObject);

            //Resolves the reference
            var result = idRefResolver.ResolveReferenceFromMap(null, reference);

            Assert.AreEqual(testingObject, result);
        }

        [Test]
        [Category("UnitTests")]
        public void AddReferenceFromMapTestCaseDuplicatedGuid()
        {
            //Guid to use as reference
            Guid guid = Guid.NewGuid();
            var reference = guid.ToString();

            //Adds new reference to the map
            idRefResolver.AddToReferenceMap(guid, testingObject);

            //Adds new object with same guid
            var newObj = new object();
            var ex = Assert.Throws<InvalidOperationException>(() => idRefResolver.AddToReferenceMap(guid, newObj));

            //Checks that not only the exception type is correct but also the message
            Assert.AreEqual(@"the map already contains a model with this id, the id must
                    be unique for the workspace that is currently being deserialized: " + guid, ex.Message);
        }

        [Test]
        [Category("UnitTests")]
        public void ResolveReferenceFromMapTestCaseInvalidGuid()
        {
            //Invalid Guid to use as reference
            string reference = "Invalid Guid";

            //Resolves the reference
            var result = idRefResolver.ResolveReferenceFromMap(null, reference);

            Assert.IsNull(result);
        }

        #endregion

        #region ReferenceModel Tests

        [Test]
        [Category("UnitTests")]
        public void AddAndResolveReferenceTest()
        {
            //Guid to use as reference
            Guid guid = Guid.NewGuid();
            var reference = guid.ToString();

            //Adds new reference to the map
            idRefResolver.AddReference(null, reference, testingObject);

            //Resolves the reference
            var result = idRefResolver.ResolveReference(null, reference);

            Assert.AreEqual(testingObject, result);
        }

        [Test]
        [Category("UnitTests")]
        public void AddReferenceCaseDuplicatedGuid()
        {
            //Guid to use as reference
            Guid guid = Guid.NewGuid();
            var reference = guid.ToString();

            //Adds new reference to the map
            idRefResolver.AddReference(null, reference, testingObject);

            //Adds new object with same guid
            var newObj = new object();
            var ex = Assert.Throws<InvalidOperationException>(() => idRefResolver.AddReference(null, reference, newObj));

            //Checks that not only the exception type is correct but also the message
            Assert.AreEqual(@"the map already contains a model with this id, the id must
                    be unique for the workspace that is currently being deserialized :" + guid, ex.Message);
        }

        [Test]
        [Category("UnitTests")]
        public void ResolveReferenceTestCaseInvalidGuid()
        {
            //Invalid Guid to use as reference
            string reference = "Invalid Guid";

            //Resolves the reference
            var result = idRefResolver.ResolveReference(null, reference);

            Assert.IsNull(result);
        }

        #endregion
    }
}
using Dynamo.Annotations;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
    /// <summary>
    /// Test class to test the classes in the Properties folder
    /// </summary>
    [TestFixture]
    public class PropertiesTests : DynamoModelTestBase
    {
        /// <summary>
        /// Tests for the NotifyPropertyChangedInvocatorAttributeTest class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void NotifyPropertyChangedInvocatorAttributeTest()
        {
            var testObj = new NotifyPropertyChangedInvocatorAttribute();
            Assert.IsNull(testObj.ParameterName);

            testObj = new NotifyPropertyChangedInvocatorAttribute("ParameterName");
            Assert.AreEqual("ParameterName", testObj.ParameterName);
        }
    }
}
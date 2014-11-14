using System;

using NUnit.Framework;

using TestServices;

namespace SampleLibraryZeroTouch
{
    // --------------------------------------------------
    // NOTES ON GEOMETRY TESTING:
    //
    // In order to use the Dynamo geometry library during 
    // testing, the geometry library needs to be 
    // initialized. If you inherit from the 
    // GeometricTestBase class the initialization of the
    // geometry library will be handled for you. 

    // In order to initialize the geometry library, Dynamo
    // needs to know where your base install of Dynamo is
    // located. Typically, a zero touch library doesn't 
    // need to know anything about Dynamo. But, to 
    // load the goemetry library we look in paths relative
    // to your core Dynamo installation to find binaries
    // that are required by the geometry library. To allow
    // you to build your library anywhere on your machine
    // we include a TestServices.dll.config file in the 
    // output directory. You need to set the "value" 
    // attribute in that config file to the base location 
    // of Dynamo.

    // If you are making a library that does not utilize 
    // Dynamo's geometry library, then you do not need to 
    // inherit from GeometricTestBase, and you do not need
    // to supply a config file with the core Dynamo install
    // location.
    
    [TestFixture]
    class HelloDynamoZeroTouchTests : GeometricTestBase
    {
        [Test]
        public void ConstructHelloDynamoZeroTouchObject_DefaultArgs()
        {
            var myObject = HelloDynamoZeroTouch.ByCoordinates();
            Assert.NotNull(myObject);
        }

        [Test]
        public void ConstructHelloDynamoZeroTouchObject_ValidArgs()
        {
            var myObject = HelloDynamoZeroTouch.ByCoordinates(5, 5, 5);
            Assert.NotNull(myObject);
        }

        [Test]
        public void ConstructHelloDynamoZeroTouchObject_BadArgs()
        {
            Assert.Throws<ArgumentException>(()=>HelloDynamoZeroTouch.ByCoordinates(-5, -5, -5));
        }
    }
}

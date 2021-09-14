using Dynamo.Logging;
using Microsoft.Win32;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Tests.Loggings
{
    [TestFixture]
    class StabilityTrackingTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the IsLastShutdownClean() and also the Get from the StabilityUtils.IsLastShutdownClean property
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void test_stability_cookie_stability_shutdown()
        {
            //Arrange
            bool stabilityLastShutdown = true; //The last shutdown time was successfull

            //Act
            StabilityCookie.WriteCrashingShutdown(); //Simulate that the DynamoApp crashed
            StabilityCookie.Startup();//Inside this method will call IsLastShutdownClean()
            stabilityLastShutdown = StabilityUtils.IsLastShutdownClean; //Execute the Get method of the property

            //Assert
            //Because we called WriteCrashingShutdown() method means that it wasn't a clean shutdown 
            Assert.IsFalse(stabilityLastShutdown);

        }
    }
}

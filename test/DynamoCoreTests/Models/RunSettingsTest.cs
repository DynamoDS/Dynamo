using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class RunSettingsTest
    {
        /// <summary>
        /// This test method will execute the RunSettings constructor without parameters
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void RunSettingsConstructorTest()
        {
            //Act
            //Executing the constructor without parameters
            var runSettings = new RunSettings();

            //Assert
            //Validate that the default properties values are set when using the parameterless constructor
            Assert.AreEqual(runSettings.RunPeriod, RunSettings.DefaultRunPeriod);
            Assert.AreEqual(runSettings.RunType, RunType.Manual);
            Assert.AreEqual(runSettings.RunEnabled, true);
        }
    }
}

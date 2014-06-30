using System;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.ApplicationModel;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    [TestClass]
    public class MeasurementConfigurationTests
    {
        [TestMethod]
        public void MeasurementConfiguration_Constructor_With_Required_Parameters_Sets_Correct_Properties()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4");

            Assert.AreEqual("UA-1234-5", configuration.AccountId);
            Assert.AreEqual("ApplicationName", configuration.ApplicationName);
            Assert.AreEqual("1.2.3.4", configuration.ApplicationVersion);
        }

        [TestMethod]
        public void MeasurementConfiguration_Constructor_With_Required_Parameters_Sets_Correct_Defaults()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4");

            Assert.IsTrue(configuration.AnonymizeIp);
            Assert.IsFalse(configuration.UseSsl);
            Assert.AreEqual(100.0, configuration.SampleRate);
        }

        [TestMethod]
        public void MeasurementConfiguration_SampleRate_Property_Can_Be_Set()
        {
            var expected = 51.2;
            var configuration = new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4") {
                SampleRate = expected
            };

            Assert.AreEqual(expected, configuration.SampleRate);
        }

#if WINDOWS_STORE || WINDOWS_PHONE
        [TestMethod]
        public void MeasurementConfiguration_Constructor_Throws_ArgumentException_If_AccountID_Does_Not_Start_With_UA()
        {
            Assert.ThrowsException<ArgumentException>(() => new MeasurementConfiguration("NO-1234-5", "ApplicationName", "1.2.3.4"));
        }

        [TestMethod]
        public void MeasurementConfiguration_Constructor_Throws_ArgumentException_If_AccountID_Does_Not_Have_Two_Numeric_Parts()
        {
            Assert.ThrowsException<ArgumentException>(() => new MeasurementConfiguration("UA-1234", "ApplicationName", "1.2.3.4"));
        }

        [TestMethod]
        public void MeasurementConfiguration_SampleRate_Property_Throws_ArgumentOutOfRangeException_If_Below_0()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => configuration.SampleRate = -0.01);
        }

        [TestMethod]
        public void MeasurementConfiguration_SampleRate_Property_Throws_ArgumentOutOfRangeException_If_Above_100()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => configuration.SampleRate = 100.01);
        }
#endif

#if WINDOWS_STORE
        [TestMethod]
        public void MeasurementConfiguration_FormatVersion_Formats_Version_Correctly()
        {
            var version = new PackageVersion { Major = 4, Minor = 3, Build = 2, Revision = 1 };

            var actual = MeasurementConfiguration.FormatVersion(version);

            Assert.AreEqual("4.3.2.1", actual);
        }
#endif

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CustomVariable_Constructor_Throws_ArgumentOutOfRange_If_Enum_Undefined()
        {
            new MeasurementConfiguration("NO-1234-5", "ApplicationName", "1.2.3.4");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MeasurementConfiguration_Constructor_Throws_ArgumentException_If_AccountID_Does_Not_Have_Two_Numeric_Parts()
        {
            new MeasurementConfiguration("UA-1234", "ApplicationName", "1.2.3.4");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MeasurementConfiguration_SampleRate_Property_Throws_ArgumentOutOfRangeException_If_Below_0()
        {
            new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4") { SampleRate = -0.01 };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void MeasurementConfiguration_SampleRate_Property_Throws_ArgumentOutOfRangeException_If_Above_100()
        {
            new MeasurementConfiguration("UA-1234-5", "ApplicationName", "1.2.3.4") { SampleRate = 100.01 };
        }
#endif
    }
}
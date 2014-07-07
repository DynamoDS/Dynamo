using System;
using CSharpAnalytics.Debugging;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols
{
    [TestClass]
    public class ParameterDefinitionTests
    {
        [TestMethod]
        public void ParameterDefinition_Constructor_Sets_Required_Properties()
        {
            var parameterDefinition = new ParameterDefinition("name", "label");

            Assert.AreEqual("name", parameterDefinition.Name);
            Assert.AreEqual("label", parameterDefinition.Label);
        }

        [TestMethod]
        public void ParameterDefinition_Constructor_Sets_Optional_Properties()
        {
            Func<string, string> formatter = s => s + "great";
            var parameterDefinition = new ParameterDefinition("name", "label", formatter, true);

            Assert.AreEqual(true, parameterDefinition.IsRegexMatch);
            Assert.AreEqual(formatter, parameterDefinition.Formatter);
            Assert.AreEqual("so great", parameterDefinition.Formatter("so "));
        }

        [TestMethod]
        public void ParameterDefinition_Constructor_Sets_Default_Formatter()
        {
            var parameterDefinition = new ParameterDefinition("name", "label");

            Assert.IsNotNull(parameterDefinition.Formatter);
            Assert.AreEqual("testing", parameterDefinition.Formatter("testing"));
        }
    }
}
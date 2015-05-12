using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dynamo.Tests
{
    public class PackageValidationTest : DSEvaluationViewModelUnitTest
    {
        protected override string GetUserDataDirectory()
        {
            string userDataDirectory = Path.Combine(TestDirectory, @"core\userdata");
            return userDataDirectory;
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }
        
        [Test]
        public void TestLoadPackageFromOtherLocation()
        {
            var testFilePath = Path.Combine(TestDirectory, @"core\userdata\packageTest.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("5e44d891-d2bb-4188-9f20-6ced9b430b4f", new object[] { 1,2,3}); 
        }
    }
}

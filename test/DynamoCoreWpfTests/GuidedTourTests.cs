using System.IO;
using Dynamo.Wpf.UI.GuidedTour;
using NUnit.Framework;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class GuidedTourTests : SystemTestBase
    {
        [Test]
        public void CanReadGuidedTourJsonFile()
        {
            //Initialize the GuideManager to null to later we can verify that it was created correctly
            GuidesManager testGuide = null;

            //Check that the directory location where the assembly is created already exists
            string failMessageDirectory = string.Format("The assembly directory path doesn't exits: {0}", GuidesManager.GuidesExecutingDirectory);
            Assert.IsTrue(Directory.Exists(GuidesManager.GuidesExecutingDirectory), failMessageDirectory);

            //Check that the Guides Json file location exists
            string failMessageFile = string.Format("The guides JSON file: {0} couldn't be located or doesn't exits", GuidesManager.GuidesJsonFilePath);
            Assert.IsTrue(File.Exists(GuidesManager.GuidesJsonFilePath), failMessageFile);

            //Creates the Guides Manager passing the Dynamo Window and ViewModel and validates that no exception is thrown
            Assert.DoesNotThrow( () => testGuide = new GuidesManager(View._this, ViewModel));
            
            //Initializes variables and reads json properties
            Assert.DoesNotThrow( () => testGuide.Initialize());

            //Validate that the Guides were read correctly from the json file
            Assert.NotNull(testGuide);
            Assert.NotNull(testGuide.Guides);
            Assert.That(testGuide.Guides.Count > 0,"No guides read from the JSON file");

            //Validates that each Guide read from the Json file has at least one step
            foreach(var guide in testGuide.Guides)
            {
                Assert.That(guide.GuideSteps.Count > 0, "No steps read from the JSON file"); 
            }
        }
    }
}

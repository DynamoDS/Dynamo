using System.IO;
using System.Linq;
using Dynamo.Controls;
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
            Assert.DoesNotThrow( () => testGuide = new GuidesManager(View, ViewModel));
            
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

        /// <summary>
        /// This test will validate that all the control names read from the json file for the GetStarted Guide exists in DynamoView
        /// </summary>
        [Test]
        public void GetStartedGuide_ValidateControlNames()
        {
            //Initialize the GuideManager to null to later we can verify that it was created correctly
            GuidesManager testGuide = null;

            string failMessageFile = string.Format("The guides JSON file: {0} couldn't be located or doesn't exits", GuidesManager.GuidesJsonFilePath);
            Assert.IsTrue(File.Exists(GuidesManager.GuidesJsonFilePath), failMessageFile);

            //Creates the Guides Manager passing the Dynamo Window and ViewModel and validates that no exception is thrown
            Assert.DoesNotThrow(() => testGuide = new GuidesManager(View, ViewModel));

            //Initializes variables and reads json properties
            Assert.DoesNotThrow(() => testGuide.Initialize());

            //Find the Get Started guide in the list
            var getStartedGuide = (from guide in testGuide.Guides 
                                  where guide.Name.Equals(GuidesManager.GetStartedGuideName)
                                  select guide).FirstOrDefault();

            //Validate that the GetStarted guide exists
            Assert.IsNotNull(getStartedGuide,"Get Started guide doesn't exists in the guides json file");

            string elementNotFoundMessage = string.Empty;
            foreach (var step in getStartedGuide.GuideSteps)
            {
                //Validate that for each Step, the HostUIElementString element exists in the Dynamo VisualTree
                var childFound = GuideUtilities.FindChild(View, step.HostPopupInfo.HostUIElementString);
                elementNotFoundMessage = string.Format("The UIElement: {0} was not found in the DynamoView VisualTree", step.HostPopupInfo.HostUIElementString);
                Assert.IsNotNull(childFound, elementNotFoundMessage);
            }
        }

        /// <summary>
        /// This test will validate that all the control names read from the json file for the Onboard Guide exists in DynamoView
        /// </summary>
        [Test]
        public void OnboardingGuideFirstStep_ValidateTexts()
        {
            //Initialize the GuideManager to null to later we can verify that it was created correctly
            GuidesManager testGuide = null;

            string failMessageFile = string.Format("The guides JSON file: {0} couldn't be located or doesn't exits", GuidesManager.GuidesJsonFilePath);
            Assert.IsTrue(File.Exists(GuidesManager.GuidesJsonFilePath), failMessageFile);

            //Creates the Guides Manager passing the Dynamo Window and ViewModel and validates that no exception is thrown
            Assert.DoesNotThrow(() => testGuide = new GuidesManager(View, ViewModel));

            //Initializes variables and reads json properties
            Assert.DoesNotThrow(() => testGuide.LaunchTour(GuidesManager.OnboardingGuideName));

            //Find the Get Started guide in the list
            var OnboardingGuide = (from guide in testGuide.Guides
                                   where guide.Name.ToUpper().Equals(GuidesManager.OnboardingGuideName.ToUpper())
                                   select guide).FirstOrDefault();

            //Validate that the GetStarted guide exists
            Assert.IsNotNull(OnboardingGuide, "Onboarding guide doesn't exists in the guides json file");

            Assert.IsTrue(OnboardingGuide.GuideSteps.Count > 1);
            Assert.IsTrue(OnboardingGuide.GuideSteps[0].StepUIPopup.IsOpen);
        }
    }
}

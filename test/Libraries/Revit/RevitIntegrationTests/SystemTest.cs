using System.IO;
using System.Reflection;

using DynamoUtilities;

using RevitServices.Persistence;

using RevitTestServices;

namespace RevitSystemTests
{
    /// <summary>
    /// SystemTest is a Dynamo-specific subclass of the
    /// RevitSystemTestBase class. 
    /// </summary>
    public class SystemTest : RevitSystemTestBase
    {
        protected string samplesPath;
        protected string defsPath;
        protected string emptyModelPath1;
        protected string emptyModelPath;

        public override void SetupCore()
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;

            //get the test path
            string testsLoc = Path.Combine(assDir, @"..\..\..\..\test\System\revit\");
            workingDirectory = Path.GetFullPath(testsLoc);

            //get the samples path
            string samplesLoc = Path.Combine(assDir, @"..\..\..\..\doc\distrib\Samples\");
            samplesPath = Path.GetFullPath(samplesLoc);

            //set the custom node loader search path
            string defsLoc = Path.Combine(DynamoPathManager.Instance.Packages, "Dynamo Sample Custom Nodes", "dyf");
            defsPath = Path.GetFullPath(defsLoc);

            emptyModelPath = Path.Combine(workingDirectory, "empty.rfa");

            if (DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber.Contains("2014") &&
                DocumentManager.Instance.CurrentUIApplication.Application.VersionName.Contains("Vasari"))
            {
                emptyModelPath = Path.Combine(workingDirectory, "emptyV.rfa");
                emptyModelPath1 = Path.Combine(workingDirectory, "emptyV1.rfa");
            }
            else
            {
                emptyModelPath = Path.Combine(workingDirectory, "empty.rfa");
                emptyModelPath1 = Path.Combine(workingDirectory, "empty1.rfa");
            }
        }

        public void OpenModel(string relativeFilePath)
        {
            string samplePath = Path.Combine(samplesPath, relativeFilePath);
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
        }
    }
}

using System.IO;
using System.Reflection;

using DynamoUtilities;

using RevitServices.Persistence;

namespace RevitSystemTests
{
    public class SystemTest : RevitTestServices.SystemTestBase
    {
        protected string _samplesPath;
        protected string _defsPath;
        protected string _emptyModelPath1;
        protected string _emptyModelPath;

        protected override void SetupCore()
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;

            //get the test path
            string testsLoc = Path.Combine(assDir, @"..\..\..\..\test\System\revit\");
            workingDirectory = Path.GetFullPath(testsLoc);

            //get the samples path
            string samplesLoc = Path.Combine(assDir, @"..\..\..\..\doc\distrib\Samples\");
            _samplesPath = Path.GetFullPath(samplesLoc);

            //set the custom node loader search path
            string defsLoc = Path.Combine(DynamoPathManager.Instance.Packages, "Dynamo Sample Custom Nodes", "dyf");
            _defsPath = Path.GetFullPath(defsLoc);

            _emptyModelPath = Path.Combine(workingDirectory, "empty.rfa");

            if (DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber.Contains("2014") &&
                DocumentManager.Instance.CurrentUIApplication.Application.VersionName.Contains("Vasari"))
            {
                _emptyModelPath = Path.Combine(workingDirectory, "emptyV.rfa");
                _emptyModelPath1 = Path.Combine(workingDirectory, "emptyV1.rfa");
            }
            else
            {
                _emptyModelPath = Path.Combine(workingDirectory, "empty.rfa");
                _emptyModelPath1 = Path.Combine(workingDirectory, "empty1.rfa");
            }
        }

        public void OpenModel(string relativeFilePath)
        {
            string samplePath = Path.Combine(_samplesPath, relativeFilePath);
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
        }
    }
}

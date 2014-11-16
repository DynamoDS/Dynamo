using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
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

        /// <summary>
        /// This function gets all the family instances in the current Revit document
        /// </summary>
        /// <param name="startNewTransaction">whether do the filtering in a new transaction</param>
        /// <returns>the family instances</returns>
        protected static IList<Element> GetAllFamilyInstances(bool startNewTransaction)
        {
            if (startNewTransaction)
            {
                using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "FilteringElements"))
                {
                    trans.Start();

                    ElementClassFilter ef = new ElementClassFilter(typeof(FamilyInstance));
                    FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                    fec.WherePasses(ef);

                    trans.Commit();
                    return fec.ToElements();
                }
            }
            else
            {
                ElementClassFilter ef = new ElementClassFilter(typeof(FamilyInstance));
                FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                fec.WherePasses(ef);
                return fec.ToElements();
            }
        }
    }
}

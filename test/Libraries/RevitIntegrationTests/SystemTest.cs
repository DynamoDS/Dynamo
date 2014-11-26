using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Autodesk.Revit.DB;
using DynamoUtilities;
using RevitServices.Persistence;
using RevitTestServices;

namespace RevitSystemTests
{
    public class RevitTestConfiguration
    {
        /// <summary>
        /// Directory where test files are kept
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Directory where Samples are kept
        /// </summary>
        public string SamplesPath { get; set; }

        /// <summary>
        /// Directory where custom node definitions are kept
        /// </summary>
        public string DefinitionsPath { get; set; }

        /// <summary>
        /// TestConfiguration file name
        /// </summary>
        private const string TEST_CONFIGURATION_FILE_S = "RevitTestConfigurations.xml";

        /// <summary>
        /// Loads configuration
        /// </summary>
        /// <returns></returns>
        public static RevitTestConfiguration LoadConfiguration()
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            var config = new RevitTestConfiguration();
            try
            {
                var configPath = Path.Combine(assDir, TEST_CONFIGURATION_FILE_S);
                if (File.Exists(configPath))
                {
                    var serializer = new XmlSerializer(typeof(RevitTestConfiguration));
                    using (var fs = new FileStream(configPath, FileMode.Open, FileAccess.Read))
                    {
                        config = serializer.Deserialize(fs) as RevitTestConfiguration;
                    }
                }
#if DEBUG
                else
                {
                    config.SetDefaultValues();
                    config.Save(configPath);
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            config.SetDefaultValues();
            return config;
        }

        private void SetDefaultValues()
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;

            //get the test path
            if (string.IsNullOrEmpty(WorkingDirectory))
            {
                string testsLoc = Path.Combine(assDir, @"..\..\..\..\test\System\");
                WorkingDirectory = Path.GetFullPath(testsLoc);
            }

            //get the samples path
            if (string.IsNullOrEmpty(SamplesPath))
            {
                string samplesLoc = Path.Combine(assDir, @"..\..\..\..\doc\distrib\Samples\");
                SamplesPath = Path.GetFullPath(samplesLoc);
            }

            //set the custom node loader search path
            if (string.IsNullOrEmpty(DefinitionsPath))
            {
                string defsLoc = Path.Combine(
                    DynamoPathManager.Instance.Packages,
                    "Dynamo Sample Custom Nodes",
                    "dyf");
                DefinitionsPath = Path.GetFullPath(defsLoc);
            }
        }

        private void Save(string filePath)
        {
            var serializer = new XmlSerializer(typeof(RevitTestConfiguration));
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(fs, this);
            }
        }
    }

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
            var config = RevitTestConfiguration.LoadConfiguration();

            //get the test path
            workingDirectory = config.WorkingDirectory;

            //get the samples path
            samplesPath = config.SamplesPath;

            //set the custom node loader search path
            defsPath = config.DefinitionsPath;

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

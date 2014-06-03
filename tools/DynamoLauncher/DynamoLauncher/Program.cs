using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DynamoLauncher.Properties;

namespace DynamoLauncher
{
    class Program
    {
        private static readonly string pluginGuid = "16abab2f-fe6c-4a10-b7f2-475728f66831";
        private static readonly string pluginClass = "Dynamo.Applications.VersionLoader";
        
        [STAThread]
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("The argument to the Dynamo Launcher executable should be a folder.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
                return;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("The specified folder does not exist.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
                return;
            }
            var dirInfo = new DirectoryInfo(args[0]);

            var files = dirInfo.GetFiles();
            
            // Check that we have one Revit file and one .dyn file.
            if (files.Count() < 2)
            {
               Console.WriteLine("The specified folder does not contain a model file and a dyn file.");
               Console.WriteLine("Press any key to quit.");
               Console.ReadKey();
                return;
            }

            var modelPath = "";
            var dynPath = "";
            var modelFile = files.FirstOrDefault(f => f.Extension == ".rvt" || f.Extension == ".rfa");
            var dynFile = files.FirstOrDefault(f => f.Extension == ".dyn");

            if (modelFile != null)
            {
                modelPath = modelFile.FullName;
            }
            else
            {
                Console.WriteLine("There doesn't seem to be a .rvt or .rfa file in your folder.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
                return;
            }

            if (dynFile != null)
            {
                dynPath = dynFile.FullName;
            }
            else
            {
                Console.WriteLine("There doesn't seem to be a .dyn file in your folder.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
                return;
            }

            // Get the revit path and the dynamo assembly path from the settings file
            // if they don't exist, present a file open dialog to select them.
            SetDefaultRevitPath();
            SetDefaultDynamoAssemblyPath();

            // Double check that revit path and assembly path are set
            if(string.IsNullOrEmpty(modelPath) || 
                string.IsNullOrEmpty(dynPath) ||
                string.IsNullOrEmpty(Settings.Default.assemblyPath) ||
                string.IsNullOrEmpty(Settings.Default.revitPath))
            {
                Console.WriteLine("One or more of your inputs was invalid.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadKey();
                return;
            }

            //Console.WriteLine(modelPath);
            //Console.WriteLine(dynPath);
            //Console.WriteLine(Settings.Default.assemblyPath);
            //Console.WriteLine(Settings.Default.revitPath);
            //Console.ReadKey();

            // Write the journal and the addin into the same
            // directory as you are passing in for the test
            var journalPath = Path.Combine(dirInfo.FullName, dirInfo.Name + ".txt");
            var addinPath = Path.Combine(dirInfo.FullName, "Dynamo.addin");

            CreateAddin(addinPath, pluginGuid, pluginClass, Settings.Default.assemblyPath);
            CreateJournal(journalPath, modelPath, dynPath);

            var startInfo = new ProcessStartInfo()
            {
                FileName = Settings.Default.revitPath,
                Arguments = journalPath,
                UseShellExecute = false
            };

            Console.WriteLine("Running {0}", journalPath);
            var process = new Process { StartInfo = startInfo };
            process.Start();

            Settings.Default.Save();
        }

        private static void SetDefaultDynamoAssemblyPath()
        {
            if (string.IsNullOrEmpty(Settings.Default.assemblyPath) || !File.Exists(Settings.Default.assemblyPath))
            {
                // Present the open file dialogue and 
                var openFileDialog1 = new OpenFileDialog
                {
                    InitialDirectory = "c:\\",
                    Filter = "dll files (*.dll)|*.dll",
                    RestoreDirectory = true,
                    Title = "Set the Dynamo assembly path."
                };

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.assemblyPath = openFileDialog1.FileName;
                }
            }
        }

        private static void SetDefaultRevitPath()
        {
            if (string.IsNullOrEmpty(Settings.Default.revitPath) || !File.Exists(Settings.Default.revitPath))
            {
                // Present the open file dialogue and 
                var openFileDialog1 = new OpenFileDialog
                {
                    InitialDirectory = "c:\\",
                    Filter = "exe files (*.exe)|*.exe",
                    RestoreDirectory = true,
                    Title="Set the Revit path."
                };

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.revitPath = openFileDialog1.FileName;
                }
            }
        }

        private static void CreateJournal(string path, string modelPath, string dynPath)
        {
            using (var tw = new StreamWriter(path, false))
            {
                var journal = String.Format(@"'" +
                                            "Dim Jrn \n" +
                                            "Set Jrn = CrsJournalScript \n" +
                                            "Jrn.Command \"StartupPage\" , \"Open this project , ID_FILE_MRU_FIRST\" \n" +
                                            "Jrn.Data \"MRUFileName\"  , \"{0}\" \n" +
                                            //"Jrn.RibbonEvent \"Execute external command:{1}:{2}\" \n" +
                                            "Jrn.RibbonEvent \"Execute external command:CustomCtrl_%CustomCtrl_%Add-Ins%Visual Programming Alpha%Dynamo 0.7 Alpha:Dynamo.Applications.DynamoRevit\"\n"+
                                            "Jrn.Data \"APIStringStringMapJournalData\", 2, \"dynPath\", \"{1}\", \"debug\", \"True\"",
                    modelPath, dynPath);

                tw.Write(journal);
                tw.Flush();
            }
        }

        private static void CreateAddin(string addinPath, string pluginGuid, string pluginClass, string assemblyPath)
        {
            using (var tw = new StreamWriter(addinPath, false))
            {
                var addin = String.Format(
                    "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>\n" +
                    "<RevitAddIns>\n" +
                    "<AddIn Type=\"Application\">\n" +
                    "<Name>Dynamo</Name>\n" +
                    "<Assembly>\"{0}\"</Assembly>\n" +
                    "<AddInId>{1}</AddInId>\n" +
                    "<FullClassName>{2}</FullClassName>\n" +
                    "<VendorId>ADSK</VendorId>\n" +
                    "<VendorDescription>Autodesk</VendorDescription>\n" +
                    "</AddIn>\n" +
                    "</RevitAddIns>",
                    assemblyPath, pluginGuid, pluginClass
                    );

                /*<?xml version="1.0" encoding="utf-8" standalone="no"?>
                <RevitAddIns>
                  <AddIn Type="Application">
                    <Name>Dynamo For Revit</Name>
                    <Assembly>C:\Users\Ian\Documents\GitHub\Dynamo\bin\AnyCPU\Debug\DynamoRevitVersionSelector.dll</Assembly>
                    <AddInId>8D83C886-B739-4ACD-A9DB-1BC78F315B2B</AddInId>
                    <FullClassName>Dynamo.Applications.VersionLoader</FullClassName>
                  <VendorId>ADSK</VendorId>
                  <VendorDescription>Autodesk, github.com/ikeough/dynamo</VendorDescription>
                  </AddIn>
                </RevitAddIns>*/

                tw.Write(addin);
                tw.Flush();
            }
        }
    }
}

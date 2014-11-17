using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Dynamo.Applications
{
    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class VersionLoader : IExternalApplication
    {
        internal static string BasePath = @"C:\Autodesk\Dynamo\Core";
        internal static string BetaPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public Result OnStartup(UIControlledApplication application)
        {
            var versions = new List<string>();

            // default to loading 0.6.x
            // if no version of 0.6.x is found, then load 0.7.0
            var loadPath = String.Empty;

            var path06x = Path.Combine(BasePath, "DynamoRevit.dll");
            var path07x = Path.Combine(BetaPath, "DynamoRevitDS.dll");

            if (File.Exists(path06x))
            {
                loadPath = path06x;
                var vi = FileVersionInfo.GetVersionInfo(path06x);
                versions.Add(string.Format("{0}.{1}.{2}",vi.FileMajorPart,vi.FileMinorPart,vi.FileBuildPart));
            }

            if (File.Exists(path07x))
            {
                loadPath = path07x;
                var vi = FileVersionInfo.GetVersionInfo(path07x);
                versions.Add(string.Format("{0}.{1}.{2}", vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart));
            }

            // If there are multiple versions installed, then create
            // a couple of push buttons in a panel to allow selection of a version.
            // If only one version is installed, no multi-selection is required.
            if (versions.Count > 1)
            {
                RibbonPanel ribbonPanel = application.CreateRibbonPanel("Dynamo Version");

                var pushButton06x = new PushButtonData(
                                "Dynamo06x",
                                String.Format("Dynamo {0}", versions[0]),
                                Assembly.GetExecutingAssembly().Location,
                                "Dynamo.Applications.Start06x");

                var pushButton07x = new PushButtonData(
                                "Dynamo07x",
                                String.Format("Dynamo {0}", versions[1]),
                                Assembly.GetExecutingAssembly().Location,
                                "Dynamo.Applications.Start07x");

                ribbonPanel.AddStackedItems(pushButton06x, pushButton07x);
            }

            // now we have a default path, but let's look at
            // the load path file to see what was last selected
            var cachedPath = String.Empty;
            var fileLoc = Utils.GetVersionSaveFileLocation(
                application.ControlledApplication.VersionName);

            if (File.Exists(fileLoc))
            {
                using (var sr = new StreamReader(fileLoc))
                {
                    cachedPath = sr.ReadToEnd().TrimEnd('\r', '\n');
                }
            }

            if (File.Exists(cachedPath))
            {
                loadPath = cachedPath;
            }

            if (String.IsNullOrEmpty(loadPath))
                return Result.Failed;

            var ass = Assembly.LoadFrom(loadPath);
            var revitApp = ass.CreateInstance("Dynamo.Applications.DynamoRevitApp");
            revitApp.GetType().GetMethod("OnStartup").Invoke(revitApp, new object[] { application });

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }

    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class Start06x : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var loadPath = Path.Combine(VersionLoader.BasePath, "DynamoRevit.dll");

            Utils.WriteToFile(loadPath, commandData.Application.Application.VersionName);

            Utils.ShowRestartMessage(FileVersionInfo.GetVersionInfo(loadPath).FileVersion);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class Start07x : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var loadPath = Path.Combine(VersionLoader.BetaPath, "DynamoRevitDS.dll");

            Utils.WriteToFile(loadPath, commandData.Application.Application.VersionName);

            Utils.ShowRestartMessage(FileVersionInfo.GetVersionInfo(loadPath).FileVersion);

            return Result.Succeeded;
        }
    }

    internal class Utils
    {
        internal static void WriteToFile(string loadPath, string versionName)
        {
            var path = GetVersionSaveFileLocation(versionName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var tw = new StreamWriter(path))
            {
                tw.WriteLine(loadPath);
            }
        }

        /// <summary>
        /// Return PreferenceSettings Default XML File Path if possible
        /// </summary>
        internal static string GetVersionSaveFileLocation(string versionName)
        {
            try
            {
                string appDataFolder = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData);

                return (Path.Combine(appDataFolder, "Dynamo","0.7", string.Format("DynamoDllForLoad_{0}.txt", versionName)));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        internal static void ShowRestartMessage(string version)
        {
            MessageBox.Show(string.Format("Dynamo version {0} will be loaded after Revit restart.", version),
                "Dynamo Version", MessageBoxButton.OK);
        }
    }
}

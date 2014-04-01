using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Dynamo.Applications;

namespace DynamoRevitVersionSelector
{
    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitVersionSelector : IExternalApplication
    {
        private static string basePath = @"C:\Autodesk\Dynamo\Core";
        private static string debugPath = @"C:\Users\Ian\Documents\Dynamo\bin\AnyCPU\Debug";

        public ObservableCollection<string> Versions { get; set; }

        public Result OnStartup(UIControlledApplication application)
        {
            Versions = new ObservableCollection<string> { "0.6.3", "0.7.0" };

            var versionSelector = new DynamoVersionSelector {DataContext = this, VersionDropDown = {SelectedIndex = 0}};
            versionSelector.ShowDialog();

            Assembly ass = null;

            if (versionSelector.VersionDropDown.SelectedIndex == 0)
            {
                var assPath = Path.Combine(basePath, "DynamoRevit.dll");
                ass = Assembly.LoadFrom(assPath);
            }
            else if (versionSelector.VersionDropDown.SelectedIndex == 1)
            {
                var assPath = Path.Combine(debugPath, "DynamoRevitDS.dll");
                ass = Assembly.LoadFrom(assPath);
            }

            var revitApp = ass.CreateInstance("Dynamo.Applications.DynamoRevitApp");
            revitApp.GetType().GetMethod("OnStartup").Invoke(revitApp, new object[] { application });

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Dynamo.FSchemeInterop;
using Dynamo.NUnit.Tests;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;

using System.ServiceModel;
using System.ServiceModel.Description;

using Dynamo;
using Dynamo.Applications.Properties;
using Dynamo.Controls;
using Dynamo.Utilities;
using RevitServices.Elements;
using RevitServices.Threading;

namespace Dynamo.Revit.Server
{

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class ServerApp : IExternalApplication
    {
        public static ServiceHost Host { get; private set; }
        public static UIControlledApplication UiControlledApp { get; private set; }

        public Result OnStartup(UIControlledApplication application)
        {
            return StartAsync(application);
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return TryStop();
        }

        private Result StartAsync(UIControlledApplication application)
        {
            Task.Run(() => Start(application));
            return Result.Succeeded;
        }

        private void Start(UIControlledApplication application)
        {
            // this is required to make sure that we load LibG
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;

            UiControlledApp = application;
            Host = new ServiceHost(typeof(DynamoRevitService), new Uri("http://localhost:8000/DynamoRevitService/"));

            try
            {
                Host.AddServiceEndpoint(typeof (IDynamoRevitService), new BasicHttpBinding(),
                    "DynamoRevitService");

                var smb = new ServiceMetadataBehavior {HttpGetEnabled = true};
                Host.Description.Behaviors.Add(smb);

                IdlePromise.RegisterIdle(application);

                Host.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Host.Abort();
            }
        }

        private Result TryStop()
        {
            try
            {
                Host.Close();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }
    }

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class StartCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            try
            {
                // assign static members
                dynRevitSettings.Revit = revit.Application;
                dynRevitSettings.Doc = revit.Application.ActiveUIDocument;
                dynRevitSettings.DefaultLevel = GetDefaultLevel(revit.Application.ActiveUIDocument.Document);

                //// start controller
                //var updater = new RevitServicesUpdater(ServerApp.UiControlledApp.ControlledApplication);
                //var context = GetRevitContext(revit.Application.Application);
                //new DynamoController_Revit(new ExecutionEnvironment(),
                //                            updater,
                //                            typeof(DynamoRevitViewModel),
                //                            context);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public Level GetDefaultLevel(Document doc)
        {
            var fecLevel = new FilteredElementCollector(doc);
            fecLevel.OfClass(typeof (Level));
            return fecLevel.ToElements()[0] as Level;
        }

        public string GetRevitContext(Autodesk.Revit.ApplicationServices.Application app)
        {
            var r = new Regex(@"\b(Autodesk |Structure |MEP |Architecture )\b");
            string context = r.Replace(app.VersionName, "");

            if (context == "Vasari")
                context = "Vasari 2014";

            return context;
        }

    }

}

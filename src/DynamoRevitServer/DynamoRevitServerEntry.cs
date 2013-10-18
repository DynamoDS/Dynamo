using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RevitServices;
using RevitServices.Elements;
using RevitServices.Threading;
using RevitServices.Transactions;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;

using System.ServiceModel;
using System.ServiceModel.Description;

namespace DynamoRevitServer
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoRevitServerApp : IExternalApplication
    {
        ServiceHost selfHost;

        public Result OnStartup(UIControlledApplication application)
        {
            Task.Run(() =>
            {

                Uri baseAddress = new Uri("http://localhost:8000/DynamoRevitService/");
                selfHost = new ServiceHost(typeof(DynamoRevitService), baseAddress);

                try
                {
                    var endPoint = selfHost.AddServiceEndpoint(typeof(IDynamoRevitService), new BasicHttpBinding(), "DynamoRevitService");
                    Console.WriteLine(endPoint.Address.Uri);

                    var smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = true;
                    selfHost.Description.Behaviors.Add(smb);

                    IdlePromise.RegisterIdle(application);

                    selfHost.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    selfHost.Abort();
                }

            });

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                selfHost.Close();
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return Result.Failed;
            }
        }
    }

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DynamoRevitServerCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            try
            {
                DynamoRevitService.Application = revit.Application;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }

            return Result.Succeeded;
        }

    }

}

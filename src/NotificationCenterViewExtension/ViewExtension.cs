using Dynamo.Models;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationCenterViewExtension
{
    public class ViewExtension : IViewExtension
    {
        public static readonly string ExtensionName = "NotificationUI - WebView2";

        public string UniqueId { 
            get { return "2c24fadf-38c0-47fb-8b7d-18e6c3053754"; } 
        }

        public string Name => ExtensionName;

        public void Dispose()
        {
            System.GC.SuppressFinalize(this);
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (!DynamoModel.IsTestMode)
            {

            }
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            throw new NotImplementedException();
        }
    }
}

using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.PrettyException
{
    public class PrettyExceptionExtension : IViewExtension
    {
        public string Name
        {
            get
            {
                return "PrettyPrintExceptionHandler";
            }
        }

        public string UniqueId
        {
            get
            {
                return "ef6cd025-514f-44cd-b6b1-69d9f5cce004";
            }
        }

        public void Dispose()
        {
           // UnregisterEventHandlers();
        }

        public void Loaded(ViewLoadedParams p)
        {
            //this is hacky, but I do not want to further disturb our public API.
            var data = (p.DynamoWindow.DataContext as DynamoViewModel).Model.PreloadData;
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Startup(ViewStartupParams p)
        {

        }
    }
}

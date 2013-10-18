using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;

using RevitServices.Threading;

namespace DynamoRevitServer
{
    public class DynamoRevitService : IDynamoRevitService
    {
        public static UIApplication Application { get; set; }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public bool OpenFile(string path)
        {
            IdlePromise.ExecuteOnIdleAsync( () => Application.OpenAndActivateDocument(path) );
            return true;
        }
    }
}

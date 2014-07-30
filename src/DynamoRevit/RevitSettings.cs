#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Dynamo.Revit;
using Dynamo.ViewModels;

using RevitServices.Persistence;

#endregion

namespace Dynamo.Utilities
{
    internal class dynUtils
    {


    }

    public static class dynRevitSettings
    {
        public static Level DefaultLevel { get; set; }
        public static DynamoWarningSwallower WarningSwallower { get; set; }
        public static DynamoViewModel Controller { get; internal set; }

    }
}

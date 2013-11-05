using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Xml;
using Autodesk.Revit.DB;
using DSCoreNodes;
using Dynamo.Models;
using Dynamo.UI.Prompts;
using Microsoft.FSharp.Collections;

using Dynamo.Utilities;
using Value = Dynamo.FScheme.Value;
using Dynamo.Controls;
using Curve = Autodesk.Revit.DB.Curve;
using Vector = Autodesk.LibG.Vector;

namespace Dynamo.Nodes
{
    public abstract class MeasurementBase:NodeWithOneOutput
    {
        protected MeasurementBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }
}



using System.Globalization;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Units;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Applications
{
    public class RevitWatchHandler : IWatchHandler
    {
        internal WatchNode ProcessThing(Element element, string tag, bool showRawData = true)
        {
            var id = element.Id;

            var node = new WatchNode(element.Name);
            node.Clicked += () => dynRevitSettings.Doc.ShowElements(element);
            node.Link = id.IntegerValue.ToString(CultureInfo.InvariantCulture);

            return node;
        }

        internal WatchNode ProcessThing(XYZ pt, string tag, bool showRawData = true)
        {
            var um = dynSettings.Controller.UnitsManager;

            var node = new WatchNode();

            if (!showRawData)
            {
                ///xyzs will be in feet, but we need to show them
                ///in the display units of choice
                /// 

                var xyzStr = string.Format("{0:f3}, {1:f3}, {2:f3}",
                    new Units.Length(pt.X / SIUnit.ToFoot, um),
                    new Units.Length(pt.Y / SIUnit.ToFoot, um),
                    new Units.Length(pt.Z / SIUnit.ToFoot, um));

                node.NodeLabel = "{" + xyzStr + "}";
            }
            else
            {
                node.NodeLabel = pt.ToString();
            }

            return node;
        }

        internal WatchNode ProcessThing(object value, string tag, bool showRawData = true)
        {
            var node = new WatchNode(value.ToString(), tag);
            return node;
        }

        internal WatchNode ProcessThing(SIUnit unit, string tag, bool showRawData = true)
        {
            if (showRawData)
                return new WatchNode(unit.Value.ToString(CultureInfo.InvariantCulture), tag);

            return new WatchNode(unit.ToString(), tag);
        }

        internal WatchNode ProcessThing(double value, string tag, bool showRawData = true)
        {
            return new WatchNode(value.ToString("0.000"), tag);
        }

        internal WatchNode ProcessThing(string value, string tag, bool showRawData = true)
        {
            return new WatchNode(value, tag);
        }

        public WatchNode Process(dynamic value, string tag, bool showRawData = true)
        {
            return ProcessThing(value, tag, showRawData);
        }
    }
}

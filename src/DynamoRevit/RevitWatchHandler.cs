using System.Globalization;
using Dynamo.Interfaces;
using Dynamo.Units;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using ProtoCore.Mirror;
using RevitServices.Persistence;
using Element = Revit.Elements.Element;

namespace Dynamo.Applications
{
    /// <summary>
    /// An Revit-specific implementation of IWatchHandler that is set on the Controller at startup.
    /// The main Process method dynamically dispatches to the appropriate
    /// internal method based on the type. For every time for which you would like
    /// to have a custom representation in the watch, you will need an additional
    /// method on this handler
    /// 
    /// NOTE:
    /// Many of these methods duplicate those found in the DefaultWatchHandler.
    /// As such, this class should extend DefaultWatchHandler. However, because the processsing
    /// methods are dynamically dispatched, it doesn't play nicely with inheritance and these
    /// methods have to be duplicated here.
    /// </summary>
    public class RevitWatchHandler : IWatchHandler
    {
        internal WatchViewModel ProcessThing(Element element, string tag, bool showRawData = true)
        {
            var id = element.Id;

            var node = new WatchViewModel(element.ToString(dynSettings.Controller.PreferenceSettings.NumberFormat, CultureInfo.InvariantCulture), tag);
            node.Clicked += () => DocumentManager.Instance.CurrentUIDocument.ShowElements(element.InternalElement);
            node.Link = id.ToString(CultureInfo.InvariantCulture);

            return node;
        }

        internal WatchViewModel ProcessThing(object value, string tag, bool showRawData = true)
        {
            var node = new WatchViewModel(ToString(value), tag);
            return node;
        }

        internal WatchViewModel ProcessThing(SIUnit unit, string tag, bool showRawData = true)
        {
            if (showRawData)
                return new WatchViewModel(unit.Value.ToString(dynSettings.Controller.PreferenceSettings.NumberFormat, CultureInfo.InvariantCulture), tag);

            return new WatchViewModel(unit.ToString(), tag);
        }

        internal WatchViewModel ProcessThing(double value, string tag, bool showRawData = true)
        {
            return new WatchViewModel(value.ToString(dynSettings.Controller.PreferenceSettings.NumberFormat, CultureInfo.InvariantCulture), tag);
        }

        internal WatchViewModel ProcessThing(string value, string tag, bool showRawData = true)
        {
            return new WatchViewModel(value, tag);
        }

        internal WatchViewModel ProcessThing(MirrorData data, string tag, bool showRawData = true)
        {
            //If the input data is an instance of a class, create a watch node
            //with the class name and let WatchHandler process the underlying CLR data
            var classMirror = data.Class;
            if (null != classMirror)
            {
                if (data.Data == null && !data.IsNull) //Must be a DS Class instance.
                    return ProcessThing(classMirror.ClassName, tag); //just show the class name.
                return ProcessThing(data.Data as dynamic, tag, showRawData);
            }

            //Finally for all else get the string representation of data as watch content.
            return ProcessThing(data.Data as dynamic, tag);
        }

        private string ToString(object obj)
        {
            return obj != null ? obj.ToString() : "null";
        }

        public WatchViewModel Process(dynamic value, string tag, bool showRawData = true)
        {
            if(value == null)
                return new WatchViewModel("null", tag);

            return ProcessThing(value, tag, showRawData);
        }
    }
}

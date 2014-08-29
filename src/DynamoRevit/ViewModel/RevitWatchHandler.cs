using System;
using System.Collections;
using System.Globalization;
using System.Linq;

using Dynamo.Interfaces;
using Dynamo.ViewModels;
using DynamoUnits;
using ProtoCore.Mirror;
using RevitServices.Persistence;
using Element = Revit.Elements.Element;

namespace Dynamo.Applications
{
    /// <summary>
    /// An Revit-specific implementation of IWatchHandler that is set on the DynamoViewModel at startup.
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
        private const string NULL_STRING = "null";

        private readonly IVisualizationManager visualizationManager;
        private readonly IPreferences preferences;

        public RevitWatchHandler(IVisualizationManager vizManager, IPreferences prefs)
        {
            preferences = prefs;
            visualizationManager = vizManager;
        }

        internal WatchViewModel ProcessThing(Element element, string tag, bool showRawData = true)
        {
            var id = element.Id;

            var node = new WatchViewModel(visualizationManager, 
                element.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture), tag);

            node.Clicked += () =>
            {
                if (element.InternalElement.IsValidObject)
                    DocumentManager.Instance.CurrentUIDocument.ShowElements(element.InternalElement);
            };

            node.Link = id.ToString(CultureInfo.InvariantCulture);

            return node;
        }

        internal WatchViewModel ProcessThing(object value, string tag, bool showRawData = true)
        {
            WatchViewModel node;

            if (value is IEnumerable)
            {
                var list = (value as IEnumerable).Cast<dynamic>().ToList();

                node = new WatchViewModel(visualizationManager, list.Count == 0 ? "Empty List" : "List", tag, true);
                foreach (var e in list.Select((element, idx) => new { element, idx }))
                {
                    node.Children.Add(Process(e.element, tag + ":" + e.idx, showRawData));
                }
            }
            else
            {
                node = new WatchViewModel(visualizationManager, ToString(value), tag);
            }

            return node;
        }

        internal WatchViewModel ProcessThing(SIUnit unit, string tag, bool showRawData = true)
        {
            if (showRawData)
                return new WatchViewModel(visualizationManager, 
                    unit.Value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture), 
                    tag);

            return new WatchViewModel(visualizationManager, unit.ToString(), tag);
        }

        internal WatchViewModel ProcessThing(double value, string tag, bool showRawData = true)
        {
            return new WatchViewModel(visualizationManager, value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture), tag);
        }

        internal WatchViewModel ProcessThing(string value, string tag, bool showRawData = true)
        {
            return new WatchViewModel(visualizationManager, value, tag);
        }

        internal WatchViewModel ProcessThing(MirrorData data, string tag, bool showRawData = true)
        {
            try
            {
                if (data.IsCollection)
                {
                    var list = data.GetElements();

                    var node = new WatchViewModel(visualizationManager, list.Count == 0 ? "Empty List" : "List", tag, true);
                    foreach (var e in list.Select((element, idx) => new { element, idx }))
                    {
                        node.Children.Add(ProcessThing(e.element, tag + ":" + e.idx, showRawData));
                    }

                    return node;
                }

                // MAGN-3494: If "data.Data" is null, then return a "null" string 
                // representation instead of casting it as dynamic (that leads to 
                // a crash).
                if (data.IsNull || data.Data == null)
                    return new WatchViewModel(visualizationManager, NULL_STRING, tag);

                //If the input data is an instance of a class, create a watch node
                //with the class name and let WatchHandler process the underlying CLR data
                var classMirror = data.Class;
                if (null != classMirror)
                {
                    if (data.Data == null && !data.IsNull) //Must be a DS Class instance.
                        return ProcessThing(classMirror.ClassName, tag); //just show the class name.
                    return Process(data.Data, tag, showRawData);
                }

                //Finally for all else get the string representation of data as watch content.
                return Process(data.Data, tag, showRawData);
            }
            catch (Exception)
            {
                return ProcessThing(data.Data, tag, showRawData);
            }
            
        }

        private static string ToString(object obj)
        {
            return obj != null ? obj.ToString() : "null";
        }

        public WatchViewModel Process(dynamic value, string tag, bool showRawData = true)
        {
            return Object.ReferenceEquals(value, null)
                ? new WatchViewModel(visualizationManager, "null", tag)
                : ProcessThing(value, tag, showRawData);
        }
    }
}

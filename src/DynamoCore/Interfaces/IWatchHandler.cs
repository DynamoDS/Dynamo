using System;
using System.Collections;
using System.Globalization;
using System.Linq;

using Dynamo.ViewModels;
using DynamoUnits;
using ProtoCore.Mirror;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// An object implementing the IWatchHandler interface is registered 
    /// on the ViewModel at startup, and defines the methods for processing
    /// objects into string representations for visualizing in the Watch.
    /// To create a custom watch visualization scheme, you can create a simply
    /// replace this WatchHandler with one of your own creation.
    /// </summary>
    public interface IWatchHandler
    {
        WatchViewModel Process(dynamic value, string tag, bool showRawData = true);
    }

    /// <summary>
    /// The default watch handler.
    /// </summary>
    public class DefaultWatchHandler : IWatchHandler
    {
        private const string NULL_STRING = "null";

        private readonly IPreferences preferences;
        private readonly IVisualizationManager visualizationManager;

        public DefaultWatchHandler(IVisualizationManager manager, PreferenceSettings preferences)
        {
            visualizationManager = manager;
            this.preferences = preferences;
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
            return showRawData
                ? new WatchViewModel(
                    visualizationManager,
                    unit.Value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture),
                    tag)
                : new WatchViewModel(visualizationManager, unit.ToString(), tag);
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

        private static string ToString(object obj)
        {
            return ReferenceEquals(obj, null)
                ? "null"
                : (obj is bool ? obj.ToString().ToLower() : obj.ToString());
        }

        public WatchViewModel Process(dynamic value, string tag, bool showRawData = true)
        {
            return Object.ReferenceEquals(value, null)
                ? new WatchViewModel(visualizationManager, "null", tag)
                : ProcessThing(value, tag, showRawData);
        }
    }
}

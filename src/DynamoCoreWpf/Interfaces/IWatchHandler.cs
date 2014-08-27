using System.Collections;
using System.Globalization;

using Dynamo.Models;
using Dynamo.Utilities;
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
        private readonly IPreferences preferences;
        private readonly IVisualizationManager vizManager;

        public DefaultWatchHandler(IVisualizationManager manager, PreferenceSettings preferences)
        {
            this.vizManager = manager;
            this.preferences = preferences;
        }

        internal WatchViewModel ProcessThing(object value, string tag, bool showRawData = true)
        {
            WatchViewModel node;

            if (value is IEnumerable)
            {
                node = new WatchViewModel(vizManager, "List", tag);

                var enumerable = value as IEnumerable;
                foreach (var obj in enumerable)
                {
                    node.Children.Add(ProcessThing(obj, tag));
                }
            }
            else
            {
                node = new WatchViewModel(vizManager, ToString(value), tag);
            }

            return node;
        }

        internal WatchViewModel ProcessThing(SIUnit unit, string tag, bool showRawData = true)
        {
            if (showRawData)
                return new WatchViewModel(vizManager, unit.Value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture), tag);

            return new WatchViewModel(vizManager, unit.ToString(), tag);
        }

        internal WatchViewModel ProcessThing(double value, string tag, bool showRawData = true)
        {
            return new WatchViewModel(vizManager, value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture), tag);
        }

        internal WatchViewModel ProcessThing(string value, string tag, bool showRawData = true)
        {
            return new WatchViewModel(vizManager, value, tag);
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

            // MAGN-3494: If "data.Data" is null, then return a "null" string 
            // representation instead of casting it as dynamic (that leads to 
            // a crash).
            if (data.Data == null)
                return new WatchViewModel(vizManager, "null", tag);

            //Finally for all else get the string representation of data as watch content.
            return ProcessThing(data.Data as dynamic, tag, showRawData);
        }

        private static string ToString(object obj)
        {
            return obj != null ? obj.ToString() : "null";
        }

        public WatchViewModel Process(dynamic value, string tag, bool showRawData = true)
        {
            if (System.Object.ReferenceEquals(value, null))
                return new WatchViewModel(vizManager, "null", tag);

            return ProcessThing(value, tag, showRawData);
        }
    }
}

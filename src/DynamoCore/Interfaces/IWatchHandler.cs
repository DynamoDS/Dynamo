using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dynamo.Nodes;
using Dynamo.Units;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using ProtoCore.Mirror;

namespace Dynamo.Interfaces
{
    /// <summary>
    /// An object implementing the IWatchHandler interface is registered 
    /// on the controller at startup, and defines the methods for processing
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
        internal WatchViewModel ProcessThing(object value, string tag, bool showRawData = true)
        {
            WatchViewModel node;

            if (value is IEnumerable<object>)
            {
                node = new WatchViewModel("List", tag);

                var enumerable = value as IEnumerable<object>;
                var objects = enumerable as object[] ?? enumerable.ToArray();
                if (objects.Any())
                {
                    foreach (var obj in objects)
                    {
                        node.Children.Add(ProcessThing(obj, tag));
                    }
                }
            }
            else
            {
                node = new WatchViewModel(value.ToString(), tag);
            }

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
                return ProcessThing(data.Data, tag);
            }

            //Finally for all else get the string representation of data as watch content.
            string previewData = data.Data.ToString();
            return new WatchViewModel(previewData, tag);
        }

        public WatchViewModel Process(dynamic value, string tag, bool showRawData = true)
        {
            if(value == null)
                return new WatchViewModel("null", tag);

            return ProcessThing(value, tag, showRawData);
        }
    }
}

using System.Globalization;
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
        WatchItem Process(dynamic value, string tag, bool showRawData = true);
    }

    /// <summary>
    /// The default watch handler.
    /// </summary>
    public class DefaultWatchHandler : IWatchHandler
    {
        internal WatchItem ProcessThing(object value, string tag, bool showRawData = true)
        {
            var node = new WatchItem(value.ToString(), tag);
            return node;
        }

        internal WatchItem ProcessThing(SIUnit unit, string tag, bool showRawData = true)
        {
            if (showRawData)
                return new WatchItem(unit.Value.ToString(dynSettings.Controller.PreferenceSettings.NumberFormat, CultureInfo.InvariantCulture), tag);

            return new WatchItem(unit.ToString(), tag);
        }

        internal WatchItem ProcessThing(double value, string tag, bool showRawData = true)
        {
            return new WatchItem(value.ToString(dynSettings.Controller.PreferenceSettings.NumberFormat, CultureInfo.InvariantCulture), tag);
        }

        internal WatchItem ProcessThing(string value, string tag, bool showRawData = true)
        {
            return new WatchItem(value, tag);
        }

        internal WatchItem ProcessThing(MirrorData data, string tag, bool showRawData = true)
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
            return new WatchItem(previewData, tag);
        }

        public WatchItem Process(dynamic value, string tag, bool showRawData = true)
        {
            if(value == null)
                return new WatchItem("null");

            return ProcessThing(value, tag, showRawData);
        }
    }
}

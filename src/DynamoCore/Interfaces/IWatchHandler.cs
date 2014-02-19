using System.Globalization;
using Dynamo.Units;
using Dynamo.ViewModels;

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
                return new WatchItem(unit.Value.ToString(CultureInfo.InvariantCulture), tag);

            return new WatchItem(unit.ToString(), tag);
        }

        internal WatchItem ProcessThing(double value, string tag, bool showRawData = true)
        {
            return new WatchItem(value.ToString("0.000"), tag);
        }

        internal WatchItem ProcessThing(string value, string tag, bool showRawData = true)
        {
            return new WatchItem(value, tag);
        }

        public WatchItem Process(dynamic value, string tag, bool showRawData = true)
        {
            return ProcessThing(value, tag, showRawData);
        }
    }
}

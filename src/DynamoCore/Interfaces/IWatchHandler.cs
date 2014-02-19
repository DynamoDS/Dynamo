using System.Globalization;
using Dynamo.Units;
using Dynamo.ViewModels;

namespace Dynamo.Interfaces
{
    public interface IWatchHandler
    {
        WatchNode Process(dynamic value, string tag, bool showRawData = true);
    }

    /// <summary>
    /// The default watch handler.
    /// </summary>
    public class DefaultWatchHandler : IWatchHandler
    {
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

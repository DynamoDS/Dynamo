using System;
using System.Collections;
using System.Collections.Generic;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tag"></param>
        /// <param name="showRawData"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        WatchViewModel Process(dynamic value, string tag, bool showRawData, WatchHandlerCallback callback);
    }

    public delegate WatchViewModel WatchHandlerCallback(dynamic value, string tag, bool showRawData);

    public static class WatchHandler
    {
        public static WatchViewModel GenerateWatchViewModelForData(this IWatchHandler handler, dynamic value, string tag, bool showRawData = true)
        {
            return handler.Process(value, tag, showRawData, new WatchHandlerCallback(handler.GenerateWatchViewModelForData));
        }
    }

    /// <summary>
    ///     The default watch handler.
    /// </summary>
    public class DefaultWatchHandler : IWatchHandler
    {
        public const string NULL_STRING = "null";

        private readonly IPreferences preferences;
        private readonly IVisualizationManager visualizationManager;

        public DefaultWatchHandler(IVisualizationManager manager, IPreferences preferences)
        {
            visualizationManager = manager;
            this.preferences = preferences;
        }

        private WatchViewModel ProcessThing(object value, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            WatchViewModel node;

            if (value is IEnumerable)
            {
                var list = (value as IEnumerable).Cast<dynamic>().ToList();

                node = new WatchViewModel(visualizationManager, list.Count == 0 ? "Empty List" : "List", tag, true);
                foreach (var e in list.Select((element, idx) => new { element, idx }))
                {
                    node.Children.Add(callback(e.element, tag + ":" + e.idx, showRawData));
                }
            }
            else
            {
                node = new WatchViewModel(visualizationManager, ToString(value), tag);
            }

            return node;
        }

        private WatchViewModel ProcessThing(SIUnit unit, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return showRawData
                ? new WatchViewModel(
                    visualizationManager,
                    unit.Value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture),
                    tag)
                : new WatchViewModel(visualizationManager, unit.ToString(), tag);
        }

        private WatchViewModel ProcessThing(double value, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(visualizationManager, value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture), tag);
        }

        private WatchViewModel ProcessThing(string value, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(visualizationManager, value, tag);
        }

        private WatchViewModel ProcessThing(MirrorData data, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            if (data.IsCollection)
            {
                var list = data.GetElements();

                var node = new WatchViewModel(visualizationManager, list.Count == 0 ? "Empty List" : "List", tag, true);
                foreach (var e in list.Select((element, idx) => new { element, idx }))
                {
                    node.Children.Add(ProcessThing(e.element, tag + ":" + e.idx, showRawData, callback));
                }

                return node;
            }

            if (data.Data == null)
            {
                // MAGN-3494: If "data.Data" is null, then return a "null" string 
                // representation instead of casting it as dynamic (that leads to 
                // a crash).
                if (data.IsNull)
                    return new WatchViewModel(visualizationManager, NULL_STRING, tag);
                
                //If the input data is an instance of a class, create a watch node
                //with the class name and let WatchHandler process the underlying CLR data
                var classMirror = data.Class;
                if (null != classMirror)
                {
                    //just show the class name.
                    return ProcessThing(classMirror.ClassName, tag, showRawData, callback);
                }
            }

            //Finally for all else get the string representation of data as watch content.
            return callback(data.Data, tag, showRawData);
        }

        private static string ToString(object obj)
        {
            return ReferenceEquals(obj, null)
                ? NULL_STRING
                : (obj is bool ? obj.ToString().ToLower() : obj.ToString());
        }

        public WatchViewModel Process(dynamic value, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return Object.ReferenceEquals(value, null)
                ? new WatchViewModel(visualizationManager, NULL_STRING, tag)
                : ProcessThing(value, tag, showRawData, callback);
        }
    }
}

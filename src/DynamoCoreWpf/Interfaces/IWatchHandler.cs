using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Extensions;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using DynamoUnits;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using ProtoCore.Utils;

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
        event Action<string> RequestSelectGeometry;
        WatchViewModel Process(dynamic value, IEnumerable<string> preferredDictionaryOrdering, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback);
    }

    public delegate WatchViewModel WatchHandlerCallback(dynamic value, IEnumerable<string> preferredDictionaryOrdering, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData);

    public static class WatchHandler
    {
        public static WatchViewModel GenerateWatchViewModelForData(this IWatchHandler handler, dynamic value,
            IEnumerable<string> preferredDictionaryOrdering, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData = true)
        {
            return handler.Process(value, preferredDictionaryOrdering, runtimeCore, tag, showRawData, new WatchHandlerCallback(handler.GenerateWatchViewModelForData));

        }
    }

    public class DefaultWatchHandler : IWatchHandler
    {
        // Formats double value into string. E.g. 1054.32179 => "1054.32179"
        // For more info: https://msdn.microsoft.com/en-us/library/kfsatb94(v=vs.110).aspx
        private const string numberFormat = "g";
        private readonly IPreferences preferences;

        public DefaultWatchHandler(IPreferences preferences)
        {
            this.preferences = preferences;
        }

        private WatchViewModel ProcessThing(object value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback, List<string> preferredDictionaryOrdering = null)
        {
            if (value is DesignScript.Builtin.Dictionary || value is IDictionary)
            {
                IEnumerable<string> keys;
                IEnumerable<object> values;
                if (value is DesignScript.Builtin.Dictionary)
                {
                    var dict = value as DesignScript.Builtin.Dictionary;
                    keys = dict.Keys;
                    values = dict.Values;
                    if (preferredDictionaryOrdering != null && preferredDictionaryOrdering.Count > 1)
                    {
                        keys = preferredDictionaryOrdering;
                        values = keys.Select(k => dict.ValueAtKey(k));
                    }
                }
                else
                {
                    var dict = value as IDictionary;
                    keys = dict.Keys.Cast<string>();
                    values = dict.Values.Cast<object>();
                }

                var node = new WatchViewModel(keys.Any() ? WatchViewModel.DICTIONARY : WatchViewModel.EMPTY_DICTIONARY, tag, RequestSelectGeometry, true);

                foreach (var e in keys.Zip(values, (key, val) => new { key, val }))
                {
                    node.Children.Add(ProcessThing(e.val, runtimeCore, tag + ":" + e.key, showRawData, callback));
                }

                return node;
            }

            if (!(value is string) && value is IEnumerable)
            {
                var list = (value as IEnumerable).Cast<dynamic>().ToList();

                var node = new WatchViewModel(list.Count == 0 ? WatchViewModel.EMPTY_LIST : WatchViewModel.LIST, tag, RequestSelectGeometry, true);
                foreach (var e in list.Select((element, idx) => new { element, idx }))
                {
                    node.Children.Add(callback(e.element, null, runtimeCore, tag + ":" + e.idx, showRawData));
                }

                return node;
            }

            if (runtimeCore != null && value is StackValue)
            {
                StackValue stackValue = (StackValue)value;
                string stringValue = string.Empty;

                if (stackValue.IsFunctionPointer)
                {
                    stringValue = StringUtils.GetStringValue(stackValue, runtimeCore);
                }
                else
                {
                    int typeId = runtimeCore.DSExecutable.TypeSystem.GetType(stackValue);
                    stringValue = runtimeCore.DSExecutable.classTable.ClassNodes[typeId].Name;
                }
                return new WatchViewModel(stringValue, tag, RequestSelectGeometry);
            }

            if (value is Enum)
            {
                return new WatchViewModel(((Enum)value).GetDescription(), tag, RequestSelectGeometry);
            }

            return new WatchViewModel(ToString(value), tag, RequestSelectGeometry);
        }

        private WatchViewModel ProcessThing(SIUnit unit, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return showRawData
                ? new WatchViewModel(
                    unit.Value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture),
                    tag, RequestSelectGeometry)
                : new WatchViewModel(unit.ToString(), tag, RequestSelectGeometry);
        }

        private WatchViewModel ProcessThing(double value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(value.ToString(numberFormat, CultureInfo.InvariantCulture), tag, RequestSelectGeometry);
        }

        private WatchViewModel ProcessThing(System.DateTime value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(value.ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture), tag, RequestSelectGeometry);
        }

        private WatchViewModel ProcessThing(long value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(value.ToString(CultureInfo.InvariantCulture), tag, RequestSelectGeometry);
        }

        private WatchViewModel ProcessThing(string value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(value, tag, RequestSelectGeometry);
        }

        private WatchViewModel ProcessThing(MirrorData data, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback, List<string> preferredDictionaryOrdering = null)
        {
            if (data.IsCollection)
            {
                var list = data.GetElements();

                var node = new WatchViewModel(!list.Any() ? WatchViewModel.EMPTY_LIST : WatchViewModel.LIST, tag, RequestSelectGeometry, true);
                foreach (var e in list.Select((element, idx) => new { element, idx }))
                {
                    node.Children.Add(ProcessThing(e.element, runtimeCore, tag + ":" + e.idx, showRawData, callback));
                }

                return node;
            }

            if (data.IsPointer && data.IsDictionary)
            {
                var dict = data.Data as DesignScript.Builtin.Dictionary;

                var keys = dict.Keys;
                var values = dict.Values;
                if (preferredDictionaryOrdering != null && preferredDictionaryOrdering.Count > 1)
                {
                    keys = preferredDictionaryOrdering;
                    values = keys.Select(k => dict.ValueAtKey(k));
                }

                var node = new WatchViewModel(keys.Any() ? WatchViewModel.DICTIONARY : WatchViewModel.EMPTY_DICTIONARY, tag, RequestSelectGeometry, true);

                foreach (var e in keys.Zip(values, (key, value) => new { key, value }))
                {
                    node.Children.Add(ProcessThing(e.value, runtimeCore, tag + ":" + e.key, showRawData, callback));
                }

                return node;
            }

            if (data.Data is Enum)
            {
                return new WatchViewModel(((Enum)data.Data).GetDescription(), tag, RequestSelectGeometry);
            }

            if (data.Data == null)
            {
                // MAGN-3494: If "data.Data" is null, then return a "null" string 
                // representation instead of casting it as dynamic (that leads to 
                // a crash).
                if (data.IsNull)
                    return new WatchViewModel(Resources.NullString, tag, RequestSelectGeometry);
                
                //If the input data is an instance of a class, create a watch node
                //with the class name and let WatchHandler process the underlying CLR data
                var classMirror = data.Class;
                if (null != classMirror)
                {
                    //just show the class name.
                    return ProcessThing(classMirror.ClassName, runtimeCore, tag, showRawData, callback);
                }
            }

            //Finally for all else get the string representation of data as watch content.
            return callback(data.Data, null, runtimeCore, tag, showRawData);
        }

        private static string ToString(object obj)
        {
            return ReferenceEquals(obj, null)? 
                Resources.NullString
                : (obj is bool ? 
                    obj.ToString().ToLower()
                    : obj is double?
                        ((double)obj).ToString(CultureInfo.InvariantCulture)
                        : obj.ToString());
        }

        public WatchViewModel Process(dynamic value, IEnumerable<string> preferredDictionaryOrdering, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return System.Object.ReferenceEquals(value, null)
                ? new WatchViewModel(Resources.NullString, tag, RequestSelectGeometry)
                : ProcessThing(value, runtimeCore, tag, showRawData, callback, preferredDictionaryOrdering?.ToList());
        }

        public event Action<string> RequestSelectGeometry;
    }
}

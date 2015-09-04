﻿using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Dynamo.ViewModels;
using DynamoUnits;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using ProtoCore.Utils;
using Dynamo.Extensions;
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
        WatchViewModel Process(dynamic value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback);
    }

    public delegate WatchViewModel WatchHandlerCallback(dynamic value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData);

    public static class WatchHandler
    {
        public static WatchViewModel GenerateWatchViewModelForData(this IWatchHandler handler, dynamic value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData = true)
        {
            return handler.Process(value, runtimeCore, tag, showRawData, new WatchHandlerCallback(handler.GenerateWatchViewModelForData));
        }
    }

    public class DefaultWatchHandler : IWatchHandler
    {
        public const string NULL_STRING = "null";

        private readonly IPreferences preferences;

        public DefaultWatchHandler(IPreferences preferences)
        {
            this.preferences = preferences;
        }

        private WatchViewModel ProcessThing(object value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            WatchViewModel node;

            if (value is IEnumerable)
            {
                var list = (value as IEnumerable).Cast<dynamic>().ToList();

                node = new WatchViewModel(list.Count == 0 ? "Empty List" : "List", tag, true);
                foreach (var e in list.Select((element, idx) => new { element, idx }))
                {
                    node.Children.Add(callback(e.element, runtimeCore, tag + ":" + e.idx, showRawData));
                }
            }
            else if (runtimeCore != null && value is StackValue)
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
                    stringValue = runtimeCore.DSExecutable.classTable.ClassNodes[typeId].name;
                }
                node = new WatchViewModel(stringValue, tag);
            }
            else if (value is Enum)
            {
                return new WatchViewModel(((Enum)value).GetDescription(), tag);
            }
            else
            {
                node = new WatchViewModel(ToString(value), tag);
            }

            return node;
        }

        private WatchViewModel ProcessThing(SIUnit unit, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return showRawData
                ? new WatchViewModel(
                    unit.Value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture),
                    tag)
                : new WatchViewModel(unit.ToString(), tag);
        }

        private WatchViewModel ProcessThing(double value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(value.ToString(preferences.NumberFormat, CultureInfo.InvariantCulture), tag);
        }

        private WatchViewModel ProcessThing(DateTime value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(value.ToString(PreferenceSettings.DefaultDateFormat, CultureInfo.InvariantCulture), tag);
        }

        private WatchViewModel ProcessThing(long value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(value.ToString(CultureInfo.InvariantCulture), tag);
        }

        private WatchViewModel ProcessThing(string value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return new WatchViewModel(value, tag);
        }

        private WatchViewModel ProcessThing(MirrorData data, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            if (data.IsCollection)
            {
                var list = data.GetElements();

                var node = new WatchViewModel(list.Count == 0 ? "Empty List" : "List", tag, true);
                foreach (var e in list.Select((element, idx) => new { element, idx }))
                {
                    node.Children.Add(ProcessThing(e.element, runtimeCore, tag + ":" + e.idx, showRawData, callback));
                }

                return node;
            }
            if (data.Data is Enum)
            {
                return new WatchViewModel(((Enum)data.Data).GetDescription(), tag);
            }

            if (data.Data == null)
            {
                // MAGN-3494: If "data.Data" is null, then return a "null" string 
                // representation instead of casting it as dynamic (that leads to 
                // a crash).
                if (data.IsNull)
                    return new WatchViewModel(NULL_STRING, tag);
                
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
            return callback(data.Data, runtimeCore, tag, showRawData);
        }

        private static string ToString(object obj)
        {
            return ReferenceEquals(obj, null)
                ? NULL_STRING
                : (obj is bool ? obj.ToString().ToLower() : obj.ToString());
        }

        public WatchViewModel Process(dynamic value, ProtoCore.RuntimeCore runtimeCore, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return Object.ReferenceEquals(value, null)
                ? new WatchViewModel(NULL_STRING, tag)
                : ProcessThing(value, runtimeCore, tag, showRawData, callback);
        }
    }
}

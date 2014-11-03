using System;
using System.Globalization;

using Dynamo.Interfaces;
using Dynamo.ViewModels;

using ProtoCore.Mirror;
using RevitServices.Persistence;
using Element = Revit.Elements.Element;

namespace Dynamo.Applications
{
    /// <summary>
    ///     An Revit-specific implementation of IWatchHandler that is set on the DynamoViewModel at startup.
    ///     The main GenerateWatchViewModelForData method dynamically dispatches to the appropriate
    ///     internal method based on the type. For every time for which you would like
    ///     to have a custom representation in the watch, you will need an additional
    ///     method on this handler
    /// </summary>
    public class RevitWatchHandler : IWatchHandler
    {
        private readonly IWatchHandler baseHandler;
        private readonly IVisualizationManager visualizationManager;
        private readonly IPreferences preferences;

        public RevitWatchHandler(IVisualizationManager vizManager, IPreferences prefs)
        {
            baseHandler = new DefaultWatchHandler(vizManager, prefs);
            preferences = prefs;
            visualizationManager = vizManager;
        }

        private WatchViewModel ProcessThing(Element element, ProtoCore.Core core, string tag, bool showRawData, WatchHandlerCallback callback)
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

        //If no dispatch target is found, then invoke base watch handler.
        private WatchViewModel ProcessThing(object obj, ProtoCore.Core core, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return baseHandler.Process(obj, core, tag, showRawData, callback);
        }

        private WatchViewModel ProcessThing(MirrorData data, ProtoCore.Core core, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            try
            {
                return baseHandler.Process(data, core, tag, showRawData, callback);
            }
            catch (Exception)
            {
                return callback(data.Data, core, tag, showRawData);
            }
        }

        public WatchViewModel Process(dynamic value, ProtoCore.Core core, string tag, bool showRawData, WatchHandlerCallback callback)
        {
            return Object.ReferenceEquals(value, null)
                ? new WatchViewModel(visualizationManager, "null", tag)
                : ProcessThing(value, core, tag, showRawData, callback);
        }
    }
}

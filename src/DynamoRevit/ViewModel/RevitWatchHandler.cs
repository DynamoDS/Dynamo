using System;
using System.Collections;
using System.Globalization;
using System.Linq;

using Dynamo.Interfaces;
using Dynamo.ViewModels;
using DynamoUnits;
using ProtoCore.Mirror;
using RevitServices.Persistence;
using Element = Revit.Elements.Element;

namespace Dynamo.Applications
{
    /// <summary>
    ///     An Revit-specific implementation of IWatchHandler that is set on the DynamoViewModel at startup.
    ///     The main Process method dynamically dispatches to the appropriate
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

        internal WatchViewModel ProcessThing(Element element, string tag, bool showRawData)
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
        internal WatchViewModel ProcessThing(object obj, string tag, bool showRawData)
        {
            return baseHandler.Process(obj, tag, showRawData);
        }

        internal WatchViewModel ProcessThing(MirrorData data, string tag, bool showRawData)
        {
            try
            {
                return baseHandler.Process(data, tag, showRawData);
            }
            catch (Exception)
            {
                return Process(data.Data, tag, showRawData);
            }
        }

        public WatchViewModel Process(dynamic value, string tag, bool showRawData = true)
        {
            return Object.ReferenceEquals(value, null)
                ? new WatchViewModel(visualizationManager, "null", tag)
                : ProcessThing(value, tag, showRawData);
        }
    }
}

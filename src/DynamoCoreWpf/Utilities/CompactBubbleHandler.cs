using System;
using System.Linq;
using Dynamo.Extensions;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using ProtoCore.Mirror;
using DSCore;

namespace Dynamo.Wpf.Utilities
{
    /// <summary>
    /// Helper class to process node output
    /// </summary>
    public static class CompactBubbleHandler
    {
        private static int items;

        /// <summary>
        /// Creates an instance of <cref name="CompactBubbleViewModel"/> class
        /// from given node value
        /// </summary>
        /// <param name="value">Node value</param>
        /// <returns>Instance of <cref name="CompactBubbleViewModel"/> class</returns>
        public static CompactBubbleViewModel Process(MirrorData value)
        {
            items = 0;
            var viewModel = ProcessThing(value, true);
            viewModel.NumberOfItems = items;
            return viewModel;
        }

        /// <summary>
        /// Counts the number of all collection items of node output and if specified 
        /// it generates appropriate view model for compact preview bubble
        /// </summary>
        /// <param name="mirrorData">Data which represents the value of node output</param>
        /// <param name="generateViewModel">Flag to not create unused view models</param>
        /// <returns><cref name="CompactBubbleViewModel"/> instance 
        /// if <paramref name="generateViewModel"/> is specified. Otherwise, null</returns>
        private static CompactBubbleViewModel ProcessThing(MirrorData mirrorData, bool generateViewModel)
        {
            if (mirrorData == null)
            {
                return generateViewModel ? new CompactBubbleViewModel(Resources.NullString, 0) : null;
            }

            if (mirrorData.IsArray)
            {
                var list = mirrorData.GetElements();

                foreach (var item in list)
                {
                    ProcessThing(item, false);
                }

                return generateViewModel
                    ? new CompactBubbleViewModel(true)
                    {
                        NodeLabel = list.Any() ? WatchViewModel.LIST : WatchViewModel.EMPTY_LIST
                    }
                    : null;
            }
            else if (mirrorData.IsDictionary)
            {
                var list = mirrorData.GetElements();

                foreach (var item in list)
                {
                    ProcessThing(item, false);
                }

                return generateViewModel
                    ? new CompactBubbleViewModel(true)
                    {
                        NodeLabel = list.Any() ? WatchViewModel.DICTIONARY : WatchViewModel.EMPTY_DICTIONARY
                    }
                    : null;
            } else if (mirrorData.IsPointer && mirrorData.Data is Dictionary)
            {
                var dict = mirrorData.Data as Dictionary;

                return generateViewModel
                    ? new CompactBubbleViewModel(true)
                    {
                        NodeLabel = dict.Values.Any() ? WatchViewModel.DICTIONARY : WatchViewModel.EMPTY_DICTIONARY
                    }
                    : null;
            }

            items++;

            if (!generateViewModel) return null;

            var viewModel = new CompactBubbleViewModel(false);
            if (mirrorData.Data == null && !mirrorData.IsNull && mirrorData.Class != null)
            {
                viewModel.NodeLabel = mirrorData.Class.ClassName;
            }
            else if (mirrorData.Data is Enum)
            {
                viewModel.NodeLabel = ((Enum)mirrorData.Data).GetDescription();
            }
            else
            {
                // Cut StringData so that only the type name remains
                // for example, "Point (Z = 0.000, Y = 0.000, Z = 0.000)" -> "Point"
                viewModel.NodeLabel = string.IsNullOrEmpty(mirrorData.StringData)
                    ? string.Empty
                    : mirrorData.StringData.Split('(')[0];
            }

            return viewModel;
        }
    }
}

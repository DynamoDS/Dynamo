using System;
using System.Linq;
using Dynamo.Extensions;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using ProtoCore.Mirror;

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

        private static CompactBubbleViewModel ProcessThing(MirrorData mirrorData, bool generateVm)
        {
            if (mirrorData == null) return generateVm ? new CompactBubbleViewModel(Resources.NullString, 0) : null;

            if (mirrorData.IsCollection) return ProcessCollection(mirrorData, generateVm);

            items++;

            // generateVm is a flag to not create unused view models
            if (!generateVm) return null;

            var viewModel = new CompactBubbleViewModel();
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
                viewModel.NodeLabel = string.IsNullOrEmpty(mirrorData.StringData)
                    ? string.Empty
                    : mirrorData.StringData.Split('(')[0];
            }

            return viewModel;
        }

        private static CompactBubbleViewModel ProcessCollection(MirrorData mirrorData, bool generateVm)
        {
            var list = mirrorData.GetElements();

            foreach (var item in list)
            {
                ProcessThing(item, false);
            }

            // generateVm is a flag to not create unused view models
            if (!generateVm) return null;

            return new CompactBubbleViewModel
            {
                NodeLabel = list.Any() ? "List" : "Empty List",
                IsCollection = true
            };
        }
    }
}

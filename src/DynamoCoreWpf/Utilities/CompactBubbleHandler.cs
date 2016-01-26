using System;
using Dynamo.Extensions;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using ProtoCore.Mirror;

namespace Dynamo.Wpf.Utilities
{
    public class CompactBubbleHandler
    {
        private static int levels;
        private static int items;

        public static CompactBubbleViewModel Process(dynamic value)
        {
            levels = -1;
            items = 0;
            return Object.ReferenceEquals(value, null)
                ? new CompactBubbleViewModel(Resources.NullString, 0, 0)
                : ProcessThing(value);
        }        

        private static CompactBubbleViewModel ProcessThing(MirrorData mirrorData)
        {
            var viewModel = new CompactBubbleViewModel();            

            if (mirrorData != null)
            {
                if (mirrorData.IsCollection)
                {
                    viewModel = ProcessCollection(mirrorData);
                    levels--;
                }
                else if (mirrorData.Data == null && !mirrorData.IsNull && mirrorData.Class != null)
                {
                    viewModel.NodeLabel = mirrorData.Class.ClassName;
                    items++;
                }
                else if (mirrorData.Data is Enum)
                {
                    viewModel.NodeLabel = ((Enum)mirrorData.Data).GetDescription();
                    items++;
                }
                else
                {
                    items++;
                    if (String.IsNullOrEmpty(mirrorData.StringData))
                    {
                        viewModel.NodeLabel = String.Empty;
                    }
                    else
                    {

                        int index = mirrorData.StringData.IndexOf('(');
                        viewModel.NodeLabel = index != -1 ? mirrorData.StringData.Substring(0, index) : mirrorData.StringData;
                    }
                }
            }

            viewModel.NumberOfItems = items;
            viewModel.NumberOfLevels = levels;
            return viewModel;
        }

        private static CompactBubbleViewModel ProcessCollection(MirrorData mirrorData)
        {
            var viewModel = new CompactBubbleViewModel();

            var list = mirrorData.GetElements();

            viewModel.NodeLabel = list.Count == 0 ? "Empty List" : "List";
            foreach (var item in list)
            {
                ProcessThing(item);
            }

            return viewModel;
        }
    }
}

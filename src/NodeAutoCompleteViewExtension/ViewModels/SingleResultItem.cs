using Dynamo.Search.SearchElements;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.NodeAutoComplete.ViewModels
{
    internal class SingleResultItem
    {

        public SingleResultItem(NodeSearchElement model, double score = 1.0)
        {
            Assembly = model.Assembly;
            IconName = model.IconName;
            Description = model.Description;
            CreationName = model.CreationName;
            PortToConnect = model.AutoCompletionNodeElementInfo.PortToConnect;
            Score = score;
        }

        public SingleResultItem(NodeSearchElementViewModel x) : this(x.Model)
        {
            //Convert percent to probability
            Score = x.AutoCompletionNodeMachineLearningInfo.ConfidenceScore / 100.0;
        }

        internal string Assembly { get; set; }

        internal string IconName { get; set; }

        internal string Description { get; set; }

        internal string CreationName { get; set; }

        internal int PortToConnect { get; set; }

        internal double Score { get; set; }
    }
}

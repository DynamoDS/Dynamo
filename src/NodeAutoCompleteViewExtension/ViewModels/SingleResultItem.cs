using Dynamo.Graph.Nodes;
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
            Description = model.Name;
            Parameters = model.Parameters;
            CreationName = model.CreationName;
            PortToConnect = model.AutoCompletionNodeElementInfo.PortToConnect;
            Score = score;
        }

        public SingleResultItem(NodeModel nodeModel, int portToConnect, double score = 1.0)
        {
            Assembly = nodeModel.GetType().Assembly.GetName().Name;
            IconName = nodeModel.GetType().Name;
            Description = nodeModel.Name;
            CreationName = nodeModel.CreationName;
            PortToConnect = portToConnect;
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

        internal string Parameters { get; set; }

        internal string CreationName { get; set; }

        internal int PortToConnect { get; set; }

        internal double Score { get; set; }
    }
}

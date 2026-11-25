using System;
using System.Collections.Generic;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    /// <summary>
    /// Publishes data onto a named workspace-scoped channel that can be read by matching <see cref="Receive"/> nodes
    /// without a physical wire connection.
    /// </summary>
    [NodeName("Send")]
    [NodeDescription(nameof(Properties.Resources.SendDescription), typeof(Properties.Resources))]
    [NodeCategory("Core.Data")]
    [NodeSearchTags(nameof(Properties.Resources.SendSearchTags), typeof(Properties.Resources))]
    [InPortNames(">")]
    [InPortTypes("var[]..[]")]
    [InPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.SendInputToolTip))]
    [OutPortNames(">")]
    [OutPortTypes("var[]..[]")]
    [OutPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.SendOutputToolTip))]
    [IsDesignScriptCompatible]
    public class Send : NodeModel, IDataChannelSender
    {
        private string channelName = string.Empty;
        private DataChannelBindingStatus channelStatus = DataChannelBindingStatus.Unassigned;

        /// <summary>
        /// Gets or sets the channel key used to match <see cref="Receive"/> nodes in the same workspace.
        /// </summary>
        [JsonProperty("ChannelName")]
        public string ChannelName
        {
            get => channelName;
            set
            {
                var updated = value ?? string.Empty;
                if (!string.Equals(channelName, updated, StringComparison.Ordinal))
                {
                    channelName = updated;
                    RaisePropertyChanged(nameof(ChannelName));
                    ChannelNameChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Gets the current binding status of this channel sender within its workspace.
        /// </summary>
        [JsonIgnore]
        public DataChannelBindingStatus ChannelStatus => channelStatus;

        /// <inheritdoc />
        public event Action<NodeModel> ChannelNameChanged;

        /// <inheritdoc />
        [JsonIgnore]
        public override bool IsInputNode => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Send"/> node.
        /// </summary>
        public Send()
        {
            RegisterAllPorts();
        }

        [JsonConstructor]
        private Send(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(inPorts, outPorts)
        {
        }

        /// <inheritdoc />
        public IdentifierNode GetChannelOutputAstIdentifier()
        {
            return GetAstIdentifierForOutputIndex(0);
        }

        /// <inheritdoc />
        public void UpdateChannelBinding(DataChannelBindingStatus status)
        {
            if (channelStatus != status)
            {
                channelStatus = status;
                RaisePropertyChanged(nameof(ChannelStatus));
            }
        }

        /// <inheritdoc />
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var valueAst = InPorts.Count > 0 && InPorts[0].IsConnected && inputAstNodes != null && inputAstNodes.Count > 0
                ? inputAstNodes[0]
                : AstFactory.BuildNullNode();

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), valueAst)
            };
        }

        /// <inheritdoc />
        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            if (updateValueParams.PropertyName == nameof(ChannelName))
            {
                ChannelName = updateValueParams.PropertyValue;
                return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        /// <inheritdoc />
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("channelName", ChannelName);
        }

        /// <inheritdoc />
        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            var helper = new XmlElementHelper(nodeElement);
            ChannelName = helper.ReadString("channelName", string.Empty);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    /// <summary>
    /// Receives data published by one or more matching <see cref="Send"/> nodes in the same workspace
    /// without a physical wire connection. Supports case-sensitive wildcard patterns (<c>*</c> and <c>?</c>).
    /// </summary>
    [NodeName("Receive")]
    [NodeDescription(nameof(Properties.Resources.ReceiveDescription), typeof(Properties.Resources))]
    [NodeCategory("Core.Data")]
    [NodeSearchTags(nameof(Properties.Resources.ReceiveSearchTags), typeof(Properties.Resources))]
    [OutPortNames(">")]
    [OutPortTypes("var[]..[]")]
    [OutPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.ReceiveOutputToolTip))]
    [IsDesignScriptCompatible]
    public class Receive : NodeModel, IDataChannelReceiver
    {
        private string channelName = string.Empty;
        private IReadOnlyList<NodeModel> boundSenders = Array.Empty<NodeModel>();
        private DataChannelBindingStatus channelStatus = DataChannelBindingStatus.Unassigned;
        private string currentChannelWarning;

        /// <summary>
        /// Gets or sets the case-sensitive key or wildcard pattern (<c>*</c>, <c>?</c>) used to match
        /// <see cref="Send"/> nodes in the same workspace.
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

        /// <inheritdoc />
        [JsonIgnore]
        public IReadOnlyList<NodeModel> BoundSenders => boundSenders;

        /// <summary>
        /// Gets the current binding status of this channel receiver within its workspace.
        /// </summary>
        [JsonIgnore]
        public DataChannelBindingStatus ChannelStatus => channelStatus;

        /// <summary>
        /// Gets the sorted list of active channel names currently defined by <see cref="Send"/> nodes
        /// in the same workspace.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<string> AvailableChannels { get; } = new ObservableCollection<string>();

        /// <inheritdoc />
        public event Action<NodeModel> ChannelNameChanged;

        /// <inheritdoc />
        [JsonIgnore]
        public override bool IsInputNode => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Receive"/> node.
        /// </summary>
        public Receive()
        {
            RegisterAllPorts();
        }

        [JsonConstructor]
        private Receive(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(inPorts, outPorts)
        {
        }

        /// <inheritdoc />
        public bool UpdateChannelBinding(IReadOnlyList<NodeModel> senders, DataChannelBindingStatus status)
        {
            var nextSenders = senders ?? Array.Empty<NodeModel>();
            bool sendersChanged = !SequenceEqualReferences(boundSenders, nextSenders);
            if (sendersChanged)
            {
                boundSenders = nextSenders;
                RaisePropertyChanged(nameof(BoundSenders));
            }

            if (channelStatus != status)
            {
                channelStatus = status;
                RaisePropertyChanged(nameof(ChannelStatus));
            }

            var displayKey = ChannelName?.Trim();
            switch (status)
            {
                case DataChannelBindingStatus.MissingSender:
                    SetChannelWarning(string.Format(Properties.Resources.ReceiveMissingSenderWarning, displayKey));
                    break;
                case DataChannelBindingStatus.CyclicDependency:
                    SetChannelWarning(string.Format(Properties.Resources.ReceiveCyclicDependencyWarning, displayKey));
                    break;
                default:
                    ClearChannelWarning();
                    break;
            }

            return sendersChanged;
        }

        /// <inheritdoc />
        public void UpdateAvailableChannels(IReadOnlyList<string> availableChannels)
        {
            var next = availableChannels ?? Array.Empty<string>();
            if (AvailableChannels.SequenceEqual(next, StringComparer.Ordinal))
            {
                return;
            }

            AvailableChannels.Clear();
            foreach (var channel in next)
            {
                AvailableChannels.Add(channel);
            }
        }

        /// <inheritdoc />
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var senderIdentifiers = boundSenders
                .OfType<IDataChannelSender>()
                .Select(s => (AssociativeNode)s.GetChannelOutputAstIdentifier())
                .ToList();

            AssociativeNode sourceAst;
            if (senderIdentifiers.Count == 0)
            {
                sourceAst = AstFactory.BuildNullNode();
            }
            else if (senderIdentifiers.Count == 1)
            {
                sourceAst = senderIdentifiers[0];
            }
            else
            {
                sourceAst = AstFactory.BuildExprList(senderIdentifiers);
            }

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), sourceAst)
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

        private static bool SequenceEqualReferences(IReadOnlyList<NodeModel> a, IReadOnlyList<NodeModel> b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!ReferenceEquals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void SetChannelWarning(string message)
        {
            if (string.Equals(currentChannelWarning, message, StringComparison.Ordinal) &&
                State == ElementState.PersistentWarning)
            {
                return;
            }

            ClearChannelWarning();
            currentChannelWarning = message;
            Warning(message, isPersistent: true);
        }

        private void ClearChannelWarning()
        {
            if (currentChannelWarning == null &&
                State != ElementState.PersistentWarning &&
                !NodeInfos.Any(x => x.State == ElementState.PersistentWarning))
            {
                return;
            }

            currentChannelWarning = null;
            ClearErrorsAndWarnings();
        }
    }
}

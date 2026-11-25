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
    /// Receives data published by a matching <see cref="Send"/> node in the same workspace
    /// without a physical wire connection.
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
        private NodeModel boundSender;
        private DataChannelBindingStatus channelStatus = DataChannelBindingStatus.Unassigned;
        private string currentChannelWarning;

        /// <summary>
        /// Gets or sets the channel name matching a <see cref="Send"/> node in the same workspace.
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
                    SyncDisplayChannels();
                    RaisePropertyChanged(nameof(ChannelName));
                    ChannelNameChanged?.Invoke(this);
                }
            }
        }

        /// <inheritdoc />
        [JsonIgnore]
        public NodeModel BoundSender => boundSender;

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

        /// <summary>
        /// Gets the channel names displayed in the selection dropdown, including the currently configured
        /// <see cref="ChannelName"/> if its matching sender is missing.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<string> DisplayChannels { get; } = new ObservableCollection<string>();

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
        public bool UpdateChannelBinding(NodeModel sender, DataChannelBindingStatus status)
        {
            bool senderChanged = !ReferenceEquals(boundSender, sender);
            if (senderChanged)
            {
                boundSender = sender;
                RaisePropertyChanged(nameof(BoundSender));
            }

            if (channelStatus != status)
            {
                channelStatus = status;
                RaisePropertyChanged(nameof(ChannelStatus));
            }

            var displayKey = ChannelName?.Trim();
            switch (status)
            {
                case DataChannelBindingStatus.DuplicateSender:
                    SetChannelWarning(string.Format(Properties.Resources.ReceiveDuplicateSenderWarning, displayKey));
                    break;
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

            return senderChanged;
        }

        /// <inheritdoc />
        public void UpdateAvailableChannels(IReadOnlyList<string> availableChannels)
        {
            var next = availableChannels ?? Array.Empty<string>();
            if (!AvailableChannels.SequenceEqual(next, StringComparer.Ordinal))
            {
                AvailableChannels.Clear();
                foreach (var channel in next)
                {
                    AvailableChannels.Add(channel);
                }
            }

            SyncDisplayChannels();
        }

        /// <inheritdoc />
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode sourceAst = boundSender is IDataChannelSender sender
                ? sender.GetChannelOutputAstIdentifier()
                : AstFactory.BuildNullNode();

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

        private void SyncDisplayChannels()
        {
            var desired = new List<string>(AvailableChannels.Count + 1);
            desired.AddRange(AvailableChannels);

            var trimmed = ChannelName?.Trim();
            if (!string.IsNullOrEmpty(trimmed) && !desired.Contains(trimmed, StringComparer.Ordinal))
            {
                desired.Add(trimmed);
            }

            if (DisplayChannels.SequenceEqual(desired, StringComparer.Ordinal))
            {
                return;
            }

            DisplayChannels.Clear();
            foreach (var item in desired)
            {
                DisplayChannels.Add(item);
            }
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

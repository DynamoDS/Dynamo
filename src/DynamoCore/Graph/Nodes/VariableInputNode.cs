using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// Base class for nodes that have dynamic incoming ports.
    /// E.g. list.create.
    /// </summary>
    public abstract class VariableInputNode : NodeModel
    {
        /// <summary>
        /// Protected constructor for deserialization
        /// </summary>
        /// <param name="inPorts"></param>
        /// <param name="outPorts"></param>
        protected VariableInputNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :base(inPorts,outPorts)
        {
            VariableInputController = new BasicVariableInputNodeController(this);
        }

        protected VariableInputNode()
        {
            VariableInputController = new BasicVariableInputNodeController(this);
        }

        /// <summary>
        /// A flag used during serialization to indicated that
        /// the node has a variable number of input ports.
        /// </summary>
        public bool VariableInputPorts
        {
            get { return true; }
        }

        private BasicVariableInputNodeController VariableInputController { get; set; }

        private sealed class BasicVariableInputNodeController : VariableInputNodeController
        {
            private readonly VariableInputNode model;

            public BasicVariableInputNodeController(VariableInputNode node) : base(node)
            {
                model = node;
            }

            protected override string GetInputName(int index)
            {
                return model.GetInputName(index);
            }

            protected override string GetInputTooltip(int index)
            {
                return model.GetInputTooltip(index);
            }

            public void RemoveInputBase()
            {
                base.RemoveInputFromModel();
            }

            public void AddInputBase()
            {
                base.AddInputToModel();
            }

            public int GetInputIndexBase()
            {
                return base.GetInputIndexFromModel();
            }

            public override void AddInputToModel()
            {
                model.AddInput();
            }

            public override void RemoveInputFromModel()
            {
                model.RemoveInput();
            }

            public override int GetInputIndexFromModel()
            {
                return model.GetInputIndex();
            }
        }

        protected abstract string GetInputTooltip(int index);
        protected abstract string GetInputName(int index);

        protected virtual void RemoveInput()
        {
            VariableInputController.RemoveInputBase();
        }

        protected virtual void AddInput()
        {
            VariableInputController.AddInputBase();
        }

        protected virtual int GetInputIndex()
        {
            return VariableInputController.GetInputIndexBase();
        }

        protected override void OnBuilt()
        {
            VariableInputController.OnBuilt();
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            VariableInputController.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            VariableInputController.DeserializeCore(nodeElement, context);
        }

        internal override bool HandleModelEventCore(string eventName, int value, UndoRedoRecorder recorder)
        {
            return VariableInputController.HandleModelEventCore(eventName, value, recorder)
                || base.HandleModelEventCore(eventName, value, recorder);
        }
    }

    /// <summary>
    /// This is a helper class that processess inputs of VariableInputNode.
    /// </summary>
    public abstract class VariableInputNodeController
    {
        private readonly NodeModel model;

        private int inputAmtLastBuild;
        private readonly Dictionary<int, bool> connectedLastBuild = new Dictionary<int, bool>();

        protected VariableInputNodeController(NodeModel model)
        {
            this.model = model;
        }

        protected abstract string GetInputName(int index);
        protected abstract string GetInputTooltip(int index);

        /// <summary>
        ///     Fetches the index number to use for the next port.
        /// </summary>
        public virtual int GetInputIndexFromModel()
        {
            return model.InPorts.Count;
        }

        private void MarkNodeDirty()
        {
            var dirty = model.InPorts.Count != inputAmtLastBuild
                || Enumerable.Range(0, model.InPorts.Count).Any(idx => connectedLastBuild[idx] == model.InPorts[idx].IsConnected);

            if (dirty)
            {
                model.OnNodeModified();
            }
        }

        /// <summary>
        /// Removes an input from this node. Called when the '-' button is clicked.
        /// </summary>
        public virtual void RemoveInputFromModel()
        {
            var count = model.InPorts.Count;
            if (count > 0)
            {
                var port = model.InPorts[count - 1];
                port.DestroyConnectors();
                model.InPorts.Remove(port);
            }

            MarkNodeDirty();
        }

        /// <summary>
        /// Adds an input to this node. Called when the '+' button is clicked.
        /// </summary>
        public virtual void AddInputToModel()
        {
            var idx = GetInputIndexFromModel();
            model.InPorts.Add(new PortModel(PortType.Input,model, new PortData(GetInputName(idx), GetInputTooltip(idx))));
            MarkNodeDirty();
        }

        /// <summary>
        /// Set the number of inputs.
        /// </summary>
        /// <param name="numInputs"></param>
        public void SetNumInputs(int numInputs)
        {
            // Ignore negative values, as those are impossible.
            if (numInputs < 0)
            {
                return;
            }

            // While the current number of ports doesn't match
            // the desired number of ports, add or remove ports
            // as appropriate.  This is intentionally a "best effort"
            // operation, as the node may reject attempts to create
            // or remove too many ports.  As such, we ignore any
            // failures to add or remove ports.
            for (int current = model.InPorts.Count; current != numInputs; )
            {
                if (current < numInputs)
                {
                    AddInputToModel();
                    ++current;
                }
                else
                {
                    RemoveInputFromModel();
                    --current;
                }
            }
        }

        /// <summary>
        /// This is called when a node is built.
        /// </summary>
        public void OnBuilt()
        {
            inputAmtLastBuild = model.InPorts.Count;

            foreach (var idx in Enumerable.Range(0, model.InPorts.Count))
                connectedLastBuild[idx] = model.InPorts[idx].IsConnected;
        }

        /// <summary>
        ///     Serializes the input count of a VariableInputNode to Xml.
        /// </summary>
        /// <param name="nodeElement"></param>
        /// <param name="amount"></param>
        public static void SerializeInputCount(XmlElement nodeElement, int amount)
        {
            nodeElement.SetAttribute("inputcount", amount.ToString());
        }

        #region Serialization/Deserialization Methods

        /// <summary>
        /// Serializes object
        /// </summary>
        /// <param name="element">xml node</param>
        /// <param name="context">save context</param>
        public void SerializeCore(XmlElement element, SaveContext context)
        {
            SerializeInputCount(element, model.InPorts.Count);
        }

        /// <summary>
        /// Deserializes object
        /// </summary>
        /// <param name="element">xml node</param>
        /// <param name="context">save context</param>
        public void DeserializeCore(XmlElement element, SaveContext context)
        {
            int amt = Convert.ToInt32(element.Attributes["inputcount"].Value);
            SetNumInputs(amt);
        }

        #endregion

        #region Undo/Redo

        private void RecordModels(UndoRedoRecorder recorder)
        {
            if (model.InPorts.Count == 0)
                return;

            var connectors = model.InPorts.Last().Connectors;
            if (connectors.Count != 0)
            {
                if (connectors.Count != 1)
                {
                    throw new InvalidOperationException(
                        "There should be only one connection to an input port");
                }
                var models = new Dictionary<ModelBase, UndoRedoRecorder.UserAction>
                {
                    { connectors[0], UndoRedoRecorder.UserAction.Deletion },
                    { model, UndoRedoRecorder.UserAction.Modification }
                };
                WorkspaceModel.RecordModelsForUndo(models, recorder);
            }
            else
                WorkspaceModel.RecordModelForModification(model, recorder);
        }

        internal bool HandleModelEventCore(string eventName, int value, UndoRedoRecorder recorder)
        {
            switch (eventName)
            {
                case "AddInPort":
                    AddInputToModel();
                    return true; // Handled here.
                case "RemoveInPort":
                    RemoveInputFromModel();
                    return true; // Handled here.
                case "SetInPortCount":
                    SetNumInputs(value);
                    return true; // Handled here.
            }

            return false; // base.HandleModelEventCore(eventName);
        }

        #endregion
    }
}

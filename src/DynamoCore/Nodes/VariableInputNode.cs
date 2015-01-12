using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Dynamo.Core;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    public abstract class VariableInputNode : NodeModel
    {
        protected VariableInputNode()
        {
            VariableInputController = new BasicVariableInputNodeController(this);
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
            OnAstUpdated();
        }

        protected virtual void AddInput()
        {
            VariableInputController.AddInputBase();
            OnAstUpdated();
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

        protected override bool HandleModelEventCore(string eventName, UndoRedoRecorder recorder)
        {
            return VariableInputController.HandleModelEventCore(eventName, recorder)
                || base.HandleModelEventCore(eventName, recorder);
        }
    }

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
            return model.InPortData.Count;
        }

        /// <summary>
        /// Removes an input from this node. Called when the '-' button is clicked.
        /// </summary>
        public virtual void RemoveInputFromModel()
        {
            var count = model.InPortData.Count;
            if (count > 0)
                model.InPortData.RemoveAt(count - 1);
        }

        /// <summary>
        /// Adds an input to this node. Called when the '+' button is clicked.
        /// </summary>
        public virtual void AddInputToModel()
        {
            var idx = GetInputIndexFromModel();
            model.InPortData.Add(new PortData(GetInputName(idx), GetInputTooltip(idx)));
        }

        /// <summary>
        /// Set the number of inputs.  
        /// </summary>
        /// <param name="numInputs"></param>
        public void SetNumInputs(int numInputs)
        {
            while (model.InPortData.Count < numInputs)
                AddInputToModel();

            while (model.InPortData.Count > numInputs)
                RemoveInputFromModel();
        }

        public void OnBuilt()
        {
            inputAmtLastBuild = model.InPortData.Count;

            foreach (var idx in Enumerable.Range(0, model.InPortData.Count))
                connectedLastBuild[idx] = model.HasInput(idx);
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

        public void SerializeCore(XmlElement element, SaveContext context)
        {
            //base.SerializeCore(element, context); //Base implementation must be called
            SerializeInputCount(element, model.InPortData.Count);
        }

        public void DeserializeCore(XmlElement element, SaveContext context)
        {
            //base.DeserializeCore(element, context); //Base implementation must be called
            int amt = Convert.ToInt32(element.Attributes["inputcount"].Value);
            SetNumInputs(amt);
            model.RegisterAllPorts();
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

        public bool HandleModelEventCore(string eventName, UndoRedoRecorder recorder)
        {
            if (eventName == "AddInPort")
            {
                AddInputToModel();
                model.RegisterAllPorts();
                return true; // Handled here.
            }

            if (eventName == "RemoveInPort")
            {
                // When an in-port is removed, it is possible that a connector 
                // is almost removed along with it. Both node modification and 
                // connector deletion have to be recorded as one action group.
                // But before HandleModelEventCore is called, node modification 
                // has already been recorded (in WorkspaceModel.SendModelEvent).
                // For that reason, that entry on the undo-stack needs to be 
                // popped (the node modification will be recorded here instead).
                // 
                recorder.PopFromUndoGroup();

                RecordModels(recorder);
                RemoveInputFromModel();
                model.RegisterAllPorts();
                return true; // Handled here.
            }

            return false; // base.HandleModelEventCore(eventName);
        }

        #endregion
    }
}

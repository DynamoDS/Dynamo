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
        protected VariableInputNode(WorkspaceModel workspace) : base(workspace)
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

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);
            VariableInputController.SaveNode(xmlDoc, nodeElement, context);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);
            VariableInputController.LoadNode(nodeElement);
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            VariableInputController.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            VariableInputController.DeserializeCore(element, context);
        }

        protected override bool HandleModelEventCore(string eventName)
        {
            return VariableInputController.HandleModelEventCore(eventName)
                || base.HandleModelEventCore(eventName);
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
            UpdateRecalcState();
        }

        /// <summary>
        /// Adds an input to this node. Called when the '+' button is clicked.
        /// </summary>
        public virtual void AddInputToModel()
        {
            var idx = GetInputIndexFromModel();
            model.InPortData.Add(new PortData(GetInputName(idx), GetInputTooltip(idx)));
            UpdateRecalcState();
        }

        private void UpdateRecalcState()
        {
            var dirty = model.InPortData.Count != inputAmtLastBuild
                || Enumerable.Range(0, model.InPortData.Count).Any(idx => connectedLastBuild[idx] == model.HasInput(idx));

            if (dirty)
                model.RequiresRecalc = true;
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

        #region Load/Save

        public void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("inputcount", model.InPortData.Count.ToString());
        }

        public void LoadNode(XmlNode nodeElement)
        {
            if (nodeElement.Attributes != null) 
            {
                int amt = Convert.ToInt32(nodeElement.Attributes["inputcount"].Value);
                SetNumInputs(amt);
                model.RegisterAllPorts();
            }
        }

        #endregion

        #region Serialization/Deserialization Methods

        public void SerializeCore(XmlElement element, SaveContext context)
        {
            //base.SerializeCore(element, context); //Base implementation must be called
            XmlDocument xmlDoc = element.OwnerDocument;
            foreach (var inport in model.InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");
                input.SetAttribute("name", inport.NickName);
                element.AppendChild(input);
            }
        }

        public void DeserializeCore(XmlElement element, SaveContext context)
        {
            //base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                //Reads in the new number of ports required from the data stored in the Xml Element
                //during Serialize (nextLength). Changes the current In Port Data to match the
                //required size by adding or removing port data.
                XmlNodeList inNodes = element.SelectNodes("Input");
                int nextLength = inNodes.Count;
                SetNumInputs(nextLength);
                model.RegisterAllPorts();
            }
        }

        #endregion

        #region Undo/Redo

        private void RecordModels()
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
                model.Workspace.RecordModelsForUndo(models);
            }
            else
                model.Workspace.RecordModelForModification(model);
        }

        public bool HandleModelEventCore(string eventName)
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
                model.Workspace.UndoRecorder.PopFromUndoGroup();

                RecordModels();
                RemoveInputFromModel();
                model.RegisterAllPorts();
                return true; // Handled here.
            }

            return false; // base.HandleModelEventCore(eventName);
        }

        #endregion
    }
}

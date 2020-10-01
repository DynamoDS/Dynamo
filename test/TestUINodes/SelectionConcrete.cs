using System.Collections.Generic;
using System.Xml;
using CoreNodeModels;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;

namespace TestUINodes
{
    /// <summary>
    /// Class created in order to test protected methods in SelectionBase parent
    /// </summary>
    public class SelectionConcrete : SelectionBase<ModelBase, ModelBase>
    {
        //Property used to allow testing with implementation specific method
        public NodeModel testNode { get; set; }
        public override IModelSelectionHelper<ModelBase> SelectionHelper { get; }

        public void SerializeCore(XmlElement nodeElement, SaveContext context) =>
            base.SerializeCore(nodeElement, context);

        public void DeserializeCore(XmlElement nodeElement, SaveContext context) =>
            base.DeserializeCore(nodeElement, context);

        public bool UpdateValueCore(UpdateValueParams updateValueParams) =>
            base.UpdateValueCore(updateValueParams);

        public string GetOutputPortName() =>
             base.GetOutputPortName();

        //Implemented this way for testing so the selection suffers no modifications
        protected override IEnumerable<ModelBase> ExtractSelectionResults(ModelBase selections)
        {
            return new List<ModelBase> { selections };
        }

        protected override string GetIdentifierFromModelObject(ModelBase modelObject)
        {
            return modelObject.GUID.ToString();
        }

        protected override ModelBase GetModelObjectFromIdentifer(string id)
        {
            return testNode;
        }

        //This constructor is only used during testing to allow the initialization of the SelectionHelper property
        public SelectionConcrete(
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix,
            IModelSelectionHelper<ModelBase> selectionHelper)
            : base(
                selectionType,
                selectionObjectType,
                message,
                prefix)
        {
            SelectionHelper = selectionHelper;
        }

        public SelectionConcrete(
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
                : base(
                    selectionType,
                    selectionObjectType,
                    message,
                    prefix)
        {
        }

        public SelectionConcrete(
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix,
            IEnumerable<string> selectionIdentifier,
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts)
            : base(
                selectionType,
                selectionObjectType,
                message,
                prefix,
                selectionIdentifier,
                inPorts,
                outPorts)
        {
        }
    }
}

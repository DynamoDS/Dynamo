using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Applications.Models;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using ProtoCore.AST.AssociativeAST;
using Revit.GeometryConversion;
using RevitServices.Persistence;

namespace DSRevitNodesUI
{
    [NodeName("Analyze.ProjectLocation"), NodeCategory(BuiltinNodeCategories.ANALYZE),
     NodeDescription("Returns the current Revit project location."), IsDesignScriptCompatible]
    public class ProjectLocation : RevitNodeModel, IWpfNode
    {
        private RevitDynamoModel model;

        public ProjectLocation(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            OutPortData.Add(new PortData("Location", "The location of the current Revit project."));

            ArgumentLacing = LacingStrategy.Disabled;
            model = (RevitDynamoModel)workspaceModel.DynamoModel;
            model.RevitDocumentChanged += model_RevitDocumentChanged;
            model.RevitServicesUpdater.ElementsModified += RevitServicesUpdater_ElementsModified;
        }

        void RevitServicesUpdater_ElementsModified(IEnumerable<string> updated)
        {
            var locUuid = DocumentManager.Instance.CurrentDBDocument.SiteLocation.UniqueId;

            if (updated.Contains(locUuid))
            {
                ForceReExecuteOfNode = true;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            model.RevitDocumentChanged -= model_RevitDocumentChanged;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var location = DocumentManager.Instance.CurrentDBDocument.SiteLocation;

            var latNode = AstFactory.BuildDoubleNode(location.Latitude.ToDegrees());
            var longNode = AstFactory.BuildDoubleNode(location.Longitude.ToDegrees());

            var node =
                AstFactory.BuildFunctionCall(
                    new Func<double, double, DynamoUnits.Location>(
                        DynamoUnits.Location.ByLatitudeAndLongitude), new List<AssociativeNode>(){latNode, longNode});

            return base.BuildOutputAst(new List<AssociativeNode>(){node});
        }

        void model_RevitDocumentChanged(object sender, System.EventArgs e)
        {
            // When the document changes, we frx to ensure that the 
            // AST is rebuilt and the new project's location is reflected in the graph.
            ForceReExecuteOfNode = true;
        }

        public void SetupCustomUIElements(dynNodeView view)
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Applications.Models;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.Wpf;
using ProtoCore.AST.AssociativeAST;
using Revit.GeometryConversion;
using RevitServices.Persistence;

namespace DSRevitNodesUI
{
    public class SiteLocationNodeViewCustomization : INodeViewCustomization<SiteLocation>
    {
        public void CustomizeView(SiteLocation model, NodeView nodeView)
        {
            var locCtrl = new LocationControl { DataContext = this };
            nodeView.inputGrid.Children.Add(locCtrl);
        }

        public void Dispose()
        {

        }
    }

    [NodeName("SiteLocation"), NodeCategory(BuiltinNodeCategories.ANALYZE),
     NodeDescription("Returns the current Revit site location."), IsDesignScriptCompatible]
    public class SiteLocation : RevitNodeModel
    {
        private readonly RevitDynamoModel model;

        public DynamoUnits.Location Location { get; set; }

        public SiteLocation(WorkspaceModel workspaceModel)
            : base(workspaceModel)
        {
            OutPortData.Add(new PortData("Location", "The location of the current Revit project."));
            RegisterAllPorts();

            Location = DynamoUnits.Location.ByLatitudeAndLongitude(0.0, 0.0);

            ArgumentLacing = LacingStrategy.Disabled;

            model = (RevitDynamoModel)workspaceModel.DynamoModel;
            model.RevitDocumentChanged += model_RevitDocumentChanged;
            model.RevitServicesUpdater.ElementsModified += RevitServicesUpdater_ElementsModified;

            Update();
        }

        #region public methods

        public override void Destroy()
        {
            base.Destroy();
            model.RevitDocumentChanged -= model_RevitDocumentChanged;
            model.RevitServicesUpdater.ElementsModified -= RevitServicesUpdater_ElementsModified;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var latNode = AstFactory.BuildDoubleNode(Location.Latitude);
            var longNode = AstFactory.BuildDoubleNode(Location.Longitude);
            var nameNode = AstFactory.BuildStringNode(Location.Name);

            var node =
                AstFactory.BuildFunctionCall(
                    new Func<double, double,string, DynamoUnits.Location>(
                        DynamoUnits.Location.ByLatitudeAndLongitude), new List<AssociativeNode>() { latNode, longNode, nameNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        public override string ToString()
        {
            return string.Format(
                "Name: {0}, Lat: {1}, Lon: {2}",
                Location.Name,
                Location.Latitude,
                Location.Longitude);
        }
        
        #endregion

        #region private methods

        private void RevitServicesUpdater_ElementsModified(IEnumerable<string> updated)
        {
            var locUuid = DocumentManager.Instance.CurrentDBDocument.SiteLocation.UniqueId;

            if (updated.Contains(locUuid))
            {
                Update();
            }
        }

        private void model_RevitDocumentChanged(object sender, System.EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            ForceReExecuteOfNode = true; 
            RequiresRecalc = true;

            var location = DocumentManager.Instance.CurrentDBDocument.SiteLocation;
            Location.Name = location.PlaceName;
            Location.Latitude = location.Latitude.ToDegrees();
            Location.Longitude = location.Longitude.ToDegrees();

            RaisePropertyChanged("Location");
        }

        #endregion

    }
}

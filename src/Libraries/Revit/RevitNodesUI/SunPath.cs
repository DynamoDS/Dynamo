using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.UI.Events;

using Dynamo.Applications.Models;
using Dynamo.Models;
using Dynamo.Nodes;

using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace DSRevitNodesUI
{
    [NodeName("SunSettings.Current"), NodeCategory(BuiltinNodeCategories.REVIT_VIEW),
     NodeDescription("Returns the SunSettings of the current View."), IsDesignScriptCompatible]
    public class SunSettings : RevitNodeModel
    {
        private string settingsID;

        public SunSettings(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            OutPortData.Add(new PortData("SunSettings", "SunSettings element."));
            
            RegisterAllPorts();
            
            RevitDynamoModel.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
            DocumentManager.Instance.CurrentUIApplication.ViewActivated += CurrentUIApplication_ViewActivated;

            CurrentUIApplicationOnViewActivated();
        }

        public override void Destroy()
        {
            base.Destroy();

            RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
            DocumentManager.Instance.CurrentUIApplication.ViewActivated -=
                CurrentUIApplication_ViewActivated;
        }

        private void CurrentUIApplication_ViewActivated(object sender, ViewActivatedEventArgs e)
        {
            CurrentUIApplicationOnViewActivated();
        }

        private void CurrentUIApplicationOnViewActivated()
        {
            settingsID =
                DocumentManager.Instance.CurrentDBDocument.ActiveView.SunAndShadowSettings.UniqueId;
            ForceReExecuteOfNode = true;
            RequiresRecalc = true;
        }

        private void Updater_ElementsModified(IEnumerable<string> updated)
        {
            if (updated.Contains(settingsID))
            {
                ForceReExecuteOfNode = true;
                RequiresRecalc = true;
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            Func<Revit.Elements.SunSettings> func = Revit.Elements.SunSettings.Current;

            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(func, new List<AssociativeNode>()))
            };
        }
    }
}

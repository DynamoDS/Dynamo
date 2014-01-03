using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    [TestFixture]
    class SelectionTests
    {
        [Test]
        public void SelectElementASTGeneration()
        {
            ReferencePoint refPoint = null;

            using (var trans = new Transaction(DocumentManager.GetInstance().CurrentDBDocument, "Create a ReferencePoint"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                refPoint = DocumentManager.GetInstance().CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ());

                trans.Commit();
            }

            var sel = new DSModelElementSelection {SelectedElement = refPoint};

            var buildOutput = sel.BuildOutputAst(new List<AssociativeNode>());

            var funCall = (FunctionCallNode)((BinaryExpressionNode)buildOutput.First()).RightNode;

            Assert.IsInstanceOf<IdentifierNode>(funCall.Function);
            Assert.AreEqual(1, funCall.FormalArguments.Count);
            Assert.IsInstanceOf<IntNode>(funCall.FormalArguments[0]);

            Assert.AreEqual(refPoint.Id.IntegerValue.ToString(), ((IntNode)funCall.FormalArguments[0]).value);
        }

        [Test]
        public void SelectElementsASTGeneration()
        {
            var refPoints = new List<ReferencePoint>();

            using (var trans = new Transaction(DocumentManager.GetInstance().CurrentDBDocument, "Create some ReferencePoints"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                refPoints.Add(DocumentManager.GetInstance().CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ(0,0,0)));
                refPoints.Add(DocumentManager.GetInstance().CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ(0,0,1)));
                refPoints.Add(DocumentManager.GetInstance().CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ(0,0,2)));

                trans.Commit();
            }

            var sel = new DSModelElementsSelection() { SelectedElement = refPoints.Cast<Element>().ToList() };

            var buildOutput = sel.BuildOutputAst(new List<AssociativeNode>());
            var funCall = (ExprListNode)((BinaryExpressionNode)buildOutput.First()).RightNode;

            Assert.AreEqual(funCall.list.Count, 3);
            Assert.Inconclusive("Need more robust testing here.");
        }

        [Test]
        public void SelectReferenceASTGeneration()
        {
            Form extrude = null;

            using (var trans = new Transaction(DocumentManager.GetInstance().CurrentDBDocument, "Create an extrusion Form"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                var p = new Plane(new XYZ(0, 0, 1), new XYZ());
                var arc = Arc.Create(p, 2, 0, System.Math.PI);
                var sp = SketchPlane.Create(DocumentManager.GetInstance().CurrentDBDocument, p);
                var mc = DocumentManager.GetInstance().CurrentDBDocument.FamilyCreate.NewModelCurve(arc, sp);

                var profiles = new ReferenceArray();
                profiles.Append(mc.GeometryCurve.Reference);
                extrude = DocumentManager.GetInstance().CurrentDBDocument.FamilyCreate.NewExtrusionForm(false, profiles, new XYZ(0,0,1));
                trans.Commit();
            }

            var geom = extrude.get_Geometry(new Options()
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Medium,
                IncludeNonVisibleObjects = true
            });

            var solid = geom.FirstOrDefault(x => x is Solid) as Solid;
            var face = solid.Faces.get_Item(0);
            Assert.Greater(solid.Faces.Size, 0);

            var sel = new DSFaceSelection() { SelectedElement = face.Reference };

            var buildOutput = sel.BuildOutputAst(new List<AssociativeNode>());

            var funCall = (FunctionCallNode)((BinaryExpressionNode)buildOutput.First()).RightNode;

            Assert.IsInstanceOf<IdentifierNode>(funCall.Function);
            Assert.AreEqual(1, funCall.FormalArguments.Count);
            Assert.IsInstanceOf<StringNode>(funCall.FormalArguments[0]);

            var stableRef = face.Reference.ConvertToStableRepresentation(DocumentManager.GetInstance().CurrentDBDocument);
            Assert.AreEqual(stableRef, ((StringNode)funCall.FormalArguments[0]).value);
        }
    }
}

﻿using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using DSRevitNodesTests;
using Dynamo.Nodes;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    [TestFixture]
    class SelectionTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void SelectElementASTGeneration()
        {
            ReferencePoint refPoint;

            using (var trans = new Transaction(DocumentManager.Instance.CurrentDBDocument, "CreateAndDeleteAreReferencePoint"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                refPoint = DocumentManager.Instance.CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ());

                trans.Commit();
            }

            var sel = new DSModelElementSelection { SelectedElement = refPoint.Id };

            var buildOutput = sel.BuildOutputAst(new List<AssociativeNode>());

            //function call node builds this
            /*return new IdentifierListNode
            {
                LeftNode = new IdentifierNode(className),
                RightNode = AstFactory.BuildFunctionCall(functionName, arguments)
            };*/

            var funCall = (FunctionCallNode)(((IdentifierListNode)((BinaryExpressionNode)buildOutput.First()).RightNode)).RightNode;
            Assert.IsInstanceOf<IdentifierNode>(funCall.Function);
            Assert.AreEqual(1, funCall.FormalArguments.Count);
            Assert.IsInstanceOf<IntNode>(funCall.FormalArguments[0]);

            Assert.AreEqual(refPoint.Id.IntegerValue, ((IntNode)funCall.FormalArguments[0]).Value);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void SelectElementsASTGeneration()
        {
            var refPoints = new List<ReferencePoint>();

            using (var trans = new Transaction(DocumentManager.Instance.CurrentDBDocument, "Create some ReferencePoints"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                refPoints.Add(DocumentManager.Instance.CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ(0,0,0)));
                refPoints.Add(DocumentManager.Instance.CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ(0,0,1)));
                refPoints.Add(DocumentManager.Instance.CurrentDBDocument.FamilyCreate.NewReferencePoint(new XYZ(0,0,2)));

                trans.Commit();
            }

            var sel = new DSModelElementsSelection { SelectedElement = refPoints.Cast<Element>().Select(x => x.UniqueId).ToList() };

            var buildOutput = sel.BuildOutputAst(new List<AssociativeNode>());
            var funCall = (ExprListNode)((BinaryExpressionNode)buildOutput.First()).RightNode;

            Assert.AreEqual(funCall.list.Count, 3);
            Assert.Inconclusive("Need more robust testing here.");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void SelectReferenceASTGeneration()
        {
            Form extrude;

            using (var trans = new Transaction(DocumentManager.Instance.CurrentDBDocument, "Create an extrusion Form"))
            {
                trans.Start();

                FailureHandlingOptions fails = trans.GetFailureHandlingOptions();
                fails.SetClearAfterRollback(true);
                trans.SetFailureHandlingOptions(fails);

                var p = new Plane(new XYZ(0, 0, 1), new XYZ());
                var arc = Arc.Create(p, 2, 0, System.Math.PI);
                var sp = SketchPlane.Create(DocumentManager.Instance.CurrentDBDocument, p);
                var mc = DocumentManager.Instance.CurrentDBDocument.FamilyCreate.NewModelCurve(arc, sp);

                var profiles = new ReferenceArray();
                profiles.Append(mc.GeometryCurve.Reference);
                extrude = DocumentManager.Instance.CurrentDBDocument.FamilyCreate.NewExtrusionForm(false, profiles, new XYZ(0,0,1));
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

            var funCall = (FunctionCallNode)((IdentifierListNode)((BinaryExpressionNode)buildOutput.First()).RightNode).RightNode;

            Assert.IsInstanceOf<IdentifierNode>(funCall.Function);
            Assert.AreEqual(1, funCall.FormalArguments.Count);
            Assert.IsInstanceOf<StringNode>(funCall.FormalArguments[0]);

            var stableRef = face.Reference.ConvertToStableRepresentation(DocumentManager.Instance.CurrentDBDocument);
            Assert.AreEqual(stableRef, ((StringNode)funCall.FormalArguments[0]).value);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectAnalysisResults()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectModelElement()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectAFamilyInstance()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectALevel()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectCurveElement()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectAReferencePoint()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectAFace()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectAnEdge()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectAPointOnAFace()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectAUVOnAFace()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectDividedSurfaceFamilies()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CanSelectMultipleModelElements()
        {
            Assert.Inconclusive("Finish me!");
        }
    }
}

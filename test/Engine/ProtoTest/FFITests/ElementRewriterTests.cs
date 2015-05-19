using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Namespace;
using ProtoCore.Utils;

namespace ProtoTest.FFITests
{
    [TestFixture]
    class ElementRewriterTests : ProtoTestBase
    {
        [Test]
        public void LookupResolvedName_FromElementResolver_RewriteAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "p = Point.ByCoordinates(0,0,0);");

            var elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(0, 0, 0);\n", newNodes.ElementAt(0).ToString());
        }

        [Test]
        public void LookupResolvedName_FromCompiler_RewriteAst()
        {

            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            var testCore = thisTest.GetTestCore();
            var astNodes = CoreUtils.BuildASTList(testCore, "d = ElementResolverTarget.ElementResolverTarget();");

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("d = FFITarget.ElementResolverTarget.ElementResolverTarget();\n", newNodes.ElementAt(0).ToString());

            // Add verification for contents of element resolver resolution map
            Assert.AreEqual(1, elementResolver.ResolutionMap.Count);

            var assembly = elementResolver.LookupAssemblyName("ElementResolverTarget");
            var resolvedName = elementResolver.LookupResolvedName("ElementResolverTarget");

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual("FFITarget.ElementResolverTarget", resolvedName);
        }

        [Test]
        public void LookupResolvedName_ForComplexExpressionFromCompiler_RewriteAst()
        {

            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            var testCore = thisTest.GetTestCore();
            var astNodes = CoreUtils.BuildASTList(testCore, "d = ElementResolverTarget.Create().Property.Method();");

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("d = FFITarget.ElementResolverTarget.Create().Property.Method();\n", newNodes.ElementAt(0).ToString());

            // Add verification for contents of element resolver resolution map
            Assert.AreEqual(1, elementResolver.ResolutionMap.Count);

            var assembly = elementResolver.LookupAssemblyName("ElementResolverTarget");
            var resolvedName = elementResolver.LookupResolvedName("ElementResolverTarget");

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual("FFITarget.ElementResolverTarget", resolvedName);
        }

        [Test]
        public void LookupResolvedName_ForNestedExpressionFromCompiler_RewriteAst()
        {

            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            var testCore = thisTest.GetTestCore();
            var astNodes = CoreUtils.BuildASTList(testCore, "d = ElementResolverTarget.Create().Property.Method(ElementResolverTarget.Create().Property);");

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("d = FFITarget.ElementResolverTarget.Create().Property.Method(FFITarget.ElementResolverTarget.Create().Property);\n", newNodes.ElementAt(0).ToString());

            // Add verification for contents of element resolver resolution map
            Assert.AreEqual(1, elementResolver.ResolutionMap.Count);

            var assembly = elementResolver.LookupAssemblyName("ElementResolverTarget");
            var resolvedName = elementResolver.LookupResolvedName("ElementResolverTarget");

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual("FFITarget.ElementResolverTarget", resolvedName);
        }

        [Test]
        public void LookupResolvedName_ForNestedExpressionFromCompiler_RewriteAst2()
        {

            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            var testCore = thisTest.GetTestCore();
            var astNodes = CoreUtils.BuildASTList(testCore, "d = ElementResolverTarget.StaticProperty.Method(ElementResolverTarget.Create().Property.Method(ElementResolverTarget.Create().Property));");

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("d = FFITarget.ElementResolverTarget.StaticProperty.Method(FFITarget.ElementResolverTarget.Create().Property.Method(FFITarget.ElementResolverTarget.Create().Property));\n", newNodes.ElementAt(0).ToString());

            // Add verification for contents of element resolver resolution map
            var assembly = elementResolver.LookupAssemblyName("ElementResolverTarget");
            var resolvedName = elementResolver.LookupResolvedName("ElementResolverTarget");

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual("FFITarget.ElementResolverTarget", resolvedName);
        }

        [Test]
        public void LookupResolvedName_ForTypedIdentifierFromCompiler_RewriteAst()
        {

            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            var testCore = thisTest.GetTestCore();
            var astNodes = CoreUtils.BuildASTList(testCore, "d : ElementResolverTarget;");

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("d : FFITarget.ElementResolverTarget", newNodes.ElementAt(0).ToString());

            // Add verification for contents of element resolver resolution map
            var assembly = elementResolver.LookupAssemblyName("ElementResolverTarget");
            var resolvedName = elementResolver.LookupResolvedName("ElementResolverTarget");

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual("FFITarget.ElementResolverTarget", resolvedName);
        }

        [Test]
        public void LookupResolvedName_ForComplexExpression_RewriteAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "p = Point.ByCoordinates(x, y, z).X;");

            var elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X;\n", newNodes.ElementAt(0).ToString());
        }

        [Test]
        public void LookupResolvedName_ForPartialComplexExpression_RewriteAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "p = Autodesk.Point.ByCoordinates(x, y, z).X;");

            var elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X;\n", newNodes.ElementAt(0).ToString());

            /////////////////////////////
            astNodes = CoreUtils.BuildASTList(core, "p = Autodesk.DS.Point.ByCoordinates(x, y, z).X;");

            elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.DS.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X;\n", newNodes.ElementAt(0).ToString());

            /////////////////////////////
            astNodes = CoreUtils.BuildASTList(core, "p = Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X;");

            elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.DS.Geometry.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X;\n", newNodes.ElementAt(0).ToString());
        }

        [Test]
        public void LookupResolvedName_ForNestedExpression_RewriteAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "p = Point.ByCoordinates(Point.ByCoordinates(x, y, z).X, y, z).X;");

            var elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X, y, z).X;\n", newNodes.ElementAt(0).ToString());
        }

        [Test]
        public void LookupResolvedName_ForPartialNestedExpression_RewriteAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "p = Autodesk.Point.ByCoordinates(Autodesk.Point.ByCoordinates(x, y, z).X, y, z).X;");

            var elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X, y, z).X;\n", newNodes.ElementAt(0).ToString());

            /////////////////////////////////////
            astNodes = CoreUtils.BuildASTList(core, "p = Autodesk.DS.Point.ByCoordinates(Autodesk.Point.ByCoordinates(x, y, z).X, y, z).X;");

            elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");
            elementResolver.AddToResolutionMap("Autodesk.DS.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X, y, z).X;\n", newNodes.ElementAt(0).ToString());

            //////////////////////////////////////
            astNodes = CoreUtils.BuildASTList(core, "p = Autodesk.DS.Geometry.Point.ByCoordinates(Autodesk.Point.ByCoordinates(x, y, z).X, y, z).X;");

            elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");
            elementResolver.AddToResolutionMap("Autodesk.DS.Geometry.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(Autodesk.DS.Geometry.Point.ByCoordinates(x, y, z).X, y, z).X;\n", newNodes.ElementAt(0).ToString());
        }

        [Test]
        public void LookupResolvedName_ForTypedIdentifier_RewriteAst()
        {
            IEnumerable<Node> astNodes = CoreUtils.BuildASTList(core, "p : Point;");

            var elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p : Autodesk.DS.Geometry.Point", newNodes.ElementAt(0).ToString());
        }

        [Test]
        public void LookupResolvedName_ForPartialTypedIdentifier_RewriteAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "p : Autodesk.Point;");

            var elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p : Autodesk.DS.Geometry.Point", newNodes.ElementAt(0).ToString());

            astNodes = CoreUtils.BuildASTList(core, "p : Autodesk.DS.Point;");

            elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.DS.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p : Autodesk.DS.Geometry.Point", newNodes.ElementAt(0).ToString());

            astNodes = CoreUtils.BuildASTList(core, "p : Autodesk.DS.Geometry.Point;");

            elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Autodesk.DS.Geometry.Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes);

            Assert.AreEqual("p : Autodesk.DS.Geometry.Point", newNodes.ElementAt(0).ToString());
        }

        [Test]
        public void SkipResolvingName_ForPrimitiveTypedIdentifier_RetainAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "p : int;");

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, new ElementResolver(), astNodes);

            Assert.AreEqual("p : int", newNodes.ElementAt(0).ToString());
        }

    }
}

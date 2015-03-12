using System.Linq;
using NUnit.Framework;
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
            var astNode = astNodes[0];

            var elementResolver = new ElementResolver();
            elementResolver.AddToResolutionMap("Point", "Autodesk.DS.Geometry.Point", "Protogeometry.dll");

            var elementRewriter = new ElementRewriter(core.ClassTable);
            elementRewriter.LookupResolvedNameAndRewriteAst(elementResolver, ref astNode);

            Assert.AreEqual("p = Autodesk.DS.Geometry.Point.ByCoordinates(0, 0, 0);\n", astNode.ToString());
        }

        [Test]
        public void LookupResolvedName_FromCompiler_RewriteAst()
        {

            string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            var testCore = thisTest.GetTestCore();
            var astNodes = CoreUtils.BuildASTList(testCore, "d = ElementResolverTarget.ElementResolverTarget();");
            var astNode = astNodes[0];

            var elementResolver = new ElementResolver();

            var elementRewriter = new ElementRewriter(testCore.ClassTable);
            elementRewriter.LookupResolvedNameAndRewriteAst(elementResolver, ref astNode);

            Assert.AreEqual("d = FFITarget.ElementResolverTarget.ElementResolverTarget();\n", astNode.ToString());

            // Add verification for contents of element resolver resolution map
            Assert.AreEqual(1, elementResolver.ResolutionMap.Count);
            
            var assembly = elementResolver.LookupAssemblyName("ElementResolverTarget");
            var resolvedName = elementResolver.LookupResolvedName("ElementResolverTarget");

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual("FFITarget.ElementResolverTarget", resolvedName);
        }

        // Add tests for ElementRewriter.GetClassIdentifiers() api
        [Test]
        public void GetClassIdentifiers_FromAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "d = Point.ByCoordinates(0, 0, pt.X + 5);");
            var astNode = astNodes[0];

            var elementRewriter = new ElementRewriter(null);
            var identifiers = elementRewriter.GetClassIdentifiers(astNode);
            var identifierListNodes = identifiers as IdentifierListNode[] ?? identifiers.ToArray();
            Assert.AreEqual(2, identifierListNodes.Count());
            Assert.AreEqual("pt.X", identifierListNodes.ElementAt(0).ToString());
            Assert.AreEqual("Point.ByCoordinates(0, 0, (pt.X) + 5)", identifierListNodes.ElementAt(1).ToString());
        }
    }
}

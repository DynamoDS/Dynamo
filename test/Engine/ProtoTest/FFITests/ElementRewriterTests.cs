using System;
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
        private void VerifyResult(string fullName, string partialName, string functionOrProperty, bool isProperty = false)
        {
            var testCore = thisTest.GetTestCore();

            var astNodes = CoreUtils.BuildASTList(testCore, string.Format("d = {0}.{1};", partialName, functionOrProperty));

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes);

            Assert.AreEqual(string.Format("d = {0}.{1};\n", fullName, functionOrProperty), newNodes.ElementAt(0).ToString());

            if (!isProperty)
            {
                // Add verification for contents of element resolver resolution map
                var assembly = elementResolver.LookupAssemblyName(partialName);
                var resolvedName = elementResolver.LookupResolvedName(partialName);

                Assert.AreEqual("FFITarget.dll", assembly);
                Assert.AreEqual(fullName, resolvedName);
            }
        }


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
        public void LookupResolvedName_ForNestedNamespacesExpressionFromCompiler_RewriteAst()
        {
            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            var testCore = thisTest.GetTestCore();
            string class1 = "NestedResolverTarget";
            string class2 = "ElementResolverTarget";
            string fullName1 = "FFITarget.NameSpaceA.NameSpaceB.NameSpaceC.NestedResolverTarget";
            string fullName2 = "FFITarget.ElementResolverTarget";
            var astNodes = CoreUtils.BuildASTList(testCore, string.Format("d = {0}.Property.Method({1}.Create().Property.Method({0}.Property.Property));", class1, class2));

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes);

            Assert.AreEqual(
                string.Format("d = {0}.Property.Method({1}.Create().Property.Method({0}.Property.Property));\n", fullName1, fullName2), 
                newNodes.ElementAt(0).ToString());

            // Add verification for contents of element resolver resolution map
            var assembly = elementResolver.LookupAssemblyName(class2);
            var resolvedName = elementResolver.LookupResolvedName(class2);

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual(fullName2, resolvedName);

            ///////////////////////////////////////////////
            astNodes = CoreUtils.BuildASTList(testCore, string.Format("d = {0}.Property.Method({1}.Create().Property.Method({0}.Property.Property));", fullName1, fullName2));

            elementResolver = new ElementResolver();

            newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes);

            Assert.AreEqual(
                string.Format("d = {0}.Property.Method({1}.Create().Property.Method({0}.Property.Property));\n", fullName1, fullName2),
                newNodes.ElementAt(0).ToString());

            // Add verification for contents of element resolver resolution map
            assembly = elementResolver.LookupAssemblyName(fullName2);
            resolvedName = elementResolver.LookupResolvedName(fullName2);

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual(fullName2, resolvedName);
        }

        [Test]
        public void LookupResolvedName_ForNestedNamespacesFromCompiler_RewriteAst()
        {

            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            const string functionName = "NestedResolverTarget()";
            const string fullName = "FFITarget.NameSpaceA.NameSpaceB.NameSpaceC.NestedResolverTarget";

            var partialName = "NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            VerifyResult(fullName, fullName, functionName);

            partialName = "FFITarget.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceA.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceB.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceA.NameSpaceB.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceB.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceA.NameSpaceB.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "NameSpaceA.NameSpaceB.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "NameSpaceB.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "NameSpaceA.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

            partialName = "NameSpaceA.NameSpaceB.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, functionName);

        }

        [Test]
        public void LookupResolvedName_ForNestedNamespacesPropertyFromCompiler_RewriteAst()
        {

            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            const string propertyName = "Property";
            const string fullName = "FFITarget.NameSpaceA.NameSpaceB.NameSpaceC.NestedResolverTarget";

            var partialName = "NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            VerifyResult(fullName, fullName, propertyName, true);

            partialName = "FFITarget.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "FFITarget.NameSpaceA.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "FFITarget.NameSpaceB.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "FFITarget.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "FFITarget.NameSpaceA.NameSpaceB.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "FFITarget.NameSpaceB.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "FFITarget.NameSpaceA.NameSpaceB.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "NameSpaceA.NameSpaceB.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "NameSpaceB.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "NameSpaceA.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

            partialName = "NameSpaceA.NameSpaceB.NameSpaceC.NestedResolverTarget";
            VerifyResult(fullName, partialName, propertyName, true);

        }

        [Test]
        public void LookupResolvedName_ForSameNamespaceClassNameFromCompiler_RewriteAst()
        {
            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            const string fullName = "FFITarget.NameSpaceA.NameSpaceB.NameSpaceC.NameSpaceC";
            const string functionName = "NameSpaceC()";

            var partialName = "NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            VerifyResult(fullName, fullName, functionName);

            partialName = "FFITarget.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceA.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceB.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceC.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceA.NameSpaceB.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceB.NameSpaceC.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "FFITarget.NameSpaceA.NameSpaceB.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "NameSpaceA.NameSpaceB.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "NameSpaceB.NameSpaceC.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "NameSpaceA.NameSpaceC.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);

            partialName = "NameSpaceA.NameSpaceB.NameSpaceC.NameSpaceC";
            VerifyResult(fullName, partialName, functionName);
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
        public void LookupResolvedName_ForGlobalFunction_RewriteAst()
        {
            var astNodes = CoreUtils.BuildASTList(core, "a = Flatten(a).DifferenceAll(Flatten(b));");

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(core.ClassTable, elementResolver, astNodes).ToList();

            Assert.AreEqual("a = Flatten(a).DifferenceAll(Flatten(b));\n", newNodes[0].ToString());
        }

        [Test]
        public void LookupResolvedName_ForGlobalClass_RewriteAst()
        {
            const string code = @"import (""FFITarget.dll"");";
            var mirror = thisTest.RunScriptSource(code);

            var testCore = thisTest.GetTestCore();
            var astNodes = CoreUtils.BuildASTList(testCore, "a = GlobalClass.GlobalClass(a);");

            var elementResolver = new ElementResolver();

            var newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes).ToList();

            Assert.AreEqual("a = GlobalClass.GlobalClass(a);\n", newNodes[0].ToString());

            astNodes = CoreUtils.BuildASTList(testCore, "a : GlobalClass;");

            newNodes = ElementRewriter.RewriteElementNames(testCore.ClassTable, elementResolver, astNodes).ToList();

            Assert.AreEqual("a : GlobalClass", newNodes[0].ToString());

            // Add verification for contents of element resolver resolution map
            Assert.AreEqual(1, elementResolver.ResolutionMap.Count);

            var assembly = elementResolver.LookupAssemblyName("GlobalClass");
            var resolvedName = elementResolver.LookupResolvedName("GlobalClass");

            Assert.AreEqual("FFITarget.dll", assembly);
            Assert.AreEqual("GlobalClass", resolvedName);
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

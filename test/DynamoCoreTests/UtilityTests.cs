using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DynNodes = Dynamo.Nodes;

namespace Dynamo.Tests
{
    internal class UtilityTests : DynamoUnitTest
    {
        [Test]
        public void PreprocessTypeName00()
        {
            // 'null' as fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string qualifiedName = null;
                DynNodes.Utilities.PreprocessTypeName(qualifiedName);
            });
        }

        [Test]
        public void PreprocessTypeName01()
        {
            // Empty fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string qualifiedName = string.Empty;
                DynNodes.Utilities.PreprocessTypeName(qualifiedName);
            });
        }

        [Test]
        public void PreprocessTypeName02()
        {
            // "Dynamo.Elements." prefix should be replaced.
            string fqn = "Dynamo.Elements.MyClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        public void PreprocessTypeName03()
        {
            // "Dynamo.Nodes." prefix should never be replaced.
            string fqn = "Dynamo.Nodes.MyClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        public void PreprocessTypeName04()
        {
            // System type names should never be modified.
            string fqn = "System.Environment";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("System.Environment", result);
        }

        [Test]
        public void PreprocessTypeName05()
        {
            // "Dynamo.Elements.dyn" prefix should be replaced.
            string fqn = "Dynamo.Elements.dynMyClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        public void PreprocessTypeName06()
        {
            // "Dynamo.Nodes.dyn" prefix should be replaced.
            string fqn = "Dynamo.Nodes.dynMyClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyClass", result);
        }

        [Test]
        public void PreprocessTypeName07()
        {
            // "Dynamo.Elements.dynXYZ" prefix should be replaced.
            string fqn = "Dynamo.Elements.dynMyXYZClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyXyzClass", result);
        }

        [Test]
        public void PreprocessTypeName08()
        {
            // "Dynamo.Nodes.dynUV" prefix should be replaced.
            string fqn = "Dynamo.Nodes.dynMyUVClass";
            string result = DynNodes.Utilities.PreprocessTypeName(fqn);
            Assert.AreEqual("Dynamo.Nodes.MyUvClass", result);
        }

        [Test]
        public void ResolveType00()
        {
            // 'null' as fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string fqn = null;
                DynNodes.Utilities.ResolveType(fqn);
            });
        }

        [Test]
        public void ResolveType01()
        {
            // Empty fullyQualifiedName throws an exception.
            Assert.Throws<ArgumentNullException>(() =>
            {
                string fqn = string.Empty;
                DynNodes.Utilities.ResolveType(fqn);
            });
        }

        [Test]
        public void ResolveType02()
        {
            // Unknown type returns a 'null'.
            string fqn = "Dynamo.Connectors.ConnectorModel";
            System.Type type = DynNodes.Utilities.ResolveType(fqn);
            Assert.AreEqual(null, type);
        }

        [Test]
        public void ResolveType03()
        {
            // Known internal type.
            string fqn = "Dynamo.Nodes.Addition";
            System.Type type = DynNodes.Utilities.ResolveType(fqn);
            Assert.AreNotEqual(null, type);
            Assert.AreEqual("Dynamo.Nodes.Addition", type.FullName);
        }

        [Test]
        public void ResolveType04()
        {
            // System type names should be discoverable.
            string fqn = "System.Environment";
            System.Type type = DynNodes.Utilities.ResolveType(fqn);
            Assert.AreNotEqual(null, type);
            Assert.AreEqual("System.Environment", type.FullName);
        }

        [Test]
        public void ResolveType05()
        {
            // 'NumberRange' class makes use of this attribute.
            string fqn = "Dynamo.Nodes.dynBuildSeq";
            System.Type type = DynNodes.Utilities.ResolveType(fqn);
            Assert.AreNotEqual(null, type);
            Assert.AreEqual("Dynamo.Nodes.NumberRange", type.FullName);
        }
    }
}

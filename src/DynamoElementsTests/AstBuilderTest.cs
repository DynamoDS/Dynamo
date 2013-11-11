using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;

namespace Dynamo
{
    [Category("DSExecution")]
    class AstBuilderTest: DynamoUnitTest
    {
        private class AstNodeTestContainer: IAstNodeContainer
        {
            public AstNodeTestContainer()
            {
                AstNodes = new List<AssociativeNode>();
            }

            public void OnAstNodeBuilding(NodeModel node)
            {
            
            }

            public void OnAstNodeBuilt(NodeModel node, IEnumerable<AssociativeNode> astNodes)
            {
                AstNodes.AddRange(astNodes); 
            }

            public void Reset()
            {
                AstNodes.Clear();
            }

            public List<AssociativeNode> AstNodes
            {
                get;
                set;
            }
        }

        [Test]
        public void TestCompileToAstNodes()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\astbuilder\nodes.dyn");
            model.Open(openPath);

            AstNodeTestContainer container = new AstNodeTestContainer();
            AstBuilder builder = new AstBuilder(container);

            // Get some nodes and convert them to AST nodes
            {
                List<NodeModel> nodes = new List<NodeModel>();
                NodeModel node = null;

                node = model.CurrentWorkspace.NodeFromWorkspace("c8e30669-0265-4a8e-89a1-a2f5b2c346f2");
                nodes.Add(node);

                node = model.CurrentWorkspace.NodeFromWorkspace("9d94a1d5-19c2-49e1-8f49-247bed003d90");
                nodes.Add(node);

                node = model.CurrentWorkspace.NodeFromWorkspace("fd3c7a64-a488-4706-9368-d508294abce1");
                nodes.Add(node);

                builder.CompileToAstNodes(nodes);
                var astNodes = container.AstNodes;
                Assert.IsTrue(astNodes.Count > 0);
                container.Reset();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Interfaces;

namespace NodeCompilers
{
    public class DSFunctionCompiler : INodeCompiler<Expression>
    {
        public Type NodeType { get { return typeof (DSFunction); } }

        public IEnumerable<Expression> Compile(NodeModel node, List<Expression> inputs)
        {
            // Assume one output and no replication for now...
            return new List<Expression> { Expression.Call(GetMethodInfo(node), inputs) };
        }

        private MethodInfo GetMethodInfo(NodeModel node)
        {
            var function = (DSFunctionBase)node;
            var definition = function.Controller.Definition;
            var foundType =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                        .First(t => t.FullName == definition.ClassName);
            var foundMethod = foundType.GetMethods().First(m => m.Name == definition.FunctionName);
            return foundMethod;
        }
    }
}

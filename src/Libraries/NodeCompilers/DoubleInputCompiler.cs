using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Interfaces;

namespace NodeCompilers
{
    public class DoubleInputCompiler : INodeCompiler<Expression>
    {
        public Type NodeType { get { return typeof (DoubleInput); } }

        public IEnumerable<Expression> Compile(NodeModel node, List<Expression> inputs)
        {
            var doubleInput = (DoubleInput) node;

            return new List<Expression>
            {
                Expression.Constant(double.Parse(doubleInput.Value), typeof(double))
            };
        }
    }
}

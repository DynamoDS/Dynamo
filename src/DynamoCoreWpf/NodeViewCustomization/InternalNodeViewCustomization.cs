using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Navigation;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;

using ProtoCore.Exceptions;

namespace Dynamo.Wpf
{
    internal class InternalNodeViewCustomization
    {
        private readonly Type nodeModelType;
        private readonly Type customizerType;
        private readonly MethodInfo customizeViewMethodInfo;
        private Func<NodeModel, NodeView, IDisposable> compiledCustomizationCall;

        internal static InternalNodeViewCustomization Create(Type nodeModelType, Type customizerType)
        {
            if (nodeModelType == null) throw new ArgumentNullException("nodeModelType");
            if (customizerType == null) throw new ArgumentNullException("customizerType");

            // get the CustomizeView method appropriate to the supplied NodeModelType
            var methodInfo = customizerType.GetMethods()
                .Where(x => x.Name == "CustomizeView")
                .Where(
                    x =>
                    {
                        var firstParm = x.GetParameters().FirstOrDefault();
                        return firstParm != null && firstParm.ParameterType == nodeModelType;
                    }).FirstOrDefault();

            // if it doesn't exist, fail early
            if (methodInfo == null)
            {
                throw new Exception(
                    "A CustomizeView method with type '" + nodeModelType.Name +
                        "' does not exist on the supplied INodeViewCustomization type.");
            }

            return new InternalNodeViewCustomization(nodeModelType, customizerType, methodInfo);
        }

        internal InternalNodeViewCustomization(Type nodeModelType, Type customizerType, MethodInfo customizeViewMethod)
        {
            this.nodeModelType = nodeModelType;
            this.customizerType = customizerType;
            this.customizeViewMethodInfo = customizeViewMethod;
        }

        public Func<NodeModel, NodeView, IDisposable> CustomizeView
        {
            get { return Compile(); }
        }

        private Func<NodeModel, NodeView, IDisposable> Compile()
        {
            // generate:
            //
            // (model, view) => {
            //      var c = new NodeViewCustomizer();
            //      c.CustomizeView( model as NodeModelType, view );
            //      return new OnceDisposable( c );
            // }

            // use cache
            if (compiledCustomizationCall != null) return compiledCustomizationCall;
            
            // parameters for the lambda
            var modelParam = Expression.Parameter(typeof(NodeModel), "model");
            var viewParam = Expression.Parameter(typeof(NodeView), "view");

            // var c = new NodeViewCustomizer();
            var custLam = Expression.Lambda(Expression.New(customizerType));
            var custExp = Expression.Invoke(custLam);
            var varExp = Expression.Variable(customizerType);
            var assignExp = Expression.Assign(varExp, custExp);

            // c.CustomizeView( model as NodeModelType, view );
            var castModelExp = Expression.TypeAs(modelParam, nodeModelType);
            var invokeExp = Expression.Call(varExp, customizeViewMethodInfo, castModelExp, viewParam);

            // new OnceDisposable(c);
            var onceDispConstInfo = typeof(OnceDisposable).GetConstructor(new[] { typeof(IDisposable) });
            if (onceDispConstInfo == null) throw new Exception("Could not obtain OnceDisposable constructor!");
            var onceDisp = Expression.Lambda(Expression.New(onceDispConstInfo, varExp));
            var onceDispExp = Expression.Invoke(onceDisp);

            // make full block
            var block = Expression.Block(
                new[] { varExp },
                assignExp,
                invokeExp,
                onceDispExp);

            // compile
            return compiledCustomizationCall = Expression.Lambda<Func<NodeModel, NodeView, IDisposable>>(
                block,
                modelParam,
                viewParam).Compile();
        }

    }
}

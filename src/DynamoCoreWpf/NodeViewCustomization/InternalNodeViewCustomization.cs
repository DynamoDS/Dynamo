using System;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Navigation;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Wpf
{
    internal class InternalNodeViewCustomization
    {
        private class NodeViewCustomizationApplier
        {
            private IDisposable Apply<T>(INodeViewCustomization<T> t,
                NodeModel model, dynNodeView view)
                where T : NodeModel
            {
                t.CustomizeView(model as T, view);
                return new OnceDisposable(t);
            }
        }

        private readonly Type customizerType;
        private Delegate constructor;

        internal InternalNodeViewCustomization(Type customizerType)
        {
            this.customizerType = customizerType;
        }

        internal IDisposable CustomizeView(NodeModel model, dynNodeView view)
        {
            var custConst = Expression.New(customizerType);
            var custLam = Expression.Lambda(custConst);
            InvocationExpression custExp = Expression.Invoke(custLam);

            Expression<Func<NodeViewCustomizationApplier>> invokerLam = () => new NodeViewCustomizationApplier();
            InvocationExpression invokerExp = Expression.Invoke(invokerLam);

            Expression<Func<NodeModel>> modelLam = () => model;
            InvocationExpression modelExp = Expression.Invoke(modelLam);
            UnaryExpression castModelExp = Expression.Convert(modelExp, model.GetType());

            Expression<Func<dynNodeView>> viewLam = () => view;
            InvocationExpression viewExp = Expression.Invoke(viewLam);

            var res = Expression.Call(invokerExp, "Apply", new[] { model.GetType() }, custExp, castModelExp, viewExp);
            return (IDisposable) Expression.Lambda(res).Compile().DynamicInvoke();
        }

        internal static InternalNodeViewCustomization Create(Type custType)
        {
            // TODO CORESPLIT
            // fail early
            return new InternalNodeViewCustomization(custType);
        }
    }
}

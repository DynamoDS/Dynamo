using System;
using System.Linq.Expressions;

using Dynamo.Controls;
using Dynamo.Models;

namespace Dynamo.Wpf
{
    internal class OnceDisposable : IDisposable
    {
        private bool called;
        private IDisposable disposable;

        internal OnceDisposable(IDisposable disposable)
        {
            this.disposable = disposable;
        }

        public void Dispose()
        {
            if (called) return;
            called = true;
            if (disposable != null) disposable.Dispose();
        }
    }

    internal class InternalNodeViewCustomization
    {
        private readonly Type customizerType;
        private readonly Delegate constructor;

        internal InternalNodeViewCustomization(Type customizerType)
        {
            this.customizerType = customizerType;
        }

        internal IDisposable CustomizeView(NodeModel model, dynNodeView view)
        {
            // construct and invoke the customizer
            var customizer = Compile().DynamicInvoke();

            return CustomizeViewInternal((dynamic)customizer, model, view);
        }

        private static IDisposable CustomizeViewInternal<T>(INodeViewCustomization<T> t, 
            NodeModel model, dynNodeView view) 
            where T : NodeModel
        {
            t.CustomizeView(model as T, view);
            return new OnceDisposable(t);
        }

        private Delegate Compile()
        {
            if (constructor != null) return constructor;

            // compile the lambda only once
            var custConst = Expression.New(customizerType);
            var newCust = Expression.Lambda(custConst);
            
            return newCust.Compile();
        }

        internal static InternalNodeViewCustomization Create(Type custType)
        {
            // TODO CORESPLIT
            // fail early
            return new InternalNodeViewCustomization(custType);
        }
    }
}

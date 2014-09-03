using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;

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

    internal class InternalNodeCustomization
    {
        private readonly Type customizerType;
        private readonly Delegate constructor;

        internal InternalNodeCustomization(Type customizerType)
        {
            this.customizerType = customizerType;
        }

        internal IDisposable CustomizeView(NodeModel model, dynNodeView view)
        {
            // construct and invoke the customizer
            var customizer = Compile().DynamicInvoke();

            return CustomizeViewInternal((dynamic)customizer, model, view);
        }

        private static IDisposable CustomizeViewInternal<T>(INodeCustomization<T> t, NodeModel model, dynNodeView view) 
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

        internal static InternalNodeCustomization Create(Type custType)
        {
            // fail early
            return new InternalNodeCustomization(custType);
        }
    }
}

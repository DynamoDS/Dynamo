using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.Wpf
{
    public interface INodeCustomization<in T> : IDisposable where T : NodeModel
    {
        void CustomizeView(T model, dynNodeView view);
    }
}
    
        

    ///// <summary>
    ///// As hacky as the name sounds, this method is used to retrieve the 
    ///// "DynamoViewModel" from a given "MenuItem" object. The reason it is
    ///// needed boils down to the fact that we still do "SetupCustomUIElements"
    ///// at the "NodeModel" level. This method will be removed when we 
    ///// eventually refactor "SetupCustomUIElements" out into view layer.
    ///// </summary>
    ///// <param name="menuItem">The MenuItem from which DynamoViewModel is to 
    ///// be retrieved.</param>
    ///// <returns>Returns the corresponding DynamoViewModel retrieved from the 
    ///// given MenuItem.</returns>
    ///// 
    //protected DynamoViewModel GetDynamoViewModelFromMenuItem(MenuItem menuItem)
    //{
    //    if (menuItem == null || (menuItem.Tag == null))
    //        throw new ArgumentNullException("menuItem");

    //    var dynamoViewModel = menuItem.Tag as DynamoViewModel;
    //    if (dynamoViewModel == null)
    //    {
    //        const string message = "MenuItem.Tag is not DynamoViewModel";
    //        throw new ArgumentException(message);
    //    }

    //    return dynamoViewModel;
    //}

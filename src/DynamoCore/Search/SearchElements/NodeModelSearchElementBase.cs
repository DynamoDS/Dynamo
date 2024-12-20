using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Base class for node search elements that can be initialized from TypeLoadData.
    /// </summary>
    public abstract class NodeModelSearchElementBase : NodeSearchElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeModelSearchElementBase"/> class.
        /// </summary>
        /// <param name="typeLoadData">Data to load.</param>
        protected NodeModelSearchElementBase(TypeLoadData typeLoadData)
        {
            Name = typeLoadData.Name;
            foreach (var aka in typeLoadData.AlsoKnownAs.Concat(typeLoadData.SearchKeys))
            {
                SearchKeywords.Add(aka);
                // By default search tag has weight = 0.5
                keywordWeights.Add(0.5);
            }
            FullCategoryName = typeLoadData.Category;
            Description = typeLoadData.Description;
            Assembly = typeLoadData.Assembly.Location;
            inputParameters = typeLoadData.InputParameters.ToList();
            outputParameters = typeLoadData.OutputParameters.ToList();
            iconName = typeLoadData.Type.FullName;
            ElementType = ElementTypes.ZeroTouch;
            if(typeLoadData.IsPackageMember)
                ElementType |= ElementTypes.Packaged;
            if(typeLoadData.Assembly.Location.Contains(PathManager.BuiltinPackagesDirectory))
            {
                ElementType |= ElementTypes.BuiltIn;
            }
            IsExperimental = typeLoadData.IsExperimental;
        }
    }
}

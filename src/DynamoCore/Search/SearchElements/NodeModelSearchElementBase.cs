using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Base class for node search elements that can be initialized from TypeLoadData.
    /// </summary>
    public abstract class NodeModelSearchElementBase : NodeSearchElement
    {
        protected NodeModelSearchElementBase(TypeLoadData typeLoadData)
        {
            Name = typeLoadData.Name;
            foreach (var aka in typeLoadData.AlsoKnownAs.Concat(typeLoadData.SearchKeys))
                SearchKeywords.Add(aka);
            FullCategoryName = typeLoadData.Category;
            Description = typeLoadData.Description;
            Assembly = typeLoadData.Assembly.Location;
            inputParameters = new List<System.Tuple<string, string>>();
            outputParameters = new List<string>();
            iconName = typeLoadData.Type.FullName;
            ElementType = ElementTypeEnum.RegularNode;
        }
    }
}
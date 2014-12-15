using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Search.SearchElements;

namespace Dynamo.Wpf.ViewModels
{
    public abstract class SearchElementBaseViewModel : BrowserItemViewModel
    {
        protected SearchElementBaseViewModel(SearchElementBase element) : base(element) { }
    }

    public class NodeSearchElementViewModel : SearchElementBaseViewModel
    {
        public NodeSearchElementViewModel(NodeSearchElement element) : base(element){}
    }

    public class CustomNodeSearchElementViewModel : SearchElementBaseViewModel
    {
        public CustomNodeSearchElementViewModel(CustomNodeSearchElement element) : base(element) { }
    }

    public class CategorySearchElementViewModel : SearchElementBaseViewModel
    {
        public CategorySearchElementViewModel(CategorySearchElement element) : base(element) { }
    }

    public class DSFunctionNodeSearchElementViewModel : NodeSearchElementViewModel
    {
        public DSFunctionNodeSearchElementViewModel(DSFunctionNodeSearchElement element) : base(element) { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Search;

using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Wpf.ViewModels
{
    public class BrowserRootElementViewModel : BrowserItemViewModel
    {
        public BrowserRootElementViewModel(BrowserRootElement model) : base(model)
        {
        }
    }
}

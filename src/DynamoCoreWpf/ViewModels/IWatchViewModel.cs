using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public interface IWatchViewModel
    {
        DelegateCommand GetBranchVisualizationCommand { get; set; }
        bool WatchIsResizable { get; set; }
        DelegateCommand CheckForLatestRenderCommand { get; set; }
    }
}

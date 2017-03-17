using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Events
{
    public delegate void WorkspaceSettingsChangedEventHandler(WorkspacesSettingsChangedEventArgs args);

    public static class WorkspaceEvents2 
    {
        /// <summary>
        /// An event raised when workspace ScaleFactor setting is changed.
        /// </summary>
        public static event WorkspaceSettingsChangedEventHandler WorkspaceSettingsChanged;
        public static void OnWorkspaceSettingsChanged(double scaleFactor)
        {
            var handler = WorkspaceSettingsChanged;
            if (handler != null)
            {
                handler(new WorkspacesSettingsChangedEventArgs(scaleFactor));
            }
        }
    }

    public class WorkspacesSettingsChangedEventArgs : EventArgs
    {
        public double ScaleFactor { get; private set; }

        public WorkspacesSettingsChangedEventArgs(double scaleFactor)
        {
            ScaleFactor = scaleFactor;
        }
    }
}

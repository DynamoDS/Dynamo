using System;

namespace DynamoServices
{
    public delegate void WorkspaceOpenedEventHandler(WorkspaceOpenedEventArgs args);
    public delegate void WorkspaceClosedEventHandler(WorkspaceClosedEventArgs args);

    public static class WorkspaceEvents
    {
        public static event WorkspaceOpenedEventHandler WorkspaceOpened;
        public static void OnWorkspaceOpened(string name)
        {
            if (WorkspaceOpened != null)
                WorkspaceOpened(new WorkspaceOpenedEventArgs(name));
        }

        public static event WorkspaceClosedEventHandler WorkspaceClosed;
        public static void OnWorkspaceClosed(string name, bool isShutdown)
        {
            if (WorkspaceClosed != null)
                WorkspaceClosed(new WorkspaceClosedEventArgs(name, isShutdown));
        }
    }

    public class WorkspaceOpenedEventArgs : EventArgs
    {
        public string Name { get; internal set; }

        public WorkspaceOpenedEventArgs(string name)
        {
            Name = name;
        }
    }

    public class WorkspaceClosedEventArgs : EventArgs
    {
        public string Name { get; internal set; }
        public bool IsShutDown { get; internal set; }

        public WorkspaceClosedEventArgs(string name, bool isShutdown)
        {
            Name = name;
            IsShutDown = isShutdown;
        }
    }
}

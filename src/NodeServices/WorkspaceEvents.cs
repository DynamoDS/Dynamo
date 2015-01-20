using System;

namespace DynamoServices
{
    public delegate void WorkspaceAddedEventHandler(WorkspacesModificationEventArgs args);
    public delegate void WorkspaceRemovedEventHandler(WorkspacesModificationEventArgs args);

    public static class WorkspaceEvents
    {
        /// <summary>
        /// An event that is triggered when a workspace is added
        /// to the DynamoModel's Workspaces collection.
        /// </summary>
        public static event WorkspaceAddedEventHandler WorkspaceAdded;
        public static void OnWorkspaceAdded(Guid id, string name)
        {
            if (WorkspaceAdded != null)
                WorkspaceAdded(new WorkspacesModificationEventArgs(id,name));
        }

        /// <summary>
        /// An event that is triggered when a workspace is removed
        /// from the DynamoModel's Workspaces collection.
        /// </summary>
        public static event WorkspaceRemovedEventHandler WorkspaceRemoved;
        public static void OnWorkspaceRemoved(Guid id, string name)
        {
            if (WorkspaceRemoved != null)
                WorkspaceRemoved(new WorkspacesModificationEventArgs(id, name));
        }
    }

    public class WorkspacesModificationEventArgs : EventArgs
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }

        public WorkspacesModificationEventArgs(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

﻿using System;

namespace DynamoServices
{
    public delegate void WorkspaceAddedEventHandler(WorkspacesModificationEventArgs args);
    public delegate void WorkspaceRemoveStartedEventHandler(WorkspacesModificationEventArgs args);
    public delegate void WorkspaceRemovedEventHandler(WorkspacesModificationEventArgs args);
    public delegate void WorkspaceClearingEventHandler();
    public delegate void WorkspaceClearedEventHandler();

    public static class WorkspaceEvents
    {
        /// <summary>
        /// An event that is triggered when a workspace is added
        /// to the DynamoModel's Workspaces collection.
        /// </summary>
        public static event WorkspaceAddedEventHandler WorkspaceAdded;
        public static void OnWorkspaceAdded(Guid id, string name, Type type)
        {
            if (WorkspaceAdded != null)
                WorkspaceAdded(new WorkspacesModificationEventArgs(id, name, type));
        }

        /// <summary>
        /// An event that is triggered prior to the removal of the workspace
        /// from the Workspaces collection.
        /// </summary>
        public static event WorkspaceRemoveStartedEventHandler WorkspaceRemoveStarted;
        public static void OnWorkspaceRemoveStarted(Guid id, string name, Type type)
        {
            if (WorkspaceRemoveStarted != null)
                WorkspaceRemoveStarted(new WorkspacesModificationEventArgs(id, name, type));
        }

        /// <summary>
        /// An event that is triggered when a workspace is removed
        /// from the DynamoModel's Workspaces collection.
        /// </summary>
        public static event WorkspaceRemovedEventHandler WorkspaceRemoved;
        public static void OnWorkspaceRemoved(Guid id, string name, Type type)
        {
            if (WorkspaceRemoved != null)
                WorkspaceRemoved(new WorkspacesModificationEventArgs(id, name, type));
        }

        /// <summary>
        /// An event that is triggered before a workspace is cleared.
        /// </summary>
        public static event WorkspaceClearingEventHandler WorkspaceClearing;
        public static void OnWorkspaceClearing()
        {
            if (WorkspaceClearing != null)
                WorkspaceClearing();
        }

        /// <summary>
        /// An event that is triggered after a workspace is cleared.
        /// </summary>
        public static event WorkspaceClearedEventHandler WorkspaceCleared;
        public static void OnWorkspaceCleared()
        {
            if (WorkspaceCleared != null)
                WorkspaceCleared();
        }
    }

    public class WorkspacesModificationEventArgs : EventArgs
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public Type Type { get; internal set; }

        public WorkspacesModificationEventArgs(Guid id, string name, Type type)
        {
            Id = id;
            Name = name;
            Type = type;
        }
    }
}

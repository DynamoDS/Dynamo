using Dynamo.Models;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;

namespace Dynamo.Graph.Workspaces.Locking
{
    internal sealed class GraphLockManager : IDisposable
    {

        internal const int DefaultHeartbeatMilliseconds = 30000;
        private const int StaleFactor = 5;

        private readonly DynamoModel dynamoModel;
        private readonly StringComparer pathComparer;
        private readonly ConcurrentDictionary<string, OwnedLock> locks;
        private readonly ConcurrentDictionary<string, byte> openingPaths;
        private readonly Guid sessionId;
        private readonly int processId;
        private readonly DateTime processStartTimeUtc;
        private readonly string userName;
        private readonly string machineName;
        private readonly string dynamoVersion;
        private readonly int heartbeatMilliseconds;
        private readonly bool enabled;
        private Timer heartbeatTimer;
        private IGraphLockUserPrompt prompt;
        private bool disposed;

        private sealed class OwnedLock
        {
            internal string SidecarPath { get; set; }
            internal GraphLockInfo Info { get; set; }
            internal WorkspaceModel Workspace { get; set; }
        }

        internal GraphLockManager(DynamoModel dynamoModel, IGraphLockUserPrompt prompt = null, int heartbeatMilliseconds = DefaultHeartbeatMilliseconds, bool forceEnable = false)
        {
            this.dynamoModel = dynamoModel;
            this.prompt = prompt;
            this.heartbeatMilliseconds = heartbeatMilliseconds;
            pathComparer = StringComparer.OrdinalIgnoreCase;
            locks = new ConcurrentDictionary<string, OwnedLock>(pathComparer);
            openingPaths = new ConcurrentDictionary<string, byte>(pathComparer);
            sessionId = Guid.NewGuid();
            userName = Environment.UserName;
            machineName = Environment.MachineName;
            dynamoVersion = DynamoModel.Version;

            using (var process = Process.GetCurrentProcess())
            {
                processId = process.Id;
                processStartTimeUtc = GetProcessStartTimeUtc(process);
            }

            enabled = forceEnable || (!DynamoModel.IsTestMode && !DynamoModel.IsHeadless && !dynamoModel.IsServiceMode);
            if (!enabled)
            {
                return;
            }

            dynamoModel.WorkspaceAdded += OnWorkspaceAdded;
            dynamoModel.WorkspaceRemoveStarted += OnWorkspaceRemoveStarted;
            dynamoModel.WorkspaceRemoved += OnWorkspaceRemoved;
            dynamoModel.WorkspaceClearingStarted += OnWorkspaceClearingStarted;
            dynamoModel.ShutdownStarted += OnShutdownStarted;

            AppDomain.CurrentDomain.ProcessExit += ReleaseAll;
            AppDomain.CurrentDomain.UnhandledException += ReleaseAll;

            heartbeatTimer = new Timer(OnHeartbeat, null, this.heartbeatMilliseconds, this.heartbeatMilliseconds);
        }

        internal void SetPrompt(IGraphLockUserPrompt userPrompt)
        {
            prompt = userPrompt;
        }

        internal GraphLockAcquireResult TryAcquire(string graphPath, bool allowPromptUI)
        {
            if (!enabled || string.IsNullOrEmpty(graphPath))
            {
                return GraphLockAcquireResult.Acquired(graphPath);
            }

            var normalizedPath = Path.GetFullPath(graphPath);
            openingPaths[normalizedPath] = 0;

            var result = TryAcquireCore(normalizedPath, allowPromptUI, null);
            if (!IsSamePath(normalizedPath, result.GraphPath))
            {
                openingPaths.TryRemove(normalizedPath, out _);
            }

            return result;
        }

        internal void CompleteOpen(string graphPath, bool succeeded)
        {
            if (!enabled || string.IsNullOrEmpty(graphPath))
            {
                return;
            }

            var normalizedPath = Path.GetFullPath(graphPath);
            openingPaths.TryRemove(normalizedPath, out _);

            if (!succeeded)
            {
                Release(normalizedPath);
            }
        }

        internal void Release(string graphPath)
        {
            if (!enabled || string.IsNullOrEmpty(graphPath))
            {
                return;
            }

            var normalizedPath = Path.GetFullPath(graphPath);
            if (openingPaths.ContainsKey(normalizedPath))
            {
                return;
            }

            if (locks.TryRemove(normalizedPath, out var owned))
            {
                ReleaseOwnedLock(normalizedPath, owned);
            }
        }

        internal void ReleaseAll(object sender = null, EventArgs args = null)
        {
            foreach (var path in locks.Keys.ToList())
            {
                Release(path);
            }

            heartbeatTimer?.Dispose();
            heartbeatTimer = null;
        }

        private GraphLockAcquireResult TryAcquireCore(string normalizedPath, bool allowPromptUI, WorkspaceModel workspace)
        {
            var sidecarPath = GraphLockFile.PathFor(normalizedPath);
            var info = BuildSelfInfo(normalizedPath);
            var attempt = 0;

            while (attempt < 2)
            {
                attempt++;
                try
                {
                    if (GraphLockFile.TryCreateExclusive(sidecarPath, info))
                    {
                        RegisterOwnedLock(normalizedPath, sidecarPath, info, workspace);
                        return GraphLockAcquireResult.Acquired(normalizedPath);
                    }

                    GraphLockInfo existingLock;
                    var readable = GraphLockFile.TryRead(sidecarPath, out existingLock);
                    var isStale = !readable || IsStale(existingLock) || IsDeadLocalProcess(existingLock);

                    if (readable && IsSelf(existingLock))
                    {
                        RegisterOwnedLock(normalizedPath, sidecarPath, existingLock, workspace);
                        return GraphLockAcquireResult.Acquired(normalizedPath);
                    }

                    var response = PromptIfAllowed(normalizedPath, readable ? existingLock : null, isStale, allowPromptUI);
                    switch (response.Decision)
                    {
                        case GraphLockUserDecision.Cancel:
                            return GraphLockAcquireResult.Cancelled(existingLock);
                        case GraphLockUserDecision.SaveAs:
                            return TryCopyToSaveAsPath(normalizedPath, response.SaveAsPath, workspace, existingLock);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    dynamoModel.Logger?.Log("GraphLock unavailable: " + ex.Message);
                }
                catch (SecurityException ex)
                {
                    dynamoModel.Logger?.Log("GraphLock unavailable: " + ex.Message);
                }
                catch (IOException ex)
                {
                    dynamoModel.Logger?.Log("GraphLock unavailable: " + ex.Message);
                }
            }

            return GraphLockAcquireResult.Unavailable(normalizedPath);
        }

        private GraphLockAcquireResult TryCopyToSaveAsPath(
            string sourcePath,
            string saveAsPath,
            WorkspaceModel workspace,
            GraphLockInfo existingLock)
        {
            if (string.IsNullOrWhiteSpace(saveAsPath))
            {
                return GraphLockAcquireResult.Cancelled(existingLock);
            }

            var normalizedSaveAsPath = Path.GetFullPath(saveAsPath);
            if (IsSamePath(sourcePath, normalizedSaveAsPath))
            {
                return GraphLockAcquireResult.Cancelled(existingLock);
            }

            var sidecarPath = GraphLockFile.PathFor(normalizedSaveAsPath);
            var info = BuildSelfInfo(normalizedSaveAsPath);
            var ownsSaveAsLock = false;

            try
            {
                if (GraphLockFile.TryCreateExclusive(sidecarPath, info))
                {
                    ownsSaveAsLock = true;
                }
                else
                {
                    GraphLockInfo saveAsLock;
                    var readable = GraphLockFile.TryRead(sidecarPath, out saveAsLock);
                    if (readable && IsSelf(saveAsLock))
                    {
                        info = saveAsLock;
                        ownsSaveAsLock = true;
                    }
                    else if (!readable || IsStale(saveAsLock) || IsDeadLocalProcess(saveAsLock))
                    {
                        GraphLockFile.WriteHeartbeat(sidecarPath, info);
                        ownsSaveAsLock = true;
                    }
                    else
                    {
                        return GraphLockAcquireResult.Cancelled(saveAsLock);
                    }
                }

                File.Copy(sourcePath, normalizedSaveAsPath, true);
                openingPaths[normalizedSaveAsPath] = 0;
                RegisterOwnedLock(normalizedSaveAsPath, sidecarPath, info, workspace);
                return GraphLockAcquireResult.Acquired(normalizedSaveAsPath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                dynamoModel.Logger?.Log("GraphLock save-as copy failed: " + ex.Message);
                if (ownsSaveAsLock)
                {
                    ReleaseOwnedLock(normalizedSaveAsPath, new OwnedLock { SidecarPath = sidecarPath, Info = info });
                }

                return GraphLockAcquireResult.Cancelled(existingLock);
            }
        }

        private void RegisterOwnedLock(string normalizedPath, string sidecarPath, GraphLockInfo info, WorkspaceModel workspace)
        {
            locks[normalizedPath] = new OwnedLock
            {
                SidecarPath = sidecarPath,
                Info = info,
                Workspace = workspace
            };
        }

        private void OnHeartbeat(object state)
        {
            foreach (var pair in locks.ToList())
            {
                var owned = pair.Value;
                try
                {
                    GraphLockInfo current;
                    if (!GraphLockFile.TryRead(owned.SidecarPath, out current) ||
                        current.SessionId != owned.Info.SessionId)
                    {
                        locks.TryRemove(pair.Key, out _);
                        continue;
                    }

                    owned.Info.LastHeartbeatUtc = DateTime.UtcNow;
                    GraphLockFile.WriteHeartbeat(owned.SidecarPath, owned.Info);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
                {
                    dynamoModel.Logger?.Log("GraphLock heartbeat failed: " + owned.SidecarPath + " - " + ex.Message);
                }
            }
        }

        private void ReleaseOwnedLock(string normalizedPath, OwnedLock owned)
        {
            try
            {
                GraphLockInfo current;
                if (GraphLockFile.TryRead(owned.SidecarPath, out current) &&
                    current.SessionId == owned.Info.SessionId)
                {
                    GraphLockFile.TryDelete(owned.SidecarPath);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                dynamoModel.Logger?.Log("GraphLock release failed: " + normalizedPath + " - " + ex.Message);
            }
        }

        private void OnWorkspaceAdded(WorkspaceModel workspace)
        {
            if (workspace == null || string.IsNullOrEmpty(workspace.FileName))
            {
                return;
            }

            var normalizedPath = Path.GetFullPath(workspace.FileName);
            if (locks.TryGetValue(normalizedPath, out var owned))
            {
                owned.Workspace = workspace;
                workspace.PropertyChanged += OnWorkspacePropertyChanged;
            }
        }

        private void OnWorkspaceRemoveStarted(WorkspaceModel workspace)
        {
            ReleaseWorkspace(workspace);
        }

        private void OnWorkspaceRemoved(WorkspaceModel workspace)
        {
            if (workspace != null)
            {
                workspace.PropertyChanged -= OnWorkspacePropertyChanged;
            }
        }

        private void OnWorkspaceClearingStarted(WorkspaceModel workspace)
        {
            ReleaseWorkspace(workspace);
        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(WorkspaceModel.FileName))
            {
                return;
            }

            var workspace = sender as WorkspaceModel;
            if (workspace == null || string.IsNullOrEmpty(workspace.FileName))
            {
                return;
            }

            var normalizedPath = Path.GetFullPath(workspace.FileName);
            var oldPaths = locks
                .Where(pair => pair.Value.Workspace == workspace && !IsSamePath(pair.Key, normalizedPath))
                .Select(pair => pair.Key)
                .ToList();
            foreach (var oldPath in oldPaths)
            {
                Release(oldPath);
            }

            if (!locks.ContainsKey(normalizedPath))
            {
                TryAcquireCore(normalizedPath, false, workspace);
            }
        }

        private void OnShutdownStarted(DynamoModel model)
        {
            ReleaseAll();
        }

        private void ReleaseWorkspace(WorkspaceModel workspace)
        {
            if (workspace == null)
            {
                return;
            }

            var paths = locks
                .Where(pair => pair.Value.Workspace == workspace)
                .Select(pair => pair.Key)
                .ToList();

            foreach (var path in paths)
            {
                Release(path);
            }
        }

        private GraphLockUserResponse PromptIfAllowed(
            string graphPath,
            GraphLockInfo existingLock,
            bool isStale,
            bool allowPromptUI)
        {
            if (!allowPromptUI || prompt == null)
            {
                return GraphLockUserResponse.Cancel();
            }

            return prompt.AskUser(graphPath, existingLock, isStale);
        }

        private bool IsStale(GraphLockInfo existingLock)
        {
            if (existingLock == null)
            {
                return true;
            }

            var ageSeconds = (DateTime.UtcNow - existingLock.LastHeartbeatUtc).TotalSeconds;
            return ageSeconds > (heartbeatMilliseconds / 1000.0) * StaleFactor;
        }

        private bool IsSelf(GraphLockInfo existingLock)
        {
            return existingLock != null &&
                   (existingLock.SessionId == sessionId ||
                    (string.Equals(existingLock.MachineName, machineName, StringComparison.OrdinalIgnoreCase) &&
                     existingLock.ProcessId == processId &&
                     existingLock.ProcessStartUtc == processStartTimeUtc));
        }

        private GraphLockInfo BuildSelfInfo(string normalizedPath)
        {
            var now = DateTime.UtcNow;

            return new GraphLockInfo
            {
                SchemaVersion = 1,
                SessionId = sessionId,
                GraphPath = normalizedPath,
                UserName = userName,
                MachineName = machineName,
                ProcessId = processId,
                ProcessStartUtc = processStartTimeUtc,
                DynamoVersion = dynamoVersion,
                AcquiredUtc = now,
                LastHeartbeatUtc = now
            };
        }

        private bool IsDeadLocalProcess(GraphLockInfo existingLock)
        {
            if (existingLock == null ||
                !string.Equals(existingLock.MachineName, machineName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                using (var process = Process.GetProcessById(existingLock.ProcessId))
                {
                    return GetProcessStartTimeUtc(process) != existingLock.ProcessStartUtc;
                }
            }
            catch (ArgumentException)
            {
                return true;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
            catch (Exception ex)
            {
                dynamoModel.Logger?.Log("GraphLock process liveness check failed: " + ex.Message);
                return false;
            }
        }

        private bool IsSamePath(string firstPath, string secondPath)
        {
            if (string.IsNullOrEmpty(firstPath) || string.IsNullOrEmpty(secondPath))
            {
                return false;
            }

            return pathComparer.Equals(Path.GetFullPath(firstPath), Path.GetFullPath(secondPath));
        }

        private static DateTime GetProcessStartTimeUtc(Process process)
        {
            try
            {
                return process.StartTime.ToUniversalTime();
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            dynamoModel.WorkspaceAdded -= OnWorkspaceAdded;
            dynamoModel.WorkspaceRemoveStarted -= OnWorkspaceRemoveStarted;
            dynamoModel.WorkspaceRemoved -= OnWorkspaceRemoved;
            dynamoModel.WorkspaceClearingStarted -= OnWorkspaceClearingStarted;
            dynamoModel.ShutdownStarted -= OnShutdownStarted;

            AppDomain.CurrentDomain.ProcessExit -= ReleaseAll;
            AppDomain.CurrentDomain.UnhandledException -= ReleaseAll;

            ReleaseAll();
        }
    }
}

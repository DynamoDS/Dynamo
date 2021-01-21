using System;
using System.Threading;
using Dynamo.Graph.Workspaces;

namespace Dynamo.Core
{
    /// <summary>
    /// This class enables the delay of graph execution.
    /// Use instances of this class to specify a code scope in which you want graph execution to be delayed. 
    /// Class is thread safe. 
    /// Nested instance of this class do not have a well defined behavior.
    /// </summary>
    internal class DelayedGraphExecution : IDisposable
    {
        private readonly WorkspaceModel workspace;
        private static int counter;
        private readonly object stateMutex = new object();

        public static bool IsEnabled { get { return counter > 0; } }

        public DelayedGraphExecution(WorkspaceModel wModel)
        {
            lock (stateMutex)
            {
                counter++;
            }
            workspace = wModel;
        }

        public virtual void Dispose()
        {
            lock (stateMutex)
            {
                counter--;
            }
            workspace.RequestRun();
        }
    }
}

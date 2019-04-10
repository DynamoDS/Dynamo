using System;

namespace Dynamo.Engine.CodeGeneration
{
    /// <summary>
    /// Provides AST compilation related events. Clients can provide more AST
    /// to be included during compilation events.
    /// </summary>
    public static class AstCompilationEvents
    {
        /// <summary>
        /// Compilation for a node has begun.
        /// </summary>
        public static event EventHandler<CompilationEventArgs> PreCompilation;

        /// <summary>
        /// Compilation for a node has completed.
        /// </summary>
        public static event EventHandler<CompilationEventArgs> PostCompilation;

        /// <summary>
        /// Notifies pre compilation event
        /// </summary>
        internal static void NotifyPreCompilation(object sender, CompilationEventArgs args)
        {
            PreCompilation?.Invoke(sender, args);
        }

        /// <summary>
        /// Notifies post compilation event.
        /// </summary>
        internal static void NotifyPostCompilation(object sender, CompilationEventArgs args)
        {
            PostCompilation?.Invoke(sender, args);
        }
    }
}

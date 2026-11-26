namespace FFITarget
{
    /// <summary>
    /// Test helper for the profiling tests (DYN-11168). Each node that calls one of these functions
    /// increments a shared counter when it executes, so a test can read the counter from the node's
    /// NodeExecutionBegin and NodeExecutionEnd events and tell whether the node executed between them.
    /// </summary>
    public static class ProfilingCounter
    {
        /// <summary>
        /// How many times any ProfilingCounter function has executed since the last <see cref="Reset"/>.
        /// </summary>
        public static int Count { get; private set; }

        /// <summary>
        /// Sets the counter back to zero.
        /// </summary>
        public static void Reset()
        {
            Count = 0;
        }

        /// <summary>
        /// Increments the counter. Takes no inputs, so a node calling it has no input ports.
        /// </summary>
        /// <returns>The counter after incrementing.</returns>
        public static int Increment()
        {
            Count++;
            return Count;
        }

        /// <summary>
        /// Increments the counter. Takes an input, so a node calling it can be wired downstream of another node.
        /// </summary>
        /// <param name="previous">Any value; used only to give the node an input.</param>
        /// <returns>The counter after incrementing.</returns>
        public static int IncrementAfter(int previous)
        {
            Count++;
            return Count;
        }
    }
}

namespace DSCore
{
    /// <summary>
    ///     Functions for manipulating evaluation threads.
    /// </summary>
    public static class Thread
    {
        /// <summary>
        ///     Pauses the current evaluation thread for a given amount of time.
        /// </summary>
        /// <param name="x">Object to pass through.</param>
        /// <param name="msTimeout">
        ///     Amount of time to pause the thread, in milliseconds.
        /// </param>
        /// <returns name="x">Object passed through.</returns>
        public static object Pause(object x, int msTimeout)
        {
            System.Threading.Thread.Sleep(msTimeout);
            return x;
        }
    }
}

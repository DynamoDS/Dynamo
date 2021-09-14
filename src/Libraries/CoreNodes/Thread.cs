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
        /// <param name="object">Object to pass through.</param>
        /// <param name="msTimeout">
        ///     Amount of time to pause the thread, in milliseconds.
        /// </param>
        /// <returns name="object">Object passed through.</returns>
        public static object Pause(object @object, int msTimeout)
        {
            System.Threading.Thread.Sleep(msTimeout);
            return @object;
        }
    }
}

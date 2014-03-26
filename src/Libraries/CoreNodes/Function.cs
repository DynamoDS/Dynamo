using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    /// <summary>
    /// TODO: Move contents somewhere else.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class Function
    {
        /// <summary>
        ///     Returns whatever is passed in.
        /// </summary>
        /// <param name="x">Anything.</param>
        /// <returns name="x">The same as the input.</returns>
        /// <search>identity,function</search>
       [IsVisibleInDynamoLibrary(false)] 
        public static object Identity(object x)
        {
            return x;
        }
    }
}

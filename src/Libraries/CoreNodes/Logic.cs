namespace DSCore
{
    /// <summary>
    ///     Boolean logic methods.
    /// </summary>
    public static class Logic
    {
        /// <summary>
        ///     Boolean XOR: Returns true if and only if exactly one of the inputs is true.
        /// </summary>
        /// <param name="a">A boolean.</param>
        /// <param name="b">A boolean.</param>
        public static bool Xor(bool a, bool b)
        {
            return a ^ b;
        }
    }
}
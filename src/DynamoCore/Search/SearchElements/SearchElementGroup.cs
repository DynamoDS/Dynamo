namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// Returns one of the possible values: None, Create, Action, Query.
    /// E.g. Point.ByCoordinates is member of create group.
    /// </summary>
    public enum SearchElementGroup
    {
        None, Create, Action, Query
    }

    /// <summary>
    /// Enumeration of external elements' types
    /// </summary>
    [System.Flags]
    public enum ElementTypes
    {
        /// <summary>
        /// Represents no type
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Type for all the DLLs (can be built-in, third-party, and packaged)
        /// </summary>
        ZeroTouch = 0x00000001,
        /// <summary>
        /// Type for all the DYFs (both loose DYF files and packaged)
        /// </summary>
        CustomNode = 0x00000002,
        /// <summary>
        /// Type for things that Dynamo ships with (operator, geometries...)
        /// </summary>
        BuiltIn = 0x00010000,
        /// <summary>
        /// Type for a packaged element (either zero-touch DLLs or DYFs)
        /// </summary>
        Packaged = 0x00020000,
    }
}

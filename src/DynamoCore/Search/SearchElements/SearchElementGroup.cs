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

    [System.Flags]
    public enum ElementTypes
    {
        None = 0x00000000,
        ZeroTouch = 0x00000001, // All the DLLs (can be built-in, third-party, and packaged)
        CustomNode = 0x00000002, // All the DYFs (both loose DYF files and packaged)
        BuiltIn = 0x00010000, // Things that Dynamo ships with (operator, geometries...)
        Packaged = 0x00020000, // Packaged element (either zero-touch DLLs or DYFs)
    }
}

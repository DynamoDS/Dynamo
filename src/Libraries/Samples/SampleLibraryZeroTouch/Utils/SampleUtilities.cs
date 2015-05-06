using Autodesk.DesignScript.Runtime;

namespace SampleLibraryZeroTouch
{
    /// <summary>
    /// A utility library containing methods that can be called 
    /// from NodeModel nodes, or used as nodes in Dynamo.
    /// </summary>
    public static class SampleUtilities
    {
        [IsVisibleInDynamoLibrary(false)]
        public static double MultiplyInputByNumber(double input)
        {
            return input * 42;
        }
    }
}

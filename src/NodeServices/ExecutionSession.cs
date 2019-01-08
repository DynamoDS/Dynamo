using System.Collections.Generic;

namespace Dynamo.Session
{
    /// <summary>
    /// Represents a session object for current execution.
    /// </summary>
    public interface IExecutionSession
    {
        /// <summary>
        /// File path for the current workspace in execution. Could be null or
        /// empty string if workspace is not yet saved.
        /// </summary>
        string CurrentWorkspacePath { get; }

        /// <summary>
        /// Returns session parameter value for the given parameter name.
        /// </summary>
        /// <param name="parameter">Name of session parameter</param>
        /// <returns>Session parameter value as object</returns>
        object GetParameterValue(string parameter);

        /// <summary>
        /// Returns list of session parameter keys available in the session.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetParameterKeys();

        /// <summary>
        /// A helper method to resolve the given file path. The given file path
        /// will be resolved by searching into the current workspace, core and 
        /// host application installation folders etc.
        /// </summary>
        /// <param name="filepath">Input file path</param>
        /// <returns>True if the file is found</returns>
        bool ResolveFilePath(ref string filepath);
    }

    /// <summary>
    /// List of possible session parameter keys to obtain the session parameters.
    /// </summary>
    public class ParameterKeys
    {
        /// <summary>
        /// This key is used to get the full path for library, which implements 
        /// IGeometryFactory interface.
        /// </summary>
        public static readonly string GeometryFactory = "GeometryFactoryFileName";

        /// <summary>
        /// This key is used to get the number format used by Dynamo to format 
        /// the preview values. The returned value is string.
        /// </summary>
        public static readonly string NumberFormat = "NumberFormat";

        /// <summary>
        /// This key is used to get the Major version of Dynamo. The returned value
        /// is a number.
        /// </summary>
        public static readonly string MajorVersion = "MajorFileVersion";

        /// <summary>
        /// This key is used to get the Major version of Dynamo. The returned value
        /// is a number.
        /// </summary>
        public static readonly string MinorVersion = "MinorFileVersion";

        /// <summary>
        /// The duration of an execution covered by an <see cref="IExecutionSession"/>
        /// </summary>
        public static readonly string LastExecutionDuration = "LastExecutionDuration";
    }
}

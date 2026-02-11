using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dynamo.Events;

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
    //TODO(DYN-2879) make this class static for Dynamo 3. Maybe an enum?
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

        /// <summary>
        /// The current collection of packagepaths that Dynamo is loading packages from.
        /// The Return value is IEnumerable
        /// </summary>
        public static readonly string PackagePaths = nameof(PackagePaths);

        /// <summary>
        /// The logger that logs to the Dynamo console.
        /// The return value is an ILogger
        /// </summary>
        public static readonly string Logger = nameof(Logger);

        /// <summary>
        /// True if Dynamo is used in offline mode.
        /// </summary>
        public static readonly string NoNetworkMode = nameof(NoNetworkMode);
    }

    public static class ExecutionSessionHelper
    {
        /// <summary>
        /// Throw exception in no-network mode.
        /// This helper method can be used to display a warning on a node that
        /// needs to be prevented from running when no-network mode is enabled.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ThrowIfNoNetworkMode()
        {
            var session = ExecutionEvents.ActiveSession;
            if (session != null)
            {
                if ((bool)session.GetParameterValue(ParameterKeys.NoNetworkMode))
                {
                    throw new InvalidOperationException(DynamoServices.Properties.Resources.WebRequestOfflineWarning);
                }
            }
        }
    }

    /// <summary>
    /// Custom HTTP message handler to simulate an "offline mode" (no-network mode) for network requests. 
    /// It can be used to intercept all outgoing HTTP requests and returns a predefined response indicating that the
    /// application is offline.
    /// </summary>
    public class NoNetworkModeHandler : DelegatingHandler
    {
        public NoNetworkModeHandler() : base(new HttpClientHandler())
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var json = "{\"success\":false,\"message\":\"Application is in offline mode\",\"content\":null}";
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
                ReasonPhrase = "Offline Mode Enabled"
            };
            return Task.FromResult(response);
        }
    }
}

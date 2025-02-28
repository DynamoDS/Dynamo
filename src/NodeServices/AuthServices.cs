using System;
using System.Collections.Generic;
using System.Text;

namespace DynamoServices
{

    /// <summary>
    /// Event arguments for the RequestAuthProvider event.
    /// </summary>
    public class RequstAuthProviderEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the found auth provider.
        /// </summary>
        public object FoundAuthProvider { get; internal set; }
    }

    /// <summary>
    /// Delegate for handling the RequestAuthProvider event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public delegate void RequestAuthProviderEventHandler(RequstAuthProviderEventArgs args);


    /// <summary>
    /// Static class containing authentication service events. Useful if you want to make authenticated requests from your nodes.
    /// </summary>
    public static class AuthServicesEvents
    {
        /// <summary>
        /// Event triggered to request an authentication provider.
        /// </summary>
        internal static event RequestAuthProviderEventHandler RequestAuthProvider;

        /// <summary>
        /// Invokes the RequestAuthProvider event and returns an IOAuth2AccessTokenProvider if one is found.
        /// </summary>
        /// <returns>The found IOAuth2AccessTokenProvider.</returns>
        public static object OnRequestAuthProvider()
        {
            var args = new RequstAuthProviderEventArgs();
            RequestAuthProvider?.Invoke(args);
            return args.FoundAuthProvider;
        }
    }
}

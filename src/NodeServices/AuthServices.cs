using System;
using System.Collections.Generic;
using System.Text;

namespace DynamoServices
{

    /// <summary>
    /// Event arguments for the RequestAuthProvider event.
    /// </summary>
    internal class RequestAuthProviderEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the found auth provider, if one is found. It's possible that
        /// this willl be null.
        /// </summary>
        internal object AuthProvider { get; set; }
    }

    /// <summary>
    /// Delegate for handling the RequestAuthProvider event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    internal delegate void RequestAuthProviderEventHandler(RequestAuthProviderEventArgs args);


    /// <summary>
    /// Static class containing authentication service events. Useful if you want to make authenticated requests from your nodes.
    /// </summary>
    #if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.Experimental("AUTH_SERVICES")]
#else
        [Obsolete("This method is for evaluation purposes only and is subject to change or removal in future updates.")]
#endif
    public static class AuthServicesEvents
    {
        /// <summary>
        /// Event triggered to request an authentication provider.
        /// </summary>
        internal static event RequestAuthProviderEventHandler RequestAuthProvider;

        /// <summary>
        /// Invokes the RequestAuthProvider event and returns an IOAuth2AccessTokenProvider if one is found.
        /// If no auth provider is found, this will return null.
        /// </summary>
        /// <returns>The found IOAuth2AccessTokenProvider.</returns>
#if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.Experimental("REQUEST_AUTHPROVIDER")]
#else
        [Obsolete("This method is for evaluation purposes only and is subject to change or removal in future updates.")]
#endif
        public static object OnRequestAuthProvider()
        {
            var args = new RequestAuthProviderEventArgs();
            RequestAuthProvider?.Invoke(args);
            return args.AuthProvider;
        }
    }
}

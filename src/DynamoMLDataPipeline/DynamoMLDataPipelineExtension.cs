using Dynamo.Extensions;
using Greg;

namespace DynamoMLDataPipeline
{

    internal class DynamoMLDataPipelineExtension : IExtension
    {
        private ReadyParams ReadyParams;

        /// <summary>
        ///     Dynamo Package Manager Instance.
        /// </summary>
        internal DynamoMLDataPipeline DynamoMLDataPipeline { get; set; }

        public string UniqueId
        {
            get { return "FCABC211-D56B-4109-AF18-F434DFE48138"; }
        }

        public string Name { get { return "DynamoMLDataPipelineExtension"; } }

        public void Startup(StartupParams sp)
        {
            DynamoMLDataPipeline = new DynamoMLDataPipeline();
            DynamoMLDataPipeline.DynamoVersion = sp.DynamoVersion;

            // In no-network mode, do not wire the auth providers used for data upload. This is
            // defensive symmetry: the pipeline never transmits without user action, and the
            // AuthProvider is already null when no-network mode is on (IDSDK is not constructed).
            if (sp.NoNetworkMode)
            {
                return;
            }

            if (sp.AuthProvider is IOAuth2AccessTokenProvider tokenProvider)
            {
                DynamoMLDataPipeline.AuthTokenProvider = tokenProvider;
            }
 
            if (sp.AuthProvider is IOAuth2UserIDProvider userIdProvider)
            {
                DynamoMLDataPipeline.AuthUserInfoProvider = userIdProvider;
            }
        }

        public void Ready(ReadyParams sp)
        {
            ReadyParams = sp;
        }

        public void Shutdown()
        {
           // do nothing.
        }

        public void Dispose()
        {
            // do nothing.
        }
    }
}

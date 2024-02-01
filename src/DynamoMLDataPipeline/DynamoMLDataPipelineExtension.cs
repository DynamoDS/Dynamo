using System;
using System.Collections.Generic;
using Dynamo.Extensions;
using Dynamo.Logging;
using Greg;

namespace DynamoMLDataPipeline
{

    internal class DynamoMLDataPipelineExtension : IExtension, IExtensionSource
    {
        private ReadyParams ReadyParams;
        private DynamoLogger logger;

        /// <summary>
        ///     Dynamo Package Manager Instance.
        /// </summary>
        internal DynamoMLDataPipeline DynamoMLDataPipeline { get; set; }

        public string UniqueId
        {
            get { return "FCABC211-D56B-4109-AF18-F434DFE48138"; }
        }

        public string Name { get { return "DynamoMLDataPipelineExtension"; } }

        public IEnumerable<IExtension> RequestedExtensions => throw new NotImplementedException();

        public event Func<string, IExtension> RequestLoadExtension;
        public event Action<IExtension> RequestAddExtension;

        public void Startup(StartupParams sp)
        {
            DynamoMLDataPipeline = new DynamoMLDataPipeline();
            DynamoMLDataPipeline.AuthTokenProvider = (IOAuth2AccessTokenProvider)sp.AuthProvider;
        }

        public void Ready(ReadyParams sp)
        {
            ReadyParams = sp;
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

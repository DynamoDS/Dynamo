using System;
using Dynamo.Interfaces;
using ProtoScript.Runners;

namespace Dynamo.Library
{
    class GeometryPreloader
    {
        private readonly IGeometryConfiguration configuration;

        internal GeometryPreloader(IGeometryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.configuration = configuration;
        }
    }
}

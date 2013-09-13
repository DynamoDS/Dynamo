using System;

namespace Dynamo.Models
{
    internal class Migration
    {
        /// <summary>
        /// A version after which this migration will be applied.
        /// </summary>
        public Version Version { get; set; }
        
        /// <summary>
        /// The action to perform during the upgrade.
        /// </summary>
        public Action Upgrade { get; set; }

        /// <summary>
        /// A migration which can be applied to a workspace to upgrade the workspace to the current version.
        /// </summary>
        /// <param name="v">A version number specified as x.x.x.x after which a workspace will be upgraded</param>
        /// <param name="upgrade">The action to perform during the upgrade.</param>
        public Migration(Version v, Action upgrade)
        {
            Version = v;
            Upgrade = upgrade;
        }
    }
}

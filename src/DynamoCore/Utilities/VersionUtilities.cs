using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;

namespace Dynamo.Utilities
{
    public static class VersionUtilities
    {
        /// <summary>
        /// Parse the first n fields of a version string.  Delegates to
        /// Version.Parse.
        /// </summary>
        public static Version PartialParse(string versionString, int numberOfFields = 3)
        {
            var splitVersion = versionString.Split('.');
            if (splitVersion.Length < numberOfFields)
                throw new FormatException("You specified too many fields for the given string.");

            var rejoinedVersion = string.Join(".", splitVersion.Take(numberOfFields));

            return Version.Parse(rejoinedVersion);
        }
    }

    public static class WorkspaceUtilities
    {
        internal static void GatherAllUpstreamNodes(NodeModel nodeModel,
            List<NodeModel> gathered, Predicate<NodeModel> match)
        {
            if ((nodeModel == null) || gathered.Contains(nodeModel))
                return; // Look no further, node is already in the list.

            gathered.Add(nodeModel); // Add to list first, avoiding re-entrant.
            if (!match(nodeModel)) // Determine if the search should proceed.
                return;

            foreach (var upstreamNode in nodeModel.InputNodes)
            {
                if (upstreamNode.Value == null)
                    continue;

                // Add all the upstream nodes found into the list.
                GatherAllUpstreamNodes(upstreamNode.Value.Item2, gathered, match);
            }
        }
    }

}

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

    public static class NodeModelExtensions
    {
        internal static IEnumerable<NodeModel> UpstreamNodes(this NodeModel node, List<NodeModel> gathered, Predicate<NodeModel> match)
        {
            var upstream = node.InPorts.SelectMany(p => p.Connectors.Select(c=>c.Start.Owner)).
                Where(n=>match(n)).
                ToList();

            foreach (var n in upstream)
            {
                if (!gathered.Contains(n))
                {
                    gathered.Add(n);
                }
            }

            foreach (var n in upstream)
            {
                n.UpstreamNodes(gathered, match);
            }

            return gathered;
        }
    }

}

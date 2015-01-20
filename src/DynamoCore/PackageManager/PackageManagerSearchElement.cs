using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dynamo.Search.SearchElements;
using Dynamo.Utilities;

using Greg.Responses;

namespace Dynamo.PackageManager
{

    /// <summary>
    /// A search element representing an element from the package manager </summary>
    public class PackageManagerSearchElement : SearchElementBase
    {

        #region Properties

        public string Maintainers { get { return String.Join(", ", this.Header.maintainers.Select(x => x.username)); } }
        private int _votes;
        public int Votes
        {
            get { return _votes; }
            set { _votes = value; RaisePropertyChanged("Votes"); }
        }
        public bool IsDeprecated { get { return this.Header.deprecated; } }
        public int Downloads { get { return this.Header.downloads; } }
        public string EngineVersion { get { return Header.versions[Header.versions.Count - 1].engine_version; } }
        public int UsedBy { get { return this.Header.used_by.Count; } }
        public string LatestVersion { get { return Header.versions[Header.versions.Count - 1].version; } }
        public string LatestVersionCreated { get { return Header.versions[Header.versions.Count - 1].created; } }

        /// <summary>
        /// Header property </summary>
        /// <value>
        /// The PackageHeader used to instantiate this object </value>
        public Greg.Responses.PackageHeader Header { get; internal set; }

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        public override string Type { get { return "Community Node"; } }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        public override string Name { get { return Header.name; } }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        public override string Description { get { return Header.description ?? ""; } }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search. 
        /// Higher = closer to the top of search results </value>
        public override double Weight { get; set; }

        public override bool Searchable { get { return true; } }

        /// <summary>
        /// Guid property </summary>
        /// <value>
        /// A string that uniquely defines the CustomNodeDefinition </value>
        public Guid Guid { get; internal set; }

        /// <summary>
        /// Id property </summary>
        /// <value>
        /// A string that uniquely defines the Package on the server  </value>
        public string Id { get { return Header._id; } }

        public override string Keywords { get; set; }

        public string SiteUrl { get { return Header.site_url; } }
        public string RepositoryUrl { get { return Header.repository_url; } }

        #endregion

        private readonly PackageManagerClient client;

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="header">The PackageHeader object describing the element</param>
        public PackageManagerSearchElement(PackageManagerClient client, Greg.Responses.PackageHeader header)
        {
            this.client = client;

            this.IsExpanded = false;
            this.Header = header;
            this.Weight = header.deprecated ? 0.1 : 1;

            if (header.keywords != null && header.keywords.Count > 0)
            {
                this.Keywords = String.Join(" ", header.keywords);
            } 
            else
            {
                this.Keywords = "";
            }
            this.Votes = header.votes;
        }

        public void Upvote()
        {
            Task<bool>.Factory.StartNew(() => client.Upvote(this.Id))
                .ContinueWith((t) =>
                {
                    if (t.Result)
                    {
                        this.Votes += 1;
                    }
                }
                , TaskScheduler.FromCurrentSynchronizationContext());

        }

        public bool CanUpvote()
        {
            return client != null && client.HasAuthenticator;
        }

        public void Downvote()
        {
            Task<bool>.Factory.StartNew(() => client.Downvote(this.Id))
                .ContinueWith((t) =>
                {
                    if (t.Result)
                    {
                        this.Votes -= 1;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public bool CanDownvote()
        {
            return client != null && client.HasAuthenticator;
        }

        public static IEnumerable<Tuple<PackageHeader, PackageVersion>> ListRequiredPackageVersions(
            IEnumerable<PackageHeader> headers, PackageVersion version)
        {
            return headers.Zip(
                version.full_dependency_versions,
                (header, v) => new Tuple<PackageHeader, string>(header, v))
                .Select(
                    (pair) =>
                        new Tuple<PackageHeader, PackageVersion>(
                        pair.Item1,
                        pair.Item1.versions.First(x => x.version == pair.Item2)));
        }

       

    }

}

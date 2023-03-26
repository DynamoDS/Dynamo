using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes;
using Lucene.Net.Documents;

namespace Dynamo.Search.SearchElements
{
    internal class PackageSearchElement : NodeSearchElement
    {
        /// <summary>
        /// Name of the package the node belogs to.
        /// </summary>
        internal string PackageName { get; set; }
        /// <summary>
        /// Version of the package the node belogs to.
        /// </summary>
        internal string PackageVersion { get; set; }

        protected override NodeModel ConstructNewNodeModel()
        {
            return null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="docName"></param>
        public PackageSearchElement(Document pkgNode)
        {
            Name = pkgNode.Get("Name");
            //FullCategoryName = pkgNode.Get("Category");
            Description = pkgNode.Get("Description");
            PackageName = pkgNode.Get("PackageName");
            PackageVersion = pkgNode.Get("PackageVersion");
            ElementType = ElementTypes.Packaged;
        }
    }
}

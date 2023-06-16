using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Util;

namespace Dynamo.Configuration
{
    /// <summary>
    /// This class includes all settings of Lucene search
    /// </summary>
    internal class LuceneConfig
    {
        /// <summary>
        /// Specify the Lucene.Net compatibility version
        /// </summary>
        public static LuceneVersion LuceneNetVersion = LuceneVersion.LUCENE_48;

        /// <summary>
        /// Default operator for Lucence query parser
        /// </summary>
        internal static Operator DefaultOperator = Operator.OR;

        internal static float MinimumSimilarity = 0.5f;

        internal static int DefaultResultsCount = 50;

        /// <summary>
        /// This represent the fields that will be indexed when initializing Lucene Search
        /// </summary>
        public enum IndexFieldsEnum
        {
            /// <summary>
            /// Name - The name of the node
            /// </summary>
            Name,

            /// <summary>
            /// FullCategoryName - The category of the node
            /// </summary>
            FullCategoryName,

            /// <summary>
            /// Description - The description of the node
            /// </summary>
            Description,

            /// <summary>
            /// SearchKeywords - Several keywords that will be used when searching any word (this values are coming from xml files like BuiltIn.xml, DesignScriptBuiltin.xml or ProtoGeometry.xml)
            /// </summary>
            SearchKeywords,

            /// <summary>
            /// DocName - Name of the Document
            /// </summary>
            DocName,

            /// <summary>
            /// Documentation - Documentation of the node
            /// </summary>
            Documentation
        }

        /// <summary>
        /// Fields to be indexed by Lucene Search
        /// </summary>
        public static string[] IndexFields = { nameof(IndexFieldsEnum.Name),
                                               nameof(IndexFieldsEnum.FullCategoryName),
                                               nameof(IndexFieldsEnum.Description),
                                               nameof(IndexFieldsEnum.SearchKeywords),
                                               nameof(IndexFieldsEnum.DocName),
                                               nameof(IndexFieldsEnum.Documentation)};
    }
}

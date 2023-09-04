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

        /// <summary>
        /// MinimumSimilarity value
        /// </summary>
        internal static float MinimumSimilarity = 0.5f;

        /// <summary>
        /// Minimal length of the search term that the fuzzy search to take effect
        /// </summary>
        internal static int FuzzySearchMinimalTermLength = 4;

        /// <summary>
        /// Minimal edits for typo check in FuzzyQuery
        /// </summary>
        internal static int FuzzySearchMinEdits = 1;

        /// <summary>
        /// Minimal length of the search term that the fuzzy search max edits limit to take effect
        /// </summary>
        internal static int FuzzySearchMaxEditsThreshold = 7;

        /// <summary>
        /// Maximal edits for typo check in FuzzyQuery, value larger than 3 is not proper according to
        /// https://blog.mikemccandless.com/2011/03/lucenes-fuzzyquery-is-100-times-faster.html
        /// </summary>
        internal static int FuzzySearchMaxEdits = 2;

        /// <summary>
        /// Default max results count in Dynamo to display
        /// </summary>
        internal static int DefaultResultsCount = 50;


        #region Field Weights
        /// <summary>
        /// Search name matching weight
        /// </summary>
        internal static int SearchNameWeight = 10;

        /// <summary>
        /// Search Category matching weight
        /// </summary>
        internal static int SearchCategoryWeight = 9;

        /// <summary>
        /// Search Description matching weight
        /// </summary>
        internal static int SearchDescriptionWeight = 6;

        /// <summary>
        /// Search tags matching weight
        /// </summary>
        internal static int SearchTagsWeight = 6;

        /// <summary>
        /// other fields search matching weight
        /// </summary>
        internal static int SearchMetaFieldsWeight = 6;

        #endregion


        #region Wildcards Field Weights
        /// <summary>
        /// Wildcards search name matching weight
        /// </summary>
        internal static int WildcardsSearchNameWeight = 7;

        /// <summary>
        /// Wildcards search Category matching weight
        /// </summary>
        internal static int WildcardsSearchCategoryWeight = 6;

        /// <summary>
        /// Wildcards search Description matching weight
        /// </summary>
        internal static int WildcardsSearchDescriptionWeight = 4;

        /// <summary>
        /// Wildcards search tags matching weight
        /// </summary>
        internal static int WildcardsSearchTagsWeight = 4;

        /// <summary>
        /// other wildcards fields search matching weight
        /// </summary>
        internal static int WildcardsSearchMetaFieldsWeight = 4;

        /// <summary>
        /// Wildcards search name matching weight
        /// </summary>
        internal static int WildcardsSearchNameParsedWeight = 5;

        #endregion

        /// <summary>
        /// Fuzzy search matching weight
        /// </summary>
        internal static int FuzzySearchWeight = 2;

        /// <summary>
        /// Parent directory where information is indexed.
        /// </summary>
        internal static string Index = "Index";

        /// <summary>
        /// Directory where Nodes info are indexed
        /// </summary>
        internal static string NodesIndexingDirectory = "Nodes";

        /// <summary>
        /// Directory where packages info are indexed
        /// </summary>
        internal static string PackagesIndexingDirectory = "Packages";

        /// <summary>
        /// This represent the fields that will be indexed when initializing Lucene Search
        /// </summary>
        public enum NodeFieldsEnum
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
            Documentation,

            /// <summary>
            /// Hosts - Package hosts
            /// </summary>
            Hosts,

            /// <summary>
            /// Node Input Parameters as string (there are nodes with same name and category but different parameters)
            /// </summary>
            Parameters
        }

        /// <summary>
        /// Nodes Fields to be indexed by Lucene Search
        /// </summary>
        public static string[] NodeIndexFields = { nameof(NodeFieldsEnum.Name),
                                                   nameof(NodeFieldsEnum.FullCategoryName),
                                                   nameof(NodeFieldsEnum.Description),
                                                   nameof(NodeFieldsEnum.SearchKeywords),
                                                   nameof(NodeFieldsEnum.DocName),
                                                   nameof(NodeFieldsEnum.Documentation),
                                                   nameof(NodeFieldsEnum.Parameters)};


        /// <summary>
        /// Package Fields to be indexed by Lucene Search
        /// </summary>
        public static string[] PackageIndexFields = { nameof(NodeFieldsEnum.Name),
                                                      nameof(NodeFieldsEnum.Description),
                                                      nameof(NodeFieldsEnum.SearchKeywords),
                                                      nameof(NodeFieldsEnum.Hosts)};
    }
}

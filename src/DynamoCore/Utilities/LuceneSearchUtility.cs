using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Br;
using Lucene.Net.Analysis.Cjk;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Cz;
using Lucene.Net.Analysis.De;
using Lucene.Net.Analysis.Es;
using Lucene.Net.Analysis.Fr;
using Lucene.Net.Analysis.It;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Newtonsoft.Json;

namespace Dynamo.Utilities
{
    /// <summary>
    /// Lucene search utility class that will be used for indexing and searching nodes and packages
    /// </summary>
    internal class LuceneSearchUtility
    {
        internal DynamoModel dynamoModel;

        /// <summary>
        /// Index fields that were added to the document
        /// </summary>
        internal List<string> addedFields;

        /// <summary>
        /// Lucene Directory Reader
        /// </summary>
        internal DirectoryReader dirReader;

        /// <summary>
        /// Lucene Index Directory, it can be RAMDirectory or FSDirectory
        /// </summary>
        internal Lucene.Net.Store.Directory indexDir;

        /// <summary>
        /// Lucene Index write
        /// </summary>
        internal IndexWriter writer;

        /// <summary>
        /// Start config for Lucene
        /// </summary>
        internal LuceneStartConfig startConfig;

        /// <summary>
        /// Default start config for Lucene, it will use RAM storage type and empty directory
        /// </summary>
        internal static readonly LuceneStartConfig DefaultStartConfig = new LuceneStartConfig();

        /// <summary>
        /// Start config for node index, it will use file storage type and node index directory
        /// </summary>
        internal static readonly LuceneStartConfig DefaultNodeIndexStartConfig = new LuceneStartConfig(LuceneSearchUtility.LuceneStorage.FILE_SYSTEM, LuceneConfig.NodesIndexingDirectory);

        /// <summary>
        /// Start config for package index, it will use file storage type and package index directory
        /// </summary>
        internal static readonly LuceneStartConfig DefaultPkgIndexStartConfig = new LuceneStartConfig(LuceneSearchUtility.LuceneStorage.FILE_SYSTEM, LuceneConfig.PackagesIndexingDirectory);

        public enum LuceneStorage
        {
            //Lucene Storage will be located in RAM and all the info indexed will be lost when Dynamo app is closed
            RAM,

            //Lucene Storage will be located in the local File System and the files will remain in ...AppData\Roaming\Dynamo\Dynamo Core\2.19\Index folder
            FILE_SYSTEM
        }

        /// <summary>
        /// This enum will be used to identify which can of search should be executed based in the user search criteria 
        /// </summary>
        public enum SearchType
        {
            //Normal search using just one word matching a specific node name
            Normal,

            //Search by category using the "." character for example "list.re"
            ByDotCategory,

            //The SearchTerm contains at least one empty space
            ByEmptySpace
        }

        /// <summary>
        /// This enum will used to create different type of Wildcard queries using regular expressions
        /// </summary>
        public enum WildcardType
        {
            //This represent the same SearchTerm as was inserted e.g. num
            None,

            //This represent the SearchTerm with the regular expression num*
            PostFix,

            //This represent the SearchTerm with the regular expression *num
            Prefix,

            //This represent the SearchTerm with the regular expression  *num*
            FullCard
        }

        // Used for creating the StandardAnalyzer
        internal Analyzer Analyzer;

        // Holds the instance for the IndexSearcher
        internal IndexSearcher Searcher;

        /// <summary>
        /// Constructor for LuceneSearchUtility, it will use the storage type passed as parameter
        /// </summary>
        /// <param name="model"></param>
        /// <param name="config"></param>
        internal LuceneSearchUtility(DynamoModel model, LuceneStartConfig config)
        {
            dynamoModel = model;
            // If under test mode, use the default StartConfig - RAM storage type and empty directory
            startConfig = DynamoModel.IsTestMode? DefaultStartConfig : config;
            InitializeLuceneConfig();
        }

        /// <summary>
        /// Initialize Lucene index writer based on start config.
        /// </summary>
        internal void InitializeLuceneConfig()
        {
            if (DynamoModel.IsHeadless) return;

            addedFields = new List<string>();

            DirectoryInfo luceneUserDataFolder;
            var userDataDir = new DirectoryInfo(dynamoModel.PathManager.UserDataDirectory);
            luceneUserDataFolder = userDataDir.Exists ? userDataDir : null;
            string indexPath = Path.Combine(luceneUserDataFolder.FullName, LuceneConfig.Index, startConfig.Directory);

            if (startConfig.StorageType == LuceneStorage.RAM)
            {
                indexDir = new RAMDirectory();
            }
            else
            {
                indexDir = FSDirectory.Open(indexPath);
            }
            // Create an analyzer to process the text
            Analyzer = CreateAnalyzerByLanguage(dynamoModel.PreferenceSettings.Locale);
            CreateLuceneIndexWriter();    
        }

        /// <summary>
        /// Create index writer for followup doc indexing
        /// </summary>
        /// <param name="mode">Index open mode for Lucene index writer</param>
        internal void CreateLuceneIndexWriter(OpenMode mode = OpenMode.CREATE)
        {
            // Create an index writer
            IndexWriterConfig indexConfig = new IndexWriterConfig(LuceneConfig.LuceneNetVersion, Analyzer)
            {
                OpenMode = mode
            };
            try
            {
                writer = new IndexWriter(indexDir, indexConfig);
            }
            catch (LockObtainFailedException ex)
            {

                DisposeWriter();
                dynamoModel.Logger.LogError($"LuceneNET LockObtainFailedException {ex}");

            }
            catch (Exception ex)
            {
                dynamoModel.Logger.LogError($"LuceneNET Exception {ex}");
            }
        }

        /// <summary>
        /// Initialize Lucene index document object for reuse
        /// </summary>
        /// <returns></returns>
        internal Document InitializeIndexDocumentForNodes()
        {
            if (DynamoModel.IsTestMode && startConfig.StorageType == LuceneStorage.FILE_SYSTEM) return null;

            var name = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Name), string.Empty, Field.Store.YES);
            var nameSplitted = new TextField(nameof(LuceneConfig.NodeFieldsEnum.NameSplitted), string.Empty, Field.Store.YES);
            var fullCategory = new TextField(nameof(LuceneConfig.NodeFieldsEnum.FullCategoryName), string.Empty, Field.Store.YES);
            var categorySplitted = new TextField(nameof(LuceneConfig.NodeFieldsEnum.CategorySplitted), string.Empty, Field.Store.YES);          
            var description = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Description), string.Empty, Field.Store.YES);
            var keywords = new TextField(nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords), string.Empty, Field.Store.YES);
            var docName = new StringField(nameof(LuceneConfig.NodeFieldsEnum.DocName), string.Empty, Field.Store.YES);
            var fullDoc = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Documentation), string.Empty, Field.Store.YES);
            var parameters = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Parameters), string.Empty, Field.Store.YES);

            var d = new Document()
            {
                name,
                nameSplitted,
                fullCategory,
                categorySplitted,                
                description,
                keywords,
                fullDoc,
                docName,
                parameters
            };
            return d;
        }

        /// <summary>
        /// Initialize Lucene index document object for reuse
        /// </summary>
        /// <returns></returns>
        internal Document InitializeIndexDocumentForPackages()
        {

            var name = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Name), string.Empty, Field.Store.YES);
            var description = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Description), string.Empty, Field.Store.YES);
            var keywords = new TextField(nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords), string.Empty, Field.Store.YES);
            var hosts = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Hosts), string.Empty, Field.Store.YES);
            var author = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Author), string.Empty, Field.Store.YES);

            var d = new Document()
            {
               name, description, keywords, hosts, author
            };
            return d;
        }

        /// <summary>
        /// Remove all the current indexed node info and update it with the new ones passed as parameter
        /// </summary>
        /// <param name="nodeList">list of nodes to be indexed</param>
        internal void UpdateIndexedNodesInfo(List<NodeSearchElement> nodeList)
        {
            if(nodeList.Any())
            {
                writer.DeleteAll();
                foreach(var node in nodeList)
                {
                    var iDoc = InitializeIndexDocumentForNodes();
                    AddNodeTypeToSearchIndex(node, iDoc);
                }             
            }         
        }

        // TODO:
        // isLast option is used for the last value set in the document, and it will fetch all the other field not set for the document and add them with an empty string.
        // isTextField is used when the value need to be tokenized(broken down into pieces), whereas StringTextFields are tokenized.
        /// <summary>
        /// The SetDocumentFieldValue method should be optimized later
        /// </summary>
        /// <param name="doc">Lucene document in which the information is stored</param>
        /// <param name="field">Field that is being updated in the document</param>
        /// <param name="value">Field value</param>
        /// <param name="isTextField">This is used when the value need to be tokenized(broken down into pieces), whereas StringTextFields are tokenized.</param>
        /// <param name="isLast">This is used for the last value set in the document. It will fetch all the fields not set in the document and add them with an empty string.</param>
        /// <param name="isTSpline">Indicate if the field being indexed belongs to a T-Spline node or not</param>
        internal void SetDocumentFieldValue(Document doc, string field, string value, bool isTextField = true, bool isLast = false, bool isTSpline = false)
        {
            string[] indexedFields = null;
            if (startConfig.Directory.Equals(LuceneConfig.NodesIndexingDirectory))
            {
                indexedFields = LuceneConfig.NodeIndexFields;
            }
            else if (startConfig.Directory.Equals(LuceneConfig.PackagesIndexingDirectory))
            {
                indexedFields = LuceneConfig.PackageIndexFields;
            }

            addedFields.Add(field);
            if (isTextField && !field.Equals("DocName"))
            {
                ((TextField)doc.GetField(field)).SetStringValue(value);

                //Index-time boost, setting the weight to 0 for TSpline nodes and 1 for the other nodes, this only apply for Description and SearchKeywords fields
                ((TextField)doc.GetField(field)).SetStringValue(value);
                ((TextField)doc.GetField(field)).Boost = isTSpline == true ? 0 : 1;
            }
            else
            {
                ((StringField)doc.GetField(field)).SetStringValue(value);
            }

            if (isLast && indexedFields != null && indexedFields.Any())
            {
                List<string> diff = indexedFields.Except(addedFields).ToList();
                foreach (var d in diff)
                {
                    SetDocumentFieldValue(doc, d, "");
                }
                addedFields.Clear();
            }
        }

        /// <summary>
        /// Creates a search query with adjusted priority, fuzzy logic and wildcards.
        /// Complete Search term appearing in Name of the package will be given highest priority.
        /// Then, complete search term appearing in other metadata,
        /// Then, a part of the search term(if containing multiple words) appearing in Name of the package
        /// Then, a part of the search term appearing in other metadata of the package.
        /// Then priority will be given based on fuzzy logic- that is if the complete search term may have been misspelled for upto 2(max edits) characters.
        /// Then, the same fuzzy logic will be applied to each part of the search term.
        /// </summary>
        /// <param name="fields">All fields to be searched in.</param>
        /// <param name="SearchTerm">Search key to be searched for.</param>
        /// <param name="IsPackageContext">Set this to true if the search context is packages instead of nodes.</param>
        /// <returns></returns>
        internal string CreateSearchQuery(string[] fields, string SearchTerm, bool IsPackageContext = false)
        {
            //By Default the search will be normal
            SearchType searchType = SearchType.Normal;
            int fuzzyLogicMaxEdits = LuceneConfig.FuzzySearchMinEdits;

            //Max number of nodes allowed in the search when is a ByEmptySpace search
            const int MaxNodeNamesRepeated = 20;

            // Use a larger max edit value - more tolerant with typo when search term is longer than threshold
            if (SearchTerm.Length > LuceneConfig.FuzzySearchMaxEditsThreshold)
            {
                fuzzyLogicMaxEdits = LuceneConfig.FuzzySearchMaxEdits;
            }

            var booleanQuery = new BooleanQuery();
            string searchTerm = QueryParser.Escape(SearchTerm);

            if (!IsPackageContext)
            {
                if (searchTerm.Contains('.'))
                    searchType = SearchType.ByDotCategory;
                else if (searchTerm.Contains(' '))
                    searchType = SearchType.ByEmptySpace;
                else
                    searchType = SearchType.Normal;
            }

            foreach (string f in fields)
            {
                Occur occurQuery = Occur.SHOULD;

                searchTerm = QueryParser.Escape(SearchTerm);
                if (searchType == SearchType.ByDotCategory)
                {
                    //This code section should be only executed if the search criteria is CategoryBased like "category.nodename"
                    if (f != nameof(LuceneConfig.NodeFieldsEnum.NameSplitted) &&
                        f != nameof(LuceneConfig.NodeFieldsEnum.CategorySplitted))
                        continue;

                    var categorySearchBased = searchTerm.Split('.');
                    //In the case the search criteria is like "Core.File.FileSystem.a" it will take only the last two sections Category=FileSystem and Name=a*
                    if (categorySearchBased.Length > 1 && !string.IsNullOrEmpty(categorySearchBased[categorySearchBased.Length - 2]))
                    {
                        if (f == nameof(LuceneConfig.NodeFieldsEnum.CategorySplitted))
                            searchTerm = categorySearchBased[categorySearchBased.Length - 2];
                        else
                            searchTerm = categorySearchBased[categorySearchBased.Length - 1];
                    }                   
                }

                //For normal search we don't consider the field CategorySplitted
                if (f == nameof(LuceneConfig.NodeFieldsEnum.CategorySplitted) && searchType != SearchType.ByDotCategory)
                    continue;


                //This case is for when the user type something like "list.", I mean, not specifying the node name or part of it
                if (string.IsNullOrEmpty(searchTerm))
                    continue;

                //Adds the FuzzyQuery and 4 WildcardQueries (3 of them contain regular expressions), with the normal weights
                AddQueries(searchTerm, f, searchType, booleanQuery, occurQuery, fuzzyLogicMaxEdits);

                if (searchType == SearchType.ByEmptySpace)
                {
                    foreach (string s in searchTerm.Split(' '))
                    {
                        //If is a ByEmptySpace search and the split words match with more than MaxNodeNamesRepeated nodes then the word is skipped (otherwise the results will be polluted with hundred of not related nodes)
                        int nodesFrequency = dynamoModel.SearchModel.Entries.Where(entry => entry.Name.ToLower().Contains(s) && !string.IsNullOrEmpty(s)).Count();
                        if (nodesFrequency > MaxNodeNamesRepeated) continue;

                        if (string.IsNullOrEmpty(s)) continue;

                        //Adds the FuzzyQuery and 4 WildcardQueries (3 of them contain regular expressions), with the weights for Queries with RegularExpressions
                        AddQueries(s, f, searchType, booleanQuery, occurQuery, LuceneConfig.FuzzySearchMinEdits, true);
                    }
                }
            }
            return booleanQuery.ToString();
        }

        /// <summary>
        ///  //Adds the FuzzyQuery and 4 WildcardQueries (3 of them contain regular expressions) with specific weight for each one
        /// </summary>
        /// <param name="searchTerm">Search Term introduced by the user</param>
        /// <param name="field">Field being processed</param>
        /// <param name="searchType">Type of Search: Normal, ByDotCategory and ByEmptySpace</param>
        /// <param name="booleanQuery">The Boolean query in which the Wildcard queries will be added</param>
        /// <param name="occurQuery">Occur type can be Should or Must</param>
        /// <param name="fuzzyLogicMaxEdits">Max edit lenght for Fuzzy queries</param>
        /// <param name="termSplit">Indicates if the SearchTerm has been split by empty space or not</param>
        private void AddQueries(string searchTerm, string field, SearchType searchType, BooleanQuery booleanQuery, Occur occurQuery, int fuzzyLogicMaxEdits, bool termSplit = false)
        {
            string querySearchTerm = searchTerm.Replace(" ", string.Empty);

            FuzzyQuery fuzzyQuery;
            if (searchTerm.Length > LuceneConfig.FuzzySearchMinimalTermLength)
            {
                fuzzyQuery = new FuzzyQuery(new Term(field, querySearchTerm), fuzzyLogicMaxEdits);
                booleanQuery.Add(fuzzyQuery, Occur.SHOULD);
            }

            if (searchType == SearchType.ByDotCategory && field == nameof(LuceneConfig.NodeFieldsEnum.CategorySplitted))
            {
                occurQuery = Occur.MUST;
            }

            foreach (WildcardType enumVal in Enum.GetValues(typeof(WildcardType)))
            {
                var wildcardQuery = CalculateFieldWeight(field, querySearchTerm, enumVal, termSplit);
                booleanQuery.Add(wildcardQuery, occurQuery);
            }
        }

        private WildcardQuery CalculateFieldWeight(string fieldName, string searchTerm, WildcardType wilcardType = WildcardType.None, bool termSplit = false)
        {
            WildcardQuery query;
            bool isWildcard = true;
            float boostOffset = 0;

            if (fieldName == nameof(LuceneConfig.NodeFieldsEnum.NameSplitted))
            {
                wilcardType = WildcardType.PostFix;
            }

            string termText;
            //If the WilcardQuery contains regular expression then we will decrease the defined weight
            switch (wilcardType)
            {
                case WildcardType.Prefix:
                    termText = "*" + searchTerm;
                    boostOffset = 2;
                    break;
                case WildcardType.PostFix:
                    termText = searchTerm + "*";
                    boostOffset = 1;
                    break;
                case WildcardType.FullCard:
                    termText = "*" + searchTerm + "*";
                    boostOffset = 3;
                    break;
                default:
                    isWildcard = false;
                    termText = searchTerm;
                    break;
            }

            query = new WildcardQuery(new Term(fieldName, termText));

            switch (fieldName)
            {
                case nameof(LuceneConfig.NodeFieldsEnum.Name):
                    query.Boost = isWildcard == false ?
                        LuceneConfig.SearchNameWeight : (LuceneConfig.WildcardsSearchNameWeight - boostOffset);
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.NameSplitted):
                    //Under this case the NameSplitted field will have less weight than CategorySplitted
                    query.Boost = isWildcard == false ?
                        LuceneConfig.SearchCategoryWeight : (LuceneConfig.WildcardsSearchCategoryWeight - boostOffset);
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.FullCategoryName):
                    query.Boost = isWildcard == false ?
                        LuceneConfig.SearchCategoryWeight : (LuceneConfig.WildcardsSearchCategoryWeight - boostOffset);
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.CategorySplitted):
                    //Under this case the CategorySplitted field will have more weight than NameSplitted
                    query.Boost = isWildcard == false ?
                        LuceneConfig.SearchNameWeight : (LuceneConfig.WildcardsSearchNameWeight - boostOffset);
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.Description):
                    query.Boost = isWildcard == false ?
                        LuceneConfig.SearchDescriptionWeight : (LuceneConfig.WildcardsSearchDescriptionWeight - boostOffset);
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords):
                    query.Boost = isWildcard == false ?
                       LuceneConfig.SearchTagsWeight : (LuceneConfig.WildcardsSearchTagsWeight - boostOffset);
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.Parameters):
                    query.Boost = 1;
                    break;
                default:
                    query.Boost = isWildcard == false ?
                       LuceneConfig.SearchMetaFieldsWeight : (LuceneConfig.WildcardsSearchMetaFieldsWeight - boostOffset);
                    break;
            }

            //This section only applies if the SearchTerm contain empty spaces then is splitted by empty space and try to calculate the weight for each part
            if (fieldName.Equals(nameof(LuceneConfig.NodeFieldsEnum.Name)) && termSplit == true)
            {
                query.Boost = LuceneConfig.WildcardsSearchNameParsedWeight;
            }
            else if (termSplit == true)
            {
                query.Boost = LuceneConfig.FuzzySearchWeight;
            }

            return query;
        }

        /// <summary>
        /// It creates an Analyzer to be used in the Indexing based on the user preference language
        /// </summary>
        /// <returns></returns>
        internal Analyzer CreateAnalyzerByLanguage(string language)
        {
            switch (language)
            {
                case "en-US":
                    return new LuceneCustomAnalyzer(LuceneConfig.LuceneNetVersion);
                case "cs-CZ":
                    return new CzechAnalyzer(LuceneConfig.LuceneNetVersion);
                case "de-DE":
                    return new GermanAnalyzer(LuceneConfig.LuceneNetVersion);
                case "es-ES":
                    return new SpanishAnalyzer(LuceneConfig.LuceneNetVersion);
                case "fr-FR":
                    return new FrenchAnalyzer(LuceneConfig.LuceneNetVersion);
                case "it-IT":
                    return new ItalianAnalyzer(LuceneConfig.LuceneNetVersion);
                case "ja-JP":
                case "ko-KR":
                case "zh-CN":
                case "zh-TW":
                    return new CJKAnalyzer(LuceneConfig.LuceneNetVersion);
                case "pl-PL":
                    return new LuceneCustomAnalyzer(LuceneConfig.LuceneNetVersion);
                case "pt-BR":
                    return new BrazilianAnalyzer(LuceneConfig.LuceneNetVersion);
                case "ru-RU":
                    return new RussianAnalyzer(LuceneConfig.LuceneNetVersion);
                default:
                    return new LuceneCustomAnalyzer(LuceneConfig.LuceneNetVersion);
            }
        }

        /// <summary>
        /// Dispose Lucene index write objects and reuse other objects
        /// </summary>
        internal void DisposeWriter()
        {
            writer?.Dispose();
            writer = null;
        }

        /// <summary>
        /// Dispose all the Lucene objects
        /// </summary>
        internal void DisposeAll()
        {
            DisposeWriter();
            dirReader?.Dispose();
            indexDir?.Dispose();
            Analyzer?.Dispose();
        }

        /// <summary>
        /// Commit the changes made to the Lucene index
        /// </summary>
        internal void CommitWriterChanges()
        {
            //Commit the info indexed if index writer exists
            writer?.Commit();
        }

        /// <summary>
        /// Add node information to existing Lucene index
        /// </summary>
        /// <param name="node">node info that will be indexed</param>
        /// <param name="doc">Lucene document in which the node info will be indexed</param>
        internal void AddNodeTypeToSearchIndex(NodeSearchElement node, Document doc)
        {
            if (addedFields == null) return;
            // During DynamoModel initialization, the index writer should still be valid here
            // If the index writer is null and index not locked, it means the index writer has been disposed, e.g. DynamoModel finished initialization
            // If the index writer is null and index locked, it means another Dynamo session is currently updating the search index
            // Try to create a new index writer to amend the index
            if (writer == null && !IndexWriter.IsLocked(this.indexDir))
            {
                CreateLuceneIndexWriter(OpenMode.CREATE_OR_APPEND);
            }
            // If the index writer is still null, skip the indexing
            if (writer == null) return;

            bool isTSplineNode = node.FullCategoryName.ToLower().Contains("tspline")? true: false;

            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.FullCategoryName), node.FullCategoryName);

            var categoryParts = node.FullCategoryName.Split('.');
            string categoryParsed = categoryParts.Length > 1 ? categoryParts[categoryParts.Length - 1] : node.FullCategoryName;
            //In case the search criteria is like "filesystem.replace" we will be storing the value "filesystem" inside the CategorySplitted field
            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.CategorySplitted), categoryParsed);

            //When indexing the node.Name if the Name contains empty space then we remove it (this will allow to Search without empty spaces and fetch the expected node).
            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Name), node.Name.Trim().Replace(" ", string.Empty));

            var nameParts = node.Name.Split('.');
            string nameParsed = nameParts.Length > 1 ? nameParts[nameParts.Length - 1] : node.Name;
            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.NameSplitted), nameParsed);

            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Description), node.Description, true, false, isTSplineNode);
            if (node.SearchKeywords.Count > 0)
            {
                SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords), node.SearchKeywords.Aggregate((x, y) => x + " " + y), true, true, isTSplineNode);
            }
            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Parameters), node.Parameters ?? string.Empty);

            writer?.AddDocument(doc);
        }
    }

    /// <summary>
    /// Due that the Lucene StandardAnalyzer removes special characters/words like "+", "*", "And" then we had to implement a Custom Analyzer that support that that kind of search terms
    /// </summary>
    public class LuceneCustomAnalyzer : Analyzer
    {
        private readonly LuceneVersion luceneVersion;

        public LuceneCustomAnalyzer(LuceneVersion matchVersion)
        {
            luceneVersion = matchVersion;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {

            //This tokenizer won't remove special characters like + * / -
            Tokenizer tokenizer = new WhitespaceTokenizer(luceneVersion, reader);

            //Remove basic stop words a, an, the, in, on etc
            TokenStream tok = new StandardFilter(luceneVersion, tokenizer);

            //Lowercase all the text
            tok = new LowerCaseFilter(luceneVersion, tok);

            //List of stopwords that will be removed by the StopFilter like "a", "an", "and", "are", "as", "at", "be", "but", "by"
            CharArraySet stopWords = new CharArraySet(luceneVersion, 1, true)
            {
                StopAnalyzer.ENGLISH_STOP_WORDS_SET,
            };

            tok = new StopFilter(LuceneConfig.LuceneNetVersion, tok, stopWords);

            return new TokenStreamComponents(tokenizer, tok);
        }
    }

    /// <summary>
    /// Start up config for Lucene indexing
    /// </summary>
    internal class LuceneStartConfig
    {
        /// <summary>
        /// Lucene Index Directory name, e.g. Nodes, Packages
        /// </summary>
        internal string Directory { get; set; }

        /// <summary>
        /// Current Lucene Index Storage type, it could be either RAM or FILE_SYSTEM
        /// </summary>
        internal LuceneSearchUtility.LuceneStorage StorageType { get; set; }

        /// <summary>
        /// Constructor for LuceneStartConfig
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="storageType"></param>
        internal LuceneStartConfig(LuceneSearchUtility.LuceneStorage storageType = LuceneSearchUtility.LuceneStorage.RAM, string directory = "")
        {
            Directory = directory;
            StorageType = storageType;
        }
    }
}

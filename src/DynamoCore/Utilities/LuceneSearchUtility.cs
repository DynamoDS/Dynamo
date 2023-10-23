using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Events;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Dynamo.Session;
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
        /// Index open mode for Lucene index writer
        /// </summary>
        internal OpenMode openMode { get; set; }

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
        /// <param name="mode"></param>
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
                (ExecutionEvents.ActiveSession.GetParameterValue(ParameterKeys.Logger) as DynamoLogger).LogError($"LuceneNET LockObtainFailedException {ex}");

            }
            catch (Exception ex)
            {
                (ExecutionEvents.ActiveSession.GetParameterValue(ParameterKeys.Logger) as DynamoLogger).LogError($"LuceneNET Exception {ex}");
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
            var fullCategory = new TextField(nameof(LuceneConfig.NodeFieldsEnum.FullCategoryName), string.Empty, Field.Store.YES);
            var description = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Description), string.Empty, Field.Store.YES);
            var keywords = new TextField(nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords), string.Empty, Field.Store.YES);
            var docName = new StringField(nameof(LuceneConfig.NodeFieldsEnum.DocName), string.Empty, Field.Store.YES);
            var fullDoc = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Documentation), string.Empty, Field.Store.YES);
            var parameters = new TextField(nameof(LuceneConfig.NodeFieldsEnum.Parameters), string.Empty, Field.Store.YES);

            var d = new Document()
            {
                fullCategory,
                name,
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

            var d = new Document()
            {
               name, description, keywords, hosts
            };
            return d;
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
        internal void SetDocumentFieldValue(Document doc, string field, string value, bool isTextField = true, bool isLast = false)
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
        /// <returns></returns>
        internal string CreateSearchQuery(string[] fields, string SearchTerm)
        {
            int fuzzyLogicMaxEdits = LuceneConfig.FuzzySearchMinEdits;
            // Use a larger max edit value - more tolerant with typo when search term is longer than threshold
            if (SearchTerm.Length > LuceneConfig.FuzzySearchMaxEditsThreshold)
            {
                fuzzyLogicMaxEdits = LuceneConfig.FuzzySearchMaxEdits;
            }

            var booleanQuery = new BooleanQuery();
            string searchTerm = QueryParser.Escape(SearchTerm);

            foreach (string f in fields)
            {
                FuzzyQuery fuzzyQuery;
                if (searchTerm.Length > LuceneConfig.FuzzySearchMinimalTermLength)
                {
                    fuzzyQuery = new FuzzyQuery(new Term(f, searchTerm), fuzzyLogicMaxEdits);
                    booleanQuery.Add(fuzzyQuery, Occur.SHOULD);
                }

                var fieldQuery = CalculateFieldWeight(f, searchTerm);
                var wildcardQuery = CalculateFieldWeight(f, searchTerm, true);

                booleanQuery.Add(fieldQuery, Occur.SHOULD);
                booleanQuery.Add(wildcardQuery, Occur.SHOULD);

                if (searchTerm.Contains(' ') || searchTerm.Contains('.'))
                {
                    foreach (string s in searchTerm.Split(' ', '.'))
                    {
                        if (string.IsNullOrEmpty(s)) continue;

                        if (s.Length > LuceneConfig.FuzzySearchMinimalTermLength)
                        {
                            fuzzyQuery = new FuzzyQuery(new Term(f, s), LuceneConfig.FuzzySearchMinEdits);
                            booleanQuery.Add(fuzzyQuery, Occur.SHOULD);
                        }
                        wildcardQuery = new WildcardQuery(new Term(f, "*" + s + "*"));

                        if (f.Equals(nameof(LuceneConfig.NodeFieldsEnum.Name)))
                        {
                            wildcardQuery.Boost = LuceneConfig.WildcardsSearchNameParsedWeight;
                        }
                        else
                        {
                            wildcardQuery.Boost = LuceneConfig.FuzzySearchWeight;
                        }
                        booleanQuery.Add(wildcardQuery, Occur.SHOULD);
                    }
                }
            }
            return booleanQuery.ToString();
        }

        private WildcardQuery CalculateFieldWeight(string fieldName, string searchTerm, bool isWildcard = false)
        {
            WildcardQuery query;

            query = isWildcard == false ?
                new WildcardQuery(new Term(fieldName, searchTerm)) : new WildcardQuery(new Term(fieldName, "*" + searchTerm + "*"));

            switch (fieldName)
            {
                case nameof(LuceneConfig.NodeFieldsEnum.Name):
                    query.Boost = isWildcard == false?
                        LuceneConfig.SearchNameWeight :  LuceneConfig.WildcardsSearchNameWeight;
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.FullCategoryName):
                    query.Boost = isWildcard == false?
                        LuceneConfig.SearchCategoryWeight : LuceneConfig.WildcardsSearchCategoryWeight;
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.Description):
                    query.Boost = isWildcard == false ?
                        LuceneConfig.SearchDescriptionWeight : LuceneConfig.WildcardsSearchDescriptionWeight;
                    break;
                case nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords):
                    query.Boost = isWildcard == false ?
                       LuceneConfig.SearchTagsWeight : LuceneConfig.WildcardsSearchTagsWeight;
                    break;
                default:
                    query.Boost = isWildcard == false ?
                       LuceneConfig.SearchMetaFieldsWeight : LuceneConfig.WildcardsSearchMetaFieldsWeight;
                    break;
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
            writer?.Dispose();
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
            // If the index writer is null and index not locked, it means the index writer has been disposed, most likely due to DynamoView already launched
            // If the index writer is null and index locked, it means user is in a secondary Dynamo session
            // Then create a new index writer to amend the index should be safe
            if (writer == null && !IndexWriter.IsLocked(this.indexDir))
            {
                CreateLuceneIndexWriter(OpenMode.CREATE_OR_APPEND);
            }

            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.FullCategoryName), node.FullCategoryName);
            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Name), node.Name);
            SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.Description), node.Description);
            if (node.SearchKeywords.Count > 0)
            {
                SetDocumentFieldValue(doc, nameof(LuceneConfig.NodeFieldsEnum.SearchKeywords), node.SearchKeywords.Aggregate((x, y) => x + " " + y), true, true);
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

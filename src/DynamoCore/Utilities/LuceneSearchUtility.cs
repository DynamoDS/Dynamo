using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Models;
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
    internal class LuceneSearchUtility
    {
        internal DynamoModel dynamoModel;
        internal List<string> addedFields;
        internal DirectoryReader dirReader;
        internal Lucene.Net.Store.Directory indexDir;
        internal IndexWriter writer;
        internal string directory;
        internal LuceneStorage currentStorageType;

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

        internal LuceneSearchUtility(DynamoModel model)
        {
            dynamoModel = model;
        }

        /// <summary>
        /// Initialize Lucene config file writer.
        /// </summary>
        internal void InitializeLuceneConfig(string dirName, LuceneStorage storageType = LuceneStorage.FILE_SYSTEM)
        {
            addedFields = new List<string>();

            DirectoryInfo luceneUserDataFolder;
            var userDataDir = new DirectoryInfo(dynamoModel.PathManager.UserDataDirectory);
            luceneUserDataFolder = userDataDir.Exists ? userDataDir : null;

            directory = dirName;
            string indexPath = Path.Combine(luceneUserDataFolder.FullName, LuceneConfig.Index, dirName);

            currentStorageType = storageType;

            if (storageType == LuceneStorage.RAM)
            {
                indexDir = new RAMDirectory();
            }
            else
            {
                indexDir = FSDirectory.Open(indexPath);
            }


            // Create an analyzer to process the text
            Analyzer = CreateAnalyzerByLanguage(dynamoModel.PreferenceSettings.Locale);

            // Initialize Lucene index writer, unless in test mode or we are using RAMDirectory for indexing info. 
            if (!DynamoModel.IsTestMode || currentStorageType == LuceneStorage.RAM)
            {
                try
                {
                    // Create an index writer
                    IndexWriterConfig indexConfig = new IndexWriterConfig(LuceneConfig.LuceneNetVersion, Analyzer)
                    {
                        OpenMode = OpenMode.CREATE
                    };

                    writer = new IndexWriter(indexDir, indexConfig);
                }
                catch (LockObtainFailedException ex)
                {
                    DisposeWriter();
                    dynamoModel.Logger.LogError($"LuceneNET LockObtainFailedException {ex}");
                }
                catch (Exception ex) {
                    dynamoModel.Logger.LogError($"LuceneNET Exception {ex}");
                }
            }
        }

        /// <summary>
        /// Initialize Lucene index document object for reuse
        /// </summary>
        /// <returns></returns>
        internal Document InitializeIndexDocumentForNodes()
        {
            if (DynamoModel.IsTestMode && currentStorageType == LuceneStorage.FILE_SYSTEM) return null;

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
            if (DynamoModel.IsTestMode) return null;

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
            if (directory.Equals(LuceneConfig.NodesIndexingDirectory))
            {
                indexedFields = LuceneConfig.NodeIndexFields;
            }
            else if (directory.Equals(LuceneConfig.PackagesIndexingDirectory))
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
            //We need to check if we are not running Dynamo tests because otherwise parallel test start to fail when trying to write in the same Lucene directory location
            if (!DynamoModel.IsTestMode || currentStorageType == LuceneStorage.RAM)
            {
                writer?.Dispose();
                writer = null;
            }
        }

        internal void CommitWriterChanges()
        {
            if (!DynamoModel.IsTestMode || currentStorageType == LuceneStorage.RAM)
            {
                //Commit the info indexed
                writer?.Commit();
            }
        }
    }

    /// <summary>
    /// Due that the Lucene StandardAnalyzer removes special characters/words like "+", "*", "And" then we had to implement a Custom Analyzer that support that that kind of search terms
    /// </summary>
    public class LuceneCustomAnalyzer : Analyzer
    {
        private LuceneVersion luceneVersion;

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
}

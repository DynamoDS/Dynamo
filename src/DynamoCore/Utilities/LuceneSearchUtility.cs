using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;

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
        internal void InitializeLuceneConfig(string dirName)
        {
            addedFields = new List<string>();

            DirectoryInfo webBrowserUserDataFolder;
            var userDataDir = new DirectoryInfo(dynamoModel.PathManager.UserDataDirectory);
            webBrowserUserDataFolder = userDataDir.Exists ? userDataDir : null;

            directory = dirName;
            string indexPath = Path.Combine(webBrowserUserDataFolder.FullName, LuceneConfig.Index, dirName);
            indexDir = Lucene.Net.Store.FSDirectory.Open(indexPath);

            // Create an analyzer to process the text
            Analyzer = new StandardAnalyzer(LuceneConfig.LuceneNetVersion);

            // Initialize Lucene index writer, unless in test mode.
            if (!DynamoModel.IsTestMode)
            {
                // Create an index writer
                IndexWriterConfig indexConfig = new IndexWriterConfig(LuceneConfig.LuceneNetVersion, Analyzer)
                {
                    OpenMode = OpenMode.CREATE
                };
                writer = new IndexWriter(indexDir, indexConfig);
            }
        }

        /// <summary>
        /// Initialize Lucene index document object for reuse
        /// </summary>
        /// <returns></returns>
        internal Document InitializeIndexDocumentForNodes()
        {
            if (DynamoModel.IsTestMode) return null;

            var name = new TextField(nameof(LuceneConfig.IndexFieldsEnum.Name), string.Empty, Field.Store.YES);
            var fullCategory = new TextField(nameof(LuceneConfig.IndexFieldsEnum.FullCategoryName), string.Empty, Field.Store.YES);
            var description = new TextField(nameof(LuceneConfig.IndexFieldsEnum.Description), string.Empty, Field.Store.YES);
            var keywords = new TextField(nameof(LuceneConfig.IndexFieldsEnum.SearchKeywords), string.Empty, Field.Store.YES);
            var docName = new StringField(nameof(LuceneConfig.IndexFieldsEnum.DocName), string.Empty, Field.Store.YES);
            var fullDoc = new TextField(nameof(LuceneConfig.IndexFieldsEnum.Documentation), string.Empty, Field.Store.YES);

            var d = new Document()
            {
                fullCategory,
                name,
                description,
                keywords,
                fullDoc,
                docName
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

            var name = new TextField(nameof(LuceneConfig.IndexFieldsEnum.Name), string.Empty, Field.Store.YES);
            var description = new TextField(nameof(LuceneConfig.IndexFieldsEnum.Description), string.Empty, Field.Store.YES);
            var keywords = new TextField(nameof(LuceneConfig.IndexFieldsEnum.SearchKeywords), string.Empty, Field.Store.YES);
            var hosts = new TextField(nameof(LuceneConfig.IndexFieldsEnum.Hosts), string.Empty, Field.Store.YES);

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

            if (isLast && indexedFields.Any())
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

                var wildcardQuery = new WildcardQuery(new Term(f, searchTerm));
                if (f.Equals(nameof(LuceneConfig.IndexFieldsEnum.Name)))
                {
                    wildcardQuery.Boost = LuceneConfig.SearchNameWeight;
                }
                else
                {
                    wildcardQuery.Boost = LuceneConfig.SearchMetaFieldsWeight;
                }
                booleanQuery.Add(wildcardQuery, Occur.SHOULD);

                wildcardQuery = new WildcardQuery(new Term(f, "*" + searchTerm + "*"));
                if (f.Equals(nameof(LuceneConfig.IndexFieldsEnum.Name)))
                {
                    wildcardQuery.Boost = LuceneConfig.WildcardsSearchNameWeight;
                }
                else
                {
                    wildcardQuery.Boost = LuceneConfig.WildcardsSearchMetaFieldsWeight;
                }
                booleanQuery.Add(wildcardQuery, Occur.SHOULD);

                if (searchTerm.Contains(' ') || searchTerm.Contains('.'))
                {
                    foreach (string s in searchTerm.Split(' ', '.'))
                    {
                        if (s.Length > LuceneConfig.FuzzySearchMinimalTermLength)
                        {
                            fuzzyQuery = new FuzzyQuery(new Term(f, s), LuceneConfig.FuzzySearchMinEdits);
                            booleanQuery.Add(fuzzyQuery, Occur.SHOULD);
                        }
                        wildcardQuery = new WildcardQuery(new Term(f, "*" + s + "*"));

                        if (f.Equals(nameof(LuceneConfig.IndexFieldsEnum.Name)))
                        {
                            wildcardQuery.Boost = 5;
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
    }
}

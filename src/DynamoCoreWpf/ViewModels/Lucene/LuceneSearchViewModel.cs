using System.Linq;
using Dynamo.Configuration;
using Dynamo.Models;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;

namespace Dynamo.ViewModels
{
    internal class LuceneSearchViewModel
    {
        internal DynamoModel dynamoModel;

        internal LuceneSearchViewModel(DynamoModel model)
        {
            dynamoModel = model;
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using FinalProject.Service;
using FinalProject.Shared.Extensions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace FinalProject.Searching
{
    public class QuestionSearcher : IDisposable
    {
        private const uint DefaultHitsLimit = 10u;

        private const string IdFieldName = "id";

        private const string TitleFieldName = "title";

        private static readonly Analyzer Analyzer;

        private static readonly string IndexesDirectoryPath = AppDomain.CurrentDomain.BaseDirectory
                                                              + @"\App_Data\LuceneIndexes";

        private static readonly FSDirectory IndexesDirectory = FSDirectory.Open(IndexesDirectoryPath);

        private static readonly IndexWriter IndexWriter;

        private static QuestionSearcher _defaultInstance;

        public static QuestionSearcher Default = _defaultInstance
                                                 ?? (_defaultInstance = new QuestionSearcher(DefaultHitsLimit));

        private readonly uint _hitsLimit;

        static QuestionSearcher()
        {
            Analyzer = new BrazilianAnalyzer(Version.LUCENE_30);
            IndexWriter = new IndexWriter(IndexesDirectory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        public QuestionSearcher(uint hitsLimit)
        {
            _hitsLimit = hitsLimit;
        }

        public void Dispose()
        {
            IndexWriter.Dispose();
            Analyzer.Dispose();
        }

        public Task<bool> ClearIndexesAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        IndexWriter.DeleteAll();
                        IndexWriter.Commit();
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    return true;
                });
        }

        public async Task CreateIndexesAsync()
        {
            await ClearIndexesAsync();

            var questionService = AutofacDependencyResolver.Current.GetService<IQuestionService>();
            var questions = await questionService.GetAllQuestionsAsync();

            foreach (var question in questions)
            {
                await IndexQuestionAsync(question.Id, question.Title);
            }

            await OptimizeIndexesAsync();
        }

        public async Task<IEnumerable<string>> ExtractKeywordsAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            return await Task.Run(() => ExtracKeyworkds(text));
        }

        public Task IndexQuestionAsync(int id, string title)
        {
            return Task.Run(
                () =>
                {
                    var document = new Document();

                    document.Add(new Field(IdFieldName, id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    document.Add(
                        new Field(
                            TitleFieldName,
                            title /*.RemoveDiacritics()*/,
                            Field.Store.YES,
                            Field.Index.ANALYZED));
                    IndexWriter.AddDocument(document);
                });
        }

        public Task OptimizeIndexesAsync()
        {
            return Task.Run(() => IndexWriter.Optimize());
        }

        public async Task<IList<QuestionSearchResult>> SearchSimilarsAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<QuestionSearchResult>();
            }

            return await Task.Run(
                () =>
                {
                    using (var reader = IndexWriter.GetReader())
                    {
                        using (var searcher = new IndexSearcher(reader))
                        {
                            var searchText = text.RemoveDiacritics();
                            var keywords = ExtracKeyworkds(searchText);
                            var query = new BooleanQuery();

                            foreach (var keyword in keywords)
                            {
                                var fuzzyQuery = new FuzzyQuery(new Term(TitleFieldName, keyword), 0.49f);

                                query.Add(fuzzyQuery, Occur.SHOULD);
                            }
                            
                            var searchResult = searcher.Search(query, (int)_hitsLimit);
                            var hits = searchResult.ScoreDocs;
                            var results = GetQuestionResults(hits, searcher);

                            return results;
                        }
                    }
                });
        }

        private IEnumerable<string> ExtracKeyworkds(string text)
        {
            var words = new List<string>();

            using (var stringReader = new StringReader(text))
            {
                var tokenStream = Analyzer.TokenStream(null, stringReader);
                var charTermAttribute = tokenStream.AddAttribute<ITermAttribute>();

                while (tokenStream.IncrementToken())
                {
                    var term = charTermAttribute.Term;

                    words.Add(term);
                }

                return words;
            }
        }

        private static IList<QuestionSearchResult> GetQuestionResults(IEnumerable<ScoreDoc> scoreDocs, Searchable searcher)
        {
            return scoreDocs.Select(scoreDoc => ParseQuestionSearchResult(searcher.Doc(scoreDoc.Doc))).ToList();
        }

        private static Query ParseQuery(string title, QueryParser parser)
        {
            return parser.Parse(QueryParser.Escape(title.Trim()));
        }

        private static QuestionSearchResult ParseQuestionSearchResult(Document document)
        {
            var id = int.Parse(document.Get(IdFieldName));
            var title = document.Get(TitleFieldName);

            return new QuestionSearchResult(id, title);
        }
    }
}
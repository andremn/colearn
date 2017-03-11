using System;
using System.IO;
using FinalProject.Helpers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Version = Lucene.Net.Util.Version;

namespace FinalProject.Searching
{
    public class InsensitiveAnalyzer : Analyzer
    {
        private const int DefaultMaxTokenLength = 255;

        private readonly string _stopWordsFilePath;

        public InsensitiveAnalyzer(Version version)
        {
            Version = version;
        }

        public InsensitiveAnalyzer(Version version, string stopWordsFilePath)
        {
            if (string.IsNullOrWhiteSpace(stopWordsFilePath))
            {
                throw new ArgumentException("Invalid path");
            }

            Version = version;
            _stopWordsFilePath = stopWordsFilePath;
        }

        public Version Version { get; set; }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(Version, reader) {MaxTokenLength = DefaultMaxTokenLength};

            var stream = new StandardFilter(tokenizer) as TokenStream;

            if (_stopWordsFilePath != null)
            {
                string stopWords;

                using (var fileReader = FileSystemHelper.OpenText(_stopWordsFilePath))
                {
                    stopWords = fileReader.ReadToEnd();
                }

                var stopWordsSet = new CharArraySet(stopWords.Split(','), true);

                stream = new StopFilter(true, stream, stopWordsSet);
            }

            stream = new LowerCaseFilter(stream);
            return new ASCIIFoldingFilter(stream);
        }
    }
}
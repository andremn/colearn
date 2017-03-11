#region License

/*
* Licensed to the Apache Software Foundation (ASF) under one or more
* contributor license agreements.  See the NOTICE file distributed with
* this work for additional information regarding copyright ownership.
* The ASF licenses this file to You under the Apache License, Version 2.0
* (the "License"); you may not use this file except in compliance with
* the License.  You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

#endregion

using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Support.Compatibility;
using Lucene.Net.Util;

namespace FinalProject.Searching
{
    public sealed class BrazilianAnalyzer : Analyzer
    {
        private static readonly string[] StopWords =
        {
            "a", "ainda", "alem", "ambas", "ambos", "antes", "ao", "aonde",
            "aos", "apos", "aquele", "aqueles", "as", "assim", "com", "como",
            "contra", "contudo", "cuja", "cujas", "cujo", "cujos", "da",
            "das", "de", "dela", "dele", "deles", "demais", "depois", "desde",
            "desta", "deste", "dispoe", "dispoem", "diversa", "diversas",
            "diversos", "do", "dos", "durante", "e", "ela", "elas", "ele",
            "eles", "em", "entao", "entre", "essa", "essas", "esse", "esses",
            "esta", "estas", "este", "estes", "ha", "isso", "isto", "logo",
            "mais", "mas", "mediante", "menos", "mesma", "mesmas", "mesmo",
            "mesmos", "na", "nas", "nao", "nas", "nem", "nesse", "neste",
            "nos", "o", "os", "ou", "outra", "outras", "outro", "outros",
            "pelas", "pelas", "pelo", "pelos", "perante", "pois", "por",
            "porque", "portanto", "proprio", "propios", "quais", "qual",
            "qualquer", "quando", "quanto", "que", "quem", "quer", "se",
            "seja", "sem", "sendo", "seu", "seus", "sob", "sobre", "sua",
            "suas", "tal", "tambem", "teu", "teus", "toda", "todas", "todo",
            "todos", "tua", "tuas", "tudo", "um", "uma", "umas", "uns"
        };

        private static readonly ISet<string> DefaultStopSet =
            CharArraySet.UnmodifiableSet(new CharArraySet(StopWords, false));

        // List of typical Brazilian stopwords
        private readonly Version _matchVersion;

        private readonly ISet<string> _stoptable;

        private ISet<string> _excltable = SetFactory.CreateHashSet<string>();

        public BrazilianAnalyzer(Version matchVersion)
            : this(matchVersion, DefaultStopSet)
        {
        }

        public BrazilianAnalyzer(Version matchVersion, ISet<string> stopwords)
        {
            _stoptable = CharArraySet.UnmodifiableSet(CharArraySet.Copy(stopwords));
            _matchVersion = matchVersion;
        }

        public BrazilianAnalyzer(Version matchVersion, ISet<string> stopwords, ISet<string> stemExclusionSet)
            : this(matchVersion, stopwords)
        {
            _excltable = CharArraySet.UnmodifiableSet(CharArraySet.Copy(stemExclusionSet));
        }

        public override TokenStream ReusableTokenStream(string fieldName, TextReader reader)
        {
            var streams = (SavedStreams)PreviousTokenStream;

            if (streams == null)
            {
                streams = new SavedStreams {Source = new StandardTokenizer(_matchVersion, reader)};

                streams.Result = new LowerCaseFilter(streams.Source);
                streams.Result = new StandardFilter(streams.Result);
                streams.Result = new StopFilter(
                    StopFilter.GetEnablePositionIncrementsVersionDefault(_matchVersion),
                    streams.Result,
                    _stoptable);
                streams.Result = new BrazilianFilter(streams.Result);

                // streams.Result = new BrazilianStemFilter(streams.Result, _excltable);
                PreviousTokenStream = streams;
            }
            else
            {
                streams.Source.Reset(reader);
            }

            return streams.Result;
        }

        public void SetStemExclusionTable(params string[] exclusionlist)
        {
            _excltable = StopFilter.MakeStopSet(exclusionlist);

            // Force a new stemmer to be created
            PreviousTokenStream = null;
        }

        public void SetStemExclusionTable(IDictionary<string, string> exclusionlist)
        {
            _excltable = SetFactory.CreateHashSet(exclusionlist.Keys);

            // Force a new stemmer to be created
            PreviousTokenStream = null;
        }

        public void SetStemExclusionTable(FileInfo exclusionlist)
        {
            _excltable = WordlistLoader.GetWordSet(exclusionlist);

            // Force a new stemmer to be created
            PreviousTokenStream = null;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream result = new StandardTokenizer(_matchVersion, reader);

            result = new LowerCaseFilter(result);
            result = new StandardFilter(result);
            result = new StopFilter(
                StopFilter.GetEnablePositionIncrementsVersionDefault(_matchVersion),
                result,
                _stoptable);

            // result = new BrazilianStemFilter(result, _excltable);
            result = new BrazilianFilter(result);
            return result;
        }

        private class SavedStreams
        {
            protected internal TokenStream Result;

            protected internal Tokenizer Source;
        }
    }
}
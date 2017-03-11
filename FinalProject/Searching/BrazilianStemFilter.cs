#region License

/*
 * Licensed to the Apache Software Foundation(ASF) under one or more
 * contributor license agreements.See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

namespace FinalProject.Searching
{
    public sealed class BrazilianStemFilter : TokenFilter
    {
        private readonly ISet<string> _exclusions;

        private readonly BrazilianStemmer _stemmer;

        private readonly ITermAttribute _termAtt;

        public BrazilianStemFilter(TokenStream input)
            : base(input)
        {
            _stemmer = new BrazilianStemmer();
            _termAtt = AddAttribute<ITermAttribute>();
        }

        public BrazilianStemFilter(TokenStream input, ISet<string> exclusiontable)
            : this(input)
        {
            _exclusions = exclusiontable;
        }

        public override bool IncrementToken()
        {
            if (!input.IncrementToken())
            {
                return false;
            }

            var term = _termAtt.Term;

            // Check the exclusion table.
            if (_exclusions != null && _exclusions.Contains(term))
            {
                return true;
            }

            var s = _stemmer.Stem(term);

            // If not stemmed, don't waste the time adjusting the token.
            if ((s != null) && !s.Equals(term))
            {
                _termAtt.SetTermBuffer(s);
            }

            return true;
        }
    }
}
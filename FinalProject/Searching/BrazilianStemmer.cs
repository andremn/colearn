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

namespace FinalProject.Searching
{
    public class BrazilianStemmer
    {
        private string _ct;

        private string _r1;

        private string _r2;

        private string _rv;

        private string _term;

        public string Stem(string term)
        {
            // creates CT
            CreateCt(term);

            if (!IsIndexable(_ct))
            {
                return null;
            }

            if (!IsStemmable(_ct))
            {
                return _ct;
            }

            _r1 = GetR1(_ct);
            _r2 = GetR1(_r1);
            _rv = GetRv(_ct);
            _term = term + ";" + _ct;

            var altered = Step1();

            if (!altered)
            {
                altered = Step2();
            }

            if (altered)
            {
                Step3();
            }
            else
            {
                Step4();
            }

            Step5();

            return _ct;
        }

        private static string ChangeTerm(string value)
        {
            int j;
            var r = string.Empty;

            // be-safe !!!
            if (value == null)
            {
                return null;
            }

            value = value.ToLower();

            for (j = 0; j < value.Length; j++)
            {
                switch (value[j])
                {
                    case 'á':
                    case 'â':
                    case 'ã':
                        r = r + "a";
                        continue;
                    case 'é':
                    case 'ê':
                        r = r + "e";
                        continue;
                    case 'í':
                        r = r + "i";
                        continue;
                    case 'ó':
                    case 'ô':
                    case 'õ':
                        r = r + "o";
                        continue;
                    case 'ú':
                    case 'ü':
                        r = r + "u";
                        continue;
                    case 'ç':
                        r = r + "c";
                        continue;
                    case 'ñ':
                        r = r + "n";
                        continue;
                }

                r = r + value[j];
            }

            return r;
        }

        private static string GetR1(string value)
        {
            if (value == null)
            {
                return null;
            }

            // find 1st vowel
            int j;
            var i = value.Length - 1;

            for (j = 0; j < i; j++)
            {
                if (IsVowel(value[j]))
                {
                    break;
                }
            }

            if (!(j < i))
            {
                return null;
            }

            // find 1st non-vowel
            for (; j < i; j++)
            {
                if (!IsVowel(value[j]))
                {
                    break;
                }
            }

            return !(j < i) ? null : value.Substring(j + 1);
        }

        private static string GetRv(string value)
        {
            if (value == null)
            {
                return null;
            }

            int j;
            var i = value.Length - 1;

            // RV - IF the second letter is a consoant, RV is the region after
            // the next following vowel
            if ((i > 0) && !IsVowel(value[1]))
            {
                // find 1st vowel
                for (j = 2; j < i; j++)
                {
                    if (IsVowel(value[j]))
                    {
                        break;
                    }
                }

                if (j < i)
                {
                    return value.Substring(j + 1);
                }
            }

            // RV - OR if the first two letters are vowels, RV is the region
            // after the next consoant
            if ((i > 1) && IsVowel(value[0]) && IsVowel(value[1]))
            {
                // find 1st consoant
                for (j = 2; j < i; j++)
                {
                    if (!IsVowel(value[j]))
                    {
                        break;
                    }
                }

                if (j < i)
                {
                    return value.Substring(j + 1);
                }
            }

            // RV - AND otherwise (consoant-vowel case) RV is the region after
            // the third letter.
            return i > 2 ? value.Substring(3) : null;
        }

        private static bool IsIndexable(string term)
        {
            return (term.Length < 30) && (term.Length > 2);
        }

        private static bool IsStemmable(string term)
        {
            for (var c = 0; c < term.Length; c++)
            {
                // Discard terms that contain non-letter characters.
                if (!char.IsLetter(term[c]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsVowel(char value)
        {
            return (value == 'a') || (value == 'e') || (value == 'i') || (value == 'o') || (value == 'u');
        }

        private static string RemoveSuffix(string value, string toRemove)
        {
            if ((value == null) || (toRemove == null) || !Suffix(value, toRemove))
            {
                return value;
            }

            return value.Substring(0, value.Length - toRemove.Length);
        }

        private static string ReplaceSuffix(string value, string toReplace, string changeTo)
        {
            if ((value == null) || (toReplace == null) || (changeTo == null))
            {
                return value;
            }

            var vvalue = RemoveSuffix(value, toReplace);

            if (value.Equals(vvalue))
            {
                return value;
            }

            return vvalue + changeTo;
        }

        private static bool Suffix(string value, string suffix)
        {
            if ((value == null) || (suffix == null))
            {
                return false;
            }

            return suffix.Length <= value.Length && value.Substring(value.Length - suffix.Length).Equals(suffix);
        }

        private static bool SuffixPreceded(string value, string suffix, string preceded)
        {
            // be-safe !!!
            if ((value == null) || (suffix == null) || (preceded == null) || !Suffix(value, suffix))
            {
                return false;
            }

            return Suffix(RemoveSuffix(value, suffix), preceded);
        }

        private void CreateCt(string term)
        {
            _ct = ChangeTerm(term);

            if (_ct.Length < 2) return;

            // if the first character is ... , remove it
            if ((_ct[0] == '"') || (_ct[0] == '\'') || (_ct[0] == '-') || (_ct[0] == ',') || (_ct[0] == ';')
                || (_ct[0] == '.') || (_ct[0] == '?') || (_ct[0] == '!'))
            {
                _ct = _ct.Substring(1);
            }

            if (_ct.Length < 2) return;

            // if the last character is ... , remove it
            if ((_ct[_ct.Length - 1] == '-') || (_ct[_ct.Length - 1] == ',') || (_ct[_ct.Length - 1] == ';')
                || (_ct[_ct.Length - 1] == '.') || (_ct[_ct.Length - 1] == '?') || (_ct[_ct.Length - 1] == '!')
                || (_ct[_ct.Length - 1] == '\'') || (_ct[_ct.Length - 1] == '"'))
            {
                _ct = _ct.Substring(0, _ct.Length - 1);
            }
        }

        private bool Step1()
        {
            if (_ct == null) return false;

            // suffix lenght = 7
            if (Suffix(_ct, "uciones") && Suffix(_r2, "uciones"))
            {
                _ct = ReplaceSuffix(_ct, "uciones", "u");
                return true;
            }

            // suffix lenght = 6
            if (_ct.Length >= 6)
            {
                if (Suffix(_ct, "imentos") && Suffix(_r2, "imentos"))
                {
                    _ct = RemoveSuffix(_ct, "imentos");
                    return true;
                }

                if (Suffix(_ct, "amentos") && Suffix(_r2, "amentos"))
                {
                    _ct = RemoveSuffix(_ct, "amentos");
                    return true;
                }

                if (Suffix(_ct, "adores") && Suffix(_r2, "adores"))
                {
                    _ct = RemoveSuffix(_ct, "adores");
                    return true;
                }

                if (Suffix(_ct, "adoras") && Suffix(_r2, "adoras"))
                {
                    _ct = RemoveSuffix(_ct, "adoras");
                    return true;
                }

                if (Suffix(_ct, "logias") && Suffix(_r2, "logias"))
                {
                    ReplaceSuffix(_ct, "logias", "log");
                    return true;
                }

                if (Suffix(_ct, "encias") && Suffix(_r2, "encias"))
                {
                    _ct = ReplaceSuffix(_ct, "encias", "ente");
                    return true;
                }

                if (Suffix(_ct, "amente") && Suffix(_r1, "amente"))
                {
                    _ct = RemoveSuffix(_ct, "amente");
                    return true;
                }

                if (Suffix(_ct, "idades") && Suffix(_r2, "idades"))
                {
                    _ct = RemoveSuffix(_ct, "idades");
                    return true;
                }
            }

            // suffix lenght = 5
            if (_ct.Length >= 5)
            {
                if (Suffix(_ct, "acoes") && Suffix(_r2, "acoes"))
                {
                    _ct = RemoveSuffix(_ct, "acoes");
                    return true;
                }

                if (Suffix(_ct, "imento") && Suffix(_r2, "imento"))
                {
                    _ct = RemoveSuffix(_ct, "imento");
                    return true;
                }

                if (Suffix(_ct, "amento") && Suffix(_r2, "amento"))
                {
                    _ct = RemoveSuffix(_ct, "amento");
                    return true;
                }

                if (Suffix(_ct, "adora") && Suffix(_r2, "adora"))
                {
                    _ct = RemoveSuffix(_ct, "adora");
                    return true;
                }

                if (Suffix(_ct, "ismos") && Suffix(_r2, "ismos"))
                {
                    _ct = RemoveSuffix(_ct, "ismos");
                    return true;
                }

                if (Suffix(_ct, "istas") && Suffix(_r2, "istas"))
                {
                    _ct = RemoveSuffix(_ct, "istas");
                    return true;
                }

                if (Suffix(_ct, "logia") && Suffix(_r2, "logia"))
                {
                    _ct = ReplaceSuffix(_ct, "logia", "log");
                    return true;
                }

                if (Suffix(_ct, "ucion") && Suffix(_r2, "ucion"))
                {
                    _ct = ReplaceSuffix(_ct, "ucion", "u");
                    return true;
                }

                if (Suffix(_ct, "encia") && Suffix(_r2, "encia"))
                {
                    _ct = ReplaceSuffix(_ct, "encia", "ente");
                    return true;
                }

                if (Suffix(_ct, "mente") && Suffix(_r2, "mente"))
                {
                    _ct = RemoveSuffix(_ct, "mente");
                    return true;
                }

                if (Suffix(_ct, "idade") && Suffix(_r2, "idade"))
                {
                    _ct = RemoveSuffix(_ct, "idade");
                    return true;
                }
            }

            // suffix lenght = 4
            if (_ct.Length >= 4)
            {
                if (Suffix(_ct, "acao") && Suffix(_r2, "acao"))
                {
                    _ct = RemoveSuffix(_ct, "acao");
                    return true;
                }

                if (Suffix(_ct, "ezas") && Suffix(_r2, "ezas"))
                {
                    _ct = RemoveSuffix(_ct, "ezas");
                    return true;
                }

                if (Suffix(_ct, "icos") && Suffix(_r2, "icos"))
                {
                    _ct = RemoveSuffix(_ct, "icos");
                    return true;
                }

                if (Suffix(_ct, "icas") && Suffix(_r2, "icas"))
                {
                    _ct = RemoveSuffix(_ct, "icas");
                    return true;
                }

                if (Suffix(_ct, "ismo") && Suffix(_r2, "ismo"))
                {
                    _ct = RemoveSuffix(_ct, "ismo");
                    return true;
                }

                if (Suffix(_ct, "avel") && Suffix(_r2, "avel"))
                {
                    _ct = RemoveSuffix(_ct, "avel");
                    return true;
                }

                if (Suffix(_ct, "ivel") && Suffix(_r2, "ivel"))
                {
                    _ct = RemoveSuffix(_ct, "ivel");
                    return true;
                }

                if (Suffix(_ct, "ista") && Suffix(_r2, "ista"))
                {
                    _ct = RemoveSuffix(_ct, "ista");
                    return true;
                }

                if (Suffix(_ct, "osos") && Suffix(_r2, "osos"))
                {
                    _ct = RemoveSuffix(_ct, "osos");
                    return true;
                }

                if (Suffix(_ct, "osas") && Suffix(_r2, "osas"))
                {
                    _ct = RemoveSuffix(_ct, "osas");
                    return true;
                }

                if (Suffix(_ct, "ador") && Suffix(_r2, "ador"))
                {
                    _ct = RemoveSuffix(_ct, "ador");
                    return true;
                }

                if (Suffix(_ct, "ivas") && Suffix(_r2, "ivas"))
                {
                    _ct = RemoveSuffix(_ct, "ivas");
                    return true;
                }

                if (Suffix(_ct, "ivos") && Suffix(_r2, "ivos"))
                {
                    _ct = RemoveSuffix(_ct, "ivos");
                    return true;
                }

                if (Suffix(_ct, "iras") && Suffix(_rv, "iras") && SuffixPreceded(_ct, "iras", "e"))
                {
                    _ct = ReplaceSuffix(_ct, "iras", "ir");
                    return true;
                }
            }

            // suffix lenght = 3
            if (_ct.Length >= 3)
            {
                if (Suffix(_ct, "eza") && Suffix(_r2, "eza"))
                {
                    _ct = RemoveSuffix(_ct, "eza");
                    return true;
                }

                if (Suffix(_ct, "ico") && Suffix(_r2, "ico"))
                {
                    _ct = RemoveSuffix(_ct, "ico");
                    return true;
                }

                if (Suffix(_ct, "ica") && Suffix(_r2, "ica"))
                {
                    _ct = RemoveSuffix(_ct, "ica");
                    return true;
                }

                if (Suffix(_ct, "oso") && Suffix(_r2, "oso"))
                {
                    _ct = RemoveSuffix(_ct, "oso");
                    return true;
                }

                if (Suffix(_ct, "osa") && Suffix(_r2, "osa"))
                {
                    _ct = RemoveSuffix(_ct, "osa");
                    return true;
                }

                if (Suffix(_ct, "iva") && Suffix(_r2, "iva"))
                {
                    _ct = RemoveSuffix(_ct, "iva");
                    return true;
                }

                if (Suffix(_ct, "ivo") && Suffix(_r2, "ivo"))
                {
                    _ct = RemoveSuffix(_ct, "ivo");
                    return true;
                }

                if (Suffix(_ct, "ira") && Suffix(_rv, "ira") && SuffixPreceded(_ct, "ira", "e"))
                {
                    _ct = ReplaceSuffix(_ct, "ira", "ir");
                    return true;
                }
            }

            // no ending was removed by step1
            return false;
        }

        private bool Step2()
        {
            if (_rv == null) return false;

            // suffix lenght = 7
            if (_rv.Length >= 7)
            {
                if (Suffix(_rv, "issemos"))
                {
                    _ct = RemoveSuffix(_ct, "issemos");
                    return true;
                }

                if (Suffix(_rv, "essemos"))
                {
                    _ct = RemoveSuffix(_ct, "essemos");
                    return true;
                }

                if (Suffix(_rv, "assemos"))
                {
                    _ct = RemoveSuffix(_ct, "assemos");
                    return true;
                }

                if (Suffix(_rv, "ariamos"))
                {
                    _ct = RemoveSuffix(_ct, "ariamos");
                    return true;
                }

                if (Suffix(_rv, "eriamos"))
                {
                    _ct = RemoveSuffix(_ct, "eriamos");
                    return true;
                }

                if (Suffix(_rv, "iriamos"))
                {
                    _ct = RemoveSuffix(_ct, "iriamos");
                    return true;
                }
            }

            // suffix lenght = 6
            if (_rv.Length >= 6)
            {
                if (Suffix(_rv, "iremos"))
                {
                    _ct = RemoveSuffix(_ct, "iremos");
                    return true;
                }

                if (Suffix(_rv, "eremos"))
                {
                    _ct = RemoveSuffix(_ct, "eremos");
                    return true;
                }

                if (Suffix(_rv, "aremos"))
                {
                    _ct = RemoveSuffix(_ct, "aremos");
                    return true;
                }

                if (Suffix(_rv, "avamos"))
                {
                    _ct = RemoveSuffix(_ct, "avamos");
                    return true;
                }

                if (Suffix(_rv, "iramos"))
                {
                    _ct = RemoveSuffix(_ct, "iramos");
                    return true;
                }

                if (Suffix(_rv, "eramos"))
                {
                    _ct = RemoveSuffix(_ct, "eramos");
                    return true;
                }

                if (Suffix(_rv, "aramos"))
                {
                    _ct = RemoveSuffix(_ct, "aramos");
                    return true;
                }

                if (Suffix(_rv, "asseis"))
                {
                    _ct = RemoveSuffix(_ct, "asseis");
                    return true;
                }

                if (Suffix(_rv, "esseis"))
                {
                    _ct = RemoveSuffix(_ct, "esseis");
                    return true;
                }

                if (Suffix(_rv, "isseis"))
                {
                    _ct = RemoveSuffix(_ct, "isseis");
                    return true;
                }

                if (Suffix(_rv, "arieis"))
                {
                    _ct = RemoveSuffix(_ct, "arieis");
                    return true;
                }

                if (Suffix(_rv, "erieis"))
                {
                    _ct = RemoveSuffix(_ct, "erieis");
                    return true;
                }

                if (Suffix(_rv, "irieis"))
                {
                    _ct = RemoveSuffix(_ct, "irieis");
                    return true;
                }
            }

            // suffix lenght = 5
            if (_rv.Length >= 5)
            {
                if (Suffix(_rv, "irmos"))
                {
                    _ct = RemoveSuffix(_ct, "irmos");
                    return true;
                }

                if (Suffix(_rv, "iamos"))
                {
                    _ct = RemoveSuffix(_ct, "iamos");
                    return true;
                }

                if (Suffix(_rv, "armos"))
                {
                    _ct = RemoveSuffix(_ct, "armos");
                    return true;
                }

                if (Suffix(_rv, "ermos"))
                {
                    _ct = RemoveSuffix(_ct, "ermos");
                    return true;
                }

                if (Suffix(_rv, "areis"))
                {
                    _ct = RemoveSuffix(_ct, "areis");
                    return true;
                }

                if (Suffix(_rv, "ereis"))
                {
                    _ct = RemoveSuffix(_ct, "ereis");
                    return true;
                }

                if (Suffix(_rv, "ireis"))
                {
                    _ct = RemoveSuffix(_ct, "ireis");
                    return true;
                }

                if (Suffix(_rv, "asses"))
                {
                    _ct = RemoveSuffix(_ct, "asses");
                    return true;
                }

                if (Suffix(_rv, "esses"))
                {
                    _ct = RemoveSuffix(_ct, "esses");
                    return true;
                }

                if (Suffix(_rv, "isses"))
                {
                    _ct = RemoveSuffix(_ct, "isses");
                    return true;
                }

                if (Suffix(_rv, "astes"))
                {
                    _ct = RemoveSuffix(_ct, "astes");
                    return true;
                }

                if (Suffix(_rv, "assem"))
                {
                    _ct = RemoveSuffix(_ct, "assem");
                    return true;
                }

                if (Suffix(_rv, "essem"))
                {
                    _ct = RemoveSuffix(_ct, "essem");
                    return true;
                }

                if (Suffix(_rv, "issem"))
                {
                    _ct = RemoveSuffix(_ct, "issem");
                    return true;
                }

                if (Suffix(_rv, "ardes"))
                {
                    _ct = RemoveSuffix(_ct, "ardes");
                    return true;
                }

                if (Suffix(_rv, "erdes"))
                {
                    _ct = RemoveSuffix(_ct, "erdes");
                    return true;
                }

                if (Suffix(_rv, "irdes"))
                {
                    _ct = RemoveSuffix(_ct, "irdes");
                    return true;
                }

                if (Suffix(_rv, "ariam"))
                {
                    _ct = RemoveSuffix(_ct, "ariam");
                    return true;
                }

                if (Suffix(_rv, "eriam"))
                {
                    _ct = RemoveSuffix(_ct, "eriam");
                    return true;
                }

                if (Suffix(_rv, "iriam"))
                {
                    _ct = RemoveSuffix(_ct, "iriam");
                    return true;
                }

                if (Suffix(_rv, "arias"))
                {
                    _ct = RemoveSuffix(_ct, "arias");
                    return true;
                }

                if (Suffix(_rv, "erias"))
                {
                    _ct = RemoveSuffix(_ct, "erias");
                    return true;
                }

                if (Suffix(_rv, "irias"))
                {
                    _ct = RemoveSuffix(_ct, "irias");
                    return true;
                }

                if (Suffix(_rv, "estes"))
                {
                    _ct = RemoveSuffix(_ct, "estes");
                    return true;
                }

                if (Suffix(_rv, "istes"))
                {
                    _ct = RemoveSuffix(_ct, "istes");
                    return true;
                }

                if (Suffix(_rv, "areis"))
                {
                    _ct = RemoveSuffix(_ct, "areis");
                    return true;
                }

                if (Suffix(_rv, "aveis"))
                {
                    _ct = RemoveSuffix(_ct, "aveis");
                    return true;
                }
            }

            // suffix lenght = 4
            if (_rv.Length >= 4)
            {
                if (Suffix(_rv, "aria"))
                {
                    _ct = RemoveSuffix(_ct, "aria");
                    return true;
                }

                if (Suffix(_rv, "eria"))
                {
                    _ct = RemoveSuffix(_ct, "eria");
                    return true;
                }

                if (Suffix(_rv, "iria"))
                {
                    _ct = RemoveSuffix(_ct, "iria");
                    return true;
                }

                if (Suffix(_rv, "asse"))
                {
                    _ct = RemoveSuffix(_ct, "asse");
                    return true;
                }

                if (Suffix(_rv, "esse"))
                {
                    _ct = RemoveSuffix(_ct, "esse");
                    return true;
                }

                if (Suffix(_rv, "isse"))
                {
                    _ct = RemoveSuffix(_ct, "isse");
                    return true;
                }

                if (Suffix(_rv, "aste"))
                {
                    _ct = RemoveSuffix(_ct, "aste");
                    return true;
                }

                if (Suffix(_rv, "este"))
                {
                    _ct = RemoveSuffix(_ct, "este");
                    return true;
                }

                if (Suffix(_rv, "iste"))
                {
                    _ct = RemoveSuffix(_ct, "iste");
                    return true;
                }

                if (Suffix(_rv, "arei"))
                {
                    _ct = RemoveSuffix(_ct, "arei");
                    return true;
                }

                if (Suffix(_rv, "erei"))
                {
                    _ct = RemoveSuffix(_ct, "erei");
                    return true;
                }

                if (Suffix(_rv, "irei"))
                {
                    _ct = RemoveSuffix(_ct, "irei");
                    return true;
                }

                if (Suffix(_rv, "aram"))
                {
                    _ct = RemoveSuffix(_ct, "aram");
                    return true;
                }

                if (Suffix(_rv, "eram"))
                {
                    _ct = RemoveSuffix(_ct, "eram");
                    return true;
                }

                if (Suffix(_rv, "iram"))
                {
                    _ct = RemoveSuffix(_ct, "iram");
                    return true;
                }

                if (Suffix(_rv, "avam"))
                {
                    _ct = RemoveSuffix(_ct, "avam");
                    return true;
                }

                if (Suffix(_rv, "arem"))
                {
                    _ct = RemoveSuffix(_ct, "arem");
                    return true;
                }

                if (Suffix(_rv, "erem"))
                {
                    _ct = RemoveSuffix(_ct, "erem");
                    return true;
                }

                if (Suffix(_rv, "irem"))
                {
                    _ct = RemoveSuffix(_ct, "irem");
                    return true;
                }

                if (Suffix(_rv, "ando"))
                {
                    _ct = RemoveSuffix(_ct, "ando");
                    return true;
                }

                if (Suffix(_rv, "endo"))
                {
                    _ct = RemoveSuffix(_ct, "endo");
                    return true;
                }

                if (Suffix(_rv, "indo"))
                {
                    _ct = RemoveSuffix(_ct, "indo");
                    return true;
                }

                if (Suffix(_rv, "arao"))
                {
                    _ct = RemoveSuffix(_ct, "arao");
                    return true;
                }

                if (Suffix(_rv, "erao"))
                {
                    _ct = RemoveSuffix(_ct, "erao");
                    return true;
                }

                if (Suffix(_rv, "irao"))
                {
                    _ct = RemoveSuffix(_ct, "irao");
                    return true;
                }

                if (Suffix(_rv, "adas"))
                {
                    _ct = RemoveSuffix(_ct, "adas");
                    return true;
                }

                if (Suffix(_rv, "idas"))
                {
                    _ct = RemoveSuffix(_ct, "idas");
                    return true;
                }

                if (Suffix(_rv, "aras"))
                {
                    _ct = RemoveSuffix(_ct, "aras");
                    return true;
                }

                if (Suffix(_rv, "eras"))
                {
                    _ct = RemoveSuffix(_ct, "eras");
                    return true;
                }

                if (Suffix(_rv, "iras"))
                {
                    _ct = RemoveSuffix(_ct, "iras");
                    return true;
                }

                if (Suffix(_rv, "avas"))
                {
                    _ct = RemoveSuffix(_ct, "avas");
                    return true;
                }

                if (Suffix(_rv, "ares"))
                {
                    _ct = RemoveSuffix(_ct, "ares");
                    return true;
                }

                if (Suffix(_rv, "eres"))
                {
                    _ct = RemoveSuffix(_ct, "eres");
                    return true;
                }

                if (Suffix(_rv, "ires"))
                {
                    _ct = RemoveSuffix(_ct, "ires");
                    return true;
                }

                if (Suffix(_rv, "ados"))
                {
                    _ct = RemoveSuffix(_ct, "ados");
                    return true;
                }

                if (Suffix(_rv, "idos"))
                {
                    _ct = RemoveSuffix(_ct, "idos");
                    return true;
                }

                if (Suffix(_rv, "amos"))
                {
                    _ct = RemoveSuffix(_ct, "amos");
                    return true;
                }

                if (Suffix(_rv, "emos"))
                {
                    _ct = RemoveSuffix(_ct, "emos");
                    return true;
                }

                if (Suffix(_rv, "imos"))
                {
                    _ct = RemoveSuffix(_ct, "imos");
                    return true;
                }

                if (Suffix(_rv, "iras"))
                {
                    _ct = RemoveSuffix(_ct, "iras");
                    return true;
                }

                if (Suffix(_rv, "ieis"))
                {
                    _ct = RemoveSuffix(_ct, "ieis");
                    return true;
                }
            }

            // suffix lenght = 3
            if (_rv.Length >= 3)
            {
                if (Suffix(_rv, "ada"))
                {
                    _ct = RemoveSuffix(_ct, "ada");
                    return true;
                }

                if (Suffix(_rv, "ida"))
                {
                    _ct = RemoveSuffix(_ct, "ida");
                    return true;
                }

                if (Suffix(_rv, "ara"))
                {
                    _ct = RemoveSuffix(_ct, "ara");
                    return true;
                }

                if (Suffix(_rv, "era"))
                {
                    _ct = RemoveSuffix(_ct, "era");
                    return true;
                }

                if (Suffix(_rv, "ira"))
                {
                    _ct = RemoveSuffix(_ct, "ava");
                    return true;
                }

                if (Suffix(_rv, "iam"))
                {
                    _ct = RemoveSuffix(_ct, "iam");
                    return true;
                }

                if (Suffix(_rv, "ado"))
                {
                    _ct = RemoveSuffix(_ct, "ado");
                    return true;
                }

                if (Suffix(_rv, "ido"))
                {
                    _ct = RemoveSuffix(_ct, "ido");
                    return true;
                }

                if (Suffix(_rv, "ias"))
                {
                    _ct = RemoveSuffix(_ct, "ias");
                    return true;
                }

                if (Suffix(_rv, "ais"))
                {
                    _ct = RemoveSuffix(_ct, "ais");
                    return true;
                }

                if (Suffix(_rv, "eis"))
                {
                    _ct = RemoveSuffix(_ct, "eis");
                    return true;
                }

                if (Suffix(_rv, "ira"))
                {
                    _ct = RemoveSuffix(_ct, "ira");
                    return true;
                }

                if (Suffix(_rv, "ear"))
                {
                    _ct = RemoveSuffix(_ct, "ear");
                    return true;
                }
            }

            // suffix lenght = 2
            if (_rv.Length >= 2)
            {
                if (Suffix(_rv, "ia"))
                {
                    _ct = RemoveSuffix(_ct, "ia");
                    return true;
                }

                if (Suffix(_rv, "ei"))
                {
                    _ct = RemoveSuffix(_ct, "ei");
                    return true;
                }

                if (Suffix(_rv, "am"))
                {
                    _ct = RemoveSuffix(_ct, "am");
                    return true;
                }

                if (Suffix(_rv, "em"))
                {
                    _ct = RemoveSuffix(_ct, "em");
                    return true;
                }

                if (Suffix(_rv, "ar"))
                {
                    _ct = RemoveSuffix(_ct, "ar");
                    return true;
                }

                if (Suffix(_rv, "er"))
                {
                    _ct = RemoveSuffix(_ct, "er");
                    return true;
                }

                if (Suffix(_rv, "ir"))
                {
                    _ct = RemoveSuffix(_ct, "ir");
                    return true;
                }

                if (Suffix(_rv, "as"))
                {
                    _ct = RemoveSuffix(_ct, "as");
                    return true;
                }

                if (Suffix(_rv, "es"))
                {
                    _ct = RemoveSuffix(_ct, "es");
                    return true;
                }

                if (Suffix(_rv, "is"))
                {
                    _ct = RemoveSuffix(_ct, "is");
                    return true;
                }

                if (Suffix(_rv, "eu"))
                {
                    _ct = RemoveSuffix(_ct, "eu");
                    return true;
                }

                if (Suffix(_rv, "iu"))
                {
                    _ct = RemoveSuffix(_ct, "iu");
                    return true;
                }

                if (Suffix(_rv, "iu"))
                {
                    _ct = RemoveSuffix(_ct, "iu");
                    return true;
                }

                if (Suffix(_rv, "ou"))
                {
                    _ct = RemoveSuffix(_ct, "ou");
                    return true;
                }
            }

            // no ending was removed by step2
            return false;
        }

        private void Step3()
        {
            if (_rv == null) return;

            if (Suffix(_rv, "i") && SuffixPreceded(_rv, "i", "c"))
            {
                _ct = RemoveSuffix(_ct, "i");
            }
        }

        private void Step4()
        {
            if (_rv == null) return;

            if (Suffix(_rv, "os"))
            {
                _ct = RemoveSuffix(_ct, "os");
                return;
            }

            if (Suffix(_rv, "a"))
            {
                _ct = RemoveSuffix(_ct, "a");
                return;
            }

            if (Suffix(_rv, "i"))
            {
                _ct = RemoveSuffix(_ct, "i");
                return;
            }

            if (Suffix(_rv, "o"))
            {
                _ct = RemoveSuffix(_ct, "o");
            }
        }

        private void Step5()
        {
            if (_rv == null)
            {
                return;
            }

            if (Suffix(_rv, "e"))
            {
                if (SuffixPreceded(_rv, "e", "gu"))
                {
                    _ct = RemoveSuffix(_ct, "e");
                    _ct = RemoveSuffix(_ct, "u");
                    return;
                }

                if (SuffixPreceded(_rv, "e", "ci"))
                {
                    _ct = RemoveSuffix(_ct, "e");
                    _ct = RemoveSuffix(_ct, "i");
                    return;
                }

                _ct = RemoveSuffix(_ct, "e");
            }
        }
    }
}
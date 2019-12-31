using RegularExpressionParser.Model;
using RegularExpressionParser.Utils;
using System;
using System.Text;


namespace RegularExpressionParser.BusinessLogic
{

    public class ExpressionValidator : IExpressionValidator
    {
        private const char _nullChar = '\0';

        private bool _matchConcante = false;
        private bool _alternate = false;

        private char _charMetaSymbl = _nullChar;

        private int _patternLength = -1;
        private string _matchPattern = String.Empty;
        private int _currentPosition = -1;
        private StringBuilder _matchStringBuilder = null;

        private ValidationInfo _validationInfo = null;

        public ExpressionValidator()
        {

        }

        public ValidationInfo Validate(string sPattern)
        {
            _charMetaSymbl = _nullChar;
            _matchConcante = false;
            _alternate = false;
            _matchStringBuilder = new StringBuilder(1024);
            _currentPosition = -1;
            _matchPattern = sPattern;
            _patternLength = _matchPattern.Length;

            _validationInfo = new ValidationInfo();

            if (sPattern.Length == 0)
            {
                _validationInfo.ErrorCode = CompilationStatus.EMPTY_STRING;
                return _validationInfo;
            }

            GetNextSymbol();

            string matchStart = MetaSymbol.MATCH_START.ToString();
            string matchEnd = MetaSymbol.MATCH_END.ToString();
            string range = matchStart + matchEnd;

            if (!(sPattern.CompareTo(matchStart) == 0 || sPattern.CompareTo(matchEnd) == 0 || sPattern.CompareTo(range) == 0))
            {
                if (sPattern[0] == MetaSymbol.MATCH_START)
                {
                    _validationInfo.MatchAtStart = true;
                    Accept(MetaSymbol.MATCH_START);
                }
                if (_matchPattern[_patternLength - 1] == MetaSymbol.MATCH_END)
                {
                    _validationInfo.MatchAtEnd = true;
                    _patternLength--;
                }
            }

            try
            {
                while (_currentPosition < _patternLength)
                {
                    switch (_charMetaSymbl)
                    {
                        case MetaSymbol.ALTERNATE:
                        case MetaSymbol.ONE_OR_MORE:
                        case MetaSymbol.ZERO_OR_MORE:
                        case MetaSymbol.ZERO_OR_ONE:
                            Abort(CompilationStatus.OPERAND_MISSING, _currentPosition, 1);
                            break;
                        case MetaSymbol.CLOSE_PREN:
                            Abort(CompilationStatus.PREN_MISMATCH, _currentPosition, 1);
                            break;
                        default:
                            Expression();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            _validationInfo.FormattedString = _matchStringBuilder.ToString();

            return _validationInfo;
        }

        private void GetNextSymbol()
        {
            _currentPosition++;
            if (_currentPosition < _patternLength)
            {
                _charMetaSymbl = _matchPattern[_currentPosition];
            }
            else
            {
                _charMetaSymbl = _nullChar;
            }
        }

        private bool Accept(char ch)
        {
            if (_charMetaSymbl == ch)
            {
                GetNextSymbol();
                return true;
            }
            return false;
        }

        private bool AcceptPostfixOperator()
        {
            switch (_charMetaSymbl)
            {
                case MetaSymbol.ONE_OR_MORE:
                case MetaSymbol.ZERO_OR_MORE:
                case MetaSymbol.ZERO_OR_ONE:
                    _matchStringBuilder.Append(_charMetaSymbl);
                    return Accept(_charMetaSymbl);
                default:
                    return false;
            }
        }

        private bool AcceptNonEscapeChar()
        {
            switch (_charMetaSymbl)
            {
                case MetaSymbol.ALTERNATE:
                case MetaSymbol.CHARSET_START:
                case MetaSymbol.CLOSE_PREN:
                case MetaSymbol.ESCAPE:
                case MetaSymbol.ONE_OR_MORE:
                case MetaSymbol.OPEN_PREN:
                case MetaSymbol.ZERO_OR_MORE:
                case MetaSymbol.ZERO_OR_ONE:
                case MetaSymbol.CONCANATE:
                case _nullChar:
                    return false;
                default:
                    AppendConate();
                    _matchStringBuilder.Append(_charMetaSymbl);
                    Accept(_charMetaSymbl);
                    break;
            }
            return true;
        }

        private bool Expect(char ch)
        {
            if (Accept(ch))
            {
                return true;
            }
            return false;
        }

        private bool ExpectEscapeChar()
        {
            switch (_charMetaSymbl)
            {
                case MetaSymbol.ALTERNATE:
                case MetaSymbol.ANY_ONE_CHAR:
                case MetaSymbol.CHARSET_START:
                case MetaSymbol.CLOSE_PREN:
                case MetaSymbol.COMPLEMENT:
                case MetaSymbol.ESCAPE:
                case MetaSymbol.ONE_OR_MORE:
                case MetaSymbol.OPEN_PREN:
                case MetaSymbol.ZERO_OR_MORE:
                case MetaSymbol.ZERO_OR_ONE:
                    _matchStringBuilder.Append(MetaSymbol.ESCAPE);
                    _matchStringBuilder.Append(_charMetaSymbl);
                    Accept(_charMetaSymbl);
                    break;
                case MetaSymbol.NEW_LINE:
                    _matchStringBuilder.Append('\n');
                    Accept(_charMetaSymbl);
                    break;
                case MetaSymbol.TAB:
                    _matchStringBuilder.Append('\t');
                    Accept(_charMetaSymbl);
                    break;
                default:
                    return false;
            }
            return true;
        }

        private void Abort(CompilationStatus errCode, int nErrPosition, int nErrLen)
        {
            _validationInfo.ErrorCode = errCode;
            _validationInfo.ErrorStartAt = nErrPosition;
            _validationInfo.ErrorLength = nErrLen;

            throw new Exception("A Syntex error occured.");
        }

        private void AppendConate()
        {
            if (_matchConcante)
            {
                _matchStringBuilder.Append(MetaSymbol.CONCANATE);
                _matchConcante = false;
            }
        }

        private void AppendAlternate()
        {
            if (_alternate)
            {
                _matchStringBuilder.Append(MetaSymbol.ALTERNATE);
                _alternate = false;
            }
        }

        private void Expression()
        {
            while (Accept(MetaSymbol.ESCAPE))
            {
                AppendConate();
                if (!ExpectEscapeChar())
                {
                    Abort(CompilationStatus.INVALID_ESCAPE, _currentPosition - 1, 1);
                }
                AcceptPostfixOperator();
                _matchConcante = true;
            }

            while (Accept(MetaSymbol.CONCANATE))
            {
                AppendConate();
                _matchStringBuilder.Append(MetaSymbol.ESCAPE);
                _matchStringBuilder.Append(MetaSymbol.CONCANATE);
                AcceptPostfixOperator();
                _matchConcante = true;
            }

            while (Accept(MetaSymbol.COMPLEMENT))
            {
                AppendConate();
                _matchStringBuilder.Append(MetaSymbol.ESCAPE);
                _matchStringBuilder.Append(MetaSymbol.COMPLEMENT);
                AcceptPostfixOperator();
                _matchConcante = true;
            }

            while (AcceptNonEscapeChar())
            {
                AcceptPostfixOperator();
                _matchConcante = true;
                Expression();
            }

            if (Accept(MetaSymbol.OPEN_PREN))
            {
                int entryPos = _currentPosition - 1;
                AppendConate();
                _matchStringBuilder.Append(MetaSymbol.OPEN_PREN);
                Expression();
                if (!Expect(MetaSymbol.CLOSE_PREN))
                {
                    Abort(CompilationStatus.PREN_MISMATCH, entryPos, _currentPosition - entryPos);
                }
                _matchStringBuilder.Append(MetaSymbol.CLOSE_PREN);

                int nLen = _currentPosition - entryPos;
                if (nLen == 2)
                {
                    Abort(CompilationStatus.EMPTY_PREN, entryPos, _currentPosition - entryPos);
                }

                AcceptPostfixOperator();
                _matchConcante = true;
                Expression();
            }


            if (Accept(MetaSymbol.CHARSET_START))
            {
                int entryPos = _currentPosition - 1;
                bool complement = false;

                AppendConate();

                if (Accept(MetaSymbol.COMPLEMENT))
                {
                    complement = true;
                }

                string sTmp = _matchStringBuilder.ToString();

                _matchStringBuilder = new StringBuilder(1024);
                _alternate = false;
                CharecterSet();

                if (!Expect(MetaSymbol.CHARSET_END))
                {
                    Abort(CompilationStatus.BRACKET_MISMATCH, entryPos, _currentPosition - entryPos);
                }

                int nLen = _currentPosition - entryPos;

                if (nLen == 2)
                {
                    Abort(CompilationStatus.EMPTY_BRACKET, entryPos, _currentPosition - entryPos);
                }
                else if (nLen == 3 && complement == true)
                {
                    _matchStringBuilder = new StringBuilder(1024);
                    _matchStringBuilder.Append(sTmp);
                    _matchStringBuilder.Append(MetaSymbol.OPEN_PREN);
                    _matchStringBuilder.Append(MetaSymbol.ESCAPE);
                    _matchStringBuilder.Append(MetaSymbol.COMPLEMENT);
                    _matchStringBuilder.Append(MetaSymbol.CLOSE_PREN);
                }
                else
                {
                    string sCharset = _matchStringBuilder.ToString();
                    _matchStringBuilder = new StringBuilder(1024);
                    _matchStringBuilder.Append(sTmp);
                    if (complement)
                    {
                        _matchStringBuilder.Append(MetaSymbol.COMPLEMENT);
                    }
                    _matchStringBuilder.Append(MetaSymbol.OPEN_PREN);
                    _matchStringBuilder.Append(sCharset);
                    _matchStringBuilder.Append(MetaSymbol.CLOSE_PREN);
                }

                AcceptPostfixOperator();

                _matchConcante = true;

                Expression();
            }

            if (Accept(MetaSymbol.ALTERNATE))
            {
                int entryPos = _currentPosition - 1;
                _matchConcante = false;
                _matchStringBuilder.Append(MetaSymbol.ALTERNATE);
                Expression();
                int nLen = _currentPosition - entryPos;
                if (nLen == 1)
                {
                    Abort(CompilationStatus.OPERAND_MISSING, entryPos, _currentPosition - entryPos);
                }
                Expression();
            }
        }

        private string ExpectEscapeCharInBracket()
        {
            char charSymbol = _charMetaSymbl;

            switch (_charMetaSymbl)
            {
                case MetaSymbol.CHARSET_END:
                case MetaSymbol.ESCAPE:
                    Accept(_charMetaSymbl);
                    return MetaSymbol.ESCAPE.ToString() + charSymbol.ToString();
                case MetaSymbol.NEW_LINE:
                    Accept(_charMetaSymbl);
                    return ('\n').ToString();
                case MetaSymbol.TAB:
                    Accept(_charMetaSymbl);
                    return ('\t').ToString();
                default:
                    return String.Empty;
            }
        }

        private string AcceptNonEscapeCharInBracket()
        {
            char charSymbol = _charMetaSymbl;

            switch (charSymbol)
            {
                case MetaSymbol.CHARSET_END:
                case MetaSymbol.ESCAPE:
                case _nullChar:
                    return String.Empty;
                case MetaSymbol.ALTERNATE:
                case MetaSymbol.ANY_ONE_CHAR:
                case MetaSymbol.CLOSE_PREN:
                case MetaSymbol.COMPLEMENT:
                case MetaSymbol.ONE_OR_MORE:
                case MetaSymbol.OPEN_PREN:
                case MetaSymbol.ZERO_OR_MORE:
                case MetaSymbol.ZERO_OR_ONE:
                case MetaSymbol.CONCANATE:
                    Accept(_charMetaSymbl);
                    return MetaSymbol.ESCAPE.ToString() + charSymbol.ToString();
                default:
                    Accept(_charMetaSymbl);
                    return charSymbol.ToString();
            }
        }

        private void CharecterSet()
        {
            int rangeFormStartAt = -1;
            int startAt = -1;
            int length = -1;

            string left = String.Empty;
            string range = String.Empty;
            string right = String.Empty;

            string temp = String.Empty;

            while (true)
            {
                temp = String.Empty;

                startAt = _currentPosition;

                if (Accept(MetaSymbol.ESCAPE))
                {
                    if ((temp = ExpectEscapeCharInBracket()) == String.Empty)
                    {
                        Abort(CompilationStatus.INVALID_ESCAPE, _currentPosition - 1, 1);
                    }
                    length = 2;
                }

                if (temp == String.Empty)
                {
                    temp = AcceptNonEscapeCharInBracket();
                    length = 1;
                }

                if (temp == String.Empty)
                {
                    break;
                }

                if (left == String.Empty)
                {
                    rangeFormStartAt = startAt;
                    left = temp;
                    AppendAlternate();
                    _matchStringBuilder.Append(temp);
                    _alternate = true;
                    continue;
                }

                if (range == String.Empty)
                {
                    if (temp != MetaSymbol.RANGE.ToString())
                    {
                        rangeFormStartAt = startAt;
                        left = temp;
                        AppendAlternate();
                        _matchStringBuilder.Append(temp);
                        _alternate = true;
                        continue;
                    }
                    else
                    {
                        range = temp;
                    }
                    continue;
                }

                right = temp;


                bool expanded = ExpandRange(left, right);

                if (expanded == false)
                {
                    int substringLength = (startAt + length) - rangeFormStartAt;

                    Abort(CompilationStatus.INVALID_RANGE, rangeFormStartAt, substringLength);
                }
                left = String.Empty;
                range = String.Empty;
                range = String.Empty;
            }

            if (range != String.Empty)
            {
                AppendAlternate();
                _matchStringBuilder.Append(range);
                _alternate = true;
            }
        }

        private bool ExpandRange(string left, string right)
        {
            char charLeft = (left.Length > 1 ? left[1] : left[0]);
            char charRight = (right.Length > 1 ? right[1] : right[0]);

            if (charLeft > charRight)
            {
                return false;
            }

            charLeft++;
            while (charLeft <= charRight)
            {
                AppendAlternate();

                switch (charLeft)
                {
                    case MetaSymbol.ALTERNATE:
                    case MetaSymbol.ANY_ONE_CHAR:
                    case MetaSymbol.CLOSE_PREN:
                    case MetaSymbol.COMPLEMENT:
                    case MetaSymbol.CONCANATE:
                    case MetaSymbol.ESCAPE:
                    case MetaSymbol.ONE_OR_MORE:
                    case MetaSymbol.ZERO_OR_MORE:
                    case MetaSymbol.ZERO_OR_ONE:
                    case MetaSymbol.OPEN_PREN:
                        _matchStringBuilder.Append(MetaSymbol.ESCAPE);
                        break;
                    default:
                        break;
                }

                _matchStringBuilder.Append(charLeft);
                _alternate = true;
                charLeft++;
            }

            return true;
        }
    }
}

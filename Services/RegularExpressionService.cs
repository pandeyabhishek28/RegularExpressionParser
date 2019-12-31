using RegularExpressionParser.BusinessLogic;
using RegularExpressionParser.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegularExpressionParser.Services
{
    public class RegularExpressionService : IRegularExpressionService
    {
        private IRegularExpressionRecognizer _expressionRecognizer;

        public RegularExpressionService(IRegularExpressionRecognizer expressionRecognizer)
        {
            _expressionRecognizer = expressionRecognizer;
        }

        public ExpressionRecognizerOutput EvaluateExpression(ExpressionRecognizerInput regularExpression)
        {
            var ret = new ExpressionRecognizerOutput();
            if (regularExpression.Expression.Length == 0)
            {
                ret.ErrorText = "You must enter a regular expression before compiling.";
                ret.CompletedSuccesfully = false;
                return ret;
            }
            StringBuilder sb = new StringBuilder(10000);
            try
            {
                var statusCode = _expressionRecognizer.CompileWithStats(regularExpression.Expression, sb);
                if (statusCode != CompilationStatus.SUCCESS)
                {
                    string errSubstring = regularExpression.Expression.Substring(_expressionRecognizer.GetLastErrorPosition(), _expressionRecognizer.GetLastErrorLength());
                    string format = "Error occured during compilation.\nCode: {0}\nAt: {1}\nSubstring: {2}";
                    ret.ErrorText = String.Format(format, statusCode.ToString(), _expressionRecognizer.GetLastErrorPosition(), errSubstring);
                    ret.CompletedSuccesfully = false;
                    return ret;
                }
            }
            catch (Exception ex)
            {
                ret.ErrorText = "Error occured during compilation.\n\n" + ex.Message;
                ret.CompletedSuccesfully = false;
                return ret;
            }

            ret.OutputText = sb.ToString();
            return ret;
        }

        public ExpressionRecognizerOutput FindAllMatch(ExpressionRecognizerInput regularExpression)
        {
            var ret = new ExpressionRecognizerOutput();
            if (!_expressionRecognizer.IsReady())
            {
                ret.ErrorText = "You must first compile a regular expression.";
                ret.CompletedSuccesfully = false;
                return ret;
            }
            ret.MatchInfo = new List<MatchInfo>();

            int foundStart = -1;
            int foundEnd = -1;
            int startAt = 0;
            int matchLength = -1;

            do
            {
                bool matchFound = _expressionRecognizer.FindMatch(regularExpression.SearchString, startAt,
                   regularExpression.SearchString.Length - 1, ref foundStart, ref foundEnd);
                if (matchFound == true)
                {
                    string sSubstring = "{Empty String}";
                    matchLength = foundEnd - foundStart + 1;
                    if (matchLength > 0)
                    {
                        sSubstring = regularExpression.SearchString.Substring(foundStart,
                            matchLength);
                    }

                    ret.MatchInfo.Add(
                        new MatchInfo()
                        {
                            MatchString = sSubstring,
                            StartIndex = foundStart,
                            EndIndex = foundEnd,
                            Length = matchLength
                        });

                    startAt = foundEnd + 1;
                }
                else
                {
                    break;
                }
            } while (startAt < regularExpression.SearchString.Length);

            return ret;
        }

        public ExpressionRecognizerOutput FindFirstMatch(ExpressionRecognizerInput regularExpression)
        {
            var ret = new ExpressionRecognizerOutput();
            if (!_expressionRecognizer.IsReady())
            {
                ret.ErrorText = "You must first compile a regular expression.";
                ret.CompletedSuccesfully = false;
                return ret;
            }
            ret.MatchInfo = new List<MatchInfo>();

            int foundStart = -1;
            int foundEnd = -1;

            bool matchFound = _expressionRecognizer.FindMatch(regularExpression.SearchString, 0,
                regularExpression.SearchString.Length - 1, ref foundStart, ref foundEnd);
            if (matchFound)
            {
                int matchLength = foundEnd - foundStart + 1;
                if (matchLength == 0)
                {
                    ret.OutputText = $"Matched an empty string at position { foundStart.ToString() }. ";
                }
                else
                {
                    ret.MatchInfo.Add(
                       new MatchInfo()
                       {
                           MatchString = regularExpression.SearchString.Substring(foundStart, foundEnd),
                           StartIndex = foundStart,
                           EndIndex = foundEnd,
                           Length = matchLength
                       });
                }
            }
            else
            {
                ret.OutputText = "No match found.";
            }

            return ret;
        }

        public ExpressionRecognizerOutput FindNextMatch(ExpressionRecognizerInput regularExpression)
        {
            var ret = new ExpressionRecognizerOutput();
            if (!_expressionRecognizer.IsReady())
            {
                ret.ErrorText = "You must first compile a regular expression.";
                ret.CompletedSuccesfully = false;
                return ret;
            }
            ret.MatchInfo = new List<MatchInfo>();

            int foundStart = -1;
            int foundEnd = -1;
            int startAt = 0;

            bool matchFound = _expressionRecognizer.FindMatch(regularExpression.SearchString, startAt,
                regularExpression.SearchString.Length - 1, ref foundStart, ref foundEnd);
            if (matchFound)
            {
                int matchLength = foundEnd - foundStart + 1;
                if (matchLength == 0)
                {
                    ret.OutputText = $"Matched an empty string at position { foundStart.ToString() }. ";
                }
                else
                {
                    ret.MatchInfo.Add(
                       new MatchInfo()
                       {
                           MatchString = regularExpression.SearchString.Substring(foundStart, foundEnd),
                           StartIndex = foundStart,
                           EndIndex = foundEnd,
                           Length = matchLength
                       });
                }
            }
            else
            {
                ret.OutputText = "No match found.";
            }

            return ret;
        }
    }
}

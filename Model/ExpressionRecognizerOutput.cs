using System.Collections.Generic;

namespace RegularExpressionParser.Model
{
    public class ExpressionRecognizerOutput
    {
        public string OutputText { get; set; }

        public string ErrorText { get; set; }

        public bool CompletedSuccesfully { get; set; }

        public IList<MatchInfo> MatchInfo { get; set; }

        public ExpressionRecognizerOutput(bool completedSuccesfully = true)
        {
            CompletedSuccesfully = completedSuccesfully;
            MatchInfo = new List<MatchInfo>();
        }
    }
}

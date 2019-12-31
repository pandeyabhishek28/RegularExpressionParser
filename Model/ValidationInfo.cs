using System;


namespace RegularExpressionParser.Model
{
    internal class ValidationInfo
    {
        public CompilationStatus ErrorCode { get; set; } = CompilationStatus.SUCCESS;
        public int ErrorStartAt { get; set; } = -1;
        public int ErrorLength { get; set; } = -1;
        public string FormattedString { get; set; } = String.Empty;
        public bool MatchAtStart { get; set; } = false;
        public bool MatchAtEnd { get; set; } = false;
    }
}

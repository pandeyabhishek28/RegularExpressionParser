using RegularExpressionParser.Model;

namespace RegularExpressionParser.Services
{
    public interface IRegularExpressionService
    {
        ExpressionRecognizerOutput EvaluateExpression(ExpressionRecognizerInput regularExpression);
        ExpressionRecognizerOutput FindAllMatch(ExpressionRecognizerInput regularExpression);
        ExpressionRecognizerOutput FindNextMatch(ExpressionRecognizerInput regularExpression);
        ExpressionRecognizerOutput FindFirstMatch(ExpressionRecognizerInput regularExpression);
    }
}

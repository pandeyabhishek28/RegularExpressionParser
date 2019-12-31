using RegularExpressionParser.Model;

namespace RegularExpressionParser.BusinessLogic
{
    public interface IExpressionValidator
    {
        ValidationInfo Validate(string sPattern);
    }
}

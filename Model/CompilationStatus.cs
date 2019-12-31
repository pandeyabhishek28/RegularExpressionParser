namespace RegularExpressionParser.Model
{
    public enum CompilationStatus
    {
        SUCCESS,
        PREN_MISMATCH,
        EMPTY_PREN,
        EMPTY_BRACKET,
        BRACKET_MISMATCH,
        OPERAND_MISSING,
        INVALID_ESCAPE,
        INVALID_RANGE,
        EMPTY_STRING,
    }
}

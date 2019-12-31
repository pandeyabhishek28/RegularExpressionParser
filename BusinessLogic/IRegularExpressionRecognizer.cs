using RegularExpressionParser.Model;
using System.Text;

namespace RegularExpressionParser.BusinessLogic
{
    public interface IRegularExpressionRecognizer
    {
        CompilationStatus CompileWithStats(string argPattern, StringBuilder argSBStats);


        CompilationStatus Compile(string sPattern);

        bool IsReady();

        int GetLastErrorPosition();
        public CompilationStatus GetLastErrorCode();

        public int GetLastErrorLength();

        bool FindMatch(string sSearchIn, int nSearchStartAt,
                             int nSearchEndAt, ref int nFoundBeginAt, ref int nFoundEndAt);
    }
}

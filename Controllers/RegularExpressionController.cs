using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RegularExpressionParser.Model;
using RegularExpressionParser.Services;

namespace RegularExpressionParser.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegularExpressionController : ControllerBase
    {
        private IRegularExpressionService _regularExpressionService;
        private ILogger<RegularExpressionController> _logger;
        private static ExpressionRecognizerInput RegularExpression;
        public RegularExpressionController(IRegularExpressionService regularExpressionService,
            ILogger<RegularExpressionController> logger)
        {
            _regularExpressionService = regularExpressionService;
            _logger = logger;
        }

        [HttpPost("PostStatistics")]
        public ExpressionRecognizerOutput PostStatistics([FromBody]ExpressionRecognizerInput regularExpression)
        {
            var ret = _regularExpressionService.EvaluateExpression(regularExpression);
            RegularExpression = regularExpression;
            return ret;
        }

        [HttpGet("GetAll")]
        [ActionName("GetAll")]
        public ExpressionRecognizerOutput GetAllMatch()
        {
            var ret = _regularExpressionService.FindAllMatch(RegularExpression);
            return ret;
        }

        [HttpGet("GetFirst")]
        [ActionName("GetFirst")]
        public ExpressionRecognizerOutput GetFirstMatch()
        {
            var ret = _regularExpressionService.FindFirstMatch(RegularExpression);
            return ret;
        }

        [HttpGet("GetNext")]
        [ActionName("GetNext")]
        public ExpressionRecognizerOutput GetNextMatch()
        {
            var ret = _regularExpressionService.FindNextMatch(RegularExpression);
            return ret;
        }

    }
}
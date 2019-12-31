using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RegularExpressionParser.Model;
using RegularExpressionParser.Services;

namespace RegularExpressionParser.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RegularExpressionController : ControllerBase
    {
        private IRegularExpressionService _regularExpressionService;
        private ILogger<RegularExpressionController> _logger;

        public RegularExpressionController(IRegularExpressionService regularExpressionService,
            ILogger<RegularExpressionController> logger)
        {
            _regularExpressionService = regularExpressionService;
            _logger = logger;
        }

        [HttpGet]
        [ActionName("GetStatistics")]
        public ExpressionRecognizerOutput GetStatistics(ExpressionRecognizerInput regularExpression)
        {
            var ret = _regularExpressionService.EvaluateExpression(regularExpression);
            return ret;
        }

        [HttpGet]
        [ActionName("GetAll")]
        public ExpressionRecognizerOutput GetAllMatch(ExpressionRecognizerInput regularExpression)
        {
            var ret = _regularExpressionService.FindAllMatch(regularExpression);
            return ret;
        }

        [HttpGet]
        [ActionName("GetFirst")]
        public ExpressionRecognizerOutput GetFirstMatch(ExpressionRecognizerInput regularExpression)
        {
            var ret = _regularExpressionService.FindFirstMatch(regularExpression);
            return ret;
        }

        [HttpGet]
        [ActionName("GetNext")]
        public ExpressionRecognizerOutput GetNextMatch(ExpressionRecognizerInput regularExpression)
        {
            var ret = _regularExpressionService.FindNextMatch(regularExpression);
            return ret;
        }
    }
}
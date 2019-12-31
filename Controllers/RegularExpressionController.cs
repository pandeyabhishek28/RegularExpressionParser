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
        private static ExpressionRecognizerInput RegularExpression;
        public RegularExpressionController(IRegularExpressionService regularExpressionService,
            ILogger<RegularExpressionController> logger)
        {
            _regularExpressionService = regularExpressionService;
            _logger = logger;
        }

        [HttpPost]
        [ActionName("PostStatistics")]
        public ExpressionRecognizerOutput PostStatistics([FromBody]ExpressionRecognizerInput regularExpression)
        {
            var ret = _regularExpressionService.EvaluateExpression(regularExpression);
            RegularExpression = regularExpression;
            return ret;
        }

        [HttpGet]
        [ActionName("GetAll")]
        public ExpressionRecognizerOutput GetAllMatch()
        {
            var ret = _regularExpressionService.FindAllMatch(RegularExpression);
            return ret;
        }

        [HttpGet]
        [ActionName("GetFirst")]
        public ExpressionRecognizerOutput GetFirstMatch()
        {
            var ret = _regularExpressionService.FindFirstMatch(RegularExpression);
            return ret;
        }

        [HttpGet]
        [ActionName("GetNext")]
        public ExpressionRecognizerOutput GetNextMatch()
        {
            var ret = _regularExpressionService.FindNextMatch(RegularExpression);
            return ret;
        }

        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            Response.StatusCode = 404;
            return Ok();
        }
    }
}
using System.Threading.Tasks;
using Cuemon.AspNetCore.Http;
using Cuemon.AspNetCore.Mvc.Filters.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

namespace Codebelt.Extensions.Asp.Versioning.Assets
{
    [ApiController]
    [Route("[controller]")]
    public class FakeController : ControllerBase
    {
        private readonly MvcFaultDescriptorOptions _options;

        public FakeController(IOptions<MvcFaultDescriptorOptions> setup)
        {
            _options = setup.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Unit Test");
        }

        [HttpPost]
        public IActionResult Post(string random)
        {
            return Ok("Unit Test");
        }

        [HttpGet]
        [Route("throw")]
        public IActionResult GetException()
        {
            throw new GoneException();
        }

        [HttpGet]
        [Route("not-acceptable")]
        public IActionResult GetNotAcceptable()
        {
            return StatusCode(StatusCodes.Status406NotAcceptable);
        }

        [HttpGet]
        [Route("teapot")]
        public IActionResult GetTeapot()
        {
            return StatusCode(StatusCodes.Status418ImATeapot);
        }

        [HttpGet]
        [Route("problem418")]
        public async Task<IActionResult> GetProblem418([FromServices] IProblemDetailsService problemDetailsService)
        {
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = HttpContext,
                ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = StatusCodes.Status418ImATeapot,
                    Detail = "I'm a teapot - an unmapped status code"
                }
            });
            return new EmptyResult();
        }
    }
}

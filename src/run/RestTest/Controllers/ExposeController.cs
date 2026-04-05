using Microsoft.AspNetCore.Mvc;
using RestTest;

namespace RestTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExposeController : ControllerBase
    {
        [HttpPost("add")]
        [ProducesResponseType(typeof(AddResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Add([FromBody] AddRequest request)
        {
            try
            {
                var result = expose.Add(request.A, request.B);
                return Ok(new AddResponse { Result = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        [HttpPost("concat")]
        [ProducesResponseType(typeof(ConcatResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Concat([FromBody] ConcatRequest request)
        {
            try
            {
                var result = expose.Concat(request.Str1, request.Str2);
                return Ok(new ConcatResponse { Result = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        [HttpGet("test")]
        [ProducesResponseType(typeof(TestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Test()
        {
            try
            {
                var result = expose.Test();
                return Ok(new TestResponse { Result = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }

        [HttpPost("processuser")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult<User> ProcessUser([FromBody] User user)
        {
            try
            {
                var exposeInstance = new expose();
                var result = exposeInstance.ProcessUser(user);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse { Error = ex.Message });
            }
        }
    }

    public class AddRequest
    {
        public int A { get; set; }
        public int B { get; set; }
    }

    public class ConcatRequest
    {
        public string Str1 { get; set; } = string.Empty;
        public string Str2 { get; set; } = string.Empty;
    }

    // Response models for OpenAPI documentation
    public class AddResponse
    {
        public int Result { get; set; }
    }

    public class ConcatResponse
    {
        public string Result { get; set; } = string.Empty;
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }

  
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}




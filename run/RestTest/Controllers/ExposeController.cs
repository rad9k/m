using Microsoft.AspNetCore.Mvc;

namespace RestTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExposeController : ControllerBase
    {
        [HttpPost("add")]
        public IActionResult Add([FromBody] AddRequest request)
        {
            try
            {
                var result = expose.Add(request.A, request.B);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("concat")]
        public IActionResult Concat([FromBody] ConcatRequest request)
        {
            try
            {
                var result = expose.Concat(request.Str1, request.Str2);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("multiply")]
        public IActionResult Multiply([FromBody] MultiplyRequest request)
        {
            try
            {
                var result = expose.Multiply(request.X, request.Y);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("divide")]
        public IActionResult Divide([FromBody] DivideRequest request)
        {
            try
            {
                var result = expose.Divide(request.X, request.Y);
                return Ok(new { result });
            }
            catch (DivideByZeroException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("subtract")]
        public IActionResult Subtract([FromBody] SubtractRequest request)
        {
            try
            {
                var result = expose.subtract(request.A, request.B);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
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

    public class MultiplyRequest
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class DivideRequest
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class SubtractRequest
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}



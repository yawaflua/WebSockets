using Microsoft.AspNetCore.Mvc;

namespace Examples.Controllers;

[ApiController]
[Route("/api/v1/test")]
public class TestDefaultController : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> TestController([FromBody] string TextTest)
    {
        return Ok("Test");
    }
}
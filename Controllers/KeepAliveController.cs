using Microsoft.AspNetCore.Mvc;

namespace Edi.Translator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class KeepAliveController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Message = "Session is kept alive" });
}
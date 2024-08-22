using Microsoft.AspNetCore.Mvc;

namespace Edi.Translator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class KeepAliveController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Session is kept alive");
}
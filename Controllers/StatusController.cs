using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace APIPlugin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        var text = "Status Information";

        return text;
    }

    [HttpGet("{id}")]
    public ActionResult Get(string id)
    {
        Debug.WriteLine($"Received request for ID: {id}");
        return Ok();
    }
}
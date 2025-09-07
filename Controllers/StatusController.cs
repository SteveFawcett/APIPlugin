using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static APIPlugin.PluginBase;

namespace APIPlugin.Controllers;


[Route("api/[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
    private readonly PluginSettings _pluginSettings;
    private readonly APIPlugin.Forms.WriteMessageDelegate WriteMessage;

    public StatusController(PluginSettingsAccessor accessor)
    {
        _pluginSettings = accessor.Current;
        WriteMessage = accessor.Current.WriteMessage;
    }

    [HttpGet]
    public ActionResult<string> Get()
    {
        WriteMessage("Status endpoint accessed.");
        var text = "Status Information";

        return text;
    }

    [HttpGet("{id}")]
    public ActionResult Get(string id)
    {
        WriteMessage($"Received request for ID: {id}");
        return Ok();
    }
}
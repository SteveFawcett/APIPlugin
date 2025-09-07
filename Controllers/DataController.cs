using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using static APIPlugin.PluginBase;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace APIPlugin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DataController : Controller
{
    private readonly PluginSettings _pluginSettings;
    private readonly APIPlugin.Forms.WriteMessageDelegate WriteMessage;

    public DataController(PluginSettingsAccessor accessor)
    {
        _pluginSettings = accessor.Current;
        WriteMessage = accessor.Current.WriteMessage;
    }

    [HttpGet]
    public ActionResult<List<string>> Get()
    {
        try
        {
            var request = HttpContext.Request;
            var fullUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            
            WriteMessage($"Received GET request: {fullUrl}");

            var plugins = new List<string>();
            return Ok(CallCacheData(plugins));
        }

        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetCacheData: {ex.Message}");
            return StatusCode(500, $"Internal error fetching plugin data. {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public ActionResult Get(string id)
    {
        try
        {
            var request = HttpContext.Request;
            var fullUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

            var decodedId = HttpUtility.HtmlDecode(id);

            WriteMessage($"Received GET request: {fullUrl} ID: {id}");

            var plugins = new List<string> { decodedId };
            return Ok(CallCacheData(plugins));
        }

        catch (Exception ex)
        {
            Debug.WriteLine($"Error in GetCacheData: {ex.Message}");
            return StatusCode(500, $"Internal error fetching plugin data. {ex.Message}");
        }
    }

    private List<KeyValuePair<string, string>> CallCacheData(List<string> required)
    {
        if (_pluginSettings?.GetCacheData is null)
        {
            Debug.WriteLine("No Primary cache defined");
            return [];
        }

        var data = _pluginSettings.GetCacheData(required);

        Debug.WriteLine($"Data returned from GetCacheData: {data.Count} items");
        return data;
    }
}
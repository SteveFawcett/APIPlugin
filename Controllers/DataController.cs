using System.Diagnostics;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using static APIPlugin.PluginBase;

namespace APIPlugin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DataController : Controller
{
    private readonly PluginSettings _pluginSettings;

    public DataController(PluginSettingsAccessor accessor)
    {
        _pluginSettings = accessor.Current;
    }

    [HttpGet]
    public ActionResult<List<string>> Get()
    {
        try
        {
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
            var decodedId = HttpUtility.HtmlDecode(id);
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
        if (_pluginSettings?.GetCacheData is null) throw new NotImplementedException("No Primary cache defined");
        var data = _pluginSettings.GetCacheData(required);
        Debug.WriteLine($"Data returned from GetCacheData: {data.Count} items");
        return data;
    }
}
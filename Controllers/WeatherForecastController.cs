using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AzureSQLDBConnectionAPI.Controllers;
[ApiController]
[Route("api/WF")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly AzureDBContext context;

    private static readonly string[] Summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
    public WeatherForecastController(ILogger<WeatherForecastController> logger, AzureDBContext context)
    {
        _logger = logger;
        this.context = context;

    }

    [HttpGet("version", Name = "GetVersion")]
    public async Task<string> GetVersion()
    {
        try
        {
            var version = await context.Version.ToListAsync();
            return version?.Count > 0 ? version[0].Appversion ?? "No Version Found" : "No Version Found";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [HttpGet("forecast", Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}

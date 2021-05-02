using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Realmar.Jobbernetes.Demo.DataViewer.Shared;

namespace Realmar.Jobbernetes.Demo.DataViewer.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                              {
                                  Date         = DateTime.Now.AddDays(index),
                                  TemperatureC = rng.Next(-20, 55),
                                  Summary      = Summaries[rng.Next(Summaries.Length)]
                              })
                             .ToArray();
        }
    }
}

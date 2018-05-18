using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using OpenWeatherMapSharp;
using OpenWeatherMapSharp.Enums;

namespace Glovebox.Enviromon
{
  public static class WeatherRefernceData
  {
    static string owmKey = System.Environment.GetEnvironmentVariable("emOpenWeatherMapKey");

    [FunctionName("WeatherReferenceData")]
    public static void Run(
      [TimerTrigger("0 */15 * * * *")]TimerInfo myTimer,
      [Table("DeviceState", "Forbes", "syd-master", Connection = "emStorageCS")] TelemetryItem inputTable,
      [Blob("reference-data/weather/{datetime:yyyy-MM-dd/HH-mm}/weather.csv", FileAccess.Write, Connection = "emStorageCS")] out string ReferenceData,
      TraceWriter log)
    {
      log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
      bool status = Task.Run(async () => await GetPressure(inputTable)).Result;

      if (!status) { throw new Exception("Missing Open Weather Map data"); }

      string data = $"WeatherId;Celsius;Pressure;Humidity{Environment.NewLine}Sydney, AU;{inputTable.Celsius.ToString()};{inputTable.hPa.ToString()};{inputTable.Humidity.ToString()}";

      ReferenceData = data;
    }

    static async Task<bool> GetPressure(TelemetryItem inputTable)
    {
      OpenWeatherMapService client = new OpenWeatherMapService(owmKey);
      var result = await client.GetWeatherAsync("Sydney, AU", LanguageCode.EN, Unit.Metric);

      if (result.IsSuccess)
      {
        inputTable.hPa = result.Response.MainWeather.Pressure;
        inputTable.Humidity = result.Response.MainWeather.Humidity;

        return true;
      }
      return false;
    }
  }
}

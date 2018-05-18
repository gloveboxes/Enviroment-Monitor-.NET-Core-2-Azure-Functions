// https://www.nuget.org/packages/Microsoft.Azure.ServiceBus/
// https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.EventHubs
// https://www.nuget.org/packages/Microsoft.NET.Sdk.Functions/

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Glovebox.Enviromon
{
  public static class TelemetryProcessor
  {
    private const string ConsumerGroup = "telemetry-processor";
    static string eventHubConnectionString = System.Environment.GetEnvironmentVariable("emEventHubSenderCS");
    static string chartDataEventHub = System.Environment.GetEnvironmentVariable("emEventHubChartData");

    [FunctionName("TelemetryProcessor")]
    public static async Task RunAsync(
        [EventHubTrigger("devices", Connection = "emEventHubListenerCS", ConsumerGroup = ConsumerGroup)] string[] eventHubMessages,
        [Table("DeviceState", Connection = "emStorageCS")] CloudTable deviceStateTable,
        [Table("Calibration", Connection = "emStorageCS")] CloudTable calibrationTable,
        TraceWriter log)
    {

      IQueueClient queueClient = new QueueClient(eventHubConnectionString, chartDataEventHub);

      foreach (var message in eventHubMessages)
      {
        var t = JsonConvert.DeserializeObject<TelemetryItem>(message);

        t.PartitionKey = "Forbes";
        t.RowKey = t.DeviceId;

        if (!ValidateTelemetry(t))
        {
          log.Info("Validation Failed");
          log.Info(message.ToString());
        }
        else
        {
          TableOperation op = TableOperation.Retrieve<Calibration>("Forbes", t.DeviceId);
          var query = await calibrationTable.ExecuteAsync(op);

          if (query.Result != null)
          {
            Calibration calibration = (Calibration)query.Result;
            t.Celsius = Math.Round(t.Celsius * calibration.TemperatureSlope + calibration.TemperatureYIntercept, 1);
            t.Humidity = Math.Round(t.Humidity * calibration.HumiditySlope + calibration.HumidityYIntercept, 1);
            t.hPa = Math.Round(t.hPa * calibration.PressureSlope + calibration.PressureYIntercept, 1);
          }

          await queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(t))));

          var operation = TableOperation.InsertOrReplace(t);
          await deviceStateTable.ExecuteAsync(operation);
        }
      }
    }

    static bool ValidateTelemetry(TelemetryItem telemetry)
    {
      if (telemetry.Celsius < -10 || telemetry.Celsius > 70)
      {
        return false;
      }

      if (telemetry.Humidity < 0 || telemetry.Humidity > 100)
      {
        return false;
      }

      if (telemetry.hPa < 0 || telemetry.hPa > 1400)
      {
        return false;
      }

      return true;
    }
  }
}

using System;
using System.Linq;
using System.Text;
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
    static string eventHubConnectionString = System.Environment.GetEnvironmentVariable("enviromon-eh_RootManageSharedAccessKey_EVENTHUB");
    static string eventHubEntityPath = "chart-data";

    [FunctionName("TelemetryProcessor")]
    public static async void RunAsync(
    [EventHubTrigger("devices", Connection = "EventHubConnection", ConsumerGroup = "devicestate")] string myEventHubMessage,
    [Table("DeviceState", Connection = "AzureWebJobsStorage")] CloudTable outputTable,
    [Table("Calibration", Connection = "AzureWebJobsStorage")] CloudTable calibrationTable,
    TraceWriter log)
    {
      log.Info(myEventHubMessage as string);
      IQueueClient queueClient = new QueueClient(eventHubConnectionString, eventHubEntityPath);

      var t = JsonConvert.DeserializeObject<Item>(myEventHubMessage);

      t.PartitionKey = "Forbes";
      t.RowKey = t.DeviceId;

      if (!ValidateTelemetry(t))
      {
        log.Info("Validation Failed");
        log.Info(myEventHubMessage.ToString());
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
        await outputTable.ExecuteAsync(operation);
      }
    }

    static bool ValidateTelemetry(Item telemetry)
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

 public class Item : TableEntity
    {
      public string DeviceId { get; set; }
      public double Celsius { get; set; }
      public double Humidity { get; set; }
      public double hPa { get; set; }
      public double Light { get; set; }
      public string Geo { get; set; }
      public string Schema { get; set; }
      public int Id { get; set; }
      public int NotSent { get; set; }
    }

    public class Calibration : TableEntity
    {
      public double TemperatureSlope { get; set; }
      public double TemperatureYIntercept { get; set; }
      public double HumiditySlope { get; set; }
      public double HumidityYIntercept { get; set; }
      public double PressureSlope { get; set; }
      public double PressureYIntercept { get; set; }
    }
}

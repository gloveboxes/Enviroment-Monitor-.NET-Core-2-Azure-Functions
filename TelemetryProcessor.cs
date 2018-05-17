using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Glovebox.Enviromon
{
  public static class TelemetryProcessor
  {
    [FunctionName("TelemetryProcessor")]
    public static async void RunAsync([EventHubTrigger("devices", Connection = "EventHubConnection", ConsumerGroup = "devicestate")] string myEventHubMessage,
    [Table("DeviceState", Connection="AzureWebJobsStorage")] CloudTable outputTable,
    // [Table("Calibration", Connection="AzureWebJobsStorage")] IQueryable<Calibration> calibrationTable,
    TraceWriter log)
    // public static void Run([QueueTrigger("devices", Connection = "AzureWebJobsStorage")]string myQueueItem, TraceWriter log)
    {
      log.Info(myEventHubMessage as string);
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
        // TableResult query = calibrationTable.Execute(op);
        // var calibartionData = calibrationTable.Where(p => p.PartitionKey == "Forbes" && p.RowKey == t.DeviceId).ToList();

        // if (query.Result != null)
        // {
        //   Calibration calibration = (Calibration)query.Result;
        //   t.Celsius = Math.Round(t.Celsius * calibration.TemperatureSlope + calibration.TemperatureYIntercept, 1);
        //   t.Humidity = Math.Round(t.Humidity * calibration.HumiditySlope + calibration.HumidityYIntercept, 1);
        //   t.hPa = Math.Round(t.hPa * calibration.PressureSlope + calibration.PressureYIntercept, 1);
        // }

        // string chartJson = JsonConvert.SerializeObject(t);
        // eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(chartJson)));

        var operation = TableOperation.InsertOrReplace(t);
        await outputTable.ExecuteAsync(operation);
      }

    }
    static bool ValidateTelemetry(Item telemetry){
    if (telemetry.Celsius < -10 || telemetry.Celsius > 70){
        return false;
    }

    if (telemetry.Humidity < 0 || telemetry.Humidity > 100){
        return false;
    }

    if (telemetry.hPa < 0 || telemetry.hPa > 1400) {
        return false;
    }

    return true;
}


public class Item: TableEntity
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
    public double TemperatureSlope   { get;set; }
    public double TemperatureYIntercept { get;set; }
    public double HumiditySlope   { get;set; }
    public double HumidityYIntercept { get;set; }
    public double PressureSlope   { get;set; }
    public double PressureYIntercept { get;set; }
}
  }

  
}

using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json.Serialization;

namespace Glovebox.Enviromon
{
    public static class Gateway 
  {
    static string eventHubConnectionString = System.Environment.GetEnvironmentVariable("enviromon-eh_RootManageSharedAccessKey_EVENTHUB");
    static string eventHubEntityPath = System.Environment.GetEnvironmentVariable("EventHubPath");

    [FunctionName("ThingsNetworkGateway")]
    public static async System.Threading.Tasks.Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
    {
      IQueueClient queueClient = new QueueClient(eventHubConnectionString, eventHubEntityPath);

      string requestBody = new StreamReader(req.Body).ReadToEnd();
      dynamic data = JsonConvert.DeserializeObject(requestBody);

      TheThingsNetworkEntity ttn = JsonConvert.DeserializeObject<TheThingsNetworkEntity>(data.ToString(), new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
      Dictionary<string, float> payload = ttn.payload_fields.ToObject<Dictionary<string, float>>();

      Telemetry telemetry = new Telemetry()
      {
        DeviceId = ttn.dev_id,
        Geo = ttn.dev_id,
        Id = ttn.counter
      };

      telemetry.Celsius = (from i in payload where i.Key.StartsWith("temperature") select i).First().Value;
      telemetry.hPa = (from i in payload where i.Key.StartsWith("barometric_pressure") select i).First().Value;
      telemetry.Humidity = (from i in payload where i.Key.StartsWith("relative_humidity") select i).First().Value;

      string json = JsonConvert.SerializeObject(telemetry);

      log.Info(json);

      await queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(json)));

      return (ActionResult)new OkObjectResult("Success");
    }
  }

  public class Telemetry
  {
    public string DeviceId { get; set; }
    public float Celsius { get; set; }
    public float Humidity { get; set; }
    public float hPa { get; set; }
    public int Light { get; set; } = 0;
    public string Geo { get; set; }
    public int Schema { get; set; } = 1;
    public int Id { get; set; }
  }

  public class TheThingsNetworkEntity
  {
    public string app_id { get; set; }
    public string dev_id { get; set; }
    public int counter { get; set; }
    public string payload_raw { get; set; }
    public JObject payload_fields { get; set; }
  }
}

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
using System.Threading.Tasks;

namespace Glovebox.Enviromon
{
    public static class Gateway 
  {
    static string eventHubSenderCS = System.Environment.GetEnvironmentVariable("EventHubSenderCS");
    static string telemetryEventHub = System.Environment.GetEnvironmentVariable("TelemetryEventHub");

    [FunctionName("ThingsNetworkGateway")]
    public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
    {
      IQueueClient queueClient = new QueueClient(eventHubSenderCS, telemetryEventHub);

      string requestBody = new StreamReader(req.Body).ReadToEnd();
      dynamic data = JsonConvert.DeserializeObject(requestBody);

      TheThingsNetworkEntity ttn = JsonConvert.DeserializeObject<TheThingsNetworkEntity>(data.ToString(), new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
      Dictionary<string, float> payload = ttn.payload_fields.ToObject<Dictionary<string, float>>();

      TtnTelemetry telemetry = new TtnTelemetry()
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
}

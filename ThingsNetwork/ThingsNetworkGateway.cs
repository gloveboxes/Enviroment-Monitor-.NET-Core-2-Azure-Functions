using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Glovebox.Enviromon
{
    public static class Gateway 
  {
    static string eventHubSenderCS = System.Environment.GetEnvironmentVariable("emEventHubSenderCS");
    static string telemetryEventHub = System.Environment.GetEnvironmentVariable("emEventHubTelemetry");

    [FunctionName("ThingsNetworkGateway")]
    public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
    {
      IQueueClient queueClient = new QueueClient(eventHubSenderCS, telemetryEventHub);

      string requestBody = new StreamReader(req.Body).ReadToEnd();
      dynamic data = JsonConvert.DeserializeObject(requestBody);

      TheThingsNetworkEntity ttn = JsonConvert.DeserializeObject<TheThingsNetworkEntity>(data.ToString(), new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
      
      if (ttn.payload_fields is null) {
        return (ActionResult)new BadRequestObjectResult($"Invalid payload for {ttn.dev_id}");
      }
      
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

      await queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(json)));

      log.LogInformation(json);

      return (ActionResult)new OkObjectResult("Success");
    }
  }
}

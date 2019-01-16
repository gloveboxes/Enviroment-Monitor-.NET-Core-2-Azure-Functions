using Newtonsoft.Json.Linq;

public class TheThingsNetworkEntity
  {
    public string app_id { get; set; }
    public string dev_id { get; set; }
    public int counter { get; set; }
    public string payload_raw { get; set; }
    public JObject payload_fields { get; set; }
  }
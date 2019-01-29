using Newtonsoft.Json;

public class TtnTelemetry
  {
    public string DeviceId { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? Battery {get;set;}
    public float Celsius { get; set; }
    public float Humidity { get; set; }
    public float hPa { get; set; }
    public int Light { get; set; } = 0;
    public string Geo { get; set; }
    public int Schema { get; set; } = 1;
    public int Id { get; set; }
  }
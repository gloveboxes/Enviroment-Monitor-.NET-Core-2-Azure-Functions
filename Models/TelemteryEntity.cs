using Microsoft.WindowsAzure.Storage.Table;

public class TelemetryItem : TableEntity
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
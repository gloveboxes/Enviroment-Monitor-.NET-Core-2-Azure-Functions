using Microsoft.WindowsAzure.Storage.Table;

public class Calibration : TableEntity
  {
    public double TemperatureSlope { get; set; }
    public double TemperatureYIntercept { get; set; }
    public double HumiditySlope { get; set; }
    public double HumidityYIntercept { get; set; }
    public double PressureSlope { get; set; }
    public double PressureYIntercept { get; set; }
  }
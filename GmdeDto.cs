using System.Text.Json.Serialization;

public class GmdeDto
{
    [JsonPropertyName("Data")]
    public Data Data { get; set; }
}

public class Data
{
    [JsonPropertyName("ValueAxis")]
    public Axis[] Axis { get; set; }
    
    [JsonPropertyName("Graphs")]
    public Graph[] Graphs { get; set; }
    
    [JsonPropertyName("Datas2")]
    public  DataPointCollection[] DataPoints { get; set; }
    public string SuccessFul { get; set; }
}

public class Axis
{
    [JsonPropertyName("position")]
    public string Position { get; set; }
    [JsonPropertyName("offset")]
    public int Offset { get; set; }
    [JsonPropertyName("unit")]
    public string Unit { get; set; }
}

public class Graph
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("valueField")]
    public string ValueField { get; set; }
    [JsonPropertyName("unit")]
    public string Unit { get; set; }
    [JsonPropertyName("vaIndex")]
    public int  ValueIndex { get; set; }
}

public class DataPointCollection
{
    [JsonPropertyName("dt")]
    public string Date { get; set; }
    [JsonPropertyName("a1")]
    public float PvGeneration { get; set; }
    [JsonPropertyName("a2")]
    public float LoadConsumption { get; set; }
    [JsonPropertyName("a3")]
    public float GridFeedIn { get; set; }
    [JsonPropertyName("a4")]
    public float GridSupply { get; set; }
    [JsonPropertyName("a5")]
    public float ConsumptionFromPv { get; set; }
    [JsonPropertyName("a6")]
    public float BatteryStorage { get; set; }
}
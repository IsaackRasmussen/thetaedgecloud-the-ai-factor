using System.Text.Json.Serialization;

namespace thetaedgecloud_the_ai_factor.Models.ThetaEdgeCloud;

public abstract class BaseResult
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
}


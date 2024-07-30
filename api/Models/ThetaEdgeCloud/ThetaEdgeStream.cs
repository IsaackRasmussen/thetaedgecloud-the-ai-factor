using System.Text.Json.Serialization;

namespace thetaedgecloud_the_ai_factor.Models.ThetaEdgeCloud;

public class ThetaEdgeStream
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("service_account_id")]
    public string ServiceAccountId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonPropertyName("update_time")]
    public DateTimeOffset UpdateTime { get; set; }
    [JsonPropertyName("playback_uri")]
    public string PlaybackUri { get; set; }
    [JsonPropertyName("player_uri")]
    public string PlayerUri { get; set; }
}
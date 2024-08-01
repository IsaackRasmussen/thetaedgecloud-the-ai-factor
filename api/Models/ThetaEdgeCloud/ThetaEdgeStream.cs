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
    [JsonPropertyName("playback_uri")]
    public string PlaybackUri { get; set; }
    [JsonPropertyName("player_uri")]
    public string PlayerUri { get; set; }
    [JsonPropertyName("file_name")]
    public string FileName { get; set; }
    [JsonPropertyName("create_time")]
    public string CreateTime { get; set; }
    [JsonPropertyName("update_time")]
    public string UpdateTime { get; set; }
}
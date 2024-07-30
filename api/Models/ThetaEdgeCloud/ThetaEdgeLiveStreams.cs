using System.Text.Json.Serialization;

namespace thetaedgecloud_the_ai_factor.Models.ThetaEdgeCloud;

public class ThetaEdgeLiveStreams: BaseResult
{
    [JsonPropertyName("body")]
    public ThetaEdgeLiveStreamsBody Body { get; set; }
}

public class ThetaEdgeLiveStreamsBody
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }
    
    [JsonPropertyName("streams")]
    public IList<ThetaEdgeStream> Streams{ get; set; }
}
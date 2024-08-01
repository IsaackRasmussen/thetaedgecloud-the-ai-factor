using System.Text.Json.Serialization;

namespace thetaedgecloud_the_ai_factor.Models.ThetaEdgeCloud;

public class ThetaEdgeStreams: BaseResult
{
    [JsonPropertyName("body")]
    public ThetaEdgeStreamsBody Body { get; set; }
}

public class ThetaEdgeStreamsBody
{
    [JsonPropertyName("videos")]
    public IList<ThetaEdgeStream> Videos{ get; set; }
}
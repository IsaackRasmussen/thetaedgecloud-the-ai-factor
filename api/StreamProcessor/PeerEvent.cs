using System.Text.Json.Serialization;

namespace thetaedgecloud_the_ai_factor.StreamProcessor;

public class PeerEvent
{
    public PeerEvent()
    {
        Timestamp = DateTimeOffset.Now;
        Id = Guid.NewGuid().ToString("N");
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PeerEventType EventType { get; set; }
    public DateTimeOffset Timestamp { get; }
    public string EventObject { get; set; }
    public string EventSource { get; set; }
    public string Id { get; set; }
    public int Index { get; set; }
}

public enum PeerEventType
{
    ObjectSeen,
    AIPrompt,
    AIResponse
}
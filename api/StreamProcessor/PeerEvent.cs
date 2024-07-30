using System.Text.Json.Serialization;

namespace thetaedgecloud_the_ai_factor.StreamProcessor;

public class PeerEvent
{
    public PeerEvent()
    {
        Timestamp = DateTimeOffset.Now;
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PeerEventType EventType { get; set; }
    public DateTimeOffset Timestamp { get; }
    public string EventObject { get; set; }
    public string EventSource { get; set; }
}

public enum PeerEventType
{
    ObjectSeen,
    AIPrompt,
    AIResponse
}
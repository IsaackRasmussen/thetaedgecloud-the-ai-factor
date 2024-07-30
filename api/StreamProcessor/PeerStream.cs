using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using thetaedgecloud_the_ai_factor.MachineLearning;

namespace thetaedgecloud_the_ai_factor.StreamProcessor;

public class PeerStream
{
    private readonly ImageClassification _imageClassification = new ImageClassification();
    private DateTimeOffset _lastTimeImageClassification = DateTimeOffset.Now;
    private DateTimeOffset _lastTimeFpsCounter = DateTimeOffset.Now;
    private int _frameCounter = 0;
    private int _lastFrameCounter = 0;
    private int _framesPerSecond = 0;
    private readonly List<PeerEvent> _peerEvents = new List<PeerEvent>();
    private readonly AiAssistant _aiAssistant;
    private readonly TimeSpan _classificationDelay = TimeSpan.FromSeconds(2.5);

    public PeerStream(IConfiguration configuration)
    {
        var promptApiBaseUrl = configuration["Cloudflare:AI:ApiUrl"];

        _aiAssistant = new AiAssistant(promptApiBaseUrl, configuration["Cloudflare:AI:TokenSecret"],
            "You are an impatient judge in a music competition. You comment the performance of the artist playing with one short sentence. ");
    }

    public async Task ProcessFrame(FileStream fileStream)
    {
        _frameCounter++;
        if (DateTimeOffset.Now.Subtract(_lastTimeFpsCounter).TotalSeconds > 3)
        {
            _lastTimeFpsCounter = DateTimeOffset.Now;
            _framesPerSecond = _frameCounter - _lastFrameCounter;
            _lastFrameCounter = _frameCounter;
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"FPS: {_framesPerSecond}  ");
        }

        if (DateTimeOffset.Now.Subtract(_lastTimeImageClassification) > _classificationDelay)
        {
            _lastTimeImageClassification = DateTimeOffset.Now;

            var img = await Image.LoadAsync<Rgb24>(fileStream);
            var imgPrediction = _imageClassification.Classify(img);

            if (imgPrediction != null)
            {
                var eventObject = imgPrediction.Label;
                if (imgPrediction.Confidence > 0.6 &&
                    (_peerEvents.FindLast(e => e.EventType == PeerEventType.ObjectSeen)?.EventObject ?? "") !=
                    eventObject)
                {
                    AddEvent(PeerEventType.ObjectSeen, eventObject);

                    Task.Run(async () =>
                    {
                        string aiPrompt = EventObjectSeenCount(eventObject) == 0
                            ? $"Now a {eventObject} is seen"
                            : $"Now the {eventObject} is back";
                        AddEvent(PeerEventType.AIPrompt, aiPrompt);
                        var promptResponse = await _aiAssistant.Prompt(aiPrompt);

                        AddEvent(PeerEventType.AIResponse, promptResponse.Result.Response);
                        Console.WriteLine($"Judge: {promptResponse.Result.Response}                                    ");
                    });
                }

                Console.SetCursorPosition(20, 0);
                Console.WriteLine(
                    $"Detected: {imgPrediction?.Label ?? "None"} {(imgPrediction?.Confidence ?? 0) * 100}%            ");
            }
        }
    }

    public IEnumerable<PeerEvent> GetEvents() => _peerEvents;

    private void AddEvent(PeerEventType eventType, string eventObject)
    {
        lock (_peerEvents)
        {
            _peerEvents.Add(new PeerEvent()
            {
                EventObject = eventObject,
                EventType = eventType
            });
        }
    }

    private int EventObjectSeenCount(string eventObject) => _peerEvents.Count(e => e.EventObject.Equals(eventObject));
}
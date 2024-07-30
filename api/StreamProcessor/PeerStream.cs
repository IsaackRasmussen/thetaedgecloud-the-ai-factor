using Microsoft.CognitiveServices.Speech;
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
    private readonly List<AiAssistant> _aiAssistants = new List<AiAssistant>();
    private readonly TimeSpan _classificationDelay = TimeSpan.FromSeconds(2.5);

    public PeerStream(IConfiguration configuration)
    {
        var promptApiBaseUrl = configuration["Cloudflare:AI:ApiUrl"];

        _aiAssistants.Add(new AiAssistant(promptApiBaseUrl, configuration["Cloudflare:AI:TokenSecret"],
            "You are an impatient judge in a music competition. You comment the performance of the artist playing with one short sentence.",
            "Julie"));
        _aiAssistants.Add(new AiAssistant(promptApiBaseUrl, configuration["Cloudflare:AI:TokenSecret"],
            "You are a very helpful judge in a music competition. You give suggestions to the artist playing for how to improve with one short sentence.",
            "Andrew"));
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
                        foreach (var aiAssistant in _aiAssistants)
                        {
                            var promptResponse = await aiAssistant.Prompt(aiPrompt);

                            var pEvent = AddEvent(PeerEventType.AIResponse, promptResponse.Result.Response,
                                aiAssistant.Name);
                            await TextToSpeech(pEvent.Index, pEvent.Id,aiAssistant.Name,  promptResponse.Result.Response);

                            Console.WriteLine($"{aiAssistant.Name} Judge: {promptResponse.Result.Response} ");
                        }
                    });
                }

                Console.WriteLine(
                    $"Detected: {imgPrediction?.Label ?? "None"} {(imgPrediction?.Confidence ?? 0) * 100}%            ");
            }
        }
    }

    public IEnumerable<PeerEvent> GetEvents() => _peerEvents;

    private PeerEvent AddEvent(PeerEventType eventType, string eventObject, string source = null)
    {
        lock (_peerEvents)
        {
            var eventObj = new PeerEvent()
            {
                EventObject = eventObject,
                EventType = eventType,
                EventSource = source,
                Index = _peerEvents.Count
            };

            _peerEvents.Add(eventObj);

            return eventObj;
        }
    }

    private int EventObjectSeenCount(string eventObject) => _peerEvents.Count(e => e.EventObject.Equals(eventObject));

    private async Task TextToSpeech(int index, string id, string judgeName,string text)
    {
        try
        {
            var voiceSynthUrl = judgeName == "Andrew" ? "http://localhost:5000" : "http://localhost:5001";
            
            var config = SpeechConfig.FromHost(
                new Uri(voiceSynthUrl));
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3);

            var synthesizer = new SpeechSynthesizer(config);

            var result = await synthesizer.SpeakTextAsync(text);

            using (var fStream = new FileStream($"./speeches/{index}_{id}.mp3", FileMode.Create))
            {
                await fStream.WriteAsync(result.AudioData, 0, result.AudioData.Length);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
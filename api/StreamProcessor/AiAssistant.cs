using System.Text.Json.Serialization;
using TinyJson;

namespace thetaedgecloud_the_ai_factor.StreamProcessor;

public class AiAssistant
{
    private readonly List<PromptMessage> _promptMessages;
    private readonly string _baseApiUrl;
    private readonly string _apiTokenSecret;

    public AiAssistant(string baseApiUrl, string apiTokenSecret, string systemContent)
    {
        _baseApiUrl = baseApiUrl;
        _apiTokenSecret = apiTokenSecret;
        _promptMessages = new List<PromptMessage>();
        _promptMessages.Add(new PromptMessage()
        {
            Role = "system",
            Content = systemContent
        });
    }

    public async Task<PromptResponse> Prompt(string promptMessage)
    {
        try
        {
            _promptMessages.Add(new PromptMessage()
            {
                Role = "user",
                Content = promptMessage
            });

            using (var hClient = new HttpClient())
            {
                hClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiTokenSecret}");

                var hResponse = await hClient.PostAsJsonAsync(_baseApiUrl, new
                {
                    messages = _promptMessages.ToArray()
                });

                var result = await hResponse.Content.ReadFromJsonAsync<PromptResponse>();

                return result ?? new PromptResponse() { Success = false };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return new PromptResponse()
        {
            Success = false
        };
    }

    public class PromptMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; }
        [JsonPropertyName("content")] public string Content { get; set; }

        private DateTimeOffset Timestamp { get; set; }

        public PromptMessage()
        {
            Timestamp = DateTimeOffset.Now;
        }
    }

    public class PromptResponse
    {
        [JsonPropertyName("result")] public PromptResponseResult Result { get; set; }

        [JsonPropertyName("success")] public bool Success { get; set; }

        [JsonPropertyName("errors")] public object[] Errors { get; set; }

        [JsonPropertyName("messages")] public object[] Messages { get; set; }

        public class PromptResponseResult
        {
            [JsonPropertyName("response")] public string Response { get; set; }
        }
    }
}
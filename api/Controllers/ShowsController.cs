using System.Linq;
using LiveStreamingServerNet.Rtmp.Contracts;
using Microsoft.AspNetCore.Mvc;
using thetaedgecloud_the_ai_factor.Models;
using thetaedgecloud_the_ai_factor.Models.ThetaEdgeCloud;

namespace thetaedgecloud_the_ai_factor.Controllers;

public class ShowsController: Controller
{
    private readonly IConfiguration _configuration;
    private readonly string _baseApiUrl;
    private readonly string _baseApiSecret;
    private readonly string _baseApiKey;

    public ShowsController(IConfiguration configuration)
    {
        _configuration = configuration;
        _baseApiUrl = _configuration["ThetaEdgeCloud:VideoServices:ApiUrl"];
        _baseApiKey = _configuration["ThetaEdgeCloud:VideoServices:ApiKey"];
        _baseApiSecret = _configuration["ThetaEdgeCloud:VideoServices:ApiSecret"];
    }
    
    [HttpGet("[Controller]/Live")]
    public async Task<ActionResult> GetLiveShows()
    {
        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-tva-sa-id",_baseApiKey);
                client.DefaultRequestHeaders.Add("x-tva-sa-secret",_baseApiSecret);
                var liveStreams = await client.GetFromJsonAsync<ThetaEdgeLiveStreams>(
                    $"{_baseApiUrl}/service_account/{_baseApiKey}/streams");

                return Ok(liveStreams.Body.Streams.Select((stream) => new LiveShow
                {
                    Name = stream.Name,
                     Id = stream.Id,
                     PlayerUri = stream.PlayerUri,
                     PlaybackUri = stream.PlaybackUri,
                     Status = stream.Status
                }));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return Ok(Enumerable.Empty<LiveShow>());
    }
}
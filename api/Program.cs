using System.Diagnostics;
using System.Net;
using System.Text.Json;
using LiveStreamingServerNet;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Networking.Helpers;
using Microsoft.CognitiveServices.Speech;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
using thetaedgecloud_the_ai_factor.StreamProcessor;
using WebSocketSharp.Server;

var builder = WebApplication.CreateBuilder(args);

/*
var sw1 = Stopwatch.StartNew();
var config = SpeechConfig.FromHost(
    new Uri("http://localhost:5000"));
config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3);

var synthesizer = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(config);
var voices = await synthesizer.GetVoicesAsync();
Console.WriteLine("Voices: "+ String.Join(',',voices.Voices.Select(v=>v.Name)));
var result = await synthesizer.SpeakTextAsync("Hi, my name is Joe");
Console.WriteLine("Made speech in: "+sw1.ElapsedMilliseconds);
using (var fStream = new FileStream("speech.mp3", FileMode.Create))
{
    await fStream.WriteAsync(result.AudioData, 0, result.AudioData.Length);
}


var peer = new PeerStream(builder.Configuration);
var fileBuffer = new byte[1024 * 1024];
foreach (var fileName in Directory.GetFiles(@"C:\Users\Isaack\Downloads\videos\video-images", "*.jpg"))
{
    var sw = Stopwatch.StartNew();
    using(var imgFile = new FileStream(fileName,FileMode.Open))
    {
        //var bytesRead = imgFile.ReadAsync(fileBuffer, 0, fileBuffer.Length);
        await peer.ProcessFrame(imgFile);
    }
    sw.Stop();

    if (sw.ElapsedMilliseconds < 32)
    {
        await Task.Delay((int)(32 - sw.ElapsedMilliseconds));
    }
}

    
Console.WriteLine("Finished");
Console.WriteLine("Events: "+ JsonSerializer.Serialize(peer.GetEvents()));

return;*/
/*var imgClass = new ImageClassification();
var sw = Stopwatch.StartNew();
var result1 = imgClass.Classify(@"C:\Users\Isaack\Downloads\videos\video-images\frame073.png");
Console.WriteLine($"Spent: {sw.ElapsedMilliseconds}ms {result1.Label} {result1.Confidence}");

sw.Restart();
var result2 = imgClass.Classify("C:\\Users\\Isaack\\Downloads\\videos\\guitar.png");
Console.WriteLine($"Spent: {sw.ElapsedMilliseconds}ms {result2.Label} {result2.Confidence}");

//"-y -re -f rawvideo -vcodec rawvideo -pix_fmt bgr24 -s 224x224 -r 30 -i - -vn -i C:\\Users\\Isaack\\Downloads\\videos\\vecteezy_drummer-is-playing-drums_2496615.mov -c:v libx264 -pix_fmt yuv420p -preset ultrafast -bufsize 64M -f flv rtmp://34.173.223.115:1935/live");

var psStartInfo = new ProcessStartInfo("ffmpeg.exe",
"-y -re -f rawvideo -vcodec rawvideo -pix_fmt rgb24 -s 224x224 -r 30 -i - -vn -re -i C:\\Users\\Isaack\\Downloads\\videos\\dExPayVideo.mp4 -c:v libx264 -pix_fmt yuv420p -preset ultrafast -bufsize 64M -f flv rtmp://127.0.0.1:1935/live/test");
var proc = new Process();
psStartInfo.RedirectStandardInput = true;
//psStartInfo.CreateNoWindow = true;
//psStartInfo.UseShellExecute = false;
proc.StartInfo = psStartInfo;

proc.ErrorDataReceived += (sender, eventArgs) => { Console.WriteLine("ProcessError: " + eventArgs.Data); };
proc.Exited += (sender, eventArgs) => { Console.WriteLine("ProcessExit"); };
proc.OutputDataReceived += (sender, eventArgs) => { Console.WriteLine("ProcessOutput: " + eventArgs.Data); };
proc.Start();

var fBuf = new byte[512 * 1024];
while (true)
{
foreach (var fileName in Directory.GetFiles(@"C:\Users\Isaack\Downloads\videos\video-images", "*.png"))
{
byte[] GetPixBytes(string fileName)
{
var pixBuffer = new Span<byte>(fBuf);

var img = SixLabors.ImageSharp.Image.Load<Rgb24>(fileName);
img.CopyPixelDataTo(pixBuffer);
return pixBuffer.ToArray();
}

var bytesBuffer = GetPixBytes(fileName);
proc.StandardInput.BaseStream.Write(bytesBuffer, 0, bytesBuffer.Length);
Thread.Sleep(50);
}
}*/

var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options.AddFlv())
    .ConfigureLogging(options => options.AddConsole())
    .Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapSwagger();
app.MapControllers();

app.UseHttpsRedirection();

app.UseWebSockets();
app.UseWebSocketFlv(liveStreamingServer);

Console.WriteLine("Starting web socket server...");
var webSocketServer = new WebSocketServer(IPAddress.Any, 8081);
webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>("/",
    (peer) => peer.CreatePeerConnection = () => CreatePeerConnection());
webSocketServer.Start();


await app.RunAsync();

static Task<RTCPeerConnection> CreatePeerConnection()
{
    var pc = new RTCPeerConnection(null);

    var testPatternSource = new VideoTestPatternSource(new VpxVideoEncoder());

    MediaStreamTrack videoTrack =
        new MediaStreamTrack(testPatternSource.GetVideoSourceFormats(), MediaStreamStatusEnum.SendOnly);
    pc.addTrack(videoTrack);
    pc.OnVideoFrameReceived += PcOnOnVideoFrameReceived;

    void PcOnOnVideoFrameReceived(IPEndPoint arg1, uint arg2, byte[] arg3, VideoFormat arg4)
    {
    }

    testPatternSource.OnVideoSourceEncodedSample += pc.SendVideo;
    pc.OnVideoFormatsNegotiated += (formats) => testPatternSource.SetVideoSourceFormat(formats.First());

    pc.onconnectionstatechange += async (state) =>
    {
        Console.WriteLine($"Peer connection state change to {state}.");

        switch (state)
        {
            case RTCPeerConnectionState.connected:
                await testPatternSource.StartVideo();
                break;
            case RTCPeerConnectionState.failed:
                pc.Close("ice disconnection");
                break;
            case RTCPeerConnectionState.closed:
                await testPatternSource.CloseVideo();
                testPatternSource.Dispose();
                break;
        }
    };

    return Task.FromResult(pc);
}
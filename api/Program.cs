using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using LiveStreamingServerNet;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Networking.Helpers;
using Microsoft.CognitiveServices.Speech;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
using SIPSorceryMedia.FFmpeg;
using thetaedgecloud_the_ai_factor.StreamProcessor;
using WebSocketSharp.Server;

var builder = WebApplication.CreateBuilder(args);
/*
var peer = new PeerStream(builder.Configuration);
var fileBuffer = new byte[1024 * 1024];
foreach (var fileName in Directory.GetFiles(@"C:\Users\Isaack\Downloads\videos\video-images", "*.jpg"))
{
    var sw = Stopwatch.StartNew();
    using(var imgFile = new FileStream(fileName,FileMode.Open))
    {
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
return; */
/*
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
var corsAllowedOrigins = "MyOriginsAllowed";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsAllowedOrigins,
        builder => { builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin(); });
});

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

app.UseCors(corsAllowedOrigins);

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

/*static Task<RTCPeerConnection> CreatePeerConnection()
{
    var pc = new RTCPeerConnection(null);

    var testPatternSource = new VideoTestPatternSource(new VpxVideoEncoder());

    MediaStreamTrack videoTrack =
        new MediaStreamTrack(testPatternSource.GetVideoSourceFormats(), MediaStreamStatusEnum.SendRecv);
    pc.addTrack(videoTrack);
    pc.OnVideoFrameReceived += PcOnOnVideoFrameReceived;

    pc.OnRtpPacketReceived += (IPEndPoint rep, SDPMediaTypesEnum media, RTPPacket rtpPkt) =>
    {
        //logger.LogDebug($"RTP {media} pkt received, SSRC {rtpPkt.Header.SyncSource}.");
        if (media == SDPMediaTypesEnum.audio)
        {
            //windowsAudioEP.GotAudioRtp(rep, rtpPkt.Header.SyncSource, rtpPkt.Header.SequenceNumber, rtpPkt.Header.Timestamp, rtpPkt.Header.PayloadType, rtpPkt.Header.MarkerBit == 1, rtpPkt.Payload);
        }
    };

    void PcOnOnVideoFrameReceived(IPEndPoint arg1, uint arg2, byte[] arg3, VideoFormat arg4)
    {
        Console.WriteLine("REceived frame: " + arg4.ToString());
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
}*/

static Task<RTCPeerConnection> CreatePeerConnection()
{
    try
    {
        //var videoEP = new SIPSorceryMedia.Windows.WindowsVideoEndPoint(new VpxVideoEncoder());
        //videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.VP8);
        //var videoEP = new SIPSorceryMedia.Windows.WindowsVideoEndPoint(new FFmpegVideoEncoder());
        //videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.H264);

        /*string ffmpegLibFullPath = @"ffmpeg.exe";
    SIPSorceryMedia.FFmpeg.FFmpegInit.Initialise(SIPSorceryMedia.FFmpeg.FfmpegLogLevelEnum.AV_LOG_VERBOSE,
        ffmpegLibFullPath);*/
        var videoEP = new FFmpegVideoEndPoint();
        videoEP.RestrictFormats(format => format.Codec == VideoCodecsEnum.H264);


        videoEP.OnVideoSinkDecodedSampleFaster += (RawImage rawImage) =>
        {
            /* _form.BeginInvoke(new Action(() =>
        {
            if (rawImage.PixelFormat == SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum.Rgb)
            {
                if (_picBox.Width != rawImage.Width || _picBox.Height != rawImage.Height)
                {
                    logger.LogDebug(
                        $"Adjusting video display from {_picBox.Width}x{_picBox.Height} to {rawImage.Width}x{rawImage.Height}.");
                    _picBox.Width = rawImage.Width;
                    _picBox.Height = rawImage.Height;
                }

                Bitmap bmpImage = new Bitmap(rawImage.Width, rawImage.Height, rawImage.Stride,
                    PixelFormat.Format24bppRgb, rawImage.Sample);
                _picBox.Image = bmpImage;
            }
        }));*/
        };

        videoEP.OnVideoSinkDecodedSample +=
            (byte[] bmp, uint width, uint height, int stride, VideoPixelFormatsEnum pixelFormat) =>
            {
                /* _form.BeginInvoke(new Action(() =>
            {
                if (pixelFormat == SIPSorceryMedia.Abstractions.VideoPixelFormatsEnum.Rgb)
                {
                    if (_picBox.Width != (int)width || _picBox.Height != (int)height)
                    {
                        logger.LogDebug(
                            $"Adjusting video display from {_picBox.Width}x{_picBox.Height} to {width}x{height}.");
                        _picBox.Width = (int)width;
                        _picBox.Height = (int)height;
                    }

                    unsafe
                    {
                        fixed (byte* s = bmp)
                        {
                            Bitmap bmpImage = new Bitmap((int)width, (int)height, (int)(bmp.Length / height),
                                PixelFormat.Format24bppRgb, (IntPtr)s);
                            _picBox.Image = bmpImage;
                        }
                    }
                }
            }));*/
            };

        RTCConfiguration config = new RTCConfiguration
        {
            //iceServers = new List<RTCIceServer> { new RTCIceServer { urls = STUN_URL } }
            X_UseRtpFeedbackProfile = true
        };
        var pc = new RTCPeerConnection(config);

        // Add local receive only tracks. This ensures that the SDP answer includes only the codecs we support.
        if (true)
        {
            MediaStreamTrack audioTrack = new MediaStreamTrack(SDPMediaTypesEnum.audio, false,
                new List<SDPAudioVideoMediaFormat> { new SDPAudioVideoMediaFormat(SDPWellKnownMediaFormatsEnum.PCMU) },
                MediaStreamStatusEnum.RecvOnly);
            pc.addTrack(audioTrack);
        }

        MediaStreamTrack videoTrack = new MediaStreamTrack(videoEP.GetVideoSinkFormats(), MediaStreamStatusEnum.RecvOnly);
        //MediaStreamTrack videoTrack = new MediaStreamTrack(new VideoFormat(96, "VP8", 90000, "x-google-max-bitrate=5000000"), MediaStreamStatusEnum.RecvOnly);
        pc.addTrack(videoTrack);

        pc.OnVideoFrameReceived += videoEP.GotVideoFrame;
        pc.OnVideoFormatsNegotiated += (formats) => videoEP.SetVideoSinkFormat(formats.First());

        pc.onconnectionstatechange += async (state) =>
        {
            Console.WriteLine($"Peer connection state change to {state}.");

            if (state == RTCPeerConnectionState.failed)
            {
                pc.Close("ice disconnection");
            }
            else if (state == RTCPeerConnectionState.closed)
            {
                await videoEP.CloseVideo();
            }
        };

        // Diagnostics.
        //pc.OnReceiveReport += (re, media, rr) => logger.LogDebug($"RTCP Receive for {media} from {re}\n{rr.GetDebugSummary()}");
        pc.OnSendReport += (media, sr) => Console.WriteLine($"RTCP Send for {media}\n{sr.GetDebugSummary()}");
        //pc.GetRtpChannel().OnStunMessageReceived += (msg, ep, isRelay) => logger.LogDebug($"RECV STUN {msg.Header.MessageType} (txid: {msg.Header.TransactionId.HexStr()}) from {ep}.");
        //pc.GetRtpChannel().OnStunMessageSent += (msg, ep, isRelay) => logger.LogDebug($"SEND STUN {msg.Header.MessageType} (txid: {msg.Header.TransactionId.HexStr()}) to {ep}.");
        pc.oniceconnectionstatechange += (state) => Console.WriteLine($"ICE connection state change to {state}.");

        return Task.FromResult(pc);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

static X509Certificate2 LoadCertificate(string path)
{
    if (!File.Exists(path))
    {
        Console.WriteLine($"No certificate file could be found at {path}.");
        return null;
    }
    else
    {
        X509Certificate2 cert = new X509Certificate2(path, "", X509KeyStorageFlags.Exportable);
        if (cert == null)
        {
            Console.WriteLine($"Failed to load X509 certificate from file {path}.");
        }
        else
        {
            Console.WriteLine(
                $"Certificate file successfully loaded {cert.Subject}, thumbprint {cert.Thumbprint}, has private key {cert.HasPrivateKey}.");
        }

        return cert;
    }
}
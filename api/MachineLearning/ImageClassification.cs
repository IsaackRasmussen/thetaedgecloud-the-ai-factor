using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace thetaedgecloud_the_ai_factor.MachineLearning;

public class ImageClassification
{
    private readonly string modelFilePath = @"./onnx_models/resnet152-v2-7.onnx";
    private readonly InferenceSession _inferenceSession;

    public ImageClassification()
    {
        try
        {
            _inferenceSession = new InferenceSession(modelFilePath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public Prediction? Classify(Image<Rgb24> frameImage)
    {
        try
        {
            var targetImage = frameImage.Clone();
            
            if (targetImage.Width != targetImage.Height)
            {
                int cropWidth = targetImage.Width < targetImage.Height ? targetImage.Width : targetImage.Height;
                int cropHeight = targetImage.Width < targetImage.Height ? targetImage.Width : targetImage.Height;
                int cropX = Math.Abs(targetImage.Width - cropWidth) / 2;
                int cropY = Math.Abs(targetImage.Height - cropHeight) / 2;

                targetImage.Mutate(img =>
                    img.Crop(Rectangle.FromLTRB(cropX, cropY
                            , cropWidth + cropX, cropHeight + cropY))
                        .Resize(224, 224));
            }

// We use DenseTensor for multi-dimensional access to populate the image data
            var mean = new[] { 0.485f, 0.456f, 0.406f };
            var stddev = new[] { 0.229f, 0.224f, 0.225f };
            DenseTensor<float> processedImage = new(new[] { 1, 3, 224, 224 });
            targetImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        processedImage[0, 0, y, x] = ((pixelSpan[x].R / 255f) - mean[0]) / stddev[0];
                        processedImage[0, 1, y, x] = ((pixelSpan[x].G / 255f) - mean[1]) / stddev[1];
                        processedImage[0, 2, y, x] = ((pixelSpan[x].B / 255f) - mean[2]) / stddev[2];
                    }
                }
            });

// Pin tensor buffer and create a OrtValue with native tensor that makes use of
// DenseTensor buffer directly. This avoids extra data copy within OnnxRuntime.
// It will be unpinned on ortValue disposal
            using var inputOrtValue = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance,
                processedImage.Buffer, new long[] { 1, 3, 224, 224 });

            var inputs = new Dictionary<string, OrtValue>
            {
                { "data", inputOrtValue }
            };

            using var runOptions = new RunOptions();
            using IDisposableReadOnlyCollection<OrtValue> results =
                _inferenceSession.Run(runOptions, inputs, _inferenceSession.OutputNames);

            // We copy results to array only to apply algorithms, otherwise data can be accessed directly
// from the native buffer via ReadOnlySpan<T> or Span<T>
            var output = results[0].GetTensorDataAsSpan<float>().ToArray();
            float sum = output.Sum(x => (float)Math.Exp(x));
            IEnumerable<float> softmax = output.Select(x => (float)Math.Exp(x) / sum);


            IEnumerable<Prediction> top10 = softmax
                .Select((x, i) => new Prediction { Label = LabelMap.Labels[i], Confidence = x })
                .OrderByDescending(x => x.Confidence)
                .Take(10);

            return top10?.First();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }

    public class Prediction
    {
        public string Label { get; set; }
        public float Confidence { get; set; }
    }
}
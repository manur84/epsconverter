using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace EPSConverter.Services
{
    public class ImageTransformService
    {
        public async Task<BitmapSource> ScaleImageAsync(BitmapSource source, double scaleX, double scaleY)
        {
            return await Task.Run(() =>
            {
                int newWidth = (int)(source.PixelWidth * scaleX);
                int newHeight = (int)(source.PixelHeight * scaleY);

                var scaled = new TransformedBitmap(source, new ScaleTransform(scaleX, scaleY));

                var result = new WriteableBitmap(scaled);
                result.Freeze();
                return (BitmapSource)result;
            });
        }

        public async Task<BitmapSource> RotateImageAsync(BitmapSource source, double angle)
        {
            return await Task.Run(() =>
            {
                var rotated = new TransformedBitmap(source, new RotateTransform(angle));

                var result = new WriteableBitmap(rotated);
                result.Freeze();
                return (BitmapSource)result;
            });
        }

        public async Task<BitmapSource> RotateImageSkiaAsync(BitmapSource source, double angle)
        {
            return await Task.Run(() =>
            {
                // Convert BitmapSource to SkiaSharp
                using var stream = new System.IO.MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                stream.Position = 0;

                using var skBitmap = SKBitmap.Decode(stream);

                // Calculate new dimensions after rotation
                double radians = angle * Math.PI / 180.0;
                double cos = Math.Abs(Math.Cos(radians));
                double sin = Math.Abs(Math.Sin(radians));
                int newWidth = (int)(skBitmap.Width * cos + skBitmap.Height * sin);
                int newHeight = (int)(skBitmap.Width * sin + skBitmap.Height * cos);

                // Create rotated image
                using var surface = SKSurface.Create(new SKImageInfo(newWidth, newHeight));
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                // Translate to center, rotate, translate back
                canvas.Translate(newWidth / 2f, newHeight / 2f);
                canvas.RotateDegrees((float)angle);
                canvas.Translate(-skBitmap.Width / 2f, -skBitmap.Height / 2f);

                canvas.DrawBitmap(skBitmap, 0, 0);

                // Convert back to BitmapSource
                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = data.AsStream();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            });
        }

        public async Task<BitmapSource> CropImageAsync(BitmapSource source, int x, int y, int width, int height)
        {
            return await Task.Run(() =>
            {
                var cropped = new CroppedBitmap(source, new System.Windows.Int32Rect(x, y, width, height));

                var result = new WriteableBitmap(cropped);
                result.Freeze();
                return (BitmapSource)result;
            });
        }

        public async Task<BitmapSource> FlipImageAsync(BitmapSource source, bool horizontal, bool vertical)
        {
            return await Task.Run(() =>
            {
                TransformGroup transforms = new TransformGroup();

                if (horizontal)
                    transforms.Children.Add(new ScaleTransform(-1, 1));
                if (vertical)
                    transforms.Children.Add(new ScaleTransform(1, -1));

                var flipped = new TransformedBitmap(source, transforms);

                var result = new WriteableBitmap(flipped);
                result.Freeze();
                return (BitmapSource)result;
            });
        }

        public async Task<BitmapSource> AdjustBrightnessContrastAsync(BitmapSource source, double brightness, double contrast)
        {
            return await Task.Run(() =>
            {
                using var stream = new System.IO.MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                stream.Position = 0;

                using var skBitmap = SKBitmap.Decode(stream);
                using var surface = SKSurface.Create(new SKImageInfo(skBitmap.Width, skBitmap.Height));
                var canvas = surface.Canvas;

                using var paint = new SKPaint();

                // Adjust brightness and contrast using color matrix
                float b = (float)brightness / 100f;
                float c = (float)contrast / 100f + 1f;

                var colorMatrix = new float[]
                {
                    c, 0, 0, 0, b,
                    0, c, 0, 0, b,
                    0, 0, c, 0, b,
                    0, 0, 0, 1, 0
                };

                paint.ColorFilter = SKColorFilter.CreateColorMatrix(colorMatrix);
                canvas.DrawBitmap(skBitmap, 0, 0, paint);

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = data.AsStream();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            });
        }

        public async Task<BitmapSource> ApplyFilterAsync(BitmapSource source, string filterType)
        {
            return await Task.Run(() =>
            {
                using var stream = new System.IO.MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                stream.Position = 0;

                using var skBitmap = SKBitmap.Decode(stream);
                using var surface = SKSurface.Create(new SKImageInfo(skBitmap.Width, skBitmap.Height));
                var canvas = surface.Canvas;
                using var paint = new SKPaint();

                switch (filterType.ToLower())
                {
                    case "grayscale":
                        var grayMatrix = new float[]
                        {
                            0.299f, 0.587f, 0.114f, 0, 0,
                            0.299f, 0.587f, 0.114f, 0, 0,
                            0.299f, 0.587f, 0.114f, 0, 0,
                            0, 0, 0, 1, 0
                        };
                        paint.ColorFilter = SKColorFilter.CreateColorMatrix(grayMatrix);
                        break;

                    case "sepia":
                        var sepiaMatrix = new float[]
                        {
                            0.393f, 0.769f, 0.189f, 0, 0,
                            0.349f, 0.686f, 0.168f, 0, 0,
                            0.272f, 0.534f, 0.131f, 0, 0,
                            0, 0, 0, 1, 0
                        };
                        paint.ColorFilter = SKColorFilter.CreateColorMatrix(sepiaMatrix);
                        break;

                    case "invert":
                        var invertMatrix = new float[]
                        {
                            -1, 0, 0, 0, 1,
                            0, -1, 0, 0, 1,
                            0, 0, -1, 0, 1,
                            0, 0, 0, 1, 0
                        };
                        paint.ColorFilter = SKColorFilter.CreateColorMatrix(invertMatrix);
                        break;

                    case "blur":
                        paint.ImageFilter = SKImageFilter.CreateBlur(5, 5);
                        break;
                }

                canvas.DrawBitmap(skBitmap, 0, 0, paint);

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = data.AsStream();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            });
        }
    }
}

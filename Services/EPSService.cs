using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Text;
using System.Text.RegularExpressions;
using EPSConverter.Models;
using SkiaSharp;
using System.Windows.Media;
using System.Windows;

namespace EPSConverter.Services
{
    public class EPSService
    {
        public async Task<EPSData> LoadEPSAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var content = File.ReadAllText(filePath);
                var epsData = new EPSData
                {
                    Content = content
                };

                // Extract bounding box
                var bbMatch = Regex.Match(content, @"%%BoundingBox:\s*(\d+)\s+(\d+)\s+(\d+)\s+(\d+)");
                if (bbMatch.Success)
                {
                    epsData.BoundingBox = bbMatch.Value;
                    epsData.Width = double.Parse(bbMatch.Groups[3].Value) - double.Parse(bbMatch.Groups[1].Value);
                    epsData.Height = double.Parse(bbMatch.Groups[4].Value) - double.Parse(bbMatch.Groups[2].Value);
                }
                else
                {
                    // Default dimensions if no bounding box found
                    epsData.Width = 595;  // A4 width in points
                    epsData.Height = 842; // A4 height in points
                }

                return epsData;
            });
        }

        public async Task<BitmapSource> RenderToImageAsync(EPSData epsData, int dpi)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Create a simple preview from EPS data
                    // In a production environment, you would use Ghostscript or similar
                    // For now, we'll create a placeholder with the dimensions

                    int width = (int)(epsData.Width * dpi / 72);
                    int height = (int)(epsData.Height * dpi / 72);

                    // Create bitmap using SkiaSharp
                    using var surface = SKSurface.Create(new SKImageInfo(width, height));
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.White);

                    // Draw a simple representation
                    using var paint = new SKPaint
                    {
                        Color = SKColors.LightGray,
                        IsAntialias = true,
                        Style = SKPaintStyle.Fill
                    };

                    // Draw border
                    canvas.DrawRect(10, 10, width - 20, height - 20, paint);

                    // Draw EPS text
                    paint.Color = SKColors.Black;
                    paint.TextSize = 24;
                    paint.TextAlign = SKTextAlign.Center;
                    canvas.DrawText("EPS Document", width / 2, height / 2, paint);

                    paint.TextSize = 16;
                    canvas.DrawText($"{epsData.Width} x {epsData.Height} pt", width / 2, height / 2 + 30, paint);

                    // Convert to WPF BitmapSource
                    using var image = surface.Snapshot();
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = data.AsStream();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return bitmap;
                }
                catch (Exception ex)
                {
                    // Return error placeholder
                    return CreateErrorBitmap($"Fehler beim Rendern: {ex.Message}");
                }
            });
        }

        public async Task SaveEPSAsync(string filePath, EPSData epsData)
        {
            await File.WriteAllTextAsync(filePath, epsData.Content ?? "");
        }

        private BitmapSource CreateErrorBitmap(string message)
        {
            int width = 400;
            int height = 300;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            using var paint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextSize = 16,
                TextAlign = SKTextAlign.Center
            };

            canvas.DrawText(message, width / 2, height / 2, paint);

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = data.AsStream();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}

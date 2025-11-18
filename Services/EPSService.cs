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
using Ghostscript.NET.Rasterizer;
using Ghostscript.NET;
using ImageMagick;

namespace EPSConverter.Services
{
    public class EPSService
    {
        private GhostscriptVersionInfo? _ghostscriptVersion;

        public EPSService()
        {
            // Try to find Ghostscript installation
            try
            {
                _ghostscriptVersion = GhostscriptVersionInfo.GetLastInstalledVersion(
                    GhostscriptLicense.GPL | GhostscriptLicense.AFPL,
                    GhostscriptLicense.GPL);
            }
            catch
            {
                // Ghostscript not installed, will use fallback
                _ghostscriptVersion = null;
            }
        }

        public async Task<EPSData> LoadEPSAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var content = File.ReadAllText(filePath);
                var epsData = new EPSData
                {
                    Content = content,
                    FilePath = filePath
                };

                // Extract bounding box
                var bbMatch = Regex.Match(content, @"%%BoundingBox:\s*(\d+)\s+(\d+)\s+(\d+)\s+(\d+)");
                if (bbMatch.Success)
                {
                    epsData.BoundingBox = bbMatch.Value;
                    var x1 = double.Parse(bbMatch.Groups[1].Value);
                    var y1 = double.Parse(bbMatch.Groups[2].Value);
                    var x2 = double.Parse(bbMatch.Groups[3].Value);
                    var y2 = double.Parse(bbMatch.Groups[4].Value);

                    epsData.Width = x2 - x1;
                    epsData.Height = y2 - y1;
                }
                else
                {
                    // Try HiResBoundingBox
                    var hiBbMatch = Regex.Match(content, @"%%HiResBoundingBox:\s*([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)");
                    if (hiBbMatch.Success)
                    {
                        epsData.BoundingBox = hiBbMatch.Value;
                        var x1 = double.Parse(hiBbMatch.Groups[1].Value);
                        var y1 = double.Parse(hiBbMatch.Groups[2].Value);
                        var x2 = double.Parse(hiBbMatch.Groups[3].Value);
                        var y2 = double.Parse(hiBbMatch.Groups[4].Value);

                        epsData.Width = x2 - x1;
                        epsData.Height = y2 - y1;
                    }
                    else
                    {
                        // Default dimensions if no bounding box found
                        epsData.Width = 595;  // A4 width in points
                        epsData.Height = 842; // A4 height in points
                    }
                }

                // Extract additional metadata
                ExtractMetadata(content, epsData);

                return epsData;
            });
        }

        private void ExtractMetadata(string content, EPSData epsData)
        {
            // Extract Creator
            var creatorMatch = Regex.Match(content, @"%%Creator:\s*(.+)");
            if (creatorMatch.Success)
            {
                epsData.Creator = creatorMatch.Groups[1].Value.Trim();
            }

            // Extract Title
            var titleMatch = Regex.Match(content, @"%%Title:\s*(.+)");
            if (titleMatch.Success)
            {
                epsData.Title = titleMatch.Groups[1].Value.Trim();
            }

            // Extract CreationDate
            var dateMatch = Regex.Match(content, @"%%CreationDate:\s*(.+)");
            if (dateMatch.Success)
            {
                epsData.CreationDate = dateMatch.Groups[1].Value.Trim();
            }
        }

        public async Task<BitmapSource> RenderToImageAsync(EPSData epsData, int dpi)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Try Ghostscript first (best quality)
                    if (_ghostscriptVersion != null && !string.IsNullOrEmpty(epsData.FilePath))
                    {
                        try
                        {
                            return RenderWithGhostscript(epsData.FilePath, dpi);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ghostscript rendering failed: {ex.Message}");
                        }
                    }

                    // Fallback to ImageMagick
                    if (!string.IsNullOrEmpty(epsData.FilePath))
                    {
                        try
                        {
                            return RenderWithImageMagick(epsData.FilePath, dpi);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ImageMagick rendering failed: {ex.Message}");
                        }
                    }

                    // Last fallback: create placeholder with EPS info
                    return CreateEPSPlaceholder(epsData, dpi);
                }
                catch (Exception ex)
                {
                    return CreateErrorBitmap($"Fehler beim Rendern: {ex.Message}");
                }
            });
        }

        private BitmapSource RenderWithGhostscript(string filePath, int dpi)
        {
            if (_ghostscriptVersion == null)
                throw new Exception("Ghostscript nicht verfügbar");

            using var rasterizer = new GhostscriptRasterizer();
            rasterizer.Open(filePath, _ghostscriptVersion, false);

            var img = rasterizer.GetPage(dpi, 1);

            // Convert System.Drawing.Image to BitmapSource
            using var memory = new MemoryStream();
            img.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        private BitmapSource RenderWithImageMagick(string filePath, int dpi)
        {
            var settings = new MagickReadSettings
            {
                Density = new Density(dpi, dpi),
                BackgroundColor = MagickColors.White
            };

            using var image = new MagickImage(filePath, settings);

            // Convert to PNG in memory
            using var memory = new MemoryStream();
            image.Format = MagickFormat.Png;
            image.Write(memory);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        private BitmapSource CreateEPSPlaceholder(EPSData epsData, int dpi)
        {
            int width = Math.Max(400, (int)(epsData.Width * dpi / 72));
            int height = Math.Max(300, (int)(epsData.Height * dpi / 72));

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Draw border
            using var borderPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2
            };
            canvas.DrawRect(10, 10, width - 20, height - 20, borderPaint);

            // Draw EPS icon/text
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextSize = 32,
                TextAlign = SKTextAlign.Center,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            canvas.DrawText("📄 EPS", width / 2, height / 2 - 60, paint);

            // Draw metadata
            paint.TextSize = 16;
            paint.Typeface = SKTypeface.FromFamilyName("Arial");

            var yPos = height / 2;

            if (!string.IsNullOrEmpty(epsData.Title))
            {
                canvas.DrawText($"Titel: {epsData.Title}", width / 2, yPos, paint);
                yPos += 25;
            }

            canvas.DrawText($"Größe: {epsData.Width:F0} × {epsData.Height:F0} pt", width / 2, yPos, paint);
            yPos += 25;

            if (!string.IsNullOrEmpty(epsData.Creator))
            {
                canvas.DrawText($"Erstellt mit: {epsData.Creator}", width / 2, yPos, paint);
                yPos += 25;
            }

            paint.TextSize = 12;
            paint.Color = SKColors.Gray;
            canvas.DrawText("Ghostscript nicht installiert - Vorschau nicht verfügbar", width / 2, height - 30, paint);
            canvas.DrawText("Vektorisierung und Export funktionieren weiterhin", width / 2, height - 15, paint);

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

        public async Task SaveEPSAsync(string filePath, EPSData epsData)
        {
            await File.WriteAllTextAsync(filePath, epsData.Content ?? "");
        }

        public async Task<string> ConvertImageToEPSAsync(string imagePath, VectorizationSettings settings)
        {
            // This will be handled by VectorizationService
            return await Task.FromResult("");
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

            // Split message into lines
            var words = message.Split(' ');
            var lines = new System.Collections.Generic.List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                if (paint.MeasureText(testLine) > width - 40)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }
            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            var startY = (height - lines.Count * 25) / 2;
            for (int i = 0; i < lines.Count; i++)
            {
                canvas.DrawText(lines[i], width / 2, startY + i * 25, paint);
            }

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

        public bool IsGhostscriptAvailable()
        {
            return _ghostscriptVersion != null;
        }
    }
}

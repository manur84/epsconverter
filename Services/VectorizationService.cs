using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using EPSConverter.Models;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace EPSConverter.Services
{
    public class VectorizationService
    {
        public async Task<string> VectorizeImageAsync(string imagePath, VectorizationSettings settings)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Load image using Emgu.CV (OpenCV wrapper)
                    using var image = CvInvoke.Imread(imagePath, ImreadModes.Color);

                    if (image.IsEmpty)
                    {
                        throw new Exception("Bild konnte nicht geladen werden");
                    }

                    // Convert to grayscale for edge detection
                    using var gray = new Mat();
                    CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);

                    // Apply Gaussian blur for smoothing
                    if (settings.Smoothing > 0)
                    {
                        CvInvoke.GaussianBlur(gray, gray, new System.Drawing.Size(settings.Smoothing * 2 + 1, settings.Smoothing * 2 + 1), 0);
                    }

                    // Threshold
                    using var thresh = new Mat();
                    CvInvoke.Threshold(gray, thresh, settings.Threshold, 255, ThresholdType.Binary);

                    // Find contours
                    using var contours = new VectorOfVectorOfPoint();
                    using var hierarchy = new Mat();
                    CvInvoke.FindContours(thresh, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                    // Generate EPS content
                    var eps = GenerateEPS(image.Width, image.Height, contours, settings, image);

                    return eps;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Vektorisierung fehlgeschlagen: {ex.Message}", ex);
                }
            });
        }

        private string GenerateEPS(int width, int height, VectorOfVectorOfPoint contours, VectorizationSettings settings, Mat originalImage)
        {
            var sb = new StringBuilder();

            // EPS Header
            sb.AppendLine("%!PS-Adobe-3.0 EPSF-3.0");
            sb.AppendLine($"%%BoundingBox: 0 0 {width} {height}");
            sb.AppendLine("%%Creator: EPS Converter");
            sb.AppendLine($"%%CreationDate: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("%%DocumentData: Clean7Bit");
            sb.AppendLine("%%Origin: 0 0");
            sb.AppendLine("%%Pages: 1");
            sb.AppendLine("%%Page: 1 1");
            sb.AppendLine();

            // Set coordinate system (flip Y axis for proper orientation)
            sb.AppendLine($"0 {height} translate");
            sb.AppendLine("1 -1 scale");
            sb.AppendLine();

            // Set default line properties
            sb.AppendLine("1 setlinewidth");
            sb.AppendLine("1 setlinejoin");
            sb.AppendLine("1 setlinecap");
            sb.AppendLine();

            // Process contours
            int contourCount = Math.Min(contours.Size, settings.DetailLevel * 100);

            for (int i = 0; i < contourCount; i++)
            {
                using var contour = contours[i];

                if (contour.Size < 3)
                    continue;

                // Approximate contour to reduce points
                using var approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contour, approx, settings.DetailLevel * 0.5, true);

                if (approx.Size < 3)
                    continue;

                // Get color from original image if color mode is enabled
                if (settings.ColorMode && originalImage != null)
                {
                    var moments = CvInvoke.Moments(contour);
                    int cx = (int)(moments.M10 / moments.M00);
                    int cy = (int)(moments.M01 / moments.M00);

                    if (cx >= 0 && cx < originalImage.Width && cy >= 0 && cy < originalImage.Height)
                    {
                        var pixel = originalImage.GetData(cy, cx) as byte[];
                        if (pixel != null && pixel.Length >= 3)
                        {
                            double r = pixel[2] / 255.0;
                            double g = pixel[1] / 255.0;
                            double b = pixel[0] / 255.0;
                            sb.AppendLine($"{r:F3} {g:F3} {b:F3} setrgbcolor");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("0 0 0 setrgbcolor");
                }

                // Draw path
                sb.AppendLine("newpath");

                var points = approx.ToArray();
                if (points.Length > 0)
                {
                    sb.AppendLine($"{points[0].X} {points[0].Y} moveto");

                    for (int j = 1; j < points.Length; j++)
                    {
                        sb.AppendLine($"{points[j].X} {points[j].Y} lineto");
                    }

                    sb.AppendLine("closepath");
                    sb.AppendLine("fill");
                }

                sb.AppendLine();
            }

            // EPS Footer
            sb.AppendLine("showpage");
            sb.AppendLine("%%EOF");

            return sb.ToString();
        }

        public async Task<List<string>> BatchVectorizeAsync(string[] imagePaths, VectorizationSettings settings, string outputFolder)
        {
            var results = new List<string>();

            foreach (var imagePath in imagePaths)
            {
                try
                {
                    var eps = await VectorizeImageAsync(imagePath, settings);
                    var outputPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(imagePath) + ".eps");
                    await File.WriteAllTextAsync(outputPath, eps);
                    results.Add(outputPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler bei {imagePath}: {ex.Message}");
                }
            }

            return results;
        }
    }
}

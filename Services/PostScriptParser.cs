using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EPSConverter.Models;

namespace EPSConverter.Services
{
    /// <summary>
    /// Simple PostScript parser for extracting basic information and commands from EPS files
    /// </summary>
    public class PostScriptParser
    {
        public PostScriptDocument Parse(string epsContent)
        {
            var doc = new PostScriptDocument
            {
                RawContent = epsContent
            };

            // Parse header
            ParseHeader(epsContent, doc);

            // Extract paths and shapes
            ExtractPaths(epsContent, doc);

            // Extract text elements
            ExtractText(epsContent, doc);

            // Extract colors
            ExtractColors(epsContent, doc);

            return doc;
        }

        private void ParseHeader(string content, PostScriptDocument doc)
        {
            // Extract PS-Adobe version
            var versionMatch = Regex.Match(content, @"%!PS-Adobe-([0-9.]+)\s+EPSF-([0-9.]+)");
            if (versionMatch.Success)
            {
                doc.PSVersion = versionMatch.Groups[1].Value;
                doc.EPSFVersion = versionMatch.Groups[2].Value;
            }

            // Extract BoundingBox
            var bbMatch = Regex.Match(content, @"%%BoundingBox:\s*(\d+)\s+(\d+)\s+(\d+)\s+(\d+)");
            if (bbMatch.Success)
            {
                doc.BoundingBox = new BoundingBox
                {
                    X1 = int.Parse(bbMatch.Groups[1].Value),
                    Y1 = int.Parse(bbMatch.Groups[2].Value),
                    X2 = int.Parse(bbMatch.Groups[3].Value),
                    Y2 = int.Parse(bbMatch.Groups[4].Value)
                };
            }

            // Extract HiResBoundingBox if available
            var hiBbMatch = Regex.Match(content, @"%%HiResBoundingBox:\s*([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)");
            if (hiBbMatch.Success)
            {
                doc.HiResBoundingBox = new BoundingBox
                {
                    X1 = double.Parse(hiBbMatch.Groups[1].Value),
                    Y1 = double.Parse(hiBbMatch.Groups[2].Value),
                    X2 = double.Parse(hiBbMatch.Groups[3].Value),
                    Y2 = double.Parse(hiBbMatch.Groups[4].Value)
                };
            }

            // Extract metadata
            ExtractMetadata(content, doc);
        }

        private void ExtractMetadata(string content, PostScriptDocument doc)
        {
            var metadata = new Dictionary<string, string>();

            var metadataPatterns = new[]
            {
                ("Title", @"%%Title:\s*(.+)"),
                ("Creator", @"%%Creator:\s*(.+)"),
                ("CreationDate", @"%%CreationDate:\s*(.+)"),
                ("For", @"%%For:\s*(.+)"),
                ("Pages", @"%%Pages:\s*(.+)"),
                ("DocumentData", @"%%DocumentData:\s*(.+)"),
                ("LanguageLevel", @"%%LanguageLevel:\s*(.+)")
            };

            foreach (var (key, pattern) in metadataPatterns)
            {
                var match = Regex.Match(content, pattern);
                if (match.Success)
                {
                    metadata[key] = match.Groups[1].Value.Trim();
                }
            }

            doc.Metadata = metadata;
        }

        private void ExtractPaths(string content, PostScriptDocument doc)
        {
            var paths = new List<PSPath>();

            // Look for path construction commands
            // newpath ... moveto ... lineto ... curveto ... closepath fill/stroke

            var pathMatches = Regex.Matches(content,
                @"newpath\s+(.*?)(?:fill|stroke|clip|eofill|eoclip)",
                RegexOptions.Singleline);

            foreach (Match match in pathMatches)
            {
                var pathContent = match.Groups[1].Value;
                var path = new PSPath();

                // Extract coordinates from moveto, lineto, curveto commands
                var movetoMatches = Regex.Matches(pathContent, @"([0-9.-]+)\s+([0-9.-]+)\s+moveto");
                var linetoMatches = Regex.Matches(pathContent, @"([0-9.-]+)\s+([0-9.-]+)\s+lineto");
                var curvetoMatches = Regex.Matches(pathContent, @"([0-9.-]+)\s+([0-9.-]+)\s+([0-9.-]+)\s+([0-9.-]+)\s+([0-9.-]+)\s+([0-9.-]+)\s+curveto");

                if (movetoMatches.Count > 0 || linetoMatches.Count > 0 || curvetoMatches.Count > 0)
                {
                    path.Commands = pathContent.Trim();
                    paths.Add(path);
                }
            }

            doc.Paths = paths;
        }

        private void ExtractText(string content, PostScriptDocument doc)
        {
            var textElements = new List<PSText>();

            // Look for show commands with text
            var showMatches = Regex.Matches(content, @"\(([^)]+)\)\s*show");
            foreach (Match match in showMatches)
            {
                textElements.Add(new PSText
                {
                    Content = match.Groups[1].Value
                });
            }

            doc.TextElements = textElements;
        }

        private void ExtractColors(string content, PostScriptDocument doc)
        {
            var colors = new List<PSColor>();

            // RGB colors: r g b setrgbcolor
            var rgbMatches = Regex.Matches(content, @"([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+setrgbcolor");
            foreach (Match match in rgbMatches)
            {
                colors.Add(new PSColor
                {
                    R = double.Parse(match.Groups[1].Value),
                    G = double.Parse(match.Groups[2].Value),
                    B = double.Parse(match.Groups[3].Value),
                    ColorSpace = "RGB"
                });
            }

            // Grayscale: g setgray
            var grayMatches = Regex.Matches(content, @"([0-9.]+)\s+setgray");
            foreach (Match match in grayMatches)
            {
                var gray = double.Parse(match.Groups[1].Value);
                colors.Add(new PSColor
                {
                    R = gray,
                    G = gray,
                    B = gray,
                    ColorSpace = "Gray"
                });
            }

            // CMYK colors: c m y k setcmykcolor
            var cmykMatches = Regex.Matches(content, @"([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+([0-9.]+)\s+setcmykcolor");
            foreach (Match match in cmykMatches)
            {
                colors.Add(new PSColor
                {
                    C = double.Parse(match.Groups[1].Value),
                    M = double.Parse(match.Groups[2].Value),
                    Y = double.Parse(match.Groups[3].Value),
                    K = double.Parse(match.Groups[4].Value),
                    ColorSpace = "CMYK"
                });
            }

            doc.Colors = colors.Distinct().ToList();
        }

        public string GetSummary(PostScriptDocument doc)
        {
            var summary = new List<string>();

            if (!string.IsNullOrEmpty(doc.PSVersion))
                summary.Add($"PostScript Version: {doc.PSVersion}");

            if (!string.IsNullOrEmpty(doc.EPSFVersion))
                summary.Add($"EPSF Version: {doc.EPSFVersion}");

            if (doc.BoundingBox != null)
                summary.Add($"BoundingBox: {doc.BoundingBox.X1} {doc.BoundingBox.Y1} {doc.BoundingBox.X2} {doc.BoundingBox.Y2}");

            if (doc.Paths != null && doc.Paths.Count > 0)
                summary.Add($"Paths: {doc.Paths.Count}");

            if (doc.TextElements != null && doc.TextElements.Count > 0)
                summary.Add($"Text Elements: {doc.TextElements.Count}");

            if (doc.Colors != null && doc.Colors.Count > 0)
                summary.Add($"Colors Used: {doc.Colors.Count}");

            return string.Join("\n", summary);
        }
    }

    public class PostScriptDocument
    {
        public string RawContent { get; set; } = "";
        public string? PSVersion { get; set; }
        public string? EPSFVersion { get; set; }
        public BoundingBox? BoundingBox { get; set; }
        public BoundingBox? HiResBoundingBox { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
        public List<PSPath> Paths { get; set; } = new();
        public List<PSText> TextElements { get; set; } = new();
        public List<PSColor> Colors { get; set; } = new();
    }

    public class BoundingBox
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        public double Width => X2 - X1;
        public double Height => Y2 - Y1;
    }

    public class PSPath
    {
        public string Commands { get; set; } = "";
        public List<PSPoint> Points { get; set; } = new();
    }

    public class PSPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class PSText
    {
        public string Content { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
        public string? Font { get; set; }
        public double FontSize { get; set; }
    }

    public class PSColor
    {
        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double M { get; set; }
        public double Y { get; set; }
        public double K { get; set; }
        public string ColorSpace { get; set; } = "RGB";

        public override bool Equals(object? obj)
        {
            if (obj is PSColor other)
            {
                return R == other.R && G == other.G && B == other.B &&
                       C == other.C && M == other.M && Y == other.Y && K == other.K &&
                       ColorSpace == other.ColorSpace;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, C, M, Y, K, ColorSpace);
        }
    }
}

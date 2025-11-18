namespace EPSConverter.Models
{
    public class EPSData
    {
        public string? Content { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string? BoundingBox { get; set; }
        public byte[]? PreviewData { get; set; }
    }
}

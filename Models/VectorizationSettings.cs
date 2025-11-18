namespace EPSConverter.Models
{
    public class VectorizationSettings
    {
        public int Threshold { get; set; } = 128;
        public int Smoothing { get; set; } = 2;
        public int DetailLevel { get; set; } = 10;
        public bool ColorMode { get; set; } = true;
        public int DPI { get; set; } = 300;
    }
}

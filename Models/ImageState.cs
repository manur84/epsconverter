using System.Windows.Media.Imaging;

namespace EPSConverter.Models
{
    public class ImageState
    {
        public BitmapSource? Image { get; set; }
        public double ScaleX { get; set; } = 1.0;
        public double ScaleY { get; set; } = 1.0;
        public double Rotation { get; set; } = 0.0;
        public double OffsetX { get; set; } = 0.0;
        public double OffsetY { get; set; } = 0.0;
        public double ZoomLevel { get; set; } = 1.0;

        public ImageState Clone()
        {
            return new ImageState
            {
                Image = this.Image,
                ScaleX = this.ScaleX,
                ScaleY = this.ScaleY,
                Rotation = this.Rotation,
                OffsetX = this.OffsetX,
                OffsetY = this.OffsetY,
                ZoomLevel = this.ZoomLevel
            };
        }
    }
}

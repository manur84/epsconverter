using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace EPSConverter.Services
{
    public class ImageService
    {
        public async Task<BitmapSource> LoadImageAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return (BitmapSource)bitmap;
            });
        }

        public async Task SaveImageAsync(ImageSource imageSource, string filePath)
        {
            await Task.Run(() =>
            {
                if (imageSource is BitmapSource bitmapSource)
                {
                    BitmapEncoder encoder;
                    var ext = Path.GetExtension(filePath).ToLower();

                    encoder = ext switch
                    {
                        ".jpg" or ".jpeg" => new JpegBitmapEncoder { QualityLevel = 95 },
                        ".png" => new PngBitmapEncoder(),
                        ".bmp" => new BmpBitmapEncoder(),
                        ".tif" or ".tiff" => new TiffBitmapEncoder(),
                        ".gif" => new GifBitmapEncoder(),
                        _ => new PngBitmapEncoder()
                    };

                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    using var stream = new FileStream(filePath, FileMode.Create);
                    encoder.Save(stream);
                }
            });
        }

        public async Task<byte[]> ConvertToByteArrayAsync(BitmapSource bitmapSource)
        {
            return await Task.Run(() =>
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using var stream = new MemoryStream();
                encoder.Save(stream);
                return stream.ToArray();
            });
        }
    }
}

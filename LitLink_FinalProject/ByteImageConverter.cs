using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LitLink_FinalProject
{
    public class ByteImageConverter
    {
        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage bitImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            bitImg.BeginInit();
            bitImg.StreamSource = ms;
            bitImg.EndInit();

            ImageSource imgSrc = bitImg as ImageSource;

            return imgSrc;
        }
    }
}

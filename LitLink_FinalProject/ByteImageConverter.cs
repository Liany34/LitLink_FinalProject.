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
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;
            return imgSrc;
        }

        //public static ImageSource ByteToImage(byte[] imageData)
        //{
        //    if (imageData == null || imageData.Length == 0)
        //        return null;
        //    BitmapImage bitmap = new BitmapImage();
        //    MemoryStream ms = new MemoryStream(imageData);
        //    bitmap.BeginInit();
        //    bitmap.StreamSource = ms;
        //    bitmap.EndInit();
        //    return bitmap as ImageSource;
        //}
    }
}

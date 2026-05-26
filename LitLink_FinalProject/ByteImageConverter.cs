using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class ByteImageConverter
{
    public static ImageSource ByteToImage(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0) return null;

        BitmapImage biImg = new BitmapImage();
        MemoryStream ms = new MemoryStream(imageData);
        biImg.BeginInit();
        biImg.StreamSource = ms;
        biImg.EndInit();

        return biImg as ImageSource;
    }
}

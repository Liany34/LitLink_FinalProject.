using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Model;

namespace LitLink_FinalProject.UserControls
{
    public partial class CatalogUserControl : UserControl
    {
        public CatalogUserControl()
        {
            InitializeComponent();


            this.Loaded += (s, e) =>
            {
                Book currentBook = this.DataContext as Book;
                if (currentBook != null && !string.IsNullOrEmpty(currentBook.Cover))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(currentBook.Cover);
                        this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    catch (Exception)
                    {
                        this.BookCoverImage.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/DefaultCover.png", UriKind.RelativeOrAbsolute));
                    }
                }
            };
        }

        public static readonly DependencyProperty BookNameProperty =
            DependencyProperty.Register("BookName", typeof(string), typeof(CatalogUserControl), new PropertyMetadata(string.Empty));

        public string BookName
        {
            get { return (string)GetValue(BookNameProperty); }
            set { SetValue(BookNameProperty, value); }
        }

        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register("Price", typeof(decimal), typeof(CatalogUserControl), new PropertyMetadata(0.00m));

        public decimal Price
        {
            get { return (decimal)GetValue(PriceProperty); }
            set { SetValue(PriceProperty, value); }
        }

        public static readonly DependencyProperty CoverProperty =
            DependencyProperty.Register("Cover", typeof(ImageSource), typeof(CatalogUserControl), new PropertyMetadata(null));

        public ImageSource Cover
        {
            get { return (ImageSource)GetValue(CoverProperty); }
            set { SetValue(CoverProperty, value); }
        }
    }
}

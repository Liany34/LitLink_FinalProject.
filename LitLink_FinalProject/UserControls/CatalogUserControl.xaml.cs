using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Model;
using Service;

namespace LitLink_FinalProject.UserControls
{
    public partial class CatalogUserControl : UserControl
    {
        private Apiservice apiService = new Apiservice();

        public CatalogUserControl()
        {
            InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                Book currentBook = this.DataContext as Book;

                if (currentBook == null)
                {
                    SetDefaultCover();
                    return;
                }

                try
                {
                    string st = await apiService.GetBookCoverByBookIDByte64(currentBook.Id);

                    if (!string.IsNullOrWhiteSpace(st))
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    else
                    {
                        SetDefaultCover();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Catalog image load failed: " + ex.Message);
                    SetDefaultCover();
                }
            };
        }

        private void SetDefaultCover()
        {
            try
            {
                BookCoverImage.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/To_be_revealed.png", UriKind.Absolute));
            }
            catch
            {
                BookCoverImage.Source = null;
            }
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
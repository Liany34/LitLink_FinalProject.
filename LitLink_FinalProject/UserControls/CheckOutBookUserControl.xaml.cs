using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Model;

namespace LitLink_FinalProject.UserControls
{
    public partial class CheckOutBookUserControl : UserControl
    {
        public CheckOutBookUserControl()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += (s, e) => {
                Book currentBook = this.DataContext as Book;
                if (currentBook != null && !string.IsNullOrEmpty(currentBook.Cover))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(currentBook.Cover);
                        this.ImgBook.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    catch (Exception)
                    {
                        this.ImgBook.Source = new BitmapImage(new Uri("C:\\Users\\yahal\\source\\repos\\Liany34\\LitLink_Liany\\ViewModel\\Covers\\DefaultCover.png", UriKind.RelativeOrAbsolute));
                    }
                }
            };
        }

        public string BookTitle
        {
            get { return (string)GetValue(BookTitleProperty); }
            set { SetValue(BookTitleProperty, value); }
        }

        public static readonly DependencyProperty BookTitleProperty =
            DependencyProperty.Register("BookTitle", typeof(string), typeof(CheckOutBookUserControl),
                new PropertyMetadata(string.Empty, OnBookTitleChanged));

        private static void OnBookTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CheckOutBookUserControl;
            if (control != null) control.TxtTitle.Text = e.NewValue?.ToString();
        }

        public double BookPrice
        {
            get { return (double)GetValue(BookPriceProperty); }
            set { SetValue(BookPriceProperty, value); }
        }

        public static readonly DependencyProperty BookPriceProperty =
            DependencyProperty.Register("BookPrice", typeof(double), typeof(CheckOutBookUserControl),
                new PropertyMetadata(0.0, OnBookPriceChanged));

        private static void OnBookPriceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CheckOutBookUserControl;
            if (control != null && e.NewValue != null)
            {
                control.TxtPrice.Text = $"{Convert.ToDouble(e.NewValue):F2} ₪";
            }
        }

        public string BookImageUrl
        {
            get { return (string)GetValue(BookImageUrlProperty); }
            set { SetValue(BookImageUrlProperty, value); }
        }

        public static readonly DependencyProperty BookImageUrlProperty =
            DependencyProperty.Register("BookImageUrl", typeof(string), typeof(CheckOutBookUserControl),
                new PropertyMetadata(string.Empty, OnBookImageUrlChanged));

        private static void OnBookImageUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CheckOutBookUserControl;
            if (control != null && e.NewValue != null)
            {
                try
                {
                    byte[] imgStr = Convert.FromBase64String(e.NewValue.ToString());
                    control.ImgBook.Source = ByteImageConverter.ByteToImage(imgStr);
                }
                catch
                {
                    control.ImgBook.Source = new BitmapImage(new Uri("C:\\Users\\yahal\\source\\repos\\Liany34\\LitLink_Liany\\ViewModel\\Covers\\DefaultCover.png", UriKind.RelativeOrAbsolute));
                }
            }
        }
    }
}

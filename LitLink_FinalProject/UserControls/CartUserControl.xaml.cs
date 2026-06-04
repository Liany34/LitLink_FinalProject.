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
using Service;

namespace LitLink_FinalProject.UserControls
{
    public partial class CartUserControl : UserControl
    {
        public event EventHandler MoveToWishListRequested;
        public event EventHandler IsSelectedChanged;
        private Apiservice apiService = new Apiservice();

        public CartUserControl()
        {
            InitializeComponent();
            this.Loaded += async (s, e) => {
                Book currentBook = this.DataContext as Book;
                string st = await apiService.GetBookCoverByBookIDByte64(currentBook.Id);
                if (currentBook != null && !string.IsNullOrEmpty(currentBook.Cover))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    catch (Exception)
                    {
                        this.BookCoverImage.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/To_be_revealed.png", UriKind.RelativeOrAbsolute));
                    }
                }
            };
        }

        public bool IsBookSelected
        {
            get => Convert.ToBoolean(CartCheckBox.IsChecked);
            set => CartCheckBox.IsChecked = value;
        }

        private void MoveToWishList_Click(object sender, RoutedEventArgs e)
        {
            MoveToWishListRequested?.Invoke(this, EventArgs.Empty);
            MessageBox.Show("The book has been moved to your Wish List!", "Wish List", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CartCheckBox_CheckedChange(object sender, RoutedEventArgs e)
        {
            IsSelectedChanged?.Invoke(this, EventArgs.Empty);
        }
        public void ApplyDiscount(double percent)
        {
            Book book = this.DataContext as Book;
            if (book == null || book.Price == null)
                return;

            double originalPrice = book.Price.Value;
            double finalPrice = originalPrice - (originalPrice * percent / 100.0);

            OriginalPriceText.Text = originalPrice.ToString("C");
            FinalPriceText.Text = finalPrice.ToString("C");

            OriginalPriceText.Visibility = Visibility.Visible;
        }
    }
}
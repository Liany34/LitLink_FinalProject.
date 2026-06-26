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

        public Reader CurrentReader { get; set; }
        public Cart_Detail CartDetailRef { get; set; }

        public CartUserControl()
        {
            InitializeComponent();
            this.Loaded += async (s, e) => {
                Book currentBook = this.DataContext as Book;
                if (currentBook == null) return;
                try
                {
                    string st = await apiService.GetBookCoverByBookIDByte64(currentBook.Id);
                    if (!string.IsNullOrEmpty(st))
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    else
                    {
                        this.BookCoverImage.Source = new BitmapImage(
                            new Uri("pack://application:,,,/Covers/To_be_revealed.png", UriKind.RelativeOrAbsolute));
                    }
                }
                catch (Exception)
                {
                    this.BookCoverImage.Source = new BitmapImage(
                        new Uri("pack://application:,,,/Covers/To_be_revealed.png", UriKind.RelativeOrAbsolute));
                }
            };
        }
        public bool IsBookSelected
        {
            get => Convert.ToBoolean(CartCheckBox.IsChecked);
            set => CartCheckBox.IsChecked = value;
        }

        private async void MoveToWishList_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;

            if (currentBook == null || CurrentReader == null || CartDetailRef == null)
            {
                MessageBox.Show("Could not move this book to your Wish List.", "LitLink",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                const string WishListName = "Wish List";

                List<Book_Series> allSeries = await apiService.GetAllBookSeries();

                Book_Series wishList = allSeries?.FirstOrDefault(s =>
                    s.IdUser != null &&
                    s.IdUser.Id == CurrentReader.Id &&
                    s.NameSeries == WishListName);

                if (wishList == null)
                {
                    BookSeriesInsertDto newList = new BookSeriesInsertDto
                    {
                        NameSeries = WishListName,
                        IdUser = CurrentReader.Id
                    };

                    int inserted = await apiService.InsertBookSeries(newList);

                    if (inserted != 1)
                    {
                        MessageBox.Show("Failed to create your Wish List.", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    List<Book_Series> updatedSeries = await apiService.GetAllBookSeries();
                    wishList = updatedSeries?.FirstOrDefault(s =>
                        s.IdUser != null &&
                        s.IdUser.Id == CurrentReader.Id &&
                        s.NameSeries == WishListName);

                    if (wishList == null)
                    {
                        MessageBox.Show("Failed to retrieve your Wish List after creation.", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                List<Series_Detail> allDetails = await apiService.GetAllSeriesDetails();

                List<Series_Detail> listDetails = allDetails
                    .Where(d => d.IdSeries != null && d.IdSeries.Id == wishList.Id)
                    .ToList();

                bool alreadyInList = listDetails.Any(d => d.IdBook != null && d.IdBook.Id == currentBook.Id);

                if (!alreadyInList)
                {
                    int nextNumber = listDetails.Count + 1;

                    Series_Detail newDetail = new Series_Detail
                    {
                        IdSeries = wishList,
                        IdBook = currentBook,
                        Number = nextNumber
                    };

                    int detailResult = await apiService.InsertSeriesDetail(newDetail);

                    if (detailResult != 1)
                    {
                        MessageBox.Show("Failed to add book to your Wish List.", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                int deleteResult = await apiService.DeleteCartDetail(CartDetailRef.Id);

                if (deleteResult != 1)
                {
                    MessageBox.Show("The book was added to your Wish List, but could not be removed from the cart.",
                                    "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                MessageBox.Show("The book has been moved to your Wish List!", "Wish List",
                                MessageBoxButton.OK, MessageBoxImage.Information);

                MoveToWishListRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to move book to Wish List: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
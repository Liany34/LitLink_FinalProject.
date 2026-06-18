using LitLink_FinalProject.Pages;
using LitLink_FinalProject.WindowsFile;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LitLink_FinalProject.UserControls
{
    public partial class BookUserControl : UserControl
    {
        private bool isAdmin;
        private bool isAuthor;
        private bool bookAlreadyInCart;
        private bool userAlreadyPurchasedBook;

        private Reader currentReader;
        private Author currentAuthor;

        private Apiservice apiService = new Apiservice();

        public BookUserControl(Book bookData, bool userOwnsBook, bool isAdmin, bool isAuthor, Reader currentReader = null, Author currentAuthor = null)
        {
            InitializeComponent();

            this.DataContext = bookData;
            this.isAdmin = isAdmin;
            this.isAuthor = isAuthor;
            this.currentReader = currentReader;
            this.currentAuthor = currentAuthor;
            this.userAlreadyPurchasedBook = userOwnsBook;

            // לא קוראים SetActionButtons כאן — נקרא רק אחרי CheckReaderBookStatus
            this.Loaded += BookUserControl_Loaded;
        }

        private async void BookUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            try
            {
                ReadMoreBtn.Visibility = DescriptionTextBlock.ActualHeight < 120
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                await CheckReaderBookStatus();

                SetActionButtons();

                SetupPermissions();

                ReviewsStackPanel.Children.Clear();
                TotalReviewsTextBlock.Text = "(0)";

                List<Reviews> allReviews = await apiService.GetAllReviews();

                if (allReviews != null)
                {
                    List<Reviews> bookReviews = allReviews
                        .Where(r => r != null && r.IdBook != null && r.IdBook.Id == currentBook.Id)
                        .ToList();

                    TotalReviewsTextBlock.Text = $"({bookReviews.Count})";

                    if (bookReviews.Count > 0)
                    {
                        double average = bookReviews.Average(r => r.Stars);
                        StarsTextBlock.Text = $"{average:0.0} ★";
                        StarsTextBlock.FontSize = 22;
                        StarsTextBlock.Foreground = Brushes.Gold;

                        int currentUserId = currentReader?.Id ?? 0;

                        foreach (Reviews reviewItem in bookReviews)
                        {
                            ReviewsUserControl reviewControl = new ReviewsUserControl(reviewItem, currentUserId, isAdmin);
                            ReviewsStackPanel.Children.Add(reviewControl);
                        }
                    }
                    else
                    {
                        StarsTextBlock.Text = "No reviews yet";
                        StarsTextBlock.FontSize = 14;
                        StarsTextBlock.Foreground = Brushes.Gray;
                    }
                }

                string coverBase64 = await apiService.GetBookCoverByBookIDByte64(currentBook.Id);
                if (!string.IsNullOrEmpty(coverBase64))
                {
                    byte[] imgBytes = Convert.FromBase64String(coverBase64);
                    BookCoverImage.Source = ByteImageConverter.ByteToImage(imgBytes);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading book user control: " + ex.Message);
            }
        }

        private void SetActionButtons()
        {
            BuyBtn.Visibility = Visibility.Collapsed;
            AddReviewBtn.Visibility = Visibility.Collapsed;
            AddToListBtn.Visibility = Visibility.Collapsed;

            if (isAdmin || isAuthor)
                return;

            AddToListBtn.Visibility = Visibility.Visible;
            AddReviewBtn.Visibility = Visibility.Visible;

            if (!userAlreadyPurchasedBook && !bookAlreadyInCart)
                BuyBtn.Visibility = Visibility.Visible;
        }

        private void SetupPermissions()
        {
            Book currentBook = this.DataContext as Book;

            bool canEditOrDelete = isAdmin ||
                (isAuthor && currentAuthor != null && currentBook != null &&
                 currentBook.IdAuthor?.Id == currentAuthor.Id);

            bool canReport = !isAdmin && !isAuthor && currentReader != null;

            MenuBtn.Visibility = (canEditOrDelete || canReport)
                ? Visibility.Visible
                : Visibility.Collapsed;

            Visibility editVis = canEditOrDelete ? Visibility.Visible : Visibility.Collapsed;
            Visibility reportVis = canReport ? Visibility.Visible : Visibility.Collapsed;

            EditBookItem.Visibility = editVis;
            DeleteItem.Visibility = editVis;
            AdminSeparator.Visibility = editVis;
            ReportItem.Visibility = reportVis;

            if (currentBook != null)
                ReportItem.Header = currentBook.IsFlaged ? "Remove Report" : "Report Book";
        }

        private async Task CheckReaderBookStatus()
        {
            if (currentReader == null) return;

            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            try
            {
                List<Cart_Detail> allCartDetails = await apiService.GetAllCartDetails();
                if (allCartDetails == null) return;

                userAlreadyPurchasedBook = allCartDetails.Any(cd =>
                    cd.IdCart?.IdReader?.Id == currentReader.Id &&
                    cd.IdBook?.Id == currentBook.Id &&
                    cd.IsPurchased);

                bookAlreadyInCart = allCartDetails.Any(cd =>
                    cd.IdCart?.IdReader?.Id == currentReader.Id &&
                    cd.IdBook?.Id == currentBook.Id &&
                    !cd.IsPurchased);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error checking cart/purchase status: " + ex.Message);
            }
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            BookContextMenu.PlacementTarget = sender as Button;
            BookContextMenu.IsOpen = true;
        }

        private void ReadMoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DescriptionTextBlock.MaxHeight == 120)
            {
                DescriptionTextBlock.MaxHeight = double.PositiveInfinity;
                ReadMoreBtn.Content = "Read Less";
            }
            else
            {
                DescriptionTextBlock.MaxHeight = 120;
                ReadMoreBtn.Content = "Read More";
            }
        }

        private async void Buy_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null || currentReader == null) return;

            try
            {
                List<Cart> allCarts = await apiService.GetAllCarts();

                Cart readerCart = allCarts?.FirstOrDefault(c =>
                    c.IdReader != null && c.IdReader.Id == currentReader.Id);

                // אם אין עגלה לקורא — יוצרים אחת ואז שולפים אותה מחדש מהשרת
                if (readerCart == null)
                {
                    Cart newCart = new Cart
                    {
                        IdReader = currentReader,
                        DiscountCode = null
                    };

                    int inserted = await apiService.InsertCart(newCart);

                    if (inserted != 1)
                    {
                        MessageBox.Show("Failed to create cart.", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // שליפה מחדש כדי לקבל את ה-Id האמיתי מהדאטאבייס
                    List<Cart> updatedCarts = await apiService.GetAllCarts();
                    readerCart = updatedCarts?.FirstOrDefault(c =>
                        c.IdReader != null && c.IdReader.Id == currentReader.Id);

                    if (readerCart == null)
                    {
                        MessageBox.Show("Failed to retrieve cart after creation.", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                Cart_Detail cd = new Cart_Detail
                {
                    IdCart = readerCart,
                    IdBook = currentBook,
                    PurchasePrice = currentBook.Price ?? 0.0,
                    IsPurchased = false,
                    PurchaseDate = null
                };

                int result = await apiService.InsertCartDetail(cd);

                if (result == 1)
                {
                    MessageBox.Show($"'{currentBook.BookName}' was added to your cart.", "LitLink");
                    bookAlreadyInCart = true;
                    SetActionButtons();
                }
                else
                {
                    MessageBox.Show("Failed to add book to cart.", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add book to cart: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            MessageBox.Show($"'{currentBook.BookName}' has been successfully added to your Reading List!",
                            "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            if (MessageBox.Show($"Are you sure you want to permanently delete '{currentBook.BookName}' from LitLink?",
                                "Delete Book", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                try
                {
                    await apiService.DeleteBook(currentBook.Id);
                    MessageBox.Show("The book has been successfully deleted.", "LitLink",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete book: " + ex.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ReportBook_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            string message = currentBook.IsFlaged
                ? "Do you want to remove your report from this book?"
                : "Report this book for inappropriate content?";

            if (MessageBox.Show(message, "Report Book", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                try
                {
                    currentBook.IsFlaged = !currentBook.IsFlaged;

                    bool success = await apiService.UpdateBook(CreateBookUpdateDto(currentBook));

                    if (success)
                    {
                        ReportItem.Header = currentBook.IsFlaged ? "Remove Report" : "Report Book";
                        MessageBox.Show(currentBook.IsFlaged
                            ? "Thank you. This book has been reported."
                            : "The report was removed.", "LitLink");
                    }
                    else
                    {
                        // Rollback
                        currentBook.IsFlaged = !currentBook.IsFlaged;
                        MessageBox.Show("Report update failed.", "LitLink");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update report: " + ex.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditBook_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            try
            {
                EditBookWindow editWindow = new EditBookWindow(currentBook);

                if (editWindow.ShowDialog() == true)
                {
                    // שליפה מחדש של כל הספרים מהשרת
                    List<Book> allBooks = await apiService.GetAllBooks();
                    Book updatedBook = allBooks?.FirstOrDefault(b => b.Id == currentBook.Id);

                    if (updatedBook != null)
                    {
                        this.DataContext = updatedBook;

                        // רענון התמונה
                        string coverBase64 = await apiService.GetBookCoverByBookIDByte64(updatedBook.Id);
                        if (!string.IsNullOrEmpty(coverBase64))
                        {
                            byte[] imgBytes = Convert.FromBase64String(coverBase64);
                            BookCoverImage.Source = ByteImageConverter.ByteToImage(imgBytes);
                        }
                        else
                        {
                            BookCoverImage.Source = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open edit window: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddReview_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null || currentReader == null) return;

            var reviewWindow = new AddReviewWindow(currentReader, currentBook);
            bool? result = reviewWindow.ShowDialog();

            if (result == true)
            {
                // רענון הביקורות אחרי הוספה
                ReviewsStackPanel.Children.Clear();
                TotalReviewsTextBlock.Text = "(0)";
                StarsTextBlock.Text = "";

                List<Reviews> allReviews = await apiService.GetAllReviews();

                if (allReviews != null)
                {
                    List<Reviews> bookReviews = allReviews
                        .Where(r => r != null && r.IdBook != null && r.IdBook.Id == currentBook.Id)
                        .ToList();

                    TotalReviewsTextBlock.Text = $"({bookReviews.Count})";

                    if (bookReviews.Count > 0)
                    {
                        double average = bookReviews.Average(r => r.Stars);
                        StarsTextBlock.Text = $"{average:0.0} ★";
                        StarsTextBlock.FontSize = 22;
                        StarsTextBlock.Foreground = Brushes.Gold;

                        foreach (Reviews reviewItem in bookReviews)
                        {
                            ReviewsUserControl reviewControl = new ReviewsUserControl(reviewItem, currentReader.Id, isAdmin);
                            ReviewsStackPanel.Children.Add(reviewControl);
                        }
                    }
                    else
                    {
                        StarsTextBlock.Text = "No reviews yet";
                        StarsTextBlock.FontSize = 14;
                        StarsTextBlock.Foreground = Brushes.Gray;
                    }
                }
            }
        }

        private BookUpdateDto CreateBookUpdateDto(Book book) => new BookUpdateDto
        {
            Id = book.Id,
            BookName = book.BookName,
            PublicationDate = book.PublicationDate,
            Price = book.Price,
            Information = book.Information,
            BookLink = book.BookLink,
            IsFlaged = book.IsFlaged,
            IdAuthor = book.IdAuthor.Id,
            IdLanguage = book.IdLanguage.Id,
            CoverPath = book.CoverPath,
            FileName = null,
            Base64Image = null
        };
    }
}
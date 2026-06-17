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
                // 1. בדיקת Read More
                ReadMoreBtn.Visibility = DescriptionTextBlock.ActualHeight < 120
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                // 2. שליפת סטטוס קנייה/עגלה — חייב לפני SetActionButtons
                await CheckReaderBookStatus();

                // 3. עדכון כפתורים אחרי שיש מידע אמיתי
                SetActionButtons();

                // 4. הרשאות תפריט 3 נקודות
                SetupPermissions();

                // 5. טעינת ביקורות
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

                        foreach (Reviews reviewItem in bookReviews)
                        {
                            ReviewsUserControl reviewControl = new ReviewsUserControl(reviewItem, 0, isAdmin);
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

                // 6. טעינת תמונת כריכה
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

            // מנהל וסופר לא רואים כפתורי קנייה
            if (isAdmin || isAuthor)
                return;

            // קורא — כפתור "הוסף לרשימה" תמיד מוצג
            AddToListBtn.Visibility = Visibility.Visible;

            if (userAlreadyPurchasedBook)
            {
                // קנה כבר → הצג הוספת ביקורת
                AddReviewBtn.Visibility = Visibility.Visible;
                BuyBtn.Visibility = Visibility.Collapsed;
            }
            else if (bookAlreadyInCart)
            {
                // בעגלה אבל לא קנה → הסתר הכל חוץ מ"הוסף לרשימה"
                BuyBtn.Visibility = Visibility.Collapsed;
                AddReviewBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                // לא קנה ולא בעגלה → הצג קנייה
                BuyBtn.Visibility = Visibility.Visible;
                AddReviewBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void SetupPermissions()
        {
            Book currentBook = this.DataContext as Book;

            // סופר רואה עריכה/מחיקה רק על הספרים שלו
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

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            try
            {
                EditBookWindow editWindow = new EditBookWindow(currentBook);

                if (editWindow.ShowDialog() == true)
                {
                    this.DataContext = null;
                    this.DataContext = currentBook;

                    if (!string.IsNullOrEmpty(currentBook.Cover))
                    {
                        byte[] imgStr = Convert.FromBase64String(currentBook.Cover);
                        this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open edit window: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddReview_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null || currentReader == null) return;

            MessageBox.Show("Open add review window here.", "LitLink");
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


//using LitLink_FinalProject.Pages;
//using LitLink_FinalProject.WindowsFile;
//using Model;
//using Service;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace LitLink_FinalProject.UserControls
//{
//    public partial class BookUserControl : UserControl
//    {
//        private bool isAdmin;
//        private bool isAuthor;
//        private bool readerPurchasedBook;
//        private bool bookAlreadyInCart;
//        private bool userAlreadyPurchasedBook;

//        private Reader currentReader;

//        private Apiservice apiService = new Apiservice();

//        public BookUserControl(Book bookData, bool userOwnsBook, bool isAdmin, bool isAuthor, Reader currentUser = null)
//        {
//            InitializeComponent();

//            this.DataContext = bookData;
//            this.isAdmin = isAdmin;
//            this.isAuthor = isAuthor;
//            this.currentReader = currentUser;
//            this.userAlreadyPurchasedBook = userOwnsBook;

//            SetActionButtons();
//            SetupPermissions();

//            this.Loaded += BookUserControl_Loaded;
//        }
//        private async void BookUserControl_Loaded(object sender, RoutedEventArgs e)
//        {
//            Book currentBook = this.DataContext as Book;
//            if (currentBook == null)
//                return;

//            try
//            {
//                // בדיקה אם צריך להציג Read More
//                if (DescriptionTextBlock.ActualHeight < 120)
//                {
//                    ReadMoreBtn.Visibility = Visibility.Collapsed;
//                }
//                else
//                {
//                    ReadMoreBtn.Visibility = Visibility.Visible;
//                }

//                // בדיקה אם קורא כבר קנה את הספר או שהספר כבר בעגלה
//                await CheckReaderBookStatus();

//                // הרשאות של שלוש נקודות
//                SetupPermissions();

//                // טעינת ביקורות
//                ReviewsStackPanel.Children.Clear();
//                TotalReviewsTextBlock.Text = "(0)";

//                List<Reviews> allReviews = await apiService.GetAllReviews();

//                if (allReviews != null)
//                {
//                    List<Reviews> bookReviews = allReviews
//                        .Where(r => r != null &&
//                                    r.IdBook != null &&
//                                    r.IdBook.Id == currentBook.Id)
//                        .ToList();

//                    TotalReviewsTextBlock.Text = $"({bookReviews.Count})";

//                    if (bookReviews.Count > 0)
//                    {
//                        double average = bookReviews.Average(r => r.Stars);

//                        StarsTextBlock.Text = $"{average:0.0} ★";
//                        StarsTextBlock.FontSize = 22;
//                        StarsTextBlock.Foreground = Brushes.Gold;

//                        foreach (Reviews reviewItem in bookReviews)
//                        {
//                            ReviewsUserControl reviewControl = new ReviewsUserControl(reviewItem, 0, isAdmin);
//                            ReviewsStackPanel.Children.Add(reviewControl);
//                        }
//                    }
//                    else
//                    {
//                        StarsTextBlock.Text = "No reviews yet";
//                        StarsTextBlock.FontSize = 14;
//                        StarsTextBlock.Foreground = Brushes.Gray;
//                    }
//                }

//                // טעינת תמונת כריכה
//                string coverBase64 = await apiService.GetBookCoverByBookIDByte64(currentBook.Id);

//                if (!string.IsNullOrEmpty(coverBase64))
//                {
//                    byte[] imgBytes = Convert.FromBase64String(coverBase64);
//                    BookCoverImage.Source = ByteImageConverter.ByteToImage(imgBytes);
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine("Error loading book user control: " + ex.Message);
//            }
//        }

//        private void SetActionButtons()
//        {
//            BuyBtn.Visibility = Visibility.Collapsed;
//            AddReviewBtn.Visibility = Visibility.Collapsed;
//            AddToListBtn.Visibility = Visibility.Collapsed;

//            // מנהל וסופר לא קונים ספרים מתוך המסך הזה
//            if (isAdmin || isAuthor)
//                return;

//            // קורא רגיל
//            AddToListBtn.Visibility = Visibility.Visible;

//            if (userAlreadyPurchasedBook)
//            {
//                // אם הקורא כבר קנה את הספר — במקום קנייה הוא יכול להוסיף ביקורת
//                AddReviewBtn.Visibility = Visibility.Visible;
//                BuyBtn.Visibility = Visibility.Collapsed;
//            }
//            else if (bookAlreadyInCart)
//            {
//                // אם הספר כבר בעגלה — לא מציגים כפתור הוספה לעגלה
//                BuyBtn.Visibility = Visibility.Collapsed;
//                AddReviewBtn.Visibility = Visibility.Collapsed;
//            }
//            else
//            {
//                // אם לא קנה ולא בעגלה — אפשר להוסיף לעגלה
//                BuyBtn.Visibility = Visibility.Visible;
//                AddReviewBtn.Visibility = Visibility.Collapsed;
//            }
//        }

//        private void SetupPermissions()
//        {
//            Book currentBook = this.DataContext as Book;

//            bool canEditOrDelete = isAdmin || isAuthor;
//            bool canReport = !isAdmin && !isAuthor && currentReader != null;

//            MenuBtn.Visibility = (canEditOrDelete || canReport)
//                ? Visibility.Visible
//                : Visibility.Collapsed;

//            Visibility editVis = canEditOrDelete ? Visibility.Visible : Visibility.Collapsed;
//            Visibility reportVis = canReport ? Visibility.Visible : Visibility.Collapsed;

//            EditBookItem.Visibility = editVis;
//            DeleteItem.Visibility = editVis;
//            AdminSeparator.Visibility = editVis;

//            ReportItem.Visibility = reportVis;

//            if (currentBook != null)
//            {
//                ReportItem.Header = currentBook.IsFlaged ? "Remove Report" : "Report Book";
//            }
//        }

//        private async Task CheckReaderBookStatus()
//        {
//            if (currentReader == null)
//                return;

//            Book currentBook = this.DataContext as Book;
//            if (currentBook == null)
//                return;

//            try
//            {
//                List<Cart_Detail> allCartDetails = await apiService.GetAllCartDetails();

//                if (allCartDetails == null)
//                    return;

//                // כאן צריך להתאים לפי המודל שלך:
//                // אם ב-Cart_Detail יש IdReader ישירות — השתמשי בזה.
//                // אם יש IdCart.IdReader — השתמשי בשורה שמתאימה לך.

//                userAlreadyPurchasedBook = allCartDetails.Any(cd =>
//                    cd.IdCart != null &&
//                    cd.IdCart.IdReader != null &&
//                    cd.IdCart.IdReader.Id == currentReader.Id &&
//                    cd.IdBook != null &&
//                    cd.IdBook.Id == currentBook.Id
//                );

//                bookAlreadyInCart = allCartDetails.Any(cd =>
//                   cd.IdCart != null &&
//                   cd.IdCart.IdReader != null &&
//                   cd.IdCart.IdReader.Id == currentReader.Id &&
//                   cd.IdBook != null &&
//                   cd.IdBook.Id == currentBook.Id &&
//                   !cd.IsPurchased
//                );

//                SetActionButtons();
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine("Error checking cart/purchase status: " + ex.Message);
//            }
//        }

//        private void MenuBtn_Click(object sender, RoutedEventArgs e)
//        {
//            BookContextMenu.PlacementTarget = sender as Button;
//            BookContextMenu.IsOpen = true;
//        }

//        private void ReadMoreBtn_Click(object sender, RoutedEventArgs e)
//        {
//            if (DescriptionTextBlock.MaxHeight == 120)
//            {
//                DescriptionTextBlock.MaxHeight = double.PositiveInfinity;
//                ReadMoreBtn.Content = "Read Less";
//            }
//            else
//            {
//                DescriptionTextBlock.MaxHeight = 120;
//                ReadMoreBtn.Content = "Read More";
//            }
//        }

//        private async void Buy_Click(object sender, RoutedEventArgs e)
//        {
//            Book currentBook = this.DataContext as Book;
//            if (currentBook == null || currentReader == null) return;

//            try
//            {
//                // כאן תכניסי את פעולת ההוספה לעגלה שיש לך ב-API.
//                // לדוגמה, אם יש לך InsertCartDetail:

//                Cart_Detail cd = new Cart_Detail
//                {
//                    IdBook = currentBook,
//                    IsPurchased = false,
//                    PurchaseDate = null
//                };

//                await apiService.InsertCartDetail(cd);

//                MessageBox.Show($"'{currentBook.BookName}' was added to your cart.", "LitLink");

//                bookAlreadyInCart = true;
//                SetActionButtons();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Failed to add book to cart: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        private void AddToList_Click(object sender, RoutedEventArgs e)
//        {
//            Book currentBook = this.DataContext as Book;
//            if (currentBook == null) return;

//            try
//            {
//                MessageBox.Show($"'{currentBook.BookName}' has been successfully added to your Reading List!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error adding book to list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        private void DeleteBook_Click(object sender, RoutedEventArgs e)
//        {
//            Book currentBook = this.DataContext as Book;
//            if (currentBook == null) return;

//            if (MessageBox.Show($"Are you sure you want to permanently delete '{currentBook.BookName}' from LitLink?",
//                                "Delete Book", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
//            {
//                try
//                {
//                    apiService.DeleteBook(currentBook.Id);
//                    MessageBox.Show("The book has been successfully deleted.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);

//                    this.Visibility = Visibility.Collapsed;
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("Failed to delete book: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                }
//            }
//        }

//        private async void ReportBook_Click(object sender, RoutedEventArgs e)
//        {
//            Book currentBook = this.DataContext as Book;
//            if (currentBook == null) return;

//            try
//            {
//                string message = currentBook.IsFlaged
//                    ? "Do you want to remove your report from this book?"
//                    : "Report this book for inappropriate content?";

//                if (MessageBox.Show(message, "Report Book", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
//                {
//                    currentBook.IsFlaged = !currentBook.IsFlaged;

//                    BookUpdateDto dto = CreateBookUpdateDto(currentBook);

//                    bool success = await apiService.UpdateBook(dto);

//                    if (success)
//                    {
//                        ReportItem.Header = currentBook.IsFlaged ? "Remove Report" : "Report Book";

//                        MessageBox.Show(
//                            currentBook.IsFlaged
//                                ? "Thank you. This book has been reported."
//                                : "The report was removed.",
//                            "LitLink");
//                    }
//                    else
//                    {
//                        MessageBox.Show("Report update failed.", "LitLink");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Failed to update report: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        private void EditBook_Click(object sender, RoutedEventArgs e)
//        {
//            Book currentBook = this.DataContext as Book;
//            if (currentBook == null) return;

//            try
//            {
//                EditBookWindow editWindow = new EditBookWindow(currentBook);

//                if (editWindow.ShowDialog() == true)
//                {
//                    this.DataContext = null;
//                    this.DataContext = currentBook;

//                    if (!string.IsNullOrEmpty(currentBook.Cover))
//                    {
//                        byte[] imgStr = Convert.FromBase64String(currentBook.Cover);
//                        this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Could not open edit window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }
//        private BookUpdateDto CreateBookUpdateDto(Book book)
//        {
//            return new BookUpdateDto
//            {
//                Id = book.Id,
//                BookName = book.BookName,
//                PublicationDate = book.PublicationDate,
//                Price = book.Price,
//                Information = book.Information,
//                BookLink = book.BookLink,
//                IsFlaged = book.IsFlaged,

//                IdAuthor = book.IdAuthor.Id,
//                IdLanguage = book.IdLanguage.Id,

//                CoverPath = book.CoverPath,
//                FileName = null,
//                Base64Image = null
//            };
//        }
//        private void AddReview_Click(object sender, RoutedEventArgs e)
//        {
//            Book currentBook = this.DataContext as Book;
//            if (currentBook == null || currentReader == null) return;

//            try
//            {
//                // אם יש לך חלון להוספת ביקורת, תפתחי אותו כאן.
//                // לדוגמה:
//                // AddReviewWindow win = new AddReviewWindow(currentBook, currentReader);
//                // if (win.ShowDialog() == true)
//                // {
//                //     // לרענן ביקורות
//                // }

//                MessageBox.Show("Open add review window here.", "LitLink");
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Could not open review window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }
//    }
//}
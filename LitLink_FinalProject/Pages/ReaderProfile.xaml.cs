using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LitLink_FinalProject.UserControls;
using LitLink_FinalProject.WindowsFile;

namespace LitLink_FinalProject.Pages
{
    public partial class ReaderProfile : Page
    {
        private Apiservice apiService = new Apiservice();
        private List<Book> allBooks = new List<Book>();
        private Reader currentUser;

        public ReaderProfile(Reader reader)
        {
            InitializeComponent();
            this.currentUser = reader; // שמירת המשתמש שהועבר
            this.Loaded += ReaderProfilePage_Loaded;
        }

        private void ReaderProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            // מחק או שים בהערה את השורה הישנה:
            // currentUser = this.DataContext as Reader; 

            LoadUserData();
        }

        private async void LoadUserData()
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please log in to view your profile.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
                return;
            }

            try
            {
                TxtHelloUser.Text = $"Hello, {currentUser.Username}";

                string st = await apiService.GetPRPByUserIDByte64(currentUser.Id);
                if (!string.IsNullOrEmpty(st))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.ImgReaderProfile.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    catch
                    {
                        this.ImgReaderProfile.Source = new BitmapImage(new Uri("C:\\Users\\yahal\\source\\repos\\Liany34\\LitLink_Liany\\ViewModel\\PRP\\DefaultUser.png", UriKind.RelativeOrAbsolute));
                    }
                }
                else
                {
                    this.ImgReaderProfile.Source = new BitmapImage(new Uri("C:\\Users\\yahal\\source\\repos\\Liany34\\LitLink_Liany\\ViewModel\\PRP\\DefaultUser.png", UriKind.RelativeOrAbsolute));
                }

                allBooks = await apiService.GetAllBooks();

                BuildUserLists();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading profile details: " + ex.Message);
            }
        }

        private async void BuildUserLists()
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabList);

            if (currentUser == null)
            {
                System.Diagnostics.Debug.WriteLine("BuildUserLists: currentUser is null!");
                return;
            }

            try
            {
                // 1. שליפת כל הסדרות/רשימות מה-API
                List<Book_Series> bookLists = await apiService.GetAllBookSeries();
                if (bookLists == null) bookLists = new List<Book_Series>();

                // 2. סינון הרשימות ששייכות למשתמש הנוכחי (הגנה מפני null והשוואת מזהים בצורה בטוחה)
                List<Book_Series> userCustomLists = bookLists
                    .Where(l => l.IdUser != null &&
                                l.IdUser.Id.ToString().Trim().Equals(currentUser.Id.ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Found {userCustomLists.Count} lists for user {currentUser.Username}");

                if (userCustomLists.Count == 0)
                {
                    ShowEmptyStateMessage("You haven't created any reading lists yet.");
                    return;
                }

                // 3. שליפת כל הספרים וכל פרטי הרשימות פעם אחת בלבד (מחוץ ללולאה!)
                if (allBooks == null || allBooks.Count == 0)
                {
                    allBooks = await apiService.GetAllBooks();
                    if (allBooks == null) allBooks = new List<Book>();
                }

                List<Series_Detail> allListDetails = await apiService.GetAllSeriesDetails();
                if (allListDetails == null) allListDetails = new List<Series_Detail>();

                // 4. רזולוציה והצגה של כל רשימה והספרים שבה
                foreach (var currentList in userCustomLists)
                {
                    // סינון הפרטים השייכים לרשימה הנוכחית
                    List<Series_Detail> currentListDetails = allListDetails
                        .Where(d => d.IdSeries != null && d.IdSeries.Id == currentList.Id)
                        .ToList();

                    // שליפת הספרים המתאימים מתוך רשימת כל הספרים
                    List<Book> relatedBooks = allBooks
                        .Where(b => currentListDetails.Any(d => d.IdBook != null && d.IdBook.Id == b.Id))
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"List '{currentList.NameSeries}' has {relatedBooks.Count} related books.");

                    // תצוגת הרשימה (גם אם היא ריקה כרגע, כדי שתדעי שהרשימה קיימת)
                    GenreUserControl listRow = new GenreUserControl();
                    listRow.SetupGenreRow(currentList.NameSeries, relatedBooks);
                    listRow.BookSelected += UserRow_BookSelected;

                    UserListsContainer.Children.Add(listRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error rendering user lists: " + ex.Message);
                MessageBox.Show("Error loading lists: " + ex.Message, "LitLink Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UserRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null || currentUser == null) return;

            try
            {
                List<Book> ownedBooks = new List<Book>();
                List<Book_Series> bookLists = await apiService.GetAllBookSeries();
                List<Book_Series> userCustomLists = bookLists.Where(l => l.IdUser != null && l.IdUser.Id == currentUser.Id).ToList();
                List<Series_Detail> allListDetails = await apiService.GetAllSeriesDetails();
                List<Series_Detail> userListDetails = allListDetails.Where(d => d.IdSeries != null && userCustomLists.Any(l => l.Id == d.IdSeries.Id)).ToList();

                foreach (var detail in userListDetails)
                {
                    if (detail.IdBook != null && !ownedBooks.Any(b => b.Id == detail.IdBook.Id))
                    {
                        ownedBooks.Add(detail.IdBook);
                    }
                }

                bool ownsBook = ownedBooks.Any(b => b.Id == selectedBook.Id);

                BookPage detailsPage = new BookPage(selectedBook, ownsBook, false, false);
                this.NavigationService?.Navigate(detailsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading book availability: " + ex.Message, "LitLink Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterList_Click(object sender, RoutedEventArgs e)
        {
            BuildUserLists();
        }

        // --- החלף את FilterReviews_Click ---
        private async void FilterReviews_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabReviews);

            if (currentUser == null) return;

            try
            {
                List<Reviews> allReviews = await apiService.GetAllReviews();
                List<Reviews> userReviews = allReviews
                    .Where(r => r != null && r.IdReader != null && r.IdReader.Id == currentUser.Id)
                    .ToList();

                if (userReviews.Count == 0)
                {
                    ShowEmptyStateMessage("You haven't written any reviews yet.");
                    return;
                }

                foreach (var review in userReviews)
                {
                    Border card = new Border
                    {
                        Background = Brushes.White,
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 10, 10)
                    };
                    StackPanel sp = new StackPanel();
                    sp.Children.Add(new TextBlock
                    {
                        Text = review.IdBook?.BookName ?? "Unknown Book",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(208, 106, 141))
                    });
                    sp.Children.Add(new TextBlock
                    {
                        Text = $"★ {review.Stars}/5",
                        FontSize = 13,
                        Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74)),
                        Margin = new Thickness(0, 4, 0, 4)
                    });
                    sp.Children.Add(new TextBlock
                    {
                        Text = review.Text,
                        FontSize = 13,
                        Foreground = Brushes.Gray,
                        TextWrapping = TextWrapping.Wrap
                    });
                    card.Child = sp;
                    UserListsContainer.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading reviews: " + ex.Message);
            }
        }

        // --- החלף את FilterMyBooks_Click ---
        private async void FilterMyBooks_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabMyBooks);

            if (currentUser == null) return;

            try
            {
                List<Cart> allCarts = await apiService.GetAllCarts();
                List<Cart> userCarts = allCarts
                    .Where(c => c.IdReader != null && c.IdReader.Id == currentUser.Id)
                    .ToList();

                List<Cart_Detail> allCartDetails = await apiService.GetAllCartDetails();
                List<Book> ownedBooks = allCartDetails
                    .Where(cd => cd.IsPurchased &&
                                 cd.IdCart != null &&
                                 userCarts.Any(c => c.Id == cd.IdCart.Id) &&
                                 cd.IdBook != null)
                    .Select(cd => cd.IdBook)
                    .GroupBy(b => b.Id)
                    .Select(g => g.First())
                    .ToList();

                if (ownedBooks.Count == 0)
                {
                    ShowEmptyStateMessage("You haven't purchased any books yet.");
                    return;
                }

                GenreUserControl purchasedRow = new GenreUserControl();
                purchasedRow.SetupGenreRow("My Purchased Library", ownedBooks);
                purchasedRow.BookSelected += UserRow_BookSelected;
                UserListsContainer.Children.Add(purchasedRow);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error filtering purchased books: " + ex.Message);
            }
        }

        // --- החלף את FilterFollowing_Click ---
        private async void FilterFollowing_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabFollowing);

            if (currentUser == null) return;

            try
            {
                List<Following> allFollowings = await apiService.GetAllFollowings();
                List<Following> userFollowings = allFollowings
                    .Where(f => f != null && f.IdReader != null && f.IdReader.Id == currentUser.Id)
                    .ToList();

                if (userFollowings.Count == 0)
                {
                    ShowEmptyStateMessage("You aren't following any authors yet.");
                    return;
                }

                foreach (var follow in userFollowings)
                {
                    Author a = follow.IdAuthor;
                    if (a == null) continue;

                    Border card = new Border
                    {
                        Background = Brushes.White,
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 10, 10),
                        Cursor = Cursors.Hand
                    };
                    StackPanel sp = new StackPanel();
                    sp.Children.Add(new TextBlock
                    {
                        Text = a.PenName ?? "Unknown Author",
                        FontSize = 15,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(208, 106, 141))
                    });
                    card.Child = sp;
                    UserListsContainer.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading following list: " + ex.Message);
            }
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e) => EditProfilePopup.Visibility = Visibility.Visible;
        private void CloseEditProfile_Click(object sender, RoutedEventArgs e) => EditProfilePopup.Visibility = Visibility.Collapsed;

        private void OutsidePopup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == EditProfilePopup) EditProfilePopup.Visibility = Visibility.Collapsed;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            HomePage homePage = new HomePage();
            homePage.DataContext = currentUser;
            Window.GetWindow(this).Content = homePage;
        }
        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            var cartPage = new CartPage(currentUser.Id);
            Window.GetWindow(this).Content = cartPage;
        }
        private void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            EditReaderProfileWindow newWindow = new EditReaderProfileWindow();

            newWindow.Show();

        }
        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var resetPass = new ResetPass();
            Window.GetWindow(this).Content = resetPass;
        }
        private void Preference_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Preferences layout option clicked!", "LitLink");
        private void Support_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Support option clicked! Connecting to help center...", "LitLink");

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentUser = null;
            var signOut = new SignOut();
            Window.GetWindow(this).Content = signOut;
        }

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;

            if (MessageBox.Show("Are you sure you want to permanently delete your LitLink account?\nThis action cannot be undone!",
                "Delete Account", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await apiService.DeleteUser(currentUser.Id);
                    currentUser = null;
                    MessageBox.Show("Your account has been deleted successfully.", "LitLink");
                    var signOut = new SignOut();
                    Window.GetWindow(this).Content = signOut;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete account: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void HighlightActiveTab(Button activeBtn)
        {
            BtnTabList.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabReviews.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabMyBooks.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabFollowing.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));

            activeBtn.Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private void ShowEmptyStateMessage(string message)
        {
            TextBlock txtEmpty = new TextBlock
            {
                Text = message,
                FontSize = 14,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            UserListsContainer.Children.Add(txtEmpty);
        }
    }
}

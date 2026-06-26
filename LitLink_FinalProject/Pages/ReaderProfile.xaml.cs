using LitLink_FinalProject.Pages;
using LitLink_FinalProject.UserControls;
using LitLink_FinalProject.WindowsFile;
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

namespace LitLink_FinalProject.Pages
{
    public partial class ReaderProfile : Page
    {
        private Apiservice apiService = new Apiservice();
        private Reader currentUser;

        private List<Book> allBooks = new List<Book>();
        private List<Book_Series> allSeries = new List<Book_Series>();
        private List<Series_Detail> allSeriesDetails = new List<Series_Detail>();
        private List<Cart_Detail> allCartDetails = new List<Cart_Detail>();
        private List<Cart> allCarts = new List<Cart>();
        private List<Reviews> allReviews = new List<Reviews>();
        private List<Following> allFollowings = new List<Following>();
        private bool dataLoaded = false;

        public ReaderProfile(Reader reader)
        {
            InitializeComponent();
            this.currentUser = reader;
            this.Loaded += ReaderProfilePage_Loaded;
        }

        private void ReaderProfilePage_Loaded(object sender, RoutedEventArgs e) => LoadAllData();

        private async void LoadAllData()
        {
            if (currentUser == null)
                return;

            try
            {
                TxtHelloUser.Text = $"Hello, {currentUser.Username}";

                try
                {
                    string st = await apiService.GetPictureByUserIDByte64(currentUser.Id);

                    if (!string.IsNullOrEmpty(st))
                    {
                        byte[] imgBytes = Convert.FromBase64String(st);
                        ImgReaderProfile.Source = ByteImageConverter.ByteToImage(imgBytes);
                    }
                    else
                    {
                        SetDefaultImage();
                    }
                }
                catch
                {
                    SetDefaultImage();
                }

                allBooks = await apiService.GetAllBooks() ?? new List<Book>();
                allSeries = await apiService.GetAllBookSeries() ?? new List<Book_Series>();
                allSeriesDetails = await apiService.GetAllSeriesDetails() ?? new List<Series_Detail>();
                allCarts = await apiService.GetAllCarts() ?? new List<Cart>();
                allCartDetails = await apiService.GetAllCartDetails() ?? new List<Cart_Detail>();
                allReviews = await apiService.GetAllReviews() ?? new List<Reviews>();
                allFollowings = await apiService.GetAllFollowings() ?? new List<Following>();

                dataLoaded = true;
                BuildUserLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading profile: " + ex.Message, "LitLink", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetDefaultImage()
        {
            try { ImgReaderProfile.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/UserPicture1.png", UriKind.Absolute)); }
            catch { ImgReaderProfile.Source = null; }
        }

        private void FilterList_Click(object sender, RoutedEventArgs e) => BuildUserLists();

        private void BuildUserLists()
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabList);
            ListActionBar.Visibility = Visibility.Visible;
            BtnAddBookToList.Visibility = allBooks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (!dataLoaded || currentUser == null) return;

            List<Book_Series> userSeries = allSeries
                .Where(s => s.IdUser != null && s.IdUser.Id == currentUser.Id)
                .ToList();

            if (userSeries.Count == 0)
            {
                ShowEmptyStateMessage("You haven't created any reading lists yet.");
                return;
            }

            foreach (var series in userSeries)
            {
                List<Book> seriesBooks = allSeriesDetails
                    .Where(d => d.IdSeries != null && d.IdSeries.Id == series.Id && d.IdBook != null)
                    .OrderBy(d => d.Number)
                    .Select(d => d.IdBook)
                    .ToList();

                GenreUserControl row = new GenreUserControl();
                row.SetupGenreRow(series.NameSeries, seriesBooks);
                row.BookSelected += UserRow_BookSelected;
                UserListsContainer.Children.Add(row);
            }
        }

        private void FilterReviews_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            ListActionBar.Visibility = Visibility.Collapsed;
            HighlightActiveTab(BtnTabReviews);
            if (!dataLoaded || currentUser == null) return;

            List<Reviews> userReviews = allReviews
                .Where(r => r != null && r.IdReader != null && r.IdReader.Id == currentUser.Id)
                .ToList();

            if (userReviews.Count == 0) { ShowEmptyStateMessage("You haven't written any reviews yet."); return; }

            foreach (var review in userReviews)
            {
                Border card = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(15), Margin = new Thickness(0, 0, 10, 10) };
                StackPanel sp = new StackPanel();
                sp.Children.Add(new TextBlock { Text = review.IdBook?.BookName ?? "Unknown Book", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(208, 106, 141)) });
                sp.Children.Add(new TextBlock { Text = $"★ {review.Stars}/5", FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74)), Margin = new Thickness(0, 4, 0, 4) });
                sp.Children.Add(new TextBlock { Text = review.Text, FontSize = 13, Foreground = Brushes.Gray, TextWrapping = TextWrapping.Wrap });
                card.Child = sp;
                UserListsContainer.Children.Add(card);
            }
        }

        private void FilterMyBooks_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            ListActionBar.Visibility = Visibility.Collapsed;
            HighlightActiveTab(BtnTabMyBooks);
            if (!dataLoaded || currentUser == null) return;

            List<Cart> userCarts = allCarts.Where(c => c.IdReader != null && c.IdReader.Id == currentUser.Id).ToList();
            List<Book> ownedBooks = allCartDetails
                .Where(cd => cd.IsPurchased && cd.IdCart != null && userCarts.Any(c => c.Id == cd.IdCart.Id) && cd.IdBook != null)
                .Select(cd => cd.IdBook)
                .GroupBy(b => b.Id).Select(g => g.First()).ToList();

            if (ownedBooks.Count == 0) { ShowEmptyStateMessage("You haven't purchased any books yet."); return; }

            GenreUserControl row = new GenreUserControl();
            row.SetupGenreRow("My Purchased Library", ownedBooks);
            row.BookSelected += UserRow_BookSelected;
            UserListsContainer.Children.Add(row);
        }

        private void FilterFollowing_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            ListActionBar.Visibility = Visibility.Collapsed;
            HighlightActiveTab(BtnTabFollowing);
            if (!dataLoaded || currentUser == null) return;

            List<Following> userFollowings = allFollowings
                .Where(f => f != null && f.IdReader != null && f.IdReader.Id == currentUser.Id)
                .ToList();

            if (userFollowings.Count == 0) { ShowEmptyStateMessage("You aren't following any authors yet."); return; }

            foreach (var follow in userFollowings)
            {
                Author a = follow.IdAuthor;
                if (a == null) continue;

                Border card = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(15), Margin = new Thickness(0, 0, 10, 10), Cursor = Cursors.Hand };
                StackPanel sp = new StackPanel();
                sp.Children.Add(new TextBlock { Text = a.PenName ?? "Unknown Author", FontSize = 15, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(208, 106, 141)) });
                card.MouseDown += (s, ev) => { Window.GetWindow(this).Content = new AuthorProfile(a, currentUser); };
                card.Child = sp;
                UserListsContainer.Children.Add(card);
            }
        }

        private void UserRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null || currentUser == null) return;
            List<Cart> userCarts = allCarts.Where(c => c.IdReader != null && c.IdReader.Id == currentUser.Id).ToList();
            bool ownsBook = allCartDetails.Any(cd => cd.IsPurchased && cd.IdCart != null &&
                userCarts.Any(c => c.Id == cd.IdCart.Id) && cd.IdBook != null && cd.IdBook.Id == selectedBook.Id);
            this.NavigationService?.Navigate(new BookPage(selectedBook, ownsBook, false, false, currentUser));
        }

        private async void AddList_Click(object sender, RoutedEventArgs e)
        {
            AddSeriesWindow win = new AddSeriesWindow(currentUser);
            if (win.ShowDialog() == true)
            {
                allSeries = await apiService.GetAllBookSeries() ?? new List<Book_Series>();
                allSeriesDetails = await apiService.GetAllSeriesDetails() ?? new List<Series_Detail>();
                BuildUserLists();
            }
        }

        private async void AddBookToList_Click(object sender, RoutedEventArgs e)
        {
            List<Book_Series> userSeries = allSeries.Where(s => s.IdUser != null && s.IdUser.Id == currentUser.Id).ToList();
            if (userSeries.Count == 0) { MessageBox.Show("Create a list first.", "LitLink"); return; }

            AddBookToSeriesWindow win = new AddBookToSeriesWindow(currentUser.Id, userSeries, allBooks);
            if (win.ShowDialog() == true)
            {
                allSeriesDetails = await apiService.GetAllSeriesDetails() ?? new List<Series_Detail>();
                BuildUserLists();
            }
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e) => EditProfilePopup.Visibility = Visibility.Visible;
        private void CloseEditProfile_Click(object sender, RoutedEventArgs e) => EditProfilePopup.Visibility = Visibility.Collapsed;
        private void OutsidePopup_MouseDown(object sender, MouseButtonEventArgs e) { if (e.OriginalSource == EditProfilePopup) EditProfilePopup.Visibility = Visibility.Collapsed; }

        private void HighlightActiveTab(Button activeBtn)
        {
            BtnTabList.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabReviews.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabMyBooks.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabFollowing.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            activeBtn.Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private void ShowEmptyStateMessage(string message) =>
            UserListsContainer.Children.Add(new TextBlock { Text = message, FontSize = 14, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0) });

        private void Home_Click(object sender, RoutedEventArgs e) 
        {
            MainWindow.AppFrame.Navigate(new HomePage(currentUser));
        }
        private void Cart_Click(object sender, RoutedEventArgs e) 
        {
            MainWindow.AppFrame.Navigate(new CartPage(currentUser)); 
        }
        private void LogOut_Click(object sender, RoutedEventArgs e) 
        {
            currentUser = null; 
            MainWindow.AppFrame.Navigate(new SignOut()); 
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
                    MainWindow.AppFrame.Navigate(new SignOut());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete account: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void EditDetails_Click(object sender, RoutedEventArgs e)
        {
            EditReaderProfileWindow win = new EditReaderProfileWindow(currentUser);
            if (win.ShowDialog() == true)
            {
                // שליפת המשתמש המעודכן מחדש מה-API
                List<Reader> allReaders = await apiService.GetAllReaders();
                Reader updatedReader = allReaders.FirstOrDefault(r => r.Id == currentUser.Id);
                if (updatedReader != null)
                    currentUser = updatedReader;

                LoadAllData();
            }
        }
    }
}

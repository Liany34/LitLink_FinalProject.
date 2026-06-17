using LitLink_FinalProject.Pages;
using LitLink_FinalProject.UserControls;
using Model;
using Service;
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

namespace LitLink_FinalProject.Pages
{
    public partial class HomePage : Page
    {
        private Apiservice apiService = new Apiservice();
        private Reader currentUser;
        private bool isCatalogBuilt = false;

        public HomePage(Reader loggedInUser)
        {
            InitializeComponent();

            this.currentUser = loggedInUser;

            CheckUserSession();

            this.Loaded += HomePage_Loaded;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            _ = BuildDynamicCatalogAsync();
        }

        private async Task BuildDynamicCatalogAsync()
        {
            try
            {
                isCatalogBuilt = true;
                await Task.Delay(50);

                List<Genre> allGenres = await apiService.GetAllGenres() ?? new List<Genre>();
                List<Book_Genre> allBookGenres = await apiService.GetAllBookGenres() ?? new List<Book_Genre>();

                DynamicGenresContainer.Children.Clear();

                foreach (Genre currentGenre in allGenres)
                {
                    List<Book> relatedBooks = allBookGenres
                        .Where(b => b.IdGenre != null && b.IdGenre.Id == currentGenre.Id)
                        .Select(b => b.IdBook)
                        .ToList();

                    if (relatedBooks.Count == 0) continue;

                    GenreUserControl genreRow = new GenreUserControl();
                    genreRow.SetupGenreRow(currentGenre.Name, relatedBooks);
                    genreRow.BookSelected += GenreRow_BookSelected;
                    DynamicGenresContainer.Children.Add(genreRow);
                }

                List<News> allNews = await apiService.GetAllNews() ?? new List<News>();
                NewsListBox.ItemsSource = allNews;

                UpdateUserUI();
            }
            catch (Exception ex)
            {
                isCatalogBuilt = false;
                System.Diagnostics.Debug.WriteLine("Error building dynamic catalog: " + ex.Message);
            }
        }

        private async void UpdateUserUI()
        {
            if (currentUser != null)
            {
                UserPanel.Visibility = Visibility.Visible;
                TxtUsername.Text = currentUser.Username;
                MenuSeparator.Visibility = Visibility.Visible;
                CartItem.Visibility = Visibility.Visible;
                ProfileItem.Visibility = Visibility.Visible;
                LogOutItem.Visibility = Visibility.Visible;

                try
                {
                    string st = await apiService.GetPictureByUserIDByte64(currentUser.Id);
                    if (!string.IsNullOrEmpty(st))
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.ImgProfile.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    else SetDefaultProfilePicture();
                }
                catch (Exception imgEx)
                {
                    System.Diagnostics.Debug.WriteLine("Image loading failed: " + imgEx.Message);
                    SetDefaultProfilePicture();
                }

                try
                {
                    List<Author> allAuthors = await apiService.GetAllAuthors();
                    if (allAuthors.Any(a => a.Id == currentUser.Id))
                        BecomeAuthorItem.Header = "Author Dashboard";
                    else
                        BecomeAuthorItem.Header = "Become Author";
                    BecomeAuthorItem.Visibility = Visibility.Visible;
                }
                catch (Exception authorEx)
                {
                    System.Diagnostics.Debug.WriteLine("Author check failed: " + authorEx.Message);
                }
            }
            else
            {
                UserPanel.Visibility = Visibility.Collapsed;
                MenuSeparator.Visibility = Visibility.Collapsed;
                CartItem.Visibility = Visibility.Collapsed;
                ProfileItem.Visibility = Visibility.Collapsed;
                BecomeAuthorItem.Visibility = Visibility.Collapsed;
                LogOutItem.Visibility = Visibility.Collapsed;
            }
        }

        private void SetDefaultProfilePicture()
        {
            try
            {
                this.ImgProfile.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/UserPicture1.png", UriKind.Absolute));
            }
            catch { this.ImgProfile.Source = null; }
        }

        private async void GenreRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null) return;

            List<Book> ownedBooks = new List<Book>();

            if (currentUser != null)
            {
                List<Cart> allCarts = await apiService.GetAllCarts();
                List<Cart> cartUser = allCarts.Where(c => c.IdReader != null && c.IdReader.Id == currentUser.Id).ToList();
                List<Cart_Detail> bookDetailsList = await apiService.GetAllCartDetails();
                foreach (Cart_Detail detail in bookDetailsList.Where(cd => cd.IdCart != null && cartUser.Any(c => c.Id == cd.IdCart.Id)))
                {
                    if (detail.IsPurchased == true) ownedBooks.Add(detail.IdBook);
                }
            }

            List<Admin> allAdmins = await apiService.GetAllAdmins();
            List<Author> allAuthors = await apiService.GetAllAuthors();

            bool ownsBook = currentUser != null && ownedBooks.Any(b => b.Id == selectedBook.Id);
            bool isAdmin = currentUser != null && allAdmins.Any(a => a.Id == currentUser.Id);
            bool isAuthor = currentUser != null && allAuthors.Any(a => a.Id == currentUser.Id);

            BookPage detailsPage = new BookPage(selectedBook, ownsBook, isAdmin, isAuthor, currentUser);
            this.NavigationService?.Navigate(detailsPage);
        }

        private void CheckUserSession()
        {
            int hour = DateTime.Now.Hour;
            if (hour >= 5 && hour < 12) TxtGreeting.Text = "Good Morning,";
            else if (hour >= 12 && hour < 17) TxtGreeting.Text = "Good Noon,";
            else if (hour >= 17 && hour < 21) TxtGreeting.Text = "Good Evening,";
            else TxtGreeting.Text = "Good Night,";
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query) || query == "Search books or authors...")
            {
                MessageBox.Show("Please enter a book name or author name to search.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MainWindow.AppFrame.Navigate(new SearchResultsPage(query, currentUser));
        }

        private void NewsListBox_Refresh()
        {
            isCatalogBuilt = false;
            _ = BuildDynamicCatalogAsync();
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e) { MainMenu.PlacementTarget = sender as Button; MainMenu.IsOpen = true; }
        private void AboutUs_Click(object sender, RoutedEventArgs e) { MainWindow.AppFrame.Navigate(new AboutUs(currentUser)); }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;
            MainWindow.AppFrame.Navigate(new CartPage(currentUser));
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("No reader is logged in.", "LitLink");
                return;
            }

            MainWindow.AppFrame.Navigate(new ReaderProfile(currentUser));
        }

        private async void BecomeAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;
            try
            {
                
               var becomeAuthorPage = new BecomeAuthorPage();
               becomeAuthorPage.DataContext = currentUser;
               MainWindow.AppFrame.Navigate(becomeAuthorPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "LitLink", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentUser = null;
            MainWindow.AppFrame.Navigate(new SignOut());
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e) { if (TxtSearch.Text == "Search books or authors...") { TxtSearch.Text = ""; TxtSearch.Foreground = Brushes.Black; } }
        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(TxtSearch.Text)) { TxtSearch.Text = "Search books or authors..."; TxtSearch.Foreground = Brushes.Gray; } }
    }
}
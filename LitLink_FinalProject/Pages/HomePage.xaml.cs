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
using LitLink_FinalProject.UserControls;

namespace LitLink_FinalProject.Pages
{
    public partial class HomePage : Page
    {
        private Apiservice apiService = new Apiservice();
        private Reader currentUser;
        private bool isCatalogBuilt = false;

        public HomePage()
        {
            InitializeComponent();
            CheckUserSession();

            this.Loaded += HomePage_Loaded;

            this.DataContextChanged += HomePage_DataContextChanged;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isCatalogBuilt)
            {
                BuildDynamicCatalog();
            }
        }

        private void HomePage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is Reader user)
            {
                currentUser = user;
            }
            else
            {
                currentUser = null;
            }
            UpdateUserUI();

            // בנה מחדש את הקטלוג אם כבר נטען (כדי שהחיפוש ויתר פעולות יעבדו עם המשתמש החדש)
            if (isCatalogBuilt)
            {
                isCatalogBuilt = false;
                BuildDynamicCatalog();
            }
        }

        private async void BuildDynamicCatalog()
        {
            try
            {
                isCatalogBuilt = true;
                List<Genre> allGenres = await apiService.GetAllGenres();
                List<Book_Genre> allBookGenres = await apiService.GetAllBookGenres();

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

                List<News> allNews = await apiService.GetAllNews();
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
                GuestPanel.Visibility = Visibility.Collapsed;
                UserPanel.Visibility = Visibility.Visible;
                TxtUsername.Text = currentUser.Username;

                MenuSeparator.Visibility = Visibility.Visible;
                CartItem.Visibility = Visibility.Visible;
                ProfileItem.Visibility = Visibility.Visible;
                LogOutItem.Visibility = Visibility.Visible;

                try
                {
                    string st = await apiService.GetPRPByUserIDByte64(currentUser.Id);
                    if (!string.IsNullOrEmpty(st))
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.ImgProfile.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    else
                    {
                        SetDefaultProfilePicture();
                    }
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
                    {
                        BecomeAuthorItem.Header = "Author Dashboard";
                    }
                    else
                    {
                        BecomeAuthorItem.Header = "Become Author";
                    }
                    BecomeAuthorItem.Visibility = Visibility.Visible;
                }
                catch (Exception authorEx)
                {
                    System.Diagnostics.Debug.WriteLine("Author check failed: " + authorEx.Message);
                }
            }
            else
            {
                GuestPanel.Visibility = Visibility.Visible;
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
                this.ImgProfile.Source = new BitmapImage(new Uri("C:\\Users\\yahal\\source\\repos\\Liany34\\LitLink_Liany\\ViewModel\\PRP\\DefaultUser.png", UriKind.RelativeOrAbsolute));
            }
            catch
            {
                this.ImgProfile.Source = null;
            }
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
                List<Cart_Detail> bookDetailsUser = bookDetailsList.Where(cd => cd.IdCart != null && cartUser.Any(c => c.Id == cd.IdCart.Id)).ToList();

                foreach (Cart_Detail detail in bookDetailsUser)
                {
                    if (detail.IsPurchased == true)
                    {
                        ownedBooks.Add(detail.IdBook);
                    }
                }
            }

            List<Admin> allAdmins = await apiService.GetAllAdmins();
            List<Author> allAuthors = await apiService.GetAllAuthors();

            bool ownsBook = currentUser != null && ownedBooks.Any(b => b.Id == selectedBook.Id);
            bool isAdmin = currentUser != null && allAdmins.Any(a => a.Id == currentUser.Id);
            bool isAuthor = currentUser != null && allAuthors.Any(a => a.Id == currentUser.Id);

            BookPage detailsPage = new BookPage(selectedBook, ownsBook, isAdmin, isAuthor);
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

            SearchResultsPage resultsPage = new SearchResultsPage(query, currentUser);
            this.NavigationService?.Navigate(resultsPage);
        }

        private void NewsListBox_Refresh()
        {
            BuildDynamicCatalog();
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e) { MainMenu.PlacementTarget = sender as Button; MainMenu.IsOpen = true; }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = new Login();
            Window.GetWindow(this).Content = login;
        }

        private void AboutUs_Click(object sender, RoutedEventArgs e)
        {
            var aboutUs = new AboutUs(currentUser);
            Window.GetWindow(this).Content = aboutUs;
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;
            var cart = new CartPage(currentUser.Id);
            Window.GetWindow(this).Content = cart;
        }

        private async void Profile_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;

            List<Reader> allReaders = await apiService.GetAllReaders();
            if (allReaders.Any(a => a.Id == currentUser.Id))
            {
                // העברת ה-currentUser בתוך הסוגריים!
                var readerProfile = new ReaderProfile(currentUser);
                Window.GetWindow(this).Content = readerProfile;
            }
            else
            {
                MessageBox.Show("You are not authorized to go there.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BecomeAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null) return;

            try
            {
                List<Author> allAuthors = await apiService.GetAllAuthors();
                Author existingAuthor = allAuthors.FirstOrDefault(a => a.Id == currentUser.Id);

                if (existingAuthor != null)
                {
                    var authorProfile = new AuthorProfile(existingAuthor);
                    Window.GetWindow(this).Content = authorProfile;
                }
                else
                {
                    var becomeAuthor = new BecomeAuthorPage();
                    Window.GetWindow(this).Content = becomeAuthor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "LitLink", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentUser = null;
            var signOut = new SignOut();
            Window.GetWindow(this).Content = signOut;
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e) { if (TxtSearch.Text == "Search books or authors...") { TxtSearch.Text = ""; TxtSearch.Foreground = Brushes.Black; } }
        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(TxtSearch.Text)) { TxtSearch.Text = "Search books or authors..."; TxtSearch.Foreground = Brushes.Gray; } }
    }
}
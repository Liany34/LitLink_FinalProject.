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
        private User currentUser;

        public HomePage()
        {
            InitializeComponent();
            CheckUserSession();
            BuildDynamicCatalog();
            currentUser = this.DataContext as User;
        }

        private async void BuildDynamicCatalog()
        {
            try
            {
                List<Genre> allGenres = await apiService.GetAllGenres();
                List<Book_Genre> allBookGenres = await apiService.GetAllBookGenres();

                DynamicGenresContainer.Children.Clear();

                foreach (Genre currentGenre in allGenres)
                {
                    List<Book> relatedBooks = allBookGenres.Where(b => b.IdGenre.Id == currentGenre.Id).Select(b => b.IdBook).ToList();

                    if (relatedBooks.Count == 0) continue;

                    GenreUserControl genreRow = new GenreUserControl();

                    genreRow.SetupGenreRow(currentGenre.Name, relatedBooks);

                    genreRow.BookSelected += GenreRow_BookSelected;

                    DynamicGenresContainer.Children.Add(genreRow);
                }

                List<News> allNews = await apiService.GetAllNews();
                NewsListBox.ItemsSource = allNews;

                if (currentUser != null)
                {
                    GuestPanel.Visibility = Visibility.Collapsed;
                    UserPanel.Visibility = Visibility.Visible;
                    TxtUsername.Text = currentUser.Username;

                    if (!string.IsNullOrEmpty(currentUser.Picture))
                    {
                        try
                        {
                            byte[] imgStr = Convert.FromBase64String(currentUser.Picture);
                            this.ImgProfile.Source = ByteImageConverter.ByteToImage(imgStr);
                        }
                        catch
                        {
                            this.ImgProfile.Source = new BitmapImage(new Uri("pack://application:,,,/PRP/DefultUser.png", UriKind.RelativeOrAbsolute));
                        }
                    }
                    else
                    {
                        this.ImgProfile.Source = new BitmapImage(new Uri("pack://application:,,,/PRP/DefultUser.png", UriKind.RelativeOrAbsolute));
                    }

                    MenuSeparator.Visibility = Visibility.Visible;
                    CartItem.Visibility = Visibility.Visible;
                    ProfileItem.Visibility = Visibility.Visible;
                    LogOutItem.Visibility = Visibility.Visible;

                    List<Author> allAuthors = await apiService.GetAllAuthors();
                    if (allAuthors.Any(a => a.Id == currentUser.Id))
                    {
                        BecomeAuthorItem.Header = "Author Dashboard";
                    }
                    BecomeAuthorItem.Visibility = Visibility.Visible;
                }
                else
                {
                    GuestPanel.Visibility = Visibility.Visible;
                    UserPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error building dynamic catalog: " + ex.Message);
            }
        }

        private async void GenreRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null) return;

            List<Book> ownedBooks = new List<Book>();

            if (currentUser != null)
            {
                List<Cart> allCarts = await apiService.GetAllCarts();
                List<Cart> cartUser = allCarts.Where(c => c.IdReader.Id == currentUser.Id).ToList();
                List<Cart_Detail> bookDetailsList = await apiService.GetAllCartDetails();
                List<Cart_Detail> bookDetailsUser = bookDetailsList.Where(cd => cartUser.Any(c => c.Id == cd.IdCart.Id)).ToList();
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

            SearchResultsPage resultsPage = new SearchResultsPage();
            resultsPage.DataContext = query;
            this.NavigationService?.Navigate(resultsPage);
        }

        private void NewsListBox_Refresh()
        {
            BuildDynamicCatalog();
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e) { MainMenu.PlacementTarget = sender as Button; MainMenu.IsOpen = true; }
        private void BtnLogin_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/Login.xaml", UriKind.Relative));
        private void AboutUs_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/AboutUs.xaml", UriKind.Relative));
        private void Cart_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/CartPage.xaml", UriKind.Relative));

        private async void Profile_Click(object sender, RoutedEventArgs e)
        {
            List<Admin> allAdmins = await apiService.GetAllAdmins();
            if (allAdmins.Any(a => a.Id == currentUser.Id))
            {
                this.NavigationService?.Navigate(new Uri("Pages/AdminProfile.xaml", UriKind.Relative));
            }
            else
            {
                this.NavigationService?.Navigate(new Uri("Pages/ReaderProfile.xaml", UriKind.Relative));
            }
        }

        private async void BecomeAuthor_Click(object sender, RoutedEventArgs e)
        {
            List<Author> allAuthors = await apiService.GetAllAuthors();
            if (allAuthors.Any(a => a.Id == currentUser.Id))
            {
                this.NavigationService?.Navigate(new Uri("Pages/AuthorProfile.xaml", UriKind.Relative));
            }
            else
            {
                this.NavigationService?.Navigate(new Uri("Pages/BecomeAuthor.xaml", UriKind.Relative));
            }
        }

        private void LogOut_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/SignOut.xaml", UriKind.Relative));

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e) { if (TxtSearch.Text == "Search books or authors...") { TxtSearch.Text = ""; TxtSearch.Foreground = Brushes.Black; } }
        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(TxtSearch.Text)) { TxtSearch.Text = "Search books or authors..."; TxtSearch.Foreground = Brushes.Gray; } }
    }
}
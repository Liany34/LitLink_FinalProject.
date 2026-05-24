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
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private Apiservice _apiService = new Apiservice();
        private User currentUser;

        public HomePage()
        {
            InitializeComponent();
            CheckUserSession();
            BuildDynamicCatalog(); // טעינת הספרים והחדשות מיד עם פתיחת המסך
            currentUser = this.DataContext as User;
        }

        /// <summary>
        /// קריאה לטבלאות מה-Access ובנייה דינמית של שורות הז'אנרים והחדשות על המסך
        /// </summary>
        private async void BuildDynamicCatalog()
        {
            try
            {
                // 1. שליפת כל הז'אנרים וכל הספרים מטבלאות ה-Access דרך ה-ApiService שלך
                List<Genre> allGenres = await _apiService.GetAllGenres();
                List<Book_Genre> allBookGenres = await _apiService.GetAllBookGenres();

                // ניקוי הקונטיינר למקרה של רענון
                DynamicGenresContainer.Children.Clear();

                // 2. לולאה שרצה על כל קטגוריית ז'אנר שחזרה מה-Access
                foreach (Genre currentGenre in allGenres)
                {
                    // סינון הספרים השייכים אך ורק לז'אנר הנוכחי
                    List<Book> relatedBooks = allBookGenres.Where(b => b.IdGenre.Id == currentGenre.Id).Select(b => b.IdBook).ToList();

                    // אם אין ספרים בז'אנר הזה, נדלג עליו כדי שלא יופיע שורה ריקה
                    if (relatedBooks.Count == 0) continue;

                    // 3. יצירת מופע חדש של ה-UserControl של השורה
                    GenreUserControl genreRow = new GenreUserControl();

                    // הזרקת הנתונים (שם הז'אנר ורשימת הספרים שלו) לתוך השורה
                    genreRow.SetupGenreRow(currentGenre.Name, relatedBooks);

                    // 4. הרשמה לאירוע הלחיצה על ספר בשורה הזו כדי לפתוח את דף הפירוט
                    genreRow.BookSelected += GenreRow_BookSelected;

                    // 5. הוספת השורה השלמה לתוך ה-StackPanel המרכזי בעמוד הבית
                    DynamicGenresContainer.Children.Add(genreRow);
                }

                // 🌟 עדכון: שליפת החדשות האמיתיות מטבלת ה-Access והזרקתן ל-ListBox
                List<News> allNews = await _apiService.GetAllNews(); // ודאי שזה שם הפעולה ב-ApiService
                NewsListBox.ItemsSource = allNews;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error building dynamic catalog: " + ex.Message);
            }
        }

        // ברגע שנבחר ספר מאחת השורות, ננווט לעמוד הפירוט המלא
        private async void GenreRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null) return;

            List<Cart> allCarts = await _apiService.GetAllCarts();
            List<Cart> cartUser = allCarts.Where(c => c.IdReader.Id == currentUser.Id).ToList();
            List<Cart_Detail> bookDetailsList = await _apiService.GetAllCartDetails();
            List<Cart_Detail> bookDetailsUser = bookDetailsList.Where(cd => cartUser.Any(c => c.Id == cd.IdCart.Id)).ToList();
            List<Book> ownedBooks = new List<Book>();
            foreach (Cart_Detail detail in bookDetailsUser)
            {
                if(detail.IsPurchased == true)
                {
                    ownedBooks.Add(detail.IdBook);
                }
            }
            List<Admin> allAdmins = await _apiService.GetAllAdmins();
            List<Author> allAuthors = await _apiService.GetAllAuthors();

            bool ownsBook = currentUser != null && ownedBooks != null && ownedBooks.Contains(selectedBook);
            bool isAdmin = currentUser != null && allAdmins.Contains(currentUser);
            bool isAuthor = currentUser != null && allAuthors.Contains(currentUser);

            // ניווט לעמוד הפירוט המלא של הספר
            BookPage detailsPage = new BookPage(selectedBook, ownsBook, isAdmin, isAuthor);
            this.NavigationService?.Navigate(detailsPage);
        }

        private void CheckUserSession()
        {
            // לוגיקת ברכת המשתמש לפי שעות היום
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
            resultsPage.ExecuteSearch(query);
            this.NavigationService?.Navigate(resultsPage);
        }

        // 🌟 תיקון השגיאה: החלפת הפונקציה הלא קיימת בקריאה ל-BuildDynamicCatalog() שמחדשת את כל הדף
        private void NewsListBox_Refresh()
        {
            BuildDynamicCatalog();
        }

        // פעולות ניווט מהירות דרך התפריטים
        private void MenuBtn_Click(object sender, RoutedEventArgs e) { MainMenu.PlacementTarget = sender as Button; MainMenu.IsOpen = true; }
        private void BtnLogin_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/Login.xaml", UriKind.Relative));
        private void AboutUs_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/AboutUs.xaml", UriKind.Relative));
        private void Cart_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/CartPage.xaml", UriKind.Relative));
        private void Profile_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/ReaderProfile.xaml", UriKind.Relative));
        private void BecomeAuthor_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/BecomeAuthor.xaml", UriKind.Relative));
        private void LogOut_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/SignOut.xaml", UriKind.Relative));

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e) { if (TxtSearch.Text == "Search books or authors...") { TxtSearch.Text = ""; TxtSearch.Foreground = Brushes.Black; } }
        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(TxtSearch.Text)) { TxtSearch.Text = "Search books or authors..."; TxtSearch.Foreground = Brushes.Gray; } }
    }
}

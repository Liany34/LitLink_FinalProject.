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
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private Apiservice _apiService = new Apiservice();
        private bool _isUserLoggedIn = false; // משתנה זמני המדמה סטטוס התחברות

        public HomePage()
        {
            InitializeComponent();
            CheckUserSession();
            LoadCatalogAndNews();
        }

        // 1. בדיקת סטטוס המשתמש ועדכון הברכה לפי השעה ביום
        private void CheckUserSession()
        {
            // בדיקה האם יש משתמש מחובר במערכת (נניח דרך מחלקה גלובלית App)
            // if (App.CurrentUser != null) { _isUserLoggedIn = true; }

            if (_isUserLoggedIn)
            {
                GuestPanel.Visibility = Visibility.Collapsed;
                UserPanel.Visibility = Visibility.Visible;

                // עדכון הקישורים בתפריט ההמבורגר
                MenuSeparator.Visibility = Visibility.Visible;
                CartItem.Visibility = Visibility.Visible;
                ProfileItem.Visibility = Visibility.Visible;
                BecomeAuthorItem.Visibility = Visibility.Visible;
                LogOutItem.Visibility = Visibility.Visible;

                // קביעת הברכה הדינמית לפי השעה הנוכחית ביום
                int hour = DateTime.Now.Hour;
                if (hour >= 5 && hour < 12) TxtGreeting.Text = "Good Morning,";
                else if (hour >= 12 && hour < 17) TxtGreeting.Text = "Good Noon,";
                else if (hour >= 17 && hour < 21) TxtGreeting.Text = "Good Evening,";
                else TxtGreeting.Text = "Good Night,";

                // TxtUsername.Text = App.CurrentUser.Username;
                TxtUsername.Text = "ReaderName";
            }
            else
            {
                GuestPanel.Visibility = Visibility.Visible;
                UserPanel.Visibility = Visibility.Collapsed;
            }
        }

        // 2. טעינת הספרים לקטגוריות והחדשות מה-API
        private async void LoadCatalogAndNews()
        {
            try
            {
                // קבלת נתונים מהבסיס נתונים/API
                // List<Book> allBooks = await _apiService.GetAllBooks();
                // RomanceItemsControl.ItemsSource = allBooks.Where(b => b.Genre == "Romance");
                // FantasyItemsControl.ItemsSource = allBooks.Where(b => b.Genre == "Fantasy");

                // טעינת חדשות סופרים
                // NewsListBox.ItemsSource = await _apiService.GetAllAuthorNews();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading initial homepage data: " + ex.Message);
            }
        }

        // 3. ניווט לדף פרטי הספר בלחיצה על ספר מהקטלוג
        private void Book_Click(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            Book clickedBook = element?.DataContext as Book;

            if (clickedBook == null) return;

            // בדיקת הרשאות המשתמש הנוכחי
            bool ownsBook = false;
            bool isAdmin = false;
            bool isAuthor = false;

            // מעבר לעמוד הפירוט המלא שיצרנו בשלב הקודם
            BookPage detailsPage = new BookPage(clickedBook, ownsBook, isAdmin, isAuthor);
            this.NavigationService?.Navigate(detailsPage);
        }

        // 4. לוגיקת החיפוש הדינמי (ספרים או סופרים)
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtSearch.Text.Trim();
            if (string.IsNullOrEmpty(query) || query == "Search books or authors...") return;

            // כאן את יכולה להעביר את הטקסט לעמוד תוצאות חיפוש או לסנן את ה-ItemsSource הקיים
            MessageBox.Show($"Searching for: {query}", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // תפריט המבורגר (3 קווים)
        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.PlacementTarget = sender as Button;
            MainMenu.IsOpen = true;
        }

        // מעבר לעמודים השונים
        private void BtnLogin_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/Login.xaml", UriKind.Relative));
        private void AboutUs_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/AboutUs.xaml", UriKind.Relative));
        private void Cart_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/CartPage.xaml", UriKind.Relative));
        private void Profile_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/ReaderProfile.xaml", UriKind.Relative));
        private void BecomeAuthor_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/BecomeAuthorPage.xaml", UriKind.Relative));
        private void LogOut_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/SingOut.xaml", UriKind.Relative));

        // ניהול טקסט פלייסהולדר בתיבת החיפוש
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtSearch.Text == "Search books or authors...") { TxtSearch.Text = ""; TxtSearch.Foreground = Brushes.Black; }
        }
        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSearch.Text)) { TxtSearch.Text = "Search books or authors..."; TxtSearch.Foreground = Brushes.Gray; }
        }
    }
}

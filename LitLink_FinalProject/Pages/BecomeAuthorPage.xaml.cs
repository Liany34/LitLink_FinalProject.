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
    /// Interaction logic for BecomeAuthorPage.xaml
    /// </summary>
    public partial class BecomeAuthorPage : Page
    {
        private Apiservice _apiService = new Apiservice();
        private User currentUser;

        public BecomeAuthorPage()
        {
            InitializeComponent();
            this.Loaded += BecomeAuthorPage_Loaded;
            currentUser = this.DataContext as User; 
        }

        private void BecomeAuthorPage_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUserPermissionsAndLoadGenres();
        }

        /// <summary>
        /// בדיקה שהמשתמש אינו סופר או מנהל, וטעינת הז'אנרים מתוך בסיס הנתונים (Access)
        /// </summary>
        private async void CheckUserPermissionsAndLoadGenres()
        {
            // 1. הגנה מפני משתמש לא מחובר
            if (currentUser == null)
            {
                MessageBox.Show("Please log in to access this page.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
                return;
            }

            // 2. בדיקה: האם המשתמש כבר מוגדר כסופר או כמנהל מערכת?
            List<Admin> admins = await _apiService.GetAllAdmins();
            List<Author> authors = await _apiService.GetAllAuthors();
            if (admins.Contains(currentUser) || authors.Contains(currentUser))
            {
                MessageBox.Show("Authors and Administrators cannot create a new author profile.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Stop);

                // החזרה אוטומטית לעמוד הבית כדי למנוע מעקף של הטופס
                if (this.NavigationService.CanGoBack)
                    this.NavigationService.GoBack();
                else
                    this.NavigationService?.Navigate(new Uri("Pages/HomePage.xaml", UriKind.Relative));
                return;
            }

            try
            {
                // 3. שליפת רשימת הז'אנרים הקיימים באתר ישירות מה-Database
                // מניח שיש לך מחלקה בשם Genre ומתודה מתאימה ב-ApiService
                List<Genre> availableGenres = await _apiService.GetAllGenres();
                CmbGenres.ItemsSource = availableGenres;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dynamic site genres: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// לחיצה על כפתור ההפיכה לסופר - מעדכן את ה-Access ומעביר את האתר לתצוגת סופר
        /// </summary>
        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // ולידציה של השדות
            string penName = TxtPenName.Text.Trim();
            string bio = TxtBio.Text.Trim();
            Genre selectedGenre = CmbGenres.SelectedItem as Genre;

            if (string.IsNullOrEmpty(penName) || string.IsNullOrEmpty(bio) || selectedGenre == null)
            {
                MessageBox.Show("Please fill out all fields and select a primary genre 🌸", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. יצירת אובייקט סופר חדש המקושר ל-ID של המשתמש הנוכחי
                Author newAuthor = new Author
                {
                    PenName = penName,
                    InformationAboutAuthor = bio,
                    Genre = selectedGenre
                };

                // 2. שמירת הנתונים בבסיס הנתונים (Access) ועדכון שדה ה-IsAuthor של המשתמש ל-true
                await _apiService.InsertAuthor(newAuthor);
                bool success;
                List<Author> authors = await _apiService.GetAllAuthors();
                if(authors.Contains(newAuthor))
                {
                    success = true;
                }
                else
                {
                    success = false;
                }

                if (success)
                {
                    // 3. עדכון ה-Session הגלובלי בזמן אמת - האתר משתנה כעת לתצוגת סופר
                    MessageBox.Show($"Congratulations ✨\nYou are now officially a registered LitLink Author! Welcome, {penName}.",
                        "LitLink Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 4. ניווט מיידי לעמוד הפרופיל של הסופר החדש (למשל AuthorProfilePage)
                    this.NavigationService?.Navigate(new Uri("Pages/AuthorProfile.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show("Registration failed. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving author registration: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack) this.NavigationService.GoBack();
        }
    }
}

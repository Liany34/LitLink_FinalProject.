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
    /// Interaction logic for ReaderProfile.xaml
    /// </summary>
    public partial class ReaderProfile : Page
    {
        private Apiservice _apiService = new Apiservice();
        private List<Book> _allBooks = new List<Book>();
        private User currentUser;

        public ReaderProfile()
        {
            InitializeComponent();
            this.Loaded += ReaderProfilePage_Loaded;
            currentUser = this.DataContext as User;
        }

        // טעינת הנתונים בכל פעם שהעמוד מוצג מחדש למשתמש
        private void ReaderProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();
        }

        /// <summary>
        /// שליפת נתוני המשתמש הנוכחי מה-Session ועידכון כותרת שלום ותמונת הפרופיל
        /// </summary>
        private async void LoadUserData()
        {
            // אבטחה: אם אין משתמש מחובר בזיכרון, נחזיר אותו לעמוד ההתחברות
            if (currentUser == null)
            {
                MessageBox.Show("Please log in to view your profile.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
                return;
            }

            try
            {
                // השמת שם המשתמש הדינמי בכותרת
                TxtHelloUser.Text = $"Hello, {currentUser.Username}";

                // טעינת תמונת הפרופיל האישית שלו (במידה וקיימת בבסיס הנתונים)
                if (!string.IsNullOrEmpty(currentUser.Picture))
                {
                    try
                    {
                        ImgReaderProfile.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(currentUser.Picture, UriKind.RelativeOrAbsolute));
                    }
                    catch { /* במקרה והנתיב שבור, נשאר עם תמונת ברירת המחדל מה-XAML */ }
                }

                // שליפת כל הספרים מה-Access כדי לסנן עבור הרשימות האישיות
                _allBooks = await _apiService.GetAllBooks();

                // ברירת מחדל: הצגת הרשימות האישיות של המשתמש (List)
                BuildUserLists();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading profile details: " + ex.Message);
            }
        }

        /// <summary>
        /// פעולה המייצרת ומציגה דינמית את רשימות הספרים האישיות של המשתמש מה-Access באמצעות רכיבי שורות
        /// </summary>
        private async void BuildUserLists()
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabList);

            try
            {
                // שליפת כל הרשימות שהמשתמש הנוכחי יצר מטבלת הרשימות ב-Access לפי ה-Id שלו
                List<Book_List> bookLists = await _apiService.GetAllBookLists();
                List<Book_List> userCustomLists = bookLists.Where(l => l.IdReader.Id == currentUser.Id).ToList();

                if (userCustomLists == null || userCustomLists.Count == 0)
                {
                    ShowEmptyStateMessage("You haven't created any reading lists yet.");
                    return;
                }

                // לולאה שבונה שורה אופקית (עם חצים) לכל רשימה בנפרד
                foreach (var currentList in userCustomLists)
                {
                    // סינון הספרים ששייכים פיזית לרשימה הנוכחית
                    List<List_Detail> allListDetails = await _apiService.GetAllListDetails();
                    List<List_Detail> currentListDetails = allListDetails.Where(d => d.IdList.Id == currentList.Id).ToList();
                    List<Book> relatedBooks = _allBooks.Where(b => currentListDetails.Any(d => d.IdBook.Id == b.Id)).ToList();

                    if (relatedBooks.Count == 0) continue;

                    // יצירת מופע של רכיב שורת הז'אנר/רשימה עם חצים
                    GenreUserControl listRow = new GenreUserControl();
                    listRow.SetupGenreRow(currentList.ListName, relatedBooks);

                    // רישום לאירוע מעבר לדף ספר בלחיצה על תמונה בתוך הרשימה
                    listRow.BookSelected += UserRow_BookSelected;

                    UserListsContainer.Children.Add(listRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error rendering user lists: " + ex.Message);
            }
        }

        private async void UserRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null) return;

            try
            {
                // רשימה שתכיל את הספרים שהמשתמש כבר רכש/מחזיק ברשימות שלו
                List<Book> ownedBooks = new List<Book>();

                // 🌟 תיקון 2: הוספת await לקבלת כל הרשימות מה-API
                List<Book_List> bookLists = await _apiService.GetAllBookLists();

                // סינון הרשימות ששייכות למשתמש הנוכחי (משתמש ב-App.CurrentUser או currentUser שלך)
                List<Book_List> userCustomLists = bookLists.Where(l => l.IdReader.Id == currentUser.Id).ToList();

                // שליפת כל פרטי הרשימות בצורה אסינכרונית
                List<List_Detail> allListDetails = await _apiService.GetAllListDetails();

                // סינון פרטי הרשימות ששייכים אך ורק לרשימות של המשתמש הנוכחי
                List<List_Detail> userListDetails = allListDetails.Where(d => userCustomLists.Any(l => l.Id == d.IdList.Id)).ToList();

                // מעבר על כל הפרטים והוספת הספרים לרשימת הבעלות
                foreach (var detail in userListDetails)
                {
                    // 🌟 תיקון 3: בדיקה לפי ה-ID של הספר כדי להבטיח השוואה מדויקת ב-Access
                    if (!ownedBooks.Any(b => b.Id == detail.IdBook.Id))
                    {
                        ownedBooks.Add(detail.IdBook);
                    }
                }

                // בדיקה דינמית האם הספר שנבחר נמצא ברשימת הספרים של המשתמש
                bool ownsBook = ownedBooks.Any(b => b.Id == selectedBook.Id);


                // יצירת עמוד הספר (החלפתי ל-BookPage לפי הקוד שלך) וניווט אליו
                BookPage detailsPage = new BookPage(selectedBook, ownsBook, false, false);
                this.NavigationService?.Navigate(detailsPage);
            }
            catch (Exception ex)
            {
                // הצגת שגיאה במידה ומשהו נכשל מול ה-API או ה-Database
                MessageBox.Show("Error loading book availability: " + ex.Message, "LitLink Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== לוגיקת פעולות ה-Tabs הפנימיים בפרופיל ====================

        private void FilterList_Click(object sender, RoutedEventArgs e)
        {
            BuildUserLists();
        }

        private async void FilterReviews_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabReviews);

            // כאן תוכלי לשלוף ולהציג את כל הביקורות שהמשתמש הנוכחי כתב
            ShowEmptyStateMessage("You haven't written any reviews yet.");
        }

        private async void FilterMyBooks_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabMyBooks);

            List<Book> ownedBooks = new List<Book>();
            List<Book_List> bookLists = await _apiService.GetAllBookLists();
            List<Book_List> userCustomLists = bookLists.Where(l => l.IdReader.Id == currentUser.Id).ToList();
            List<List_Detail> allListDetails = await _apiService.GetAllListDetails();
            List<List_Detail> userListDetails = allListDetails.Where(d => userCustomLists.Any(l => l.Id == d.IdList.Id)).ToList();
            foreach (var detail in userListDetails)
            {
                if (!ownedBooks.Contains(detail.IdBook))
                {
                    ownedBooks.Add(detail.IdBook);
                }
            }

            if (ownedBooks == null || ownedBooks.Count == 0)
            {
                ShowEmptyStateMessage("You haven't purchased any books yet.");
                return;
            }

            
            GenreUserControl purchasedRow = new GenreUserControl();
            purchasedRow.SetupGenreRow("My Purchased Library", ownedBooks);
            purchasedRow.BookSelected += UserRow_BookSelected;
            UserListsContainer.Children.Add(purchasedRow);
        }

        private void FilterFollowing_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabFollowing);

            // כאן תוכלי להציג את כל הסופרים שהמשתמש הנוכחי עוקב אחריהם לפי ה-Id שלו
            ShowEmptyStateMessage("You aren't following any authors yet.");
        }

        // ==================== ניהול פאנל עריכה צידי (Popup) ====================

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e) => EditProfilePopup.Visibility = Visibility.Visible;
        private void CloseEditProfile_Click(object sender, RoutedEventArgs e) => EditProfilePopup.Visibility = Visibility.Collapsed;

        // סגירת הפאנל בלחיצה מחוץ לתיבה (על השטח הכהה ברקע)
        private void OutsidePopup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == EditProfilePopup) EditProfilePopup.Visibility = Visibility.Collapsed;
        }

        // ==================== פעולות ניווט והתנתקות מהמערכת ====================

        private void Home_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/HomePage.xaml", UriKind.Relative));
        private void Cart_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/CartPage.xaml", UriKind.Relative));
        private void EditDetails_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/EditProfileDetails.xaml", UriKind.Relative));
        private void ResetPassword_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/ResetPass.xaml", UriKind.Relative));
        private void Preference_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Preferences layout option clicked!", "LitLink");
        private void Support_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Support option clicked! Connecting to help center...", "LitLink");

        // התנתקות מלאה (Log Out) כפי שביקשת - מאפס את הסשן ומציג את עמוד הניתוק
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentUser = null; // איפוס המשתמש המחובר מהזיכרון הגלובלי
            this.NavigationService?.Navigate(new Uri("Pages/LogOutPage.xaml", UriKind.Relative));
        }

        // מחיקת חשבון קורא לצמיתות מטבלת ה-Access
        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to permanently delete your LitLink account?\nThis action cannot be undone!",
                "Delete Account", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // קריאה לפונקציית מחיקה ב-ApiService לפי ה-ID של המשתמש המחובר
                    await _apiService.DeleteUser(currentUser.Id);

                    currentUser = null; // ניקוי הזיכרון
                    MessageBox.Show("Your account has been deleted successfully.", "LitLink");
                    this.NavigationService?.Navigate(new Uri("Pages/LogOutPage.xaml", UriKind.Relative));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete account: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==================== פונקציות עזר לעיצוב ה-UI ====================

        private void HighlightActiveTab(Button activeBtn)
        {
            // איפוס צבעי כל הטאבים לאפור
            BtnTabList.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabReviews.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabMyBooks.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabFollowing.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));

            // הדגשת הטאב הלחוץ בצבע הכהה של האתר
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

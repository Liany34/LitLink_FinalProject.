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
    /// Interaction logic for AuthorProfile.xaml
    /// </summary>
    public partial class AuthorProfile : Page
    {
        private Apiservice _apiService = new Apiservice();
        private Author _currentAuthorData;
        private List<Book> _authorBooks = new List<Book>();
        private User currentUser;

        public AuthorProfile()
        {
            InitializeComponent();
            this.Loaded += AuthorProfilePage_Loaded;
            currentUser = this.DataContext as User;
        }

        // טעינה ורענון של נתוני הסופר בכל פעם שהעמוד עולה למסך
        private void AuthorProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAuthorData();
        }

        /// <summary>
        /// שליפת נתוני הסופר המחובר באופן דינמי לחלוטין מטבלאות ה-Access
        /// </summary>
        private async void LoadAuthorData()
        {
            List<Author> allAuthors = await _apiService.GetAllAuthors();
            // אבטחה: הגנה מפני כניסה של אורח או משתמש שאינו סופר
            if (currentUser == null || !allAuthors.Contains(currentUser))
            {
                MessageBox.Show("Unauthorized access. Redirecting to Home.", "LitLink Security", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.Navigate(new Uri("Pages/HomePage.xaml", UriKind.Relative));
                return;
            }

            try
            {
                // 1. שליפת רשומת הסופר מטבלת Authors לפי ה-UserId של המשתמש המחובר כרגע
                _currentAuthorData = allAuthors.FirstOrDefault(a => a.Id == currentUser.Id);

                if (_currentAuthorData != null)
                {
                    // עדכון ה-PenName והעוקבים באופן דינמי למסך
                    TxtHelloAuthor.Text = $"Hello, {_currentAuthorData.PenName}";

                    List<Following> allFollowings = await _apiService.GetAllFollowings();
                    List<Following> authorFollowings = allFollowings.Where(f => f.IdAuthor.Id == _currentAuthorData.Id).ToList();

                    int followersCount = authorFollowings.Count;
                    TxtFollowersCount.Text = $"{followersCount} Followers";
                }

                // טעינת תמונת הפרופיל האישית של המשתמש מה-Database
                if (!string.IsNullOrEmpty(currentUser.Picture))
                {
                    try { ImgAuthorProfile.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(currentUser.Picture, UriKind.RelativeOrAbsolute)); } catch { }
                }

                // 2. שליפת כל הספרים של הסופר הזה בלבד (לפי ה-AuthorId שלו)
                List<Book> allBooks = await _apiService.GetAllBooks();
                _authorBooks = allBooks.Where(b => b.IdAuthor.Id == _currentAuthorData.Id).ToList();

                // ברירת מחדל: טעינת הטאב הראשון של הספרים
                LoadMyBooksTab();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading author profile: " + ex.Message);
            }
        }

        // ==================== לוגיקת הטאבים (הסליידר התחתון) ====================

        private void TabMyBooks_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabMyBooks);
            AuthorBooksContainer.Visibility = Visibility.Visible;
            AuthorNewsContainer.Visibility = Visibility.Collapsed;
            AuthorSalesContainer.Visibility = Visibility.Collapsed;
            LoadMyBooksTab();
        }

        private void TabMyNews_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabMyNews);
            AuthorBooksContainer.Visibility = Visibility.Collapsed;
            AuthorNewsContainer.Visibility = Visibility.Visible;
            AuthorSalesContainer.Visibility = Visibility.Collapsed;
            LoadMyNewsTab();
        }

        private void TabSalesData_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabSalesData);
            AuthorBooksContainer.Visibility = Visibility.Collapsed;
            AuthorNewsContainer.Visibility = Visibility.Collapsed;
            AuthorSalesContainer.Visibility = Visibility.Visible;
            LoadSalesDataTab();
        }

        /// <summary>
        /// טאב א': הצגת הספרים של הסופר מסודרים בשורות אופקיות (כמו אצל הקורא)
        /// </summary>
        private void LoadMyBooksTab()
        {
            AuthorBooksContainer.Children.Clear();

            if (_authorBooks.Count == 0)
            {
                AuthorBooksContainer.Children.Add(CreateEmptyMessageTextBlock("You haven't published any books yet."));
                return;
            }

            // שימוש ברכיב ה-UserControl של השורה שיצרנו עם החצים והגלילה
            GenreUserControl authorRow = new GenreUserControl();
            authorRow.SetupGenreRow("Published Works", _authorBooks);

            // רישום לאירוע הלחיצה על ספר שיפתח את עמוד הספר עם הרשאות ניהול של סופר
            authorRow.BookSelected += AuthorRow_BookSelected;

            AuthorBooksContainer.Children.Add(authorRow);
        }

        private void AuthorRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null) return;

            // ניווט לעמוד הספר (BookPage), הסופר מקבל הרשאות מיוחדות (isAuthor = true)
            // בעמוד הספר, הנתון הזה יציג לו אוטומטית כפתורי עריכה, מחיקה או ניהול תגובות!
            BookPage bookPage = new BookPage(selectedBook, true, false, true);
            this.NavigationService?.Navigate(bookPage);
        }

        /// <summary>
        /// טאב ב': טעינה, הצגה וניהול מלא (עריכה/מחיקה) של כל הודעות החדשות שהסופר פרסם ב-Access
        /// </summary>
        private async void LoadMyNewsTab()
        {
            AuthorNewsContainer.Children.Clear();

            try
            {
                // שליפת כל החדשות מה-Access שמסוננות לפי ה-Id של הסופר המחובר
                List<News> allNews = await _apiService.GetAllNews();
                List<News> authorNews = allNews.Where(n => n.IdUser.Id == _currentAuthorData.Id).OrderByDescending(n => n.Time).ToList();

                if (authorNews == null || authorNews.Count == 0)
                {
                    AuthorNewsContainer.Children.Add(CreateEmptyMessageTextBlock("No news updates published yet."));
                    return;
                }

                foreach (var news in authorNews)
                {
                    // בניית כרטיס תצוגה לבן לכל הודעה
                    Border newsCard = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(15), Margin = new Thickness(0, 0, 0, 10), Effect = (System.Windows.Media.Effects.Effect)FindResource("MenuShadow") };
                    StackPanel sp = new StackPanel();

                    sp.Children.Add(new TextBlock { Text = news., FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74)) });
                    sp.Children.Add(new TextBlock { Text = news.PublishDate.ToShortDateString(), FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 8) });
                    sp.Children.Add(new TextBlock { Text = news.Content, FontSize = 14, TextWrapping = TextWrapping.Wrap, Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)) });

                    // פאנל כפתורי ניהול (Edit / Delete) לכל הודעה
                    StackPanel btnSp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
                    Button btnEdit = new Button { Content = "Edit", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = new SolidColorBrush(Color.FromRgb(208, 106, 141)), Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 15, 0) };
                    Button btnDelete = new Button { Content = "Delete", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Red, Cursor = Cursors.Hand };

                    // 🛠️ קוד פעולת עריכת הודעת חדשות קיימת
                    btnEdit.Click += (s, e) =>
                    {
                        // פתיחת חלון העריכה והעברת אובייקט החדשות הנוכחי אליו
                        EditNewsWindow editNewsWin = new EditNewsWindow(news);
                        if (editNewsWin.ShowDialog() == true)
                        {
                            LoadMyNewsTab(); // רענון הרשימה לאחר עדכון מוצלח
                        }
                    };

                    // 🛠️ קוד פעולת מחיקת הודעת חדשות לצמיתות מה-Access
                    btnDelete.Click += async (s, e) =>
                    {
                        if (MessageBox.Show($"Are you sure you want to permanently delete the update '{news.Title}'?", "LitLink", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            await _apiService.DeleteNews(news.Id);
                            List<News> updatedNewsList = await _apiService.GetAllNews();
                            bool success = false;
                            if(!updatedNewsList.Contains(news))
                            {
                                success = true;
                            }
                            if (success)
                            {
                                MessageBox.Show("News update deleted successfully.", "LitLink");
                                LoadMyNewsTab(); // רענון התצוגה
                            }
                        }
                    };

                    btnSp.Children.Add(btnEdit);
                    btnSp.Children.Add(btnDelete);
                    sp.Children.Add(btnSp);
                    newsCard.Child = sp;
                    AuthorNewsContainer.Children.Add(newsCard);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading author news: " + ex.Message);
            }
        }

        /// <summary>
        /// טאב ג': שליפת נתוני מכירות וחישוב הכנסות כספיות דינמיות מה-Access עבור הסופר הנוכחי
        /// </summary>
        private async void LoadSalesDataTab()
        {
            try
            {
                // פנייה ל-API שמחשב בזמן אמת את סך הרכישות שבוצעו על ספריו של סופר זה
                SalesSummary summary = await _apiService.GetSalesSummaryByAuthorId(_currentAuthorData.Id);

                if (summary != null)
                {
                    TxtTotalSales.Text = summary.TotalCopiesSold.ToString();
                    TxtTotalRevenue.Text = $"{summary.TotalRevenue:F2} ₪";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error calculating sales data: " + ex.Message);
            }
        }

        // ==================== ניהול תפריט המבורגר (Popup) וניווטים ====================

        private void BtnMenu_Click(object sender, RoutedEventArgs e) => AuthorMenuPopup.Visibility = Visibility.Visible;
        private void CloseMenu_Click(object sender, RoutedEventArgs e) => AuthorMenuPopup.Visibility = Visibility.Collapsed;
        private void OutsideMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == AuthorMenuPopup) AuthorMenuPopup.Visibility = Visibility.Collapsed;
        }

        // 🛠️ קוד פעולת עריכת פרופיל הסופר (Edit Profile)
        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAuthorData == null) return;

            // פתיחת חלון העריכה הייעודי לסופר (PenName ומידע אודותיו)
            EditAuthorProfileWindow editWin = new EditAuthorProfileWindow(_currentAuthorData);
            if (editWin.ShowDialog() == true)
            {
                LoadAuthorData(); // רענון הנתונים בדף הראשי לאחר שמירה
            }
        }

        // מעבר לעמודים השונים דרך תפריט הניהול
        private void AddBook_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/AddBookPage.xaml", UriKind.Relative));
        private void AddNews_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/AddNewsPage.xaml", UriKind.Relative));

        /// <summary>
        /// 🔄 כפתור ה-Reader View: מאפשר לסופר לחזור מיידית לתצוגת קורא רגיל באפליקציה
        /// </summary>
        private void ReaderView_Click(object sender, RoutedEventArgs e)
        {
            // ניווט ישיר לעמוד הפרופיל של הקורא (ReaderProfilePage)
            this.NavigationService?.Navigate(new Uri("Pages/ReaderProfile.xaml", UriKind.Relative));
        }

        // התנתקות מלאה ואיפוס ה-Session
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentUser = null; // מחיקת המשתמש מהזיכרון
            this.NavigationService?.Navigate(new Uri("Pages/SignOut.xaml", UriKind.Relative));
        }

        // ==================== פונקציות עזר עיצוביות ====================

        private void HighlightTab(Button activeBtn)
        {
            BtnTabMyBooks.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabMyNews.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabSalesData.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            activeBtn.Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private TextBlock CreateEmptyMessageTextBlock(string msg)
        {
            return new TextBlock { Text = msg, FontSize = 14, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0) };
        }
    }
}
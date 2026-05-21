using LitLink_FinalProject;
using LitLink_FinalProject.UserControls;
using Model;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LitLink_FinalProject.Pages
{
    /// <summary>
    /// Interaction logic for BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {
        public BookPage(Book bookData, bool userOwnsBook, bool isAdmin, bool isAuthor)
        {
            InitializeComponent();

            // יצירת מופע דינמי של ה-UserControl שכתבת עם הנתונים שהתקבלו
            BookUserControl bookControl = new BookUserControl(bookData, userOwnsBook, isAdmin, isAuthor);

            // הזרקת ה-UserControl לתוך ה-Grid ששמנו ב-XAML
            MainContainer.Children.Add(bookControl);
        }

        // לוגיקת כפתור החזור לעמוד הקודם בהיסטוריית הניווט
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}



//< Border Cursor = "Hand" MouseDown = "BookImage_MouseDown" >
//    < Image Source = "{Binding cover}" Stretch = "UniformToFill" Width = "120" Height = "180" />
//</ Border >

//private void BookImage_MouseDown(object sender, MouseButtonEventArgs e)
//{
//    // 1. שליפת הספר שעליו המשתמש לחץ מתוך ה-DataContext של האלמנט
//    FrameworkElement element = sender as FrameworkElement;
//    Book clickedBook = element?.DataContext as Book;

//    if (clickedBook == null) return;

//    // 2. שליפת נתוני המשתמש הנוכחי באפליקציה (שני את המשתנים האלו לשמות אצלך בפרויקט)
//    bool ownsBook = false; // כאן תבדקי אם למשתמש יש את הספר, למשל: App.CurrentUser.OwnedBooks.Contains(clickedBook.Id)
//    bool isAdmin = App.CurrentUser.IsAdmin;     // דוגמה
//    bool isAuthor = App.CurrentUser.IsAuthor;   // דוגמה

//    // 3. יצירת עמוד הפרטים החדש וניווט אליו בתוך ה-Frame הראשי של האפליקציה
//    BookDetailsPage detailsPage = new BookDetailsPage(clickedBook, ownsBook, isAdmin, isAuthor);

//    // ניווט דרך ה-NavigationService המובנה של העמוד הנוכחי
//    this.NavigationService?.Navigate(detailsPage);
//}
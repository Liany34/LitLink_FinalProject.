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
using Model;
using Service;

namespace LitLink_FinalProject.UserControls
{
    /// <summary>
    /// Interaction logic for NewsUserControl.xaml
    /// </summary>
    public partial class NewsUserControl : UserControl
    {
        private Apiservice _apiService = new Apiservice();
        private News _currentNewsData;
        private User currentUser;

        // אירוע שמופעל לאחר מחיקה מוצלח כדי שהעמוד המארח יתרענן
        public event Action NewsChanged;

        public NewsUserControl()
        {
            InitializeComponent();
            this.Loaded += NewsUserControl_Loaded;
            currentUser = this.DataContext as User;
        }


        private void NewsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _currentNewsData = this.DataContext as News;
            if (_currentNewsData == null) return;

            // בדיקה דינמית אם להציג את כפתור Read More לפי אורך הטקסט
            if (TxtNewsContent.ActualHeight < 60)
            {
                BtnReadMore.Visibility = Visibility.Collapsed;
            }

            SetupPermissions();
        }

        /// <summary>
        /// הגדרת הרשאות דינמית עבור תפריט 3 הנקודות לפי המשתמש המחובר
        /// </summary>
        private async void SetupPermissions()
        {
            if (currentUser == null || _currentNewsData == null)
            {
                BtnNewsMenu.Visibility = Visibility.Collapsed; // אורח לא רואה תפריט בכלל
                return;
            }

            List<Admin> admins = await _apiService.GetAllAdmins();
            bool isAdmin = admins.Contains(currentUser);
            List<Author> authors = await _apiService.GetAllAuthors();
            bool isOwner = authors.Contains(currentUser) && currentUser.Id == _currentNewsData.IdUser.Id;

            if (isAdmin)
            {
                BtnNewsMenu.Visibility = Visibility.Visible;
                MenuEditNews.Visibility = Visibility.Collapsed; // מנהל לא יכול לערוך, רק למחוק
                MenuDeleteNews.Visibility = Visibility.Visible;
            }
            else if (isOwner)
            {
                BtnNewsMenu.Visibility = Visibility.Visible;
                MenuEditNews.Visibility = Visibility.Visible;   // סופר של ההודעה יכול גם לערוך וגם למחוק
                MenuDeleteNews.Visibility = Visibility.Visible;
            }
            else
            {
                BtnNewsMenu.Visibility = Visibility.Collapsed; // סופר אחר או קורא רגיל לא רואים תפריט
            }
        }

        private void BtnNewsMenu_Click(object sender, RoutedEventArgs e)
        {
            NewsContextMenu.PlacementTarget = sender as Button;
            NewsContextMenu.IsOpen = true;
        }

        // לוגיקת כפתור Read More / Read Less
        private void BtnReadMore_Click(object sender, RoutedEventArgs e)
        {
            if (TxtNewsContent.MaxHeight == 60)
            {
                TxtNewsContent.MaxHeight = double.PositiveInfinity;
                BtnReadMore.Content = "Read Less";
            }
            else
            {
                TxtNewsContent.MaxHeight = 60;
                BtnReadMore.Content = "Read More";
            }
        }

        // פעולת מחיקה מה-Access דרך ה-3 נקודות
        private async void MenuDeleteNews_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete this news update?", "Delete News", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await _apiService.DeleteNews(_currentNewsData.Id);
                List<News> allNews = await _apiService.GetAllNews();
                bool success = !allNews.Contains(_currentNewsData); // בדיקה שההודעה נמחקה בהצלחה
                if (success)
                {
                    NewsChanged?.Invoke(); // קריאה לרענון העמוד שבו הקונטרול נמצא
                }
            }
        }

        // פעולת עריכה דרך ה-3 נקודות (רק עבור הסופר)
        private void MenuEditNews_Click(object sender, RoutedEventArgs e)
        {
            EditNewsWindow editWin = new EditNewsWindow(_currentNewsData);
            if (editWin.ShowDialog() == true)
            {
                NewsChanged?.Invoke(); // רענון
            }
        }
    }
}

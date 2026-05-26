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
    public partial class NewsUserControl : UserControl
    {
        private Apiservice apiService = new Apiservice();
        private News currentNewsData;
        private User currentUser;

        public event Action NewsChanged;

        public NewsUserControl()
        {
            InitializeComponent();
            this.Loaded += NewsUserControl_Loaded;
            currentUser = this.DataContext as User;
        }


        private void NewsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            currentNewsData = this.DataContext as News;
            if (currentNewsData == null) return;

            if (TxtNewsContent.ActualHeight < 60)
            {
                BtnReadMore.Visibility = Visibility.Collapsed;
            }

            SetupPermissions();
        }

        private async void SetupPermissions()
        {
            if (currentUser == null || currentNewsData == null)
            {
                BtnNewsMenu.Visibility = Visibility.Collapsed; 
                return;
            }

            List<Admin> admins = await apiService.GetAllAdmins();
            bool isAdmin = admins.Contains(currentUser);
            List<Author> authors = await apiService.GetAllAuthors();
            bool isOwner = authors.Contains(currentUser) && currentUser.Id == currentNewsData.IdUser.Id;

            if (isAdmin)
            {
                BtnNewsMenu.Visibility = Visibility.Visible;
                MenuDeleteNews.Visibility = Visibility.Visible;
            }
            else if (isOwner)
            {
                BtnNewsMenu.Visibility = Visibility.Visible;
                MenuDeleteNews.Visibility = Visibility.Visible;
            }
            else
            {
                BtnNewsMenu.Visibility = Visibility.Collapsed; 
            }
        }

        private void BtnNewsMenu_Click(object sender, RoutedEventArgs e)
        {
            NewsContextMenu.PlacementTarget = sender as Button;
            NewsContextMenu.IsOpen = true;
        }

        
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

        private async void MenuDeleteNews_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete this news update?", "Delete News", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await apiService.DeleteNews(currentNewsData.Id);
                List<News> allNews = await apiService.GetAllNews();
                bool success = !allNews.Contains(currentNewsData); 
                if (success)
                {
                    NewsChanged?.Invoke(); 
                }
            }
        }
    }
}

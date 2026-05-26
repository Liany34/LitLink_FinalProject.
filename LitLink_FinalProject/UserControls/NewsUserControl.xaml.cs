using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        }

        private void NewsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            currentNewsData = this.DataContext as News;
            if (currentNewsData == null) return;

            currentUser = this.DataContext as User;

            if (TxtNewsContent.ActualHeight < 60)
            {
                BtnReadMore.Visibility = Visibility.Collapsed;
            }

            if (currentNewsData != null && !string.IsNullOrEmpty(currentNewsData.IdUser.Picture))
            {
                try
                {
                    byte[] imgStr = Convert.FromBase64String(currentNewsData.IdUser.Picture);
                    this.AuthorProfileImage.Source = ByteImageConverter.ByteToImage(imgStr);
                }
                catch (Exception)
                {
                    this.AuthorProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/PRP/DefultUser.png", UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                this.AuthorProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/PRP/DefultUser.png", UriKind.RelativeOrAbsolute));
            }

            SetupPermissions();
        }

        private async void SetupPermissions()
        {
            if (currentNewsData == null)
            {
                BtnNewsMenu.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                List<Admin> admins = await apiService.GetAllAdmins();
                bool isAdmin = currentUser != null && admins.Exists(a => a.Id == currentUser.Id);

                List<Author> authors = await apiService.GetAllAuthors();
                bool isOwner = currentUser != null && currentNewsData.IdUser != null &&
                               authors.Exists(a => a.Id == currentUser.Id) && currentUser.Id == currentNewsData.IdUser.Id;

                if (isAdmin || isOwner)
                {
                    BtnNewsMenu.Visibility = Visibility.Visible;
                    MenuDeleteNews.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnNewsMenu.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up permissions: " + ex.Message);
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
            if (currentNewsData == null) return;

            if (MessageBox.Show($"Are you sure you want to delete this news update?", "Delete News", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await apiService.DeleteNews(currentNewsData.Id);
                List<News> allNews = await apiService.GetAllNews();

                bool isDeleted = !allNews.Exists(n => n.Id == currentNewsData.Id);
                if (isDeleted)
                {
                    NewsChanged?.Invoke();
                }
            }
        }
    }
}
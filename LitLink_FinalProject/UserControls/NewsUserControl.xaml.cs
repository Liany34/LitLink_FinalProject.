using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Linq; 

namespace LitLink_FinalProject.UserControls
{
    public partial class NewsUserControl : UserControl
    {
        private Apiservice apiService = new Apiservice();
        private News currentNewsData;
        private User currentUser;
        public User LoggedInUser { get; set; }
        public bool IsLoggedInUserAdmin { get; set; }
        public bool IsLoggedInUserAuthor { get; set; }

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

            currentUser = LoggedInUser;

            if (TxtNewsContent.ActualHeight < 60)
            {
                BtnReadMore.Visibility = Visibility.Collapsed;
            }

            if (currentNewsData.IdUser != null && !string.IsNullOrEmpty(currentNewsData.IdUser.Picture))
            {
                try
                {
                    byte[] imgStr = Convert.FromBase64String(currentNewsData.IdUser.Picture);
                    this.AuthorProfileImage.Source = ByteImageConverter.ByteToImage(imgStr);
                }
                catch
                {
                    SetDefaultUserPicture();
                }
            }
            else
            {
                SetDefaultUserPicture();
            }

            SetupPermissions();
        }

        private void SetDefaultUserPicture()
        {
            try
            {
                AuthorProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/UserPicture1.png", UriKind.Absolute));
            }
            catch
            {
                AuthorProfileImage.Source = null;
            }
        }

        private void SetupPermissions()
        {
            BtnNewsMenu.Visibility = Visibility.Collapsed;
            MenuDeleteNews.Visibility = Visibility.Collapsed;

            if (currentNewsData == null || currentNewsData.IdUser == null || currentUser == null)
                return;

            bool isOwner = currentNewsData.IdUser.Id == currentUser.Id;

            bool canDelete = IsLoggedInUserAdmin || (IsLoggedInUserAuthor && isOwner);

            if (canDelete)
            {
                BtnNewsMenu.Visibility = Visibility.Visible;
                MenuDeleteNews.Visibility = Visibility.Visible;
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
            if (currentNewsData == null || currentUser == null)
                return;

            bool isOwner = currentNewsData.IdUser != null &&
                           currentNewsData.IdUser.Id == currentUser.Id;

            bool canDelete = IsLoggedInUserAdmin || (IsLoggedInUserAuthor && isOwner);

            if (!canDelete)
            {
                MessageBox.Show("You do not have permission to delete this news update.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this news update?",
                "Delete News",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await apiService.DeleteNews(currentNewsData.Id);

                NewsChanged?.Invoke();
            }
        }
    }
}
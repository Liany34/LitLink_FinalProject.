using Model;
using Service;
using System;
using System.Windows;

namespace LitLink_FinalProject.WindowsFile
{
    public partial class AddNewsWindow : Window
    {
        private Apiservice apiService = new Apiservice();
        private User currentUser;

        public AddNewsWindow()
        {
            InitializeComponent();
            this.Loaded += AddNewsWindow_Loaded;
        }

        private void AddNewsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            currentUser = this.DataContext as User;

            if (currentUser == null)
            {
                MessageBox.Show("Error: Unauthorized access. Missing active admin context.", "LitLink Control");
                this.DialogResult = false;
                this.Close();
                return;
            }

            TxtAuthorLabel.Text = $"Publishing as: {currentUser.Username} 👑";
        }

        private async void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtNewsTitle.Text.Trim();
            string content = TxtNewsContent.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Please provide both a title and a message content 🌸", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                News newUpdate = new News
                {
                    IdUser = currentUser,
                    Titel = title,
                    Content = content,
                    PublishDate = DateTime.Now
                };

                await apiService.InsertNews(newUpdate);

                MessageBox.Show("Your admin update has been published successfully! ✨", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error publishing news: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
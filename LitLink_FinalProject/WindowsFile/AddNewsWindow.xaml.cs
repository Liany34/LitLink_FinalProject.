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

        public AddNewsWindow(User user)
        {
            InitializeComponent();

            currentUser = user;

            if (currentUser == null)
            {
                MessageBox.Show("Error: Missing active user context.", "LitLink");
                Close();
                return;
            }

            TxtAuthorLabel.Text = $"Publishing as: {currentUser.Username} 👑";
        }

        private async void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtNewsTitle.Text.Trim();
            string content = TxtNewsContent.Text.Trim();

            if (currentUser == null)
            {
                MessageBox.Show("Cannot publish news because no active user was found.", "LitLink");
                return;
            }

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Please provide both a title and a message content 🌸",
                    "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                MessageBox.Show("Your update has been published successfully! ✨",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error publishing news: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
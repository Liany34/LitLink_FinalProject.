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
using System.Windows.Shapes;

namespace LitLink_FinalProject.WindowsFile
{
    public partial class AddNewsWindow : Window
    {
        private Apiservice apiService = new Apiservice();
        private Author currentAuthor;
        private User currentUser;

        public AddNewsWindow()
        {
            InitializeComponent();
            LoadAuthorInfo();
            currentUser = this.DataContext as User;
        }

        private async void LoadAuthorInfo()
        {
            List<Author> authors = await apiService.GetAllAuthors();
            if (currentUser == null || !authors.Any(a => a.Id == currentUser.Id))
            {
                this.Close();
                return;
            }

            try
            {
                List<Author> allAuthors = await apiService.GetAllAuthors();
                currentAuthor = allAuthors.FirstOrDefault(a => a.Id == currentUser.Id);

                if (currentAuthor != null)
                {
                    TxtAuthorLabel.Text = $"Publishing as: {currentAuthor.PenName}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading author for news: " + ex.Message);
            }
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
                List<News> allNews = await apiService.GetAllNews();
                bool success = allNews.Contains(newUpdate);

                if (success)
                {
                    MessageBox.Show("Your news update has been published successfully! ✨", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; 
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to publish update. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
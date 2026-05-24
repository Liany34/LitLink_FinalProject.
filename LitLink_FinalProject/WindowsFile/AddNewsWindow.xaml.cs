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
    /// <summary>
    /// Interaction logic for AddNewsWindow.xaml
    /// </summary>
    public partial class AddNewsWindow : Window
    {
        private Apiservice _apiService = new Apiservice();
        private Author currentAuthor;
        private User currentUser;

        public AddNewsWindow()
        {
            InitializeComponent();
            LoadAuthorInfo();
            currentUser = this.DataContext as User;
        }

        /// <summary>
        /// טעינת פרטי הסופר המחובר כדי להציג מי המפרסם
        /// </summary>
        private async void LoadAuthorInfo()
        {
            List<Author> authors = await _apiService.GetAllAuthors();
            if (currentUser == null || !authors.Any(a => a.Id == currentUser.Id))
            {
                this.Close();
                return;
            }

            try
            {
                List<Author> allAuthors = await _apiService.GetAllAuthors();
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

        /// <summary>
        /// פרסום החדשות ל-Database
        /// </summary>
        private async void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtNewsTitle.Text.Trim();
            string content = TxtNewsContent.Text.Trim();

            // ולידציה בסיסית
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("Please provide both a title and a message content 🌸", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. יצירת אובייקט חדשות חדש
                News newUpdate = new News
                {
                    IdUser = currentUser,    // קישור למשתמש
                    Titel = title,
                    Content = content,
                    PublishDate = DateTime.Now      // הוספת זמן אוטומטית!
                };

                // 2. שמירה בבסיס הנתונים Access
                await _apiService.InsertNews(newUpdate);
                List<News> allNews = await _apiService.GetAllNews();
                bool success = allNews.Contains(newUpdate);

                if (success)
                {
                    MessageBox.Show("Your news update has been published successfully! ✨", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; // מחזיר 'אמת' לעמוד הבית כדי שיבצע רפרש
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
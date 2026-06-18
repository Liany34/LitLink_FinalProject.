using Model;
using Service;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LitLink_FinalProject.WindowsFile
{
    public partial class AddReviewWindow : Window
    {
        private readonly Apiservice apiService = new Apiservice();
        private readonly Reader currentReader;
        private readonly Book currentBook;

        public AddReviewWindow(Reader reader, Book book)
        {
            InitializeComponent();

            currentReader = reader;
            currentBook = book;

            if (currentReader == null || currentBook == null)
            {
                MessageBox.Show("Error: Missing reader or book context.", "LitLink");
                Close();
                return;
            }

            TxtBookLabel.Text = $"Reviewing: \"{currentBook.BookName}\" 📖";
        }

        private int GetSelectedStars()
        {
            RadioButton[] stars = { Star1, Star2, Star3, Star4, Star5 };
            RadioButton selected = stars.FirstOrDefault(rb => rb.IsChecked == true);
            return selected != null ? int.Parse(selected.Tag.ToString()) : 3;
        }

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string text = TxtReviewContent.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please write your review before submitting 🌸",
                    "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Reviews newReview = new Reviews
                {
                    IdReader = currentReader,
                    IdBook = currentBook,
                    Text = text,
                    Stars = GetSelectedStars(),
                    IsFlaged = false
                };

                await apiService.InsertReview(newReview);

                MessageBox.Show("Your review has been submitted! ✨",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting review: " + ex.Message,
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
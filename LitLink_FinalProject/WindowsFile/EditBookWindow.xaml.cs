using Microsoft.Win32;
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
using System.IO;


namespace LitLink_FinalProject.WindowsFile
{
    public partial class EditBookWindow : Window
    {
        private Book bookToEdit;
        private Apiservice apiService = new Apiservice();
        private string selectedCoverBase64 = null;

        public EditBookWindow(Book currentBook)
        {
            InitializeComponent();
            this.bookToEdit = currentBook;

            InitializeWindowData();
        }

        private async void InitializeWindowData()
        {
            await LoadLanguagesFromDatabaseAsync();

            LoadBookData();
        }

        private async Task LoadLanguagesFromDatabaseAsync()
        {
            try
            {
                var languagesList = await apiService.GetAllLanguages();

                CmbLanguage.ItemsSource = languagesList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load languages list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBookData()
        {
            TxtBookName.Text = bookToEdit.BookName;
            TxtDescription.Text = bookToEdit.Information;
            TxtPrice.Text = bookToEdit.Price.ToString();
            TxtFilePath.Text = bookToEdit.BookLink;
            TxtCoverPath.Text = bookToEdit.Cover;
            DpPublishDate.SelectedDate = bookToEdit.PublicationDate;

            if (bookToEdit.IdLanguage != null)
            {
                SetSelectedLanguage(bookToEdit.IdLanguage);
            }
            if (!string.IsNullOrEmpty(bookToEdit.Cover))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(bookToEdit.Cover);
                    ImgBookCoverPreview.Source = ByteImageConverter.ByteToImage(imageBytes);
                }
                catch
                {
                    // אם זו לא תמונת Base64, לא נעשה כלום
                }
            }
        }

        private void SetSelectedLanguage(Language currentLanguage)
        {
            if (currentLanguage == null || CmbLanguage.ItemsSource == null) return;

            foreach (Language lang in CmbLanguage.ItemsSource)
            {
                if (lang.Id == currentLanguage.Id)
                {
                    CmbLanguage.SelectedItem = lang;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtBookName.Text.Trim()) || string.IsNullOrEmpty(TxtPrice.Text.Trim()))
            {
                MessageBox.Show("Book Name and Price are required fields!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(TxtPrice.Text.Trim(), out double parsedPrice))
            {
                MessageBox.Show("Please enter a valid number for the price.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpPublishDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a valid publication date.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string filePath = TxtFilePath.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filePath) && !filePath.EndsWith(".epub"))
            {
                MessageBox.Show("LitLink only supports books in EPUB format! Please select a valid .epub file.",
                                "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbLanguage.SelectedItem == null)
            {
                MessageBox.Show("Please select a language for the book.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bookToEdit.BookName = TxtBookName.Text.Trim();
            bookToEdit.Information = TxtDescription.Text.Trim();
            bookToEdit.Price = parsedPrice;
            bookToEdit.BookLink = TxtFilePath.Text.Trim();
            if (!string.IsNullOrEmpty(selectedCoverBase64))
            {
                bookToEdit.Cover = selectedCoverBase64;
            }
            else
            {
                bookToEdit.Cover = TxtCoverPath.Text.Trim();
            }
            bookToEdit.PublicationDate = DpPublishDate.SelectedDate.Value;

            Language selectedLang = CmbLanguage.SelectedItem as Language;
            if (selectedLang != null)
            {
                bookToEdit.IdLanguage = selectedLang;
            }

            try
            {
                apiService.UpdateBook(bookToEdit);

                MessageBox.Show("Book details updated successfully!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save changes to the database: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "E-Books (*.epub)|*.epub";

            if (openFileDialog.ShowDialog() == true)
            {
                TxtFilePath.Text = openFileDialog.FileName;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void BrowseCover_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);

                selectedCoverBase64 = Convert.ToBase64String(imageBytes);

                TxtCoverPath.Text = openFileDialog.FileName;

                ImgBookCoverPreview.Source = ByteImageConverter.ByteToImage(imageBytes);
            }
        }
    }
}

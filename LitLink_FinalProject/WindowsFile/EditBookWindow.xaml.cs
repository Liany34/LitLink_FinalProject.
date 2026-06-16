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
        private string selectedCoverPath;

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
            TxtCoverPath.Text = bookToEdit.CoverPath;
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

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtBookName.Text))
                {
                    MessageBox.Show("Book name is required.", "LitLink");
                    return;
                }

                if (!double.TryParse(TxtPrice.Text, out double parsedPrice))
                {
                    MessageBox.Show("Please enter a valid price.", "LitLink");
                    return;
                }

                if (CmbLanguage.SelectedItem == null)
                {
                    MessageBox.Show("Please select a language.", "LitLink");
                    return;
                }

                BookUpdateDto dto = new BookUpdateDto
                {
                    Id = bookToEdit.Id,
                    BookName = TxtBookName.Text.Trim(),
                    PublicationDate = DpPublishDate.SelectedDate,
                    Information = TxtDescription.Text.Trim(),
                    BookLink = TxtFilePath.Text.Trim(),
                    IsFlaged = bookToEdit.IsFlaged,
                    IdAuthor = bookToEdit.IdAuthor.Id,
                    IdLanguage = ((Language)CmbLanguage.SelectedItem).Id,
                    CoverPath = bookToEdit.CoverPath,
                    Price = parsedPrice
                };

                if (!string.IsNullOrEmpty(selectedCoverPath))
                {
                    dto.FileName = System.IO.Path.GetFileName(selectedCoverPath);

                    byte[] imageBytes = File.ReadAllBytes(selectedCoverPath);
                    dto.Base64Image = Convert.ToBase64String(imageBytes);
                }

                bool success = await apiService.UpdateBook(dto);

                if (success)
                {
                    MessageBox.Show("Book updated successfully.", "LitLink");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Book was not updated.", "LitLink");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating book: " + ex.Message, "LitLink");
            }
        }

        //private void Save_Click(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrEmpty(TxtBookName.Text.Trim()) || string.IsNullOrEmpty(TxtPrice.Text.Trim()))
        //    {
        //        MessageBox.Show("Book Name and Price are required fields!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    if (!double.TryParse(TxtPrice.Text.Trim(), out double parsedPrice))
        //    {
        //        MessageBox.Show("Please enter a valid number for the price.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    if (DpPublishDate.SelectedDate == null)
        //    {
        //        MessageBox.Show("Please select a valid publication date.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    string filePath = TxtFilePath.Text.Trim().ToLower();
        //    if (!string.IsNullOrEmpty(filePath) && !filePath.EndsWith(".epub"))
        //    {
        //        MessageBox.Show("LitLink only supports books in EPUB format! Please select a valid .epub file.",
        //                        "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    if (CmbLanguage.SelectedItem == null)
        //    {
        //        MessageBox.Show("Please select a language for the book.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    bookToEdit.BookName = TxtBookName.Text.Trim();
        //    bookToEdit.Information = TxtDescription.Text.Trim();
        //    bookToEdit.Price = parsedPrice;
        //    bookToEdit.BookLink = TxtFilePath.Text.Trim();
        //    if (!string.IsNullOrEmpty(selectedCoverBase64))
        //    {
        //        bookToEdit.Cover = selectedCoverBase64;
        //    }
        //    else
        //    {
        //        bookToEdit.Cover = TxtCoverPath.Text.Trim();
        //    }
        //    bookToEdit.PublicationDate = DpPublishDate.SelectedDate.Value;

        //    Language selectedLang = CmbLanguage.SelectedItem as Language;
        //    if (selectedLang != null)
        //    {
        //        bookToEdit.IdLanguage = selectedLang;
        //    }

        //    try
        //    {
        //        apiService.UpdateBook(bookToEdit);

        //        MessageBox.Show("Book details updated successfully!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);

        //        this.DialogResult = true;
        //        this.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Failed to save changes to the database: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void BtnBrowseCover_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (dlg.ShowDialog() == true)
            {
                selectedCoverPath = dlg.FileName;

                ImgBookCoverPreview.Source = new BitmapImage(new Uri(selectedCoverPath));
            }
        }
    }
}

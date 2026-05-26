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
    public partial class AddBookWindow : Window
    {
        private Apiservice apiService = new Apiservice();
        private Author currentAuthor;
        private User currentUser;

        public class GenreSelectionWrapper
        {
            public Genre GenreData { get; set; }
            public bool IsSelected { get; set; } = false;
        }

        public AddBookWindow()
        {
            InitializeComponent();
            LoadFormDatabaseData();
            currentUser = this.DataContext as User; 
        }

        private async void LoadFormDatabaseData()
        {
            try
            {
                if (currentUser != null)
                {
                    List<Author> authors = await apiService.GetAllAuthors();
                    currentAuthor = authors.FirstOrDefault(a => a.Id == currentUser.Id);
                }

                List<Language> languages = await apiService.GetAllLanguages();
                CmbLanguages.ItemsSource = languages;

                List<Genre> genres = await apiService.GetAllGenres();

                List<GenreSelectionWrapper> wrappedGenres = genres.Select(g => new GenreSelectionWrapper
                {
                    GenreData = g,
                    IsSelected = false
                }).ToList();

                LstCategories.ItemsSource = wrappedGenres;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading database lists: " + ex.Message);
            }
        }

        private async void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            string bookName = TxtBookName.Text.Trim();
            string coverUrl = TxtCoverUrl.Text.Trim();
            string description = TxtDescription.Text.Trim();
            string bookLink = TxtBookLink.Text.Trim();
            Language selectedLanguage = CmbLanguages.SelectedItem as Language;
            DateTime? publishDate = DpPublishDate.SelectedDate;

            if (string.IsNullOrEmpty(bookName) || string.IsNullOrEmpty(coverUrl) || selectedLanguage == null || !publishDate.HasValue)
            {
                MessageBox.Show("Please fill out all required fields (Title, Cover, Language, Date) 🌸", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(TxtPrice.Text, out double price))
            {
                MessageBox.Show("Please enter a valid numeric price.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedWrappers = LstCategories.Items.Cast<GenreSelectionWrapper>().Where(w => w.IsSelected).ToList();
            if (selectedWrappers.Count == 0)
            {
                MessageBox.Show("Please select at least one category for your book.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Book newBook = new Book
                {
                    BookName = bookName,
                    Cover = coverUrl,
                    Information = description,
                    BookLink = bookLink, 
                    Price = price,
                    PublicationDate = publishDate.Value,
                    IdAuthor = currentAuthor, 
                    IdLanguage = selectedLanguage
                };

                await apiService.InsertBook(newBook);
                List<Book> allBooks = await apiService.GetAllBooks();

                Book foundBook = allBooks.Find(b => b.BookName == newBook.BookName && b.IdAuthor?.Id == currentAuthor?.Id);
                int newBookId = foundBook != null ? foundBook.Id : 0;

                if (newBookId > 0)
                {
                    foreach (GenreSelectionWrapper wrapper in selectedWrappers)
                    {
                        Book_Genre bg = new Book_Genre
                        {
                            IdBook = new Book { Id = newBookId },
                            IdGenre = wrapper.GenreData
                        };
                        await apiService.InsertBookGenre(bg);
                    }

                    MessageBox.Show($"✨ '{bookName}' has been successfully published to LitLink!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; 
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to publish book. Try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving book: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

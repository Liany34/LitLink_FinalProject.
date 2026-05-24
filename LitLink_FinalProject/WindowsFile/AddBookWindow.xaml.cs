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
    /// Interaction logic for AddBookWindow.xaml
    /// </summary>
    public partial class AddBookWindow : Window
    {
        private Apiservice _apiService = new Apiservice();
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
            currentUser = this.DataContext as User; // מניח שה-DataContext הוגדר כ-User בעמוד הקודם
        }

        /// <summary>
        /// טעינה דינמית של שפות וקטגוריות מתוך מאגר הנתונים ב-Access
        /// </summary>
        private async void LoadFormDatabaseData()
        {
            try
            {
                // 1. שליפת הסופר המחובר כרגע
                if (currentUser != null)
                {
                    List<Author> authors = await _apiService.GetAllAuthors();
                    currentAuthor = authors.FirstOrDefault(a => a.Id == currentUser.Id);
                }

                // 2. טעינת השפות ל-ComboBox
                List<Language> languages = await _apiService.GetAllLanguages();
                CmbLanguages.ItemsSource = languages;

                // 3. טעינת הז'אנרים/קטגוריות ל-ListBox
                List<Genre> genres = await _apiService.GetAllGenres();

                // 🌟 התיקון: המרת הז'אנרים ל-Wrapper שמכיל את שדה ה-IsSelected בשביל ה-XAML
                List<GenreSelectionWrapper> wrappedGenres = genres.Select(g => new GenreSelectionWrapper
                {
                    GenreData = g,
                    IsSelected = false
                }).ToList();

                // הזרקת הז'אנרים העטופים לתצוגה
                LstCategories.ItemsSource = wrappedGenres;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading database lists: " + ex.Message);
            }
        }

        /// <summary>
        /// שמירת הספר החדש וקישור הקטגוריות שלו ב-Database
        /// </summary>
        private async void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            // ולידציה של השדות
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

            // המרת המחיר לפורמט מספרי בטוח (double)
            if (!double.TryParse(TxtPrice.Text, out double price))
            {
                MessageBox.Show("Please enter a valid numeric price.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 🌟 התיקון: שליפת ה-Wrappers המסומנים במקום ה-Genre המקורי
            var selectedWrappers = LstCategories.Items.Cast<GenreSelectionWrapper>().Where(w => w.IsSelected).ToList();
            if (selectedWrappers.Count == 0)
            {
                MessageBox.Show("Please select at least one category for your book.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. בניית אובייקט הספר החדש
                Book newBook = new Book
                {
                    BookName = bookName,
                    Cover = coverUrl,
                    Information = description,
                    BookLink = bookLink, // קישור להורדה/קריאה
                    Price = price,
                    PublicationDate = publishDate.Value,
                    IdAuthor = currentAuthor, // קישור אוטומטי לסופר המחובר
                    IdLanguage = selectedLanguage
                };

                await _apiService.InsertBook(newBook);
                List<Book> allBooks = await _apiService.GetAllBooks();

                // חיפוש ה-ID של הספר החדש
                Book foundBook = allBooks.Find(b => b.BookName == newBook.BookName && b.IdAuthor?.Id == currentAuthor?.Id);
                int newBookId = foundBook != null ? foundBook.Id : 0;

                if (newBookId > 0)
                {
                    // 3. שמירת קשרי הקטגוריות המרובות בטבלת הקישור Book_Genre
                    // 🌟 התיקון: שליפת אובייקט ה-GenreData המקורי מתוך ה-Wrapper המסומן
                    foreach (GenreSelectionWrapper wrapper in selectedWrappers)
                    {
                        Book_Genre bg = new Book_Genre
                        {
                            IdBook = new Book { Id = newBookId },
                            IdGenre = wrapper.GenreData
                        };
                        await _apiService.InsertBookGenre(bg);
                    }

                    MessageBox.Show($"✨ '{bookName}' has been successfully published to LitLink!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; // מסמן לעמוד המארח לבצע רפרש
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

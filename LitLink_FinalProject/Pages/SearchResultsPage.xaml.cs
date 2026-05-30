using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LitLink_FinalProject.Pages
{
    public partial class SearchResultsPage : Page
    {
        private Apiservice apiService = new Apiservice();
        private string searchQuery;

        public SearchResultsPage(string query)
        {
            InitializeComponent();
            searchQuery = query;

            // נרשם לאירוע הטעינה בצורה מסודרת
            this.Loaded += SearchResultsPage_Loaded;
        }

        private void SearchResultsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(searchQuery))
            {
                // מריץ את החיפוש בשרשור רקע כדי שה-UI לא יקפא לעולם
                Task.Run(async () =>
                {
                    await ExecuteSearch(searchQuery);
                });
            }
        }

        public async Task ExecuteSearch(string searchQuery)
        {
            // עדכון הכותרת (צריך להתבצע ב-UI Thread)
            Dispatcher.Invoke(() =>
            {
                TxtSearchTitle.Text = $"Search Results for: '{searchQuery}'";
            });

            string cleanQuery = searchQuery.ToLower().Trim();

            try
            {
                    System.Diagnostics.Debug.WriteLine("[LITLINK] מנסה למשוך ספרים מה-API...");
                    List<Book> allBooks = await apiService.GetAllBooks();
                    System.Diagnostics.Debug.WriteLine("[LITLINK] הספרים נמשכו בהצלחה!");

                    System.Diagnostics.Debug.WriteLine("[LITLINK] מנסה למשוך סופרים מה-API...");
                    List<Author> allAuthors = await apiService.GetAllAuthors();
                    System.Diagnostics.Debug.WriteLine("[LITLINK] הסופרים נמשכו בהצלחה!");

                    if (allBooks == null) allBooks = new List<Book>();
                    if (allAuthors == null) allAuthors = new List<Author>();


                if (allBooks == null) allBooks = new List<Book>();
                if (allAuthors == null) allAuthors = new List<Author>();

                // סינון הנתונים ברקע
                List<Book> filteredBooks = allBooks.Where(b =>
                    (b.BookName != null && b.BookName.ToLower().Contains(cleanQuery)) ||
                    (b.Information != null && b.Information.ToLower().Contains(cleanQuery))
                ).ToList();

                List<Author> filteredAuthors = allAuthors.Where(a =>
                    a.PenName != null && a.PenName.ToLower().Contains(cleanQuery)
                ).ToList();

                // החזרת התוצאות ל-UI ועדכון הפקדים הגרפיים (באמצעות Dispatcher)
                Dispatcher.Invoke(() =>
                {
                    if (filteredBooks.Count > 0)
                    {
                        BooksResultSection.Visibility = Visibility.Visible;
                        BooksItemsControl.ItemsSource = filteredBooks;
                    }
                    else
                    {
                        BooksResultSection.Visibility = Visibility.Collapsed;
                    }

                    if (filteredAuthors.Count > 0)
                    {
                        AuthorsResultSection.Visibility = Visibility.Visible;
                        AuthorsItemsControl.ItemsSource = filteredAuthors;
                    }
                    else
                    {
                        AuthorsResultSection.Visibility = Visibility.Collapsed;
                    }

                    if (filteredBooks.Count == 0 && filteredAuthors.Count == 0)
                    {
                        TxtNoResults.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        TxtNoResults.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Error performing search: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void BookImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Book clickedBook = element?.DataContext as Book;

            if (clickedBook != null)
            {
                BookPage detailsPage = new BookPage(clickedBook, false, false, false);
                this.NavigationService?.Navigate(detailsPage);
            }
        }

        private void AuthorCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Author clickedAuthor = element?.DataContext as Author;

            if (clickedAuthor != null)
            {
                AuthorProfile authorPage = new AuthorProfile();
                authorPage.DataContext = clickedAuthor;
                this.NavigationService?.Navigate(authorPage);
            }
        }

        private void AuthorImage_Loaded(object sender, RoutedEventArgs e)
        {
            Image imgControl = sender as Image;
            if (imgControl != null)
            {
                Author currentAuthor = imgControl.DataContext as Author;
                if (currentAuthor != null && !string.IsNullOrEmpty(currentAuthor.Picture))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(currentAuthor.Picture);
                        imgControl.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    catch (Exception)
                    {
                        imgControl.Source = new BitmapImage(new Uri("pack://application:,,,/PRP/DefultUser.png", UriKind.RelativeOrAbsolute));
                    }
                }
                else
                {
                    imgControl.Source = new BitmapImage(new Uri("pack://application:,,,/PRP/DefultUser.png", UriKind.RelativeOrAbsolute));
                }
            }
        }

        private void ScrollBooksLeft_Click(object sender, RoutedEventArgs e) => BooksScrollViewer.ScrollToHorizontalOffset(BooksScrollViewer.HorizontalOffset - 150);
        private void ScrollBooksRight_Click(object sender, RoutedEventArgs e) => BooksScrollViewer.ScrollToHorizontalOffset(BooksScrollViewer.HorizontalOffset + 150);

        private void ScrollAuthorsLeft_Click(object sender, RoutedEventArgs e) => AuthorsScrollViewer.ScrollToHorizontalOffset(AuthorsScrollViewer.HorizontalOffset - 150);
        private void ScrollAuthorsRight_Click(object sender, RoutedEventArgs e) => AuthorsScrollViewer.ScrollToHorizontalOffset(AuthorsScrollViewer.HorizontalOffset + 150);

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}
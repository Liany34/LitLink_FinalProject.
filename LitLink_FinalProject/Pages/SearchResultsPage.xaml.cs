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
        private Reader currentUser; 

        public SearchResultsPage(string query, Reader currentUser = null)
        {
            InitializeComponent();
            searchQuery = query;
            this.currentUser = currentUser; 
            this.Loaded += SearchResultsPage_Loaded;
        }

        private async void SearchResultsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(searchQuery))
                await ExecuteSearch(searchQuery); 
        }

        public async Task ExecuteSearch(string searchQuery)
        {
            TxtSearchTitle.Text = $"Search Results for: '{searchQuery}'";
            string cleanQuery = searchQuery.ToLower().Trim();

            try
            {
                var booksTask = apiService.GetAllBooks();
                var authorsTask = apiService.GetAllAuthors();
                await Task.WhenAll(booksTask, authorsTask);

                List<Book> allBooks = booksTask.Result ?? new List<Book>();
                List<Author> allAuthors = authorsTask.Result ?? new List<Author>();

                List<Book> filteredBooks = allBooks.Where(b =>
                    (b.BookName != null && b.BookName.ToLower().Contains(cleanQuery)) ||
                    (b.Information != null && b.Information.ToLower().Contains(cleanQuery))
                ).ToList();

                List<Author> filteredAuthors = allAuthors.Where(a =>
                    a.PenName != null && a.PenName.ToLower().Contains(cleanQuery)
                ).ToList();

                BooksResultSection.Visibility = filteredBooks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                AuthorsResultSection.Visibility = filteredAuthors.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                TxtNoResults.Visibility = (filteredBooks.Count == 0 && filteredAuthors.Count == 0)
                                           ? Visibility.Visible : Visibility.Collapsed;

                if (filteredBooks.Count > 0) BooksItemsControl.ItemsSource = filteredBooks;
                if (filteredAuthors.Count > 0) AuthorsItemsControl.ItemsSource = filteredAuthors;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error performing search: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BookImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Book clickedBook = element?.DataContext as Book;

            if (clickedBook != null)
            {
                BookPage detailsPage = new BookPage(clickedBook, false, false, false, currentUser);
                this.NavigationService?.Navigate(detailsPage);
            }
        }

        private void AuthorCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Author clickedAuthor = element?.DataContext as Author;

            if (clickedAuthor != null)
            {
                AuthorProfile authorPage = new AuthorProfile(clickedAuthor, currentUser);
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
                        imgControl.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/UserPicture1.png", UriKind.RelativeOrAbsolute));
                    }
                }
                else
                {
                    imgControl.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/UserPicture1.png", UriKind.RelativeOrAbsolute));
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
            else
                MainWindow.AppFrame.Navigate(new HomePage(currentUser));
        }
    }
}
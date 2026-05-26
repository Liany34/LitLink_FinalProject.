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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LitLink_FinalProject.Pages
{
    public partial class SearchResultsPage : Page
    {
        private Apiservice _apiService = new Apiservice();

        public SearchResultsPage()
        {
            InitializeComponent();
        }

        public async void ExecuteSearch(string searchQuery)
        {
            TxtSearchTitle.Text = $"Search Results for: '{searchQuery}'";
            string cleanQuery = searchQuery.ToLower().Trim();

            try
            {
                List<Book> allBooks = await _apiService.GetAllBooks();
                List<Author> allAuthors = await _apiService.GetAllAuthors(); 

                List<Book> filteredBooks = allBooks.Where(b =>
                    b.BookName.ToLower().Contains(cleanQuery) ||
                    (b.Information != null && b.Information.ToLower().Contains(cleanQuery))
                ).ToList();

                List<Author> filteredAuthors = allAuthors.Where(a =>
                    a.PenName != null && a.PenName.ToLower().Contains(cleanQuery)
                ).ToList();

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
                this.NavigationService?.Navigate(authorPage);
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
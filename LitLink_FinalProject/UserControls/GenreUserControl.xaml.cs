using Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LitLink_FinalProject.UserControls
{
    public partial class GenreUserControl : UserControl
    {
        public event EventHandler<Book> BookSelected;

        public GenreUserControl()
        {
            InitializeComponent();
        }

        public void SetupGenreRow(string genreName, List<Book> booksList)
        {
            TxtGenreTitle.Text = genreName;
            GenreBooksItemsControl.ItemsSource = booksList;
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            BooksScrollViewer.ScrollToHorizontalOffset(BooksScrollViewer.HorizontalOffset - 150);
        }

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            BooksScrollViewer.ScrollToHorizontalOffset(BooksScrollViewer.HorizontalOffset + 150);
        }

        private void BookImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Book clickedBook = element?.DataContext as Book;

            if (clickedBook != null)
            {
                BookSelected?.Invoke(this, clickedBook);
            }
        }
    }
}
using Model;
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

        private void BookImage_Loaded(object sender, RoutedEventArgs e)
        {
            Image imgControl = sender as Image;
            if (imgControl != null)
            {
                Book currentBook = imgControl.DataContext as Book;
                if (currentBook != null && !string.IsNullOrEmpty(currentBook.Cover))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(currentBook.Cover);
                        imgControl.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}
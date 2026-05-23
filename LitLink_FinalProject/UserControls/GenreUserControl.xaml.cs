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
    /// <summary>
    /// Interaction logic for GenreUserControl.xaml
    /// </summary>
    public partial class GenreUserControl : UserControl
    {
        public event EventHandler<Book> BookSelected;

        public GenreUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// פונקציה שמזריקה את הנתונים לשורת הז'אנר
        /// </summary>
        public void SetupGenreRow(string genreName, List<Book> booksList)
        {
            TxtGenreTitle.Text = genreName;
            GenreBooksItemsControl.ItemsSource = booksList;
        }

        // גלילה שמאלה בלחיצה על החץ השמאלי
        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            BooksScrollViewer.ScrollToHorizontalOffset(BooksScrollViewer.HorizontalOffset - 150);
        }

        // גלילה ימינה בלחיצה על החץ הימני
        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            BooksScrollViewer.ScrollToHorizontalOffset(BooksScrollViewer.HorizontalOffset + 150);
        }

        // בעת לחיצה על תמונת הספר, נפעיל את האירוע ונעביר את הספר שנבחר
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
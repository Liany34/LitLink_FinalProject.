using LitLink_FinalProject;
using LitLink_FinalProject.UserControls;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LitLink_FinalProject.Pages
{
    public partial class BookPage : Page
    {
        private Reader currentUser;

        public BookPage(Book bookData, bool userOwnsBook, bool isAdmin, bool isAuthor, Reader currentReader = null, Author currentAuthor = null)
        {
            InitializeComponent();

            this.currentUser = currentReader;

            BookUserControl bookUserControl =
                new BookUserControl(bookData, userOwnsBook, isAdmin, isAuthor, currentReader, currentAuthor);

            MainContainer.Children.Clear();
            MainContainer.Children.Add(bookUserControl);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}
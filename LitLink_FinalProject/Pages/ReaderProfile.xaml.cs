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
using LitLink_FinalProject.UserControls;

namespace LitLink_FinalProject.Pages
{
    public partial class ReaderProfile : Page
    {
        private Apiservice apiService = new Apiservice();
        private List<Book> allBooks = new List<Book>();
        private User currentUser;

        public ReaderProfile()
        {
            InitializeComponent();
            this.Loaded += ReaderProfilePage_Loaded;
            currentUser = this.DataContext as User;
        }

        private void ReaderProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();
        }

        private async void LoadUserData()
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please log in to view your profile.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
                return;
            }

            try
            {
                TxtHelloUser.Text = $"Hello, {currentUser.Username}";

                if (!string.IsNullOrEmpty(currentUser.Picture))
                {
                    try
                    {
                        ImgReaderProfile.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(currentUser.Picture, UriKind.RelativeOrAbsolute));
                    }
                    catch
                    {
                        // לשים סתם תמונה }
                    }
                }

                allBooks = await apiService.GetAllBooks();

                BuildUserLists();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading profile details: " + ex.Message);
            }
        }

        private async void BuildUserLists()
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabList);

            try
            {
                List<Book_Series> bookLists = await apiService.GetAllBookSeries();
                List<Book_Series> userCustomLists = bookLists.Where(l => l.IdUser.Id == currentUser.Id).ToList();

                if (userCustomLists == null || userCustomLists.Count == 0)
                {
                    ShowEmptyStateMessage("You haven't created any reading lists yet.");
                    return;
                }

                foreach (var currentList in userCustomLists)
                {
                    List<Series_Detail> allListDetails = await apiService.GetAllSeriesDetails();
                    List<Series_Detail> currentListDetails = allListDetails.Where(d => d.IdSeries.Id == currentList.Id).ToList();
                    List<Book> relatedBooks = allBooks.Where(b => currentListDetails.Any(d => d.IdBook.Id == b.Id)).ToList();

                    if (relatedBooks.Count == 0) continue;

                    GenreUserControl listRow = new GenreUserControl();
                    listRow.SetupGenreRow(currentList.NameSeries, relatedBooks);

                    listRow.BookSelected += UserRow_BookSelected;

                    UserListsContainer.Children.Add(listRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error rendering user lists: " + ex.Message);
            }
        }

        private async void UserRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null) return;

            try
            {
                List<Book> ownedBooks = new List<Book>();

                List<Book_Series> bookLists = await apiService.GetAllBookSeries();

                List<Book_Series> userCustomLists = bookLists.Where(l => l.IdUser.Id == currentUser.Id).ToList();

                List<Series_Detail> allListDetails = await apiService.GetAllSeriesDetails();

                List<Series_Detail> userListDetails = allListDetails.Where(d => userCustomLists.Any(l => l.Id == d.IdSeries.Id)).ToList();

                foreach (var detail in userListDetails)
                {
                    if (!ownedBooks.Any(b => b.Id == detail.IdBook.Id))
                    {
                        ownedBooks.Add(detail.IdBook);
                    }
                }

                bool ownsBook = ownedBooks.Any(b => b.Id == selectedBook.Id);


                BookPage detailsPage = new BookPage(selectedBook, ownsBook, false, false);
                this.NavigationService?.Navigate(detailsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading book availability: " + ex.Message, "LitLink Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void FilterList_Click(object sender, RoutedEventArgs e)
        {
            BuildUserLists();
        }

        private async void FilterReviews_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabReviews);

            ShowEmptyStateMessage("You haven't written any reviews yet.");
        }

        private async void FilterMyBooks_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabMyBooks);

            List<Book> ownedBooks = new List<Book>();
            List<Book_Series> bookLists = await apiService.GetAllBookSeries();
            List<Book_Series> userCustomLists = bookLists.Where(l => l.IdUser.Id == currentUser.Id).ToList();
            List<Series_Detail> allListDetails = await apiService.GetAllSeriesDetails();
            List<Series_Detail> userListDetails = allListDetails.Where(d => userCustomLists.Any(l => l.Id == d.IdSeries.Id)).ToList();
            foreach (var detail in userListDetails)
            {
                if (!ownedBooks.Any(b => b.Id == detail.IdBook.Id))
                {
                    ownedBooks.Add(detail.IdBook);
                }
            }

            if (ownedBooks == null || ownedBooks.Count == 0)
            {
                ShowEmptyStateMessage("You haven't purchased any books yet.");
                return;
            }

            
            GenreUserControl purchasedRow = new GenreUserControl();
            purchasedRow.SetupGenreRow("My Purchased Library", ownedBooks);
            purchasedRow.BookSelected += UserRow_BookSelected;
            UserListsContainer.Children.Add(purchasedRow);
        }

        private void FilterFollowing_Click(object sender, RoutedEventArgs e)
        {
            UserListsContainer.Children.Clear();
            HighlightActiveTab(BtnTabFollowing);

            ShowEmptyStateMessage("You aren't following any authors yet.");
        }


        private void BtnEditProfile_Click(object sender, RoutedEventArgs e) => EditProfilePopup.Visibility = Visibility.Visible;
        private void CloseEditProfile_Click(object sender, RoutedEventArgs e) => EditProfilePopup.Visibility = Visibility.Collapsed;

        private void OutsidePopup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == EditProfilePopup) EditProfilePopup.Visibility = Visibility.Collapsed;
        }


        private void Home_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/HomePage.xaml", UriKind.Relative));
        private void Cart_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/CartPage.xaml", UriKind.Relative));
        private void EditDetails_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/EditProfileDetails.xaml", UriKind.Relative));
        private void ResetPassword_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/ResetPass.xaml", UriKind.Relative));
        private void Preference_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Preferences layout option clicked!", "LitLink");
        private void Support_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Support option clicked! Connecting to help center...", "LitLink");

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentUser = null; 
            this.NavigationService?.Navigate(new Uri("Pages/LogOutPage.xaml", UriKind.Relative));
        }

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to permanently delete your LitLink account?\nThis action cannot be undone!",
                "Delete Account", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await apiService.DeleteUser(currentUser.Id);

                    currentUser = null; 
                    MessageBox.Show("Your account has been deleted successfully.", "LitLink");
                    this.NavigationService?.Navigate(new Uri("Pages/LogOutPage.xaml", UriKind.Relative));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete account: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void HighlightActiveTab(Button activeBtn)
        {
            BtnTabList.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabReviews.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabMyBooks.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabFollowing.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));

            activeBtn.Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private void ShowEmptyStateMessage(string message)
        {
            TextBlock txtEmpty = new TextBlock
            {
                Text = message,
                FontSize = 14,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            UserListsContainer.Children.Add(txtEmpty);
        }
    }
}

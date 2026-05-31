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
using LitLink_FinalProject.WindowsFile;

namespace LitLink_FinalProject.Pages
{
    public partial class AuthorProfile : Page
    {
        private Apiservice apiService = new Apiservice();
        private List<Book> authorBooks = new List<Book>();
        private Author currentAuthor;

        public AuthorProfile()
        {
            InitializeComponent();
            this.Loaded += AuthorProfilePage_Loaded;
            currentAuthor = this.DataContext as Author;
        }
        private void AuthorProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAuthorData();
        }

        private async void LoadAuthorData()
        {
            List<Author> allAuthors = await apiService.GetAllAuthors();
            if (currentAuthor == null || !allAuthors.Contains(currentAuthor))
            {
                MessageBox.Show("Unauthorized access. Redirecting to Home.", "LitLink Security", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.Navigate(new Uri("Pages/HomePage.xaml", UriKind.Relative));
                return;
            }

            try
            {
                if (currentAuthor != null)
                {
                    TxtHelloAuthor.Text = $"Hello, {currentAuthor.PenName}";

                    List<Following> allFollowings = await apiService.GetAllFollowings();
                    List<Following> authorFollowings = allFollowings.Where(f => f.IdAuthor.Id == currentAuthor.Id).ToList();

                    int followersCount = authorFollowings.Count;
                    TxtFollowersCount.Text = $"{followersCount} Followers";
                }

                string st = await apiService.GetPRPByUserIDByte64(currentAuthor.Id);
                if (currentAuthor != null && !string.IsNullOrEmpty(currentAuthor.Picture))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.ImgAuthorProfile.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    catch
                    {
                        this.ImgAuthorProfile.Source = new BitmapImage(new Uri("C:\\Users\\yahal\\source\\repos\\Liany34\\LitLink_Liany\\ViewModel\\PRP\\DefaultUser.png", UriKind.RelativeOrAbsolute));
                    }
                }
                else
                {
                    this.ImgAuthorProfile.Source = new BitmapImage(new Uri("C:\\Users\\yahal\\source\\repos\\Liany34\\LitLink_Liany\\ViewModel\\PRP\\DefaultUser.png", UriKind.RelativeOrAbsolute));
                }

                List<Book> allBooks = await apiService.GetAllBooks();
                authorBooks = allBooks.Where(b => b.IdAuthor.Id == currentAuthor.Id).ToList();

                LoadMyBooksTab();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading author profile: " + ex.Message);
            }
        }

        private void TabMyBooks_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabMyBooks);
            AuthorBooksContainer.Visibility = Visibility.Visible;
            AuthorNewsContainer.Visibility = Visibility.Collapsed;
            AuthorSalesContainer.Visibility = Visibility.Collapsed;
            LoadMyBooksTab();
        }

        private void TabMyNews_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabMyNews);
            AuthorBooksContainer.Visibility = Visibility.Collapsed;
            AuthorNewsContainer.Visibility = Visibility.Visible;
            AuthorSalesContainer.Visibility = Visibility.Collapsed;
            LoadMyNewsTab();
        }

        private void TabSalesData_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabSalesData);
            AuthorBooksContainer.Visibility = Visibility.Collapsed;
            AuthorNewsContainer.Visibility = Visibility.Collapsed;
            AuthorSalesContainer.Visibility = Visibility.Visible;
            LoadSalesDataTab();
        }

        private void LoadMyBooksTab()
        {
            AuthorBooksContainer.Children.Clear();

            if (authorBooks.Count == 0)
            {
                AuthorBooksContainer.Children.Add(CreateEmptyMessageTextBlock("You haven't published any books yet."));
                return;
            }

            GenreUserControl authorRow = new GenreUserControl();
            authorRow.SetupGenreRow("Published Works", authorBooks);

            authorRow.BookSelected += AuthorRow_BookSelected;

            AuthorBooksContainer.Children.Add(authorRow);
        }

        private void AuthorRow_BookSelected(object sender, Book selectedBook)
        {
            if (selectedBook == null) return;

            BookPage bookPage = new BookPage(selectedBook, true, false, true);
            this.NavigationService?.Navigate(bookPage);
        }

        private async void LoadMyNewsTab()
        {
            AuthorNewsContainer.Children.Clear();

            try
            {
                List<News> allNews = await apiService.GetAllNews();
                List<News> authorNews = allNews.Where(n => n.IdUser.Id == currentAuthor.Id).ToList();

                if (authorNews == null || authorNews.Count == 0)
                {
                    AuthorNewsContainer.Children.Add(CreateEmptyMessageTextBlock("No news updates published yet."));
                    return;
                }

                foreach (var news in authorNews)
                {
                    NewsUserControl newsControl = new NewsUserControl();

                    newsControl.DataContext = news;

                    newsControl.NewsChanged += () => { LoadMyNewsTab(); };

                    AuthorNewsContainer.Children.Add(newsControl);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error rendering author news control: " + ex.Message);
            }
        }
        private async void LoadSalesDataTab()
        {
            try
            {
                int bookThisMonth = 0;
                int bookTotal = 0;
                double incomeThisMonth = 0;
                double incomeTotal = 0;
                int booksAddedToCarts = 0;
                int booksAddedTolists = 0;
                int followersCount;
                List<Following> allFollowings = await apiService.GetAllFollowings();
                List<Following> authorFollowings = allFollowings.Where(f => f.IdAuthor.Id == currentAuthor.Id).ToList();
                followersCount = authorFollowings.Count;

                List<Cart_Detail> allCartDetails = await apiService.GetAllCartDetails();
                foreach (Cart_Detail cd in allCartDetails)
                {
                    if (cd.IdBook.IdAuthor.Id == currentAuthor.Id)
                    {
                        booksAddedToCarts++;
                        if (cd.IsPurchased)
                        {
                            if (cd.PurchaseDate?.Month == DateTime.Now.Month && cd.PurchaseDate?.Year == DateTime.Now.Year)
                            {
                                bookThisMonth++;
                                incomeThisMonth += cd.IdBook.Price ?? 0.0;
                            }
                            bookTotal++;
                            incomeTotal += cd.IdBook.Price ?? 0.0;
                        }
                    }
                }

                TxtTotalSales.Text = bookTotal.ToString();
                TxtTotalRevenue.Text = $"{incomeTotal:F2} ₪";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error calculating sales data: " + ex.Message);
            }
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e) => AuthorMenuPopup.Visibility = Visibility.Visible;
        private void CloseMenu_Click(object sender, RoutedEventArgs e) => AuthorMenuPopup.Visibility = Visibility.Collapsed;
        private void OutsideMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == AuthorMenuPopup) AuthorMenuPopup.Visibility = Visibility.Collapsed;
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (currentAuthor == null) return;

            EditAuthorProfileWindow editWin = new EditAuthorProfileWindow(currentAuthor);
            if (editWin.ShowDialog() == true)
            {
                LoadAuthorData();
            }
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            WindowsFile.AddBookWindow addBookWin = new WindowsFile.AddBookWindow();

            if (addBookWin.ShowDialog() == true)
            {
                LoadAuthorData();
            }
        }
        private void AddNews_Click(object sender, RoutedEventArgs e)
        {
            WindowsFile.AddNewsWindow addNewsWin = new WindowsFile.AddNewsWindow();

            if (addNewsWin.ShowDialog() == true)
            {
                LoadMyNewsTab();
            }
        }
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentAuthor = null;
            var signOut = new SignOut();
            Window.GetWindow(this).Content = signOut;
        }

        private void HighlightTab(Button activeBtn)
        {
            BtnTabMyBooks.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabMyNews.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabSalesData.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            activeBtn.Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private TextBlock CreateEmptyMessageTextBlock(string msg)
        {
            return new TextBlock { Text = msg, FontSize = 14, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0) };
        }
    }
}
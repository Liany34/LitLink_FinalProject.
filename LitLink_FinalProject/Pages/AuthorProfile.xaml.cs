using LitLink_FinalProject.Pages;
using LitLink_FinalProject.UserControls;
using LitLink_FinalProject.WindowsFile;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace LitLink_FinalProject.Pages
{
    public partial class AuthorProfile : Page
    {
        private Apiservice apiService = new Apiservice();
        private List<Book> authorBooks = new List<Book>();
        private Author currentAuthor;
        private Reader viewingReader;

        public AuthorProfile(Author author, Reader viewingReader = null)
        {
            InitializeComponent();
            this.currentAuthor = author;
            this.viewingReader = viewingReader;
            this.Loaded += AuthorProfilePage_Loaded;
        }

        private void AuthorProfilePage_Loaded(object sender, RoutedEventArgs e) => LoadAuthorData();

        private async void LoadAuthorData()
        {
            if (currentAuthor == null)
            {
                MessageBox.Show("Author data is missing.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                TxtHelloAuthor.Text = viewingReader != null
                    ? currentAuthor.PenName
                    : $"Hello, {currentAuthor.PenName}";

                List<Following> allFollowings = await apiService.GetAllFollowings();
                List<Following> authorFollowings = allFollowings
                    .Where(f => f.IdAuthor != null && f.IdAuthor.Id == currentAuthor.Id)
                    .ToList();
                TxtFollowersCount.Text = $"{authorFollowings.Count} Followers";
                if (viewingReader != null)
                {
                    BtnEditProfile.Visibility = Visibility.Collapsed;
                    BtnMenu.Visibility = Visibility.Collapsed;
                    BtnTabSalesData.Visibility = Visibility.Collapsed;
                    BtnTabMyLists.Visibility = Visibility.Visible;
                    BtnBack.Visibility = Visibility.Visible;

                    bool isFollowing = authorFollowings.Any(f => f.IdReader != null && f.IdReader.Id == viewingReader.Id);
                    BtnFollow.Visibility = isFollowing ? Visibility.Collapsed : Visibility.Visible;
                    BtnUnfollow.Visibility = isFollowing ? Visibility.Visible : Visibility.Collapsed;
                }

                string st = await apiService.GetPictureByUserIDByte64(currentAuthor.Id);
                if (!string.IsNullOrEmpty(st))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.ImgAuthorProfile.Source = ByteImageConverter.ByteToImage(imgStr);
                        Console.WriteLine("Admin profile image loaded from byte array.");
                    }
                    catch 
                    {
                        SetDefaultAuthorImage(); 
                    }
                }
                else SetDefaultAuthorImage();

                List<Book> allBooks = await apiService.GetAllBooks();
                authorBooks = allBooks.Where(b => b.IdAuthor != null && b.IdAuthor.Id == currentAuthor.Id).ToList();
                LoadMyBooksTab();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading author profile: " + ex.Message);
            }
        }

        private void SetDefaultAuthorImage()
        {
            try 
            {
                this.ImgAuthorProfile.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/UserPicture1.png", UriKind.Absolute)); 
            }
            catch 
            {
                this.ImgAuthorProfile.Source = null; 
            }
        }

        private void TabMyBooks_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabMyBooks);
            AuthorBooksContainer.Visibility = Visibility.Visible;
            AuthorListsContainer.Visibility = Visibility.Collapsed;
            AuthorNewsContainer.Visibility = Visibility.Collapsed;
            AuthorSalesContainer.Visibility = Visibility.Collapsed;
            LoadMyBooksTab();
        }

        private void TabMyNews_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabMyNews);
            AuthorBooksContainer.Visibility = Visibility.Collapsed;
            AuthorListsContainer.Visibility = Visibility.Collapsed;
            AuthorNewsContainer.Visibility = Visibility.Visible;
            AuthorSalesContainer.Visibility = Visibility.Collapsed;
            LoadMyNewsTab();
        }

        private void TabSalesData_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabSalesData);
            AuthorBooksContainer.Visibility = Visibility.Collapsed;
            AuthorListsContainer.Visibility = Visibility.Collapsed;
            AuthorNewsContainer.Visibility = Visibility.Collapsed;
            AuthorSalesContainer.Visibility = Visibility.Visible;
            LoadSalesDataTab();
        }

        private void TabMyLists_Click(object sender, RoutedEventArgs e)
        {
            HighlightTab(BtnTabMyLists);
            AuthorBooksContainer.Visibility = Visibility.Collapsed;
            AuthorListsContainer.Visibility = Visibility.Visible;
            AuthorNewsContainer.Visibility = Visibility.Collapsed;
            AuthorSalesContainer.Visibility = Visibility.Collapsed;
            LoadMyListsTab();
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
            BookPage bookPage = new BookPage(selectedBook, false, false, viewingReader == null, viewingReader);
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
                int bookThisMonth = 0, bookTotal = 0;
                double incomeThisMonth = 0, incomeTotal = 0;
                List<Cart_Detail> allCartDetails = await apiService.GetAllCartDetails();
                foreach (Cart_Detail cd in allCartDetails)
                {
                    if (cd.IdBook.IdAuthor.Id == currentAuthor.Id)
                    {
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

        private async void LoadMyListsTab()
        {
            AuthorListsContainer.Children.Clear();
            AuthorListsContainer.Children.Add(new TextBlock
            {
                Text = $"Book Series by {currentAuthor.PenName}",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74)),
                Margin = new Thickness(0, 0, 0, 15)
            });
            try
            {
                List<Book_Series> allSeries = await apiService.GetAllBookSeries();
                List<Book_Series> authorSeries = allSeries
                    .Where(s => s.IdUser != null && s.IdUser.Id == currentAuthor.Id)
                    .ToList();
                if (authorSeries.Count == 0)
                {
                    AuthorListsContainer.Children.Add(CreateEmptyMessageTextBlock("No book lists created yet."));
                    return;
                }
                List<Series_Detail> allDetails = await apiService.GetAllSeriesDetails();
                foreach (Book_Series series in authorSeries)
                {
                    List<Book> seriesBooks = allDetails
                        .Where(d => d.IdSeries != null && d.IdSeries.Id == series.Id && d.IdBook != null)
                        .OrderBy(d => d.Number)
                        .Select(d => d.IdBook)
                        .ToList();
                    if (seriesBooks.Count == 0) continue;
                    GenreUserControl seriesRow = new GenreUserControl();
                    seriesRow.SetupGenreRow(series.NameSeries, seriesBooks);
                    seriesRow.BookSelected += AuthorRow_BookSelected;
                    AuthorListsContainer.Children.Add(seriesRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading author lists: " + ex.Message);
            }
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e) => AuthorMenuPopup.Visibility = Visibility.Visible;
        private void CloseMenu_Click(object sender, RoutedEventArgs e) => AuthorMenuPopup.Visibility = Visibility.Collapsed;
        private void OutsideMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == AuthorMenuPopup) 
                AuthorMenuPopup.Visibility = Visibility.Collapsed; 
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (currentAuthor == null) return;
            EditAuthorProfileWindow editWin = new EditAuthorProfileWindow(currentAuthor);
            if (editWin.ShowDialog() == true) LoadAuthorData();
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            AddBookWindow addBookWin = new AddBookWindow();
            if (addBookWin.ShowDialog() == true) LoadAuthorData();
        }

        private void AddNews_Click(object sender, RoutedEventArgs e)
        {
            AddNewsWindow addNewsWin = new AddNewsWindow();
            addNewsWin.DataContext = currentAuthor;
            if (addNewsWin.ShowDialog() == true) LoadMyNewsTab();
        }

        private async void AddList_Click(object sender, RoutedEventArgs e)
        {
            AddSeriesWindow win = new AddSeriesWindow(currentAuthor);
            if (win.ShowDialog() == true) LoadMyListsTab();
        }

        private async void AddBookToList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Book_Series> allSeries = await apiService.GetAllBookSeries();
                List<Book_Series> authorSeries = allSeries
                    .Where(s => s.IdUser != null && s.IdUser.Id == currentAuthor.Id)
                    .ToList();
                if (authorSeries.Count == 0)
                {
                    MessageBox.Show("Create a list first.", "LitLink");
                    return;
                }
                AddBookToSeriesWindow win = new AddBookToSeriesWindow(currentAuthor.Id, authorSeries, authorBooks);
                if (win.ShowDialog() == true) LoadMyListsTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "LitLink");
            }
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            currentAuthor = null;
            MainWindow.AppFrame.Navigate(new SignOut());
        }

        private async void BtnFollow_Click(object sender, RoutedEventArgs e)
        {
            if (viewingReader == null || currentAuthor == null) return;
            try
            {
                await apiService.InsertFollowing(new Following { IdReader = viewingReader, IdAuthor = currentAuthor });
                BtnFollow.Visibility = Visibility.Collapsed;
                BtnUnfollow.Visibility = Visibility.Visible;
                List<Following> all = await apiService.GetAllFollowings();
                TxtFollowersCount.Text = $"{all.Count(f => f.IdAuthor != null && f.IdAuthor.Id == currentAuthor.Id)} Followers";
            }
            catch (Exception ex) { MessageBox.Show("Error following author: " + ex.Message, "LitLink"); }
        }

        private async void BtnUnfollow_Click(object sender, RoutedEventArgs e)
        {
            if (viewingReader == null || currentAuthor == null) return;
            try
            {
                List<Following> all = await apiService.GetAllFollowings();
                Following toDelete = all.FirstOrDefault(f =>
                    f.IdReader != null && f.IdReader.Id == viewingReader.Id &&
                    f.IdAuthor != null && f.IdAuthor.Id == currentAuthor.Id);
                if (toDelete != null) await apiService.DeleteFollowing(toDelete.Id);
                BtnFollow.Visibility = Visibility.Visible;
                BtnUnfollow.Visibility = Visibility.Collapsed;
                List<Following> updated = await apiService.GetAllFollowings();
                TxtFollowersCount.Text = $"{updated.Count(f => f.IdAuthor != null && f.IdAuthor.Id == currentAuthor.Id)} Followers";
            }
            catch (Exception ex) { MessageBox.Show("Error unfollowing author: " + ex.Message, "LitLink"); }
        }

        private void HighlightTab(Button activeBtn)
        {
            BtnTabMyBooks.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabMyLists.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabMyNews.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabSalesData.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            activeBtn.Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private TextBlock CreateEmptyMessageTextBlock(string msg) =>
            new TextBlock { Text = msg, FontSize = 14, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0) };

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
            else
                MainWindow.AppFrame.Navigate(new HomePage(viewingReader));
        }
    }
}
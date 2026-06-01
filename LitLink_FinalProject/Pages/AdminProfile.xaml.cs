using LitLink_FinalProject.Pages;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LitLink_FinalProject.Pages
{
    public partial class AdminProfile : Page
    {
        private Apiservice apiService = new Apiservice();
        private List<Author> allAuthors = new List<Author>();
        private Admin currentAdmin;

        private List<Reader> reportedUsers = new List<Reader>();
        private List<Book> reportedBooks = new List<Book>();
        private List<Reviews> reportedReviews = new List<Reviews>();
        private List<DiscountCodes> localCoupons = new List<DiscountCodes>();

        public AdminProfile()
        {
            InitializeComponent();
            this.Loaded += AdminProfilePage_Loaded;
        }

        public AdminProfile(Admin admin) : this()
        {
            this.DataContext = admin;
        }

        private async void AdminProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            currentAdmin = this.DataContext as Admin;
            if (currentAdmin == null) { await Task.Delay(50); currentAdmin = this.DataContext as Admin; }
            if (currentAdmin == null)
            {
                MessageBox.Show("Unauthorized access.", "LitLink Security", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TxtHelloAdmin.Text = $"Hello, {currentAdmin.Username}";
            DpValidUntil.SelectedDate = DateTime.Now.AddMonths(1);
            LoadAdminImage();
            await LoadAllDataFromServer();
        }

        private async Task LoadAllDataFromServer()
        {
            try
            {
                allAuthors = await apiService.GetAllAuthors() ?? new List<Author>();
                localCoupons = await apiService.GetAllDiscountCodes() ?? new List<DiscountCodes>();

                List<Reader> localReaders = await apiService.GetAllReaders() ?? new List<Reader>();
                List<Book> localBooks = await apiService.GetAllBooks() ?? new List<Book>();
                List<Reviews> localReviews = await apiService.GetAllReviews() ?? new List<Reviews>();

                reportedUsers = localReaders.Where(r => r.IsFlaged).ToList();
                reportedBooks = localBooks.Where(b => b.IsFlaged).ToList();
                reportedReviews = localReviews.Where(r => r.IsFlaged).ToList();

                CmbAuthors.ItemsSource = null;
                CmbAuthors.ItemsSource = allAuthors;
                CmbAuthors.IsEnabled = true;
                CmbAuthors.IsHitTestVisible = true;

                LvwCoupons.ItemsSource = null;
                LvwCoupons.ItemsSource = localCoupons;

                await CalculateAndDisplaySales(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בקבלת נתונים מהשרת: {ex.Message}", "Error System");
            }
        }

        private async void CmbAuthors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbAuthors.SelectedItem == null) return;
            Author selectedAuthor = CmbAuthors.SelectedItem as Author;
            if (selectedAuthor == null) return;
            TxtSelectedAuthorTitle.Text = $"Sales Overview for: {selectedAuthor.PenName}";
            AuthorSalesResultCard.Visibility = Visibility.Visible;
            await CalculateAndDisplaySales(selectedAuthor.Id);
        }

        private async void LoadAdminImage()
        {
            try
            {
                string st = await apiService.GetPRPByUserIDByte64(currentAdmin.Id);
                if (!string.IsNullOrEmpty(st))
                {
                    byte[] imgStr = Convert.FromBase64String(st);
                    this.ImgAdminProfile.Source = ByteImageConverter.ByteToImage(imgStr);
                }
                else if (!string.IsNullOrEmpty(currentAdmin.Picture))
                    this.ImgAdminProfile.Source = new BitmapImage(new Uri(currentAdmin.Picture, UriKind.RelativeOrAbsolute));
                else
                    SetDefaultImage();
            }
            catch { SetDefaultImage(); }
        }

        private void SetDefaultImage()
        {
            try { this.ImgAdminProfile.Source = new BitmapImage(new Uri("pack://application:,,,/Covers/DefultUser.png", UriKind.Absolute)); }
            catch { this.ImgAdminProfile.Source = null; }
        }

        private async Task CalculateAndDisplaySales(int? targetAuthorId)
        {
            int bookThisMonth = 0, bookTotal = 0;
            double incomeThisMonth = 0, incomeTotal = 0;
            try
            {
                List<Cart_Detail> allCartDetails = await apiService.GetAllCartDetails();
                if (allCartDetails != null)
                {
                    foreach (Cart_Detail cd in allCartDetails)
                    {
                        if (cd.IdBook == null) continue;
                        bool matchesAuthor = targetAuthorId == null ||
                            (cd.IdBook.IdAuthor != null && cd.IdBook.IdAuthor.Id == targetAuthorId);
                        if (matchesAuthor && cd.IsPurchased)
                        {
                            double price = cd.IdBook.Price ?? 0.0;
                            bookTotal++;
                            incomeTotal += price;
                            if (cd.PurchaseDate?.Month == DateTime.Now.Month && cd.PurchaseDate?.Year == DateTime.Now.Year)
                            { bookThisMonth++; incomeThisMonth += price; }
                        }
                    }
                }
                if (targetAuthorId == null)
                {
                    TxtGlobalSales.Text = bookTotal.ToString();
                    TxtGlobalRevenue.Text = $"{incomeTotal:F2} ₪";
                }
                else
                {
                    AuthorSalesResultCard.Visibility = Visibility.Visible;
                    TxtAuthorSoldCopies.Text = $"Copies Sold This Month: {bookThisMonth} (Total: {bookTotal})";
                    TxtAuthorRevenue.Text = $"Revenue: {incomeTotal:F2} ₪ (This Month: {incomeThisMonth:F2} ₪)";
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error calculating sales data: " + ex.Message); }
        }

        private void LoadReportsData()
        {
            ReportsContainer.Children.Clear();
            if (reportedBooks.Count == 0 && reportedReviews.Count == 0 && reportedUsers.Count == 0)
            {
                ReportsContainer.Children.Add(new TextBlock { Text = "No active reports pending review. ✨", FontSize = 14, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0) });
                return;
            }
            foreach (Book report in reportedBooks.ToList())
                ReportsContainer.Children.Add(CreateReportCard($"[BOOK] Title: {report.BookName} | ID: {report.Id}", async () => { reportedBooks.Remove(report); report.IsFlaged = false; await apiService.UpdateBook(report); }, async () => { reportedBooks.Remove(report); await apiService.DeleteBook(report.Id); }));
            foreach (Reader reportU in reportedUsers.ToList())
                ReportsContainer.Children.Add(CreateReportCard($"[USER] Username: {reportU.Username} | ID: {reportU.Id}", async () => { reportedUsers.Remove(reportU); reportU.IsFlaged = false; await apiService.UpdateReader(reportU); }, async () => { reportedUsers.Remove(reportU); await apiService.DeleteReader(reportU.Id); }));
            foreach (Reviews reportR in reportedReviews.ToList())
                ReportsContainer.Children.Add(CreateReportCard($"[REVIEW] \"{reportR.Text}\" | ID: {reportR.Id}", async () => { reportedReviews.Remove(reportR); reportR.IsFlaged = false; await apiService.UpdateReview(reportR); }, async () => { reportedReviews.Remove(reportR); await apiService.DeleteReview(reportR.Id); }));
        }

        private Border CreateReportCard(string infoText, Func<Task> onDismiss, Func<Task> onDelete)
        {
            Border reportCard = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(15), Margin = new Thickness(0, 0, 0, 10), Effect = (System.Windows.Media.Effects.Effect)FindResource("AdminShadow") };
            StackPanel sp = new StackPanel();
            sp.Children.Add(new TextBlock { Text = infoText, FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74)), TextWrapping = TextWrapping.Wrap });
            StackPanel btnSp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            Button btnDismiss = new Button { Content = "Dismiss Report", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Gray, Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 15, 0) };
            Button btnDeleteTarget = new Button { Content = "Delete Item", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Red, Cursor = Cursors.Hand, FontWeight = FontWeights.Bold };
            btnDismiss.Click += async (s, e) => { await onDismiss(); LoadReportsData(); };
            btnDeleteTarget.Click += async (s, e) => { if (MessageBox.Show("Delete permanently?", "LitLink System", MessageBoxButton.YesNo) == MessageBoxResult.Yes) { await onDelete(); LoadReportsData(); } };
            btnSp.Children.Add(btnDismiss); btnSp.Children.Add(btnDeleteTarget);
            sp.Children.Add(btnSp); reportCard.Child = sp;
            return reportCard;
        }

        private void LoadCouponsData() { LvwCoupons.ItemsSource = null; LvwCoupons.ItemsSource = localCoupons; }

        private async void BtnCreateCoupon_Click(object sender, RoutedEventArgs e)
        {
            string code = TxtNewCouponCode.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code) || !int.TryParse(TxtNewCouponAmount.Text, out int amount) || DpValidUntil.SelectedDate == null)
            { MessageBox.Show("Please enter a valid code, amount and date.", "LitLink"); return; }
            int newId = localCoupons.Count > 0 ? localCoupons.Max(c => c.Id) + 1 : 1;
            DiscountCodes newCoupon = new DiscountCodes { Id = newId, CodeText = code, Amount = amount, IsActive = true, ValidUntil = DpValidUntil.SelectedDate.Value };
            localCoupons.Add(newCoupon);
            MessageBox.Show($"Coupon Code '{code}' created! ✨", "LitLink");
            TxtNewCouponCode.Text = ""; TxtNewCouponAmount.Text = "";
            DpValidUntil.SelectedDate = DateTime.Now.AddMonths(1);
            LoadCouponsData();
            await apiService.InsertDiscountCode(newCoupon);
        }

        private async void BtnDeleteCoupon_Click(object sender, RoutedEventArgs e)
        {
            var coupon = (sender as FrameworkElement)?.DataContext as DiscountCodes;
            if (coupon == null) return;
            if (MessageBox.Show($"Delete coupon code '{coupon.CodeText}'?", "LitLink", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            { localCoupons.Remove(coupon); try { await apiService.DeleteDiscountCode(coupon.Id); } catch { } LoadCouponsData(); }
        }

        private void TabNews_Click(object sender, RoutedEventArgs e) { HighlightTab(BtnTabNews); PanelSales.Visibility = Visibility.Collapsed; PanelReports.Visibility = Visibility.Collapsed; PanelDiscounts.Visibility = Visibility.Collapsed; PanelNews.Visibility = Visibility.Visible; LoadNewsTab(); }
        private async void TabSales_Click(object sender, RoutedEventArgs e) { HighlightTab(BtnTabSales); PanelSales.Visibility = Visibility.Visible; PanelReports.Visibility = Visibility.Collapsed; PanelDiscounts.Visibility = Visibility.Collapsed; PanelNews.Visibility = Visibility.Collapsed; await CalculateAndDisplaySales(null); }
        private void TabReports_Click(object sender, RoutedEventArgs e) { HighlightTab(BtnTabReports); PanelSales.Visibility = Visibility.Collapsed; PanelReports.Visibility = Visibility.Visible; PanelDiscounts.Visibility = Visibility.Collapsed; PanelNews.Visibility = Visibility.Collapsed; LoadReportsData(); }
        private void TabDiscounts_Click(object sender, RoutedEventArgs e) { HighlightTab(BtnTabDiscounts); PanelSales.Visibility = Visibility.Collapsed; PanelReports.Visibility = Visibility.Collapsed; PanelDiscounts.Visibility = Visibility.Visible; PanelNews.Visibility = Visibility.Collapsed; LoadCouponsData(); }

        private async void LoadNewsTab()
        {
            NewsContainer.Children.Clear();
            try
            {
                List<News> allNews = await apiService.GetAllNews();
                List<News> adminNews = allNews.Where(n => n.IdUser != null && n.IdUser.Id == currentAdmin.Id).ToList();
                if (adminNews.Count == 0) { NewsContainer.Children.Add(new TextBlock { Text = "No news published yet.", FontSize = 14, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0) }); return; }
                foreach (var news in adminNews)
                {
                    LitLink_FinalProject.UserControls.NewsUserControl nc = new LitLink_FinalProject.UserControls.NewsUserControl();
                    nc.DataContext = news;
                    nc.NewsChanged += () => { LoadNewsTab(); };
                    NewsContainer.Children.Add(nc);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error loading admin news: " + ex.Message); }
        }

        private void HighlightTab(Button activeBtn)
        {
            if (activeBtn == null) return;
            BtnTabSales.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabReports.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabDiscounts.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabNews.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            activeBtn.Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e) => AdminMenuPopup.Visibility = Visibility.Visible;
        private void CloseMenu_Click(object sender, RoutedEventArgs e) => AdminMenuPopup.Visibility = Visibility.Collapsed;
        private void OutsideMenu_MouseDown(object sender, MouseButtonEventArgs e) { if (e.OriginalSource == AdminMenuPopup) AdminMenuPopup.Visibility = Visibility.Collapsed; }

        private void AddNews_Click(object sender, RoutedEventArgs e)
        {
            var newsWindow = new WindowsFile.AddNewsWindow();
            newsWindow.DataContext = this.currentAdmin;
            newsWindow.ShowDialog();
        }

        private void LogOut_Click(object sender, RoutedEventArgs e) { currentAdmin = null; MainWindow.AppFrame.Navigate(new SignOut()); }
    }
}
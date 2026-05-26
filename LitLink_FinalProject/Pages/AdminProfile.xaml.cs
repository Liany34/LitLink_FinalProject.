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
    public partial class AdminProfile : Page
    {
        private Apiservice apiService = new Apiservice();
        private List<Author> allAuthors = new List<Author>();
        private User currentUser;

        private List<Reader> reportedUsers = new List<Reader>();
        private List<Book> reportedBooks = new List<Book>();
        private List<Reviews> reportedReviews = new List<Reviews>();
        private List<DiscountCodes> localCoupons = new List<DiscountCodes>();

        public AdminProfile()
        {
            InitializeComponent();
            this.Loaded += AdminProfilePage_Loaded;
            currentUser = this.DataContext as User;
            InitLocalMockData();
        }

        private async void InitLocalMockData()
        {
            List<Reader> localReaders = await apiService.GetAllReaders();
            reportedUsers = localReaders.Where(r => r.IsFlaged).ToList();
            List<Book> localBooks = await apiService.GetAllBooks();
            reportedBooks = localBooks.Where(r => r.IsFlaged).ToList();
            List<Reviews> localReviews = await apiService.GetAllReviews();
            reportedReviews = localReviews.Where(r => r.IsFlaged).ToList();

            localCoupons = await apiService.GetAllDiscountCodes();
        }

        private void AdminProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAdminDashboard();
        }

        private async void LoadAdminDashboard()
        {
            List<Author> allAuthors = await apiService.GetAllAuthors();
            if (currentUser == null || !allAuthors.Contains(currentUser))
            {
                MessageBox.Show("Unauthorized access.", "LitLink Security", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.Navigate(new Uri("Pages/AdminProfile.xaml", UriKind.Relative));
                return;
            }

            TxtHelloAdmin.Text = $"Hello, {currentUser.Username}";

            if (PanelSales.Visibility == Visibility.Visible) LoadSalesData();
            else if (PanelReports.Visibility == Visibility.Visible) LoadReportsData();
            else if (PanelDiscounts.Visibility == Visibility.Visible) LoadCouponsData();
        }


        private async void LoadSalesData()
        {
            try
            {
                await CalculateAndDisplaySales(null);

                allAuthors = await apiService.GetAllAuthors();
                CmbAuthors.ItemsSource = allAuthors;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error initialising sales tab: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task CalculateAndDisplaySales(int? targetAuthorId)
        {
            int bookThisMonth = 0;
            int bookTotal = 0;
            double incomeThisMonth = 0;
            double incomeTotal = 0;
            int booksAddedToCarts = 0;

            try
            {
                List<Cart_Detail> allCartDetails = await apiService.GetAllCartDetails();

                foreach (Cart_Detail cd in allCartDetails)
                {
                    if (targetAuthorId == null || (cd.IdBook != null && cd.IdBook.IdAuthor != null && cd.IdBook.IdAuthor.Id == targetAuthorId))
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error calculating sales data: " + ex.Message);
            }
        }

        private async void CmbAuthors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Author selectedAuthor = CmbAuthors.SelectedItem as Author;
            if (selectedAuthor == null) return;

            TxtSelectedAuthorTitle.Text = $"Sales Overview for: {selectedAuthor.PenName}";
            await CalculateAndDisplaySales(selectedAuthor.Id);
        }

        private void LoadReportsData()
        {
            ReportsContainer.Children.Clear();
            if (reportedBooks.Count == 0 && reportedReviews.Count == 0 && reportedUsers.Count == 0)
            {
                ReportsContainer.Children.Add(new TextBlock { Text = "No active reports pending review. ✨", FontSize = 14, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 40, 0, 0) });
                return;
            }
            else
            {
                if (reportedBooks.Count > 0)
                {
                    foreach (Book report in reportedBooks)
                    {
                        Border reportCard = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(15), Margin = new Thickness(0, 0, 0, 10), Effect = (System.Windows.Media.Effects.Effect)FindResource("AdminShadow") };
                        StackPanel sp = new StackPanel();

                        sp.Children.Add(new TextBlock { Text = $"Reported Item ID: {report.Id}", FontSize = 11, Foreground = Brushes.Gray });

                        StackPanel btnSp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
                        Button btnDismiss = new Button { Content = "Dismiss Report", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Gray, Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 15, 0) };
                        Button btnDeleteTarget = new Button { Content = $"Delete Reporte", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Red, Cursor = Cursors.Hand, FontWeight = FontWeights.Bold };

                        btnDismiss.Click += async (s, e) =>
                        {
                            reportedBooks.Remove(report);
                            MessageBox.Show("Report dismissed successfully.", "LitLink Control");
                            LoadReportsData(); 
                            report.IsFlaged = false; 
                            await apiService.UpdateBook(report); 
                        };

                        btnDeleteTarget.Click += async (s, e) =>
                        {
                            if (MessageBox.Show($"Are you sure you want to delete this reporte?", "LitLink System", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                reportedBooks.Remove(report);
                                MessageBox.Show($"The reported book has been removed from the platform.", "LitLink");
                                LoadReportsData(); 
                                await apiService.DeleteBook(report.Id); 
                            }
                        };

                        btnSp.Children.Add(btnDismiss); btnSp.Children.Add(btnDeleteTarget);
                        sp.Children.Add(btnSp); reportCard.Child = sp;
                        ReportsContainer.Children.Add(reportCard);
                    }
                }
                if (reportedUsers.Count > 0)
                {
                    foreach (Reader reportU in reportedUsers)
                    {
                        Border reportCard = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(15), Margin = new Thickness(0, 0, 0, 10), Effect = (System.Windows.Media.Effects.Effect)FindResource("AdminShadow") };
                        StackPanel sp = new StackPanel();

                        sp.Children.Add(new TextBlock { Text = $"Reported Item ID: {reportU.Id}", FontSize = 11, Foreground = Brushes.Gray });

                        StackPanel btnSp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
                        Button btnDismiss = new Button { Content = "Dismiss Report", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Gray, Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 15, 0) };
                        Button btnDeleteTarget = new Button { Content = $"Delete Reporte", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Red, Cursor = Cursors.Hand, FontWeight = FontWeights.Bold };

                        btnDismiss.Click += async (s, e) =>
                        {
                            reportedUsers.Remove(reportU);
                            MessageBox.Show("Report dismissed successfully.", "LitLink Control");
                            LoadReportsData(); 
                            reportU.IsFlaged = false; 
                            await apiService.UpdateReader(reportU); 
                        };

                        btnDeleteTarget.Click += async (s, e) =>
                        {
                            if (MessageBox.Show($"Are you sure you want to delete this reporte?", "LitLink System", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                reportedUsers.Remove(reportU);
                                MessageBox.Show($"The reported user has been removed from the platform.", "LitLink");
                                LoadReportsData(); 
                                await apiService.DeleteReader(reportU.Id); 
                            }
                        };

                        btnSp.Children.Add(btnDismiss); btnSp.Children.Add(btnDeleteTarget);
                        sp.Children.Add(btnSp); reportCard.Child = sp;
                        ReportsContainer.Children.Add(reportCard);
                    }
                }
                if (reportedReviews.Count > 0)
                {
                    foreach (Reviews report in reportedReviews)
                    {
                        Border reportCard = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(15), Margin = new Thickness(0, 0, 0, 10), Effect = (System.Windows.Media.Effects.Effect)FindResource("AdminShadow") };
                        StackPanel sp = new StackPanel();

                        sp.Children.Add(new TextBlock { Text = $"Reported Item ID: {report.Id}", FontSize = 11, Foreground = Brushes.Gray });

                        StackPanel btnSp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
                        Button btnDismiss = new Button { Content = "Dismiss Report", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Gray, Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 15, 0) };
                        Button btnDeleteTarget = new Button { Content = $"Delete Reporte", Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = Brushes.Red, Cursor = Cursors.Hand, FontWeight = FontWeights.Bold };

                        btnDismiss.Click += async (s, e) =>
                        {
                            reportedReviews.Remove(report);
                            MessageBox.Show("Report dismissed successfully.", "LitLink Control");
                            LoadReportsData(); 
                            report.IsFlaged = false; 
                            await apiService.UpdateReview(report); 
                        };

                        btnDeleteTarget.Click += async (s, e) =>
                        {
                            if (MessageBox.Show($"Are you sure you want to delete this reporte?", "LitLink System", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                reportedReviews.Remove(report);
                                MessageBox.Show($"The reported review has been removed from the platform.", "LitLink");
                                LoadReportsData(); 
                                await apiService.DeleteReview(report.Id); 
                            }
                        };

                        btnSp.Children.Add(btnDismiss); btnSp.Children.Add(btnDeleteTarget);
                        sp.Children.Add(btnSp); reportCard.Child = sp;
                        ReportsContainer.Children.Add(reportCard);
                    }
                }
            }
        }


        private void LoadCouponsData()
        {
            LvwCoupons.ItemsSource = null;
            LvwCoupons.ItemsSource = localCoupons; 
        }

        private async void BtnCreateCoupon_Click(object sender, RoutedEventArgs e)
        {
            string code = TxtNewCouponCode.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code) || !int.TryParse(TxtNewCouponAmount.Text, out int amount))
            {
                MessageBox.Show("Please enter a valid code and amount.", "LitLink");
                return;
            }

            int newId = localCoupons.Count > 0 ? localCoupons.Max(c => c.Id) + 1 : 1;

            DiscountCodes newCoupon = new DiscountCodes
            {
                Id = newId,
                CodeText = code,       
                Amount = amount,       
                IsActive = true,       
                ValidUntil = DateTime.Now.AddMonths(1) 
            };

            localCoupons.Add(newCoupon);

            MessageBox.Show($"Coupon Code '{code}' created and activated! ✨", "LitLink");
            TxtNewCouponCode.Text = ""; TxtNewCouponAmount.Text = "";
            LoadCouponsData(); 

            await apiService.InsertDiscountCode(newCoupon);
        }

        private async void BtnDeleteCoupon_Click(object sender, RoutedEventArgs e)
        {
            var coupon = (sender as FrameworkElement)?.DataContext as DiscountCodes;
            if (coupon == null) return;

            if (MessageBox.Show($"Delete coupon code '{coupon.CodeText}'?", "LitLink", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                localCoupons.Remove(coupon);

                try
                {
                    await apiService.DeleteDiscountCode(coupon.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete from server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                LoadCouponsData();
            }
        }

        private void TabSales_Click(object sender, RoutedEventArgs e) { HighlightTab(BtnTabSales); PanelSales.Visibility = Visibility.Visible; PanelReports.Visibility = Visibility.Collapsed; PanelDiscounts.Visibility = Visibility.Collapsed; LoadSalesData(); }
        private void TabReports_Click(object sender, RoutedEventArgs e) { HighlightTab(BtnTabReports); PanelSales.Visibility = Visibility.Collapsed; PanelReports.Visibility = Visibility.Visible; PanelDiscounts.Visibility = Visibility.Collapsed; LoadReportsData(); }
        private void TabDiscounts_Click(object sender, RoutedEventArgs e) { HighlightTab(BtnTabDiscounts); PanelSales.Visibility = Visibility.Collapsed; PanelReports.Visibility = Visibility.Collapsed; PanelDiscounts.Visibility = Visibility.Visible; LoadCouponsData(); }

        private void HighlightTab(Button activeBtn)
        {
            BtnTabSales.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabReports.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            BtnTabDiscounts.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
            activeBtn.Foreground = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e) => AdminMenuPopup.Visibility = Visibility.Visible;
        private void CloseMenu_Click(object sender, RoutedEventArgs e) => AdminMenuPopup.Visibility = Visibility.Collapsed;
        private void OutsideMenu_MouseDown(object sender, MouseButtonEventArgs e) { if (e.OriginalSource == AdminMenuPopup) AdminMenuPopup.Visibility = Visibility.Collapsed; }

        private void AddNews_Click(object sender, RoutedEventArgs e) { new WindowsFile.AddNewsWindow().ShowDialog(); }
        private void AboutUs_Click(object sender, RoutedEventArgs e) => this.NavigationService?.Navigate(new Uri("Pages/AboutUs.xaml", UriKind.Relative));
        private void LogOut_Click(object sender, RoutedEventArgs e) { currentUser = null; this.NavigationService?.Navigate(new Uri("Pages/SignOut.xaml", UriKind.Relative)); }
    }
}
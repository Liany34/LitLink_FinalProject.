using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LitLink_FinalProject.Pages
{
    public partial class CheckOut : Page
    {
        public static List<Book> MyBooks { get; set; } = new List<Book>();

        private static readonly Regex CardNumberRegex = new(@"^\d{13,19}$");
        private static readonly Regex ExpirationRegex = new(@"^(0[1-9]|1[0-2])\/\d{2}$");
        private static readonly Regex CvvRegex = new(@"^\d{3,4}$");

        private List<Book> _selectedBooks;
        private List<Cart_Detail> _selectedCartDetails;
        private double _discountCodeAmount;
        private Reader _currentReader;
        private readonly Apiservice _api = new Apiservice();

        public CheckOut()
        {
            InitializeComponent();
        }

        public void SetupCheckout(
            List<Book> cartBooks,
            List<Cart_Detail> cartDetails,
            string currentUserEmail,
            string currentUserPhone,
            double discountAmount = 0,
            Reader reader = null)
        {
            _selectedBooks = cartBooks ?? new List<Book>();
            _selectedCartDetails = cartDetails ?? new List<Cart_Detail>();
            _discountCodeAmount = discountAmount;
            _currentReader = reader;

            BooksItemsControl.ItemsSource = _selectedBooks;
            TxtConfirmEmail.Text = currentUserEmail;
            TxtConfirmPhone.Text = currentUserPhone;

            CalculatePrices();
        }

        private void CalculatePrices()
        {
            double subTotal = _selectedBooks.Sum(b => b.Price ?? 0);
            double total = Math.Max(subTotal - _discountCodeAmount, 0);

            TxtSubTotal.Text = $"{subTotal:F2} ₪";
            TxtDiscount.Text = $"{_discountCodeAmount:F2} ₪";
            TxtTotal.Text = $"{total:F2} ₪";
        }

        private void CorrectInfo_Click(object sender, RoutedEventArgs e)
        {
            ConfirmInfoPanel.Visibility = Visibility.Collapsed;
            CreditCardPanel.Visibility = Visibility.Visible;
        }

        private void IncorrectInfo_Click(object sender, RoutedEventArgs e)
        {
            EditEmailBox.Text = TxtConfirmEmail.Text;
            EditPhoneBox.Text = TxtConfirmPhone.Text;

            ConfirmInfoPanel.Visibility = Visibility.Collapsed;
            EditInfoPanel.Visibility = Visibility.Visible;
        }

        private void SaveInfo_Click(object sender, RoutedEventArgs e)
        {
            TxtConfirmEmail.Text = EditEmailBox.Text;
            TxtConfirmPhone.Text = EditPhoneBox.Text;

            EditInfoPanel.Visibility = Visibility.Collapsed;
            ConfirmInfoPanel.Visibility = Visibility.Visible;
        }

        private async void PayNow_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateCreditCard()) return;

            PayNowButton.IsEnabled = false;
            PayNowButton.Content = "Processing...";

            try
            {
                await MarkBooksAsPurchasedAsync();

                MyBooks.AddRange(_selectedBooks);

                MessageBox.Show(
                    "!הקנייה הושלמה בהצלחה\n.הספרים החדשים שלך נוספו לרשימת הספרים שלי",
                    "LitLink – Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                MainWindow.AppFrame.Navigate(new HomePage(_currentReader));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"אירעה שגיאה במהלך הרכישה:\n{ex.Message}",
                    "שגיאה",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                PayNowButton.IsEnabled = true;
                PayNowButton.Content = "Pay Now";
            }
        }

        private bool ValidateCreditCard()
        {
            string cardNumber = TxtCardNumber.Text.Trim().Replace(" ", "").Replace("-", "");
            string expiration = TxtExpiration.Text.Trim();
            string cvv = TxtCVV.Text.Trim();

            if (!CardNumberRegex.IsMatch(cardNumber))
            {
                ShowCardError("מספר כרטיס אשראי לא תקין.\nאנא הכנס בין 13 ל-19 ספרות.");
                TxtCardNumber.Focus();
                return false;
            }

            if (!ExpirationRegex.IsMatch(expiration))
            {
                ShowCardError("תאריך תפוגה לא תקין.\nאנא הכנס בפורמט MM/YY.");
                TxtExpiration.Focus();
                return false;
            }

            if (!IsExpirationValid(expiration))
            {
                ShowCardError("כרטיס האשראי פג תוקף.\nאנא השתמש בכרטיס בתוקף.");
                TxtExpiration.Focus();
                return false;
            }

            if (!CvvRegex.IsMatch(cvv))
            {
                ShowCardError("CVV לא תקין.\nאנא הכנס 3 או 4 ספרות.");
                TxtCVV.Focus();
                return false;
            }

            return true;
        }

        private static bool IsExpirationValid(string expiration)
        {
            var parts = expiration.Split('/');
            int month = int.Parse(parts[0]);
            int year = 2000 + int.Parse(parts[1]);
            var cardExpiry = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return cardExpiry >= DateTime.Today;
        }

        private static void ShowCardError(string message)
        {
            MessageBox.Show(message, "פרטי כרטיס אשראי שגויים", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private async Task MarkBooksAsPurchasedAsync()
        {
            var updateTasks = _selectedCartDetails
                .Where(cd => !cd.IsPurchased)
                .Select(cd =>
                {
                    cd.IsPurchased = true;
                    cd.PurchaseDate = DateTime.Today;
                    return _api.UpdateCartDetail(cd);
                });

            await Task.WhenAll(updateTasks);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
            else
                MainWindow.AppFrame.Navigate(new CartPage(_currentReader));
        }
    }
}
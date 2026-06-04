using Model;
using Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LitLink_FinalProject.Pages
{
    public partial class CheckOut : Page
    {
        public static List<object> MyBooks { get; set; } = new List<object>();

        private List<Book> selectedBooks;
        private double discountCodeAmount = 0;
        private Reader curretReader;

        public CheckOut()
        {
            InitializeComponent();
        }

        public void SetupCheckout(List<Book> cartBooks, string currentUserEmail, string currentUserPhone, double discountAmount = 0)
        {
            selectedBooks = cartBooks ?? new List<Book>();
            discountCodeAmount = discountAmount;
            BooksItemsControl.ItemsSource = selectedBooks;
            TxtConfirmEmail.Text = currentUserEmail;
            TxtConfirmPhone.Text = currentUserPhone;
            this.curretReader = GetID(currentUserEmail).Result;

            CalculatePrices();
        }

        private async Task<Reader> GetID(string email)
        {
            Apiservice apiservice = new Apiservice();
            List<Reader> allReader = await apiservice.GetAllReaders();
            Reader current = allReader.Find(r => r.Email == email);
            return current;
        }

        private void CalculatePrices()
        {
            double subTotal = 0;

            foreach (var book in selectedBooks)
            {
                var priceProp = book.GetType().GetProperty("Price");
                if (priceProp != null)
                {
                    subTotal += Convert.ToDouble(priceProp.GetValue(book, null));
                }
            }

            double total = subTotal - discountCodeAmount;
            if (total < 0) total = 0;

            TxtSubTotal.Text = $"{subTotal:F2} ₪";
            TxtDiscount.Text = $"{discountCodeAmount:F2} ₪";
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

        private void PayNow_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCardNumber.Text) || string.IsNullOrWhiteSpace(TxtCVV.Text))
            {
                MessageBox.Show(".אנא מלא את כל פרטי כרטיס האשראי לפני התשלום", "נתונים חסרים", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MyBooks == null) MyBooks = new List<object>();

            MyBooks.AddRange(selectedBooks);

            MessageBox.Show("!הקנייה הושלמה בהצלחה\n.הספרים החדשים שלך נוספו בהצלחה לרשימת הספרים שלי", "LitLink Success", MessageBoxButton.OK, MessageBoxImage.Information);
            HomePage homePage = new HomePage(curretReader);
            homePage.DataContext = curretReader;
            Window.GetWindow(this).Content = homePage;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
            else
                MainWindow.AppFrame.Navigate(new CartPage(curretReader)); 
        }
    }
}
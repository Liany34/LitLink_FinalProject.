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

namespace LitLink_FinalProject.Pages
{
    /// <summary>
    /// Interaction logic for CheckOut.xaml
    /// </summary>
    public partial class CheckOut : Page
    {
        public static List<object> MyBooks { get; set; } = new List<object>();

        private List<Book> _selectedBooks;
        private double _discountCodeAmount = 0;

        public CheckOut()
        {
            InitializeComponent();
        }

        /// <summary>
        /// פונקציית הטענה ראשית שמקבלת את כל הנתונים בצורה דינמית ומפעילה את הדף
        /// </summary>
        public void SetupCheckout(List<Book> cartBooks, string currentUserEmail, string currentUserPhone, double discountAmount = 0)
        {
            _selectedBooks = cartBooks ?? new List<Book>();
            _discountCodeAmount = discountAmount;

            // השמת הספרים לתוך ה-ItemsControl
            BooksItemsControl.ItemsSource = _selectedBooks;

            // עדכון פרטי המשתמש במסך
            TxtConfirmEmail.Text = currentUserEmail;
            TxtConfirmPhone.Text = currentUserPhone;

            // חישוב מחירים
            CalculatePrices();
        }

        private void CalculatePrices()
        {
            double subTotal = 0;

            // סכימה דינמית באמצעות Reflection כדי שיתאים לכל מחלקה (Model) של ספר שיש לך בפרויקט
            foreach (var book in _selectedBooks)
            {
                var priceProp = book.GetType().GetProperty("Price");
                if (priceProp != null)
                {
                    subTotal += Convert.ToDouble(priceProp.GetValue(book, null));
                }
            }

            double total = subTotal - _discountCodeAmount;
            if (total < 0) total = 0;

            // עדכון השדות בסיכום ההזמנה
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

        private void PayNow_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtCardNumber.Text) || string.IsNullOrWhiteSpace(TxtCVV.Text))
            {
                MessageBox.Show(".אנא מלא את כל פרטי כרטיס האשראי לפני התשלום", "נתונים חסרים", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // יצירת הרשימה MyBooks במידה והיא לא אותחלה עדיין
            if (MyBooks == null) MyBooks = new List<object>();

            // הוספת הספרים שנרכשו
            MyBooks.AddRange(_selectedBooks);

            // הצגת חלון הודעה שהקנייה הושלמה בהצלחה
            MessageBox.Show("!הקנייה הושלמה בהצלחה\n.הספרים החדשים שלך נוספו בהצלחה לרשימת הספרים שלי", "LitLink Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}
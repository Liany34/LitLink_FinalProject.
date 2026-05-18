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
    /// <summary>
    /// Interaction logic for CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        private Apiservice _apiService = new Apiservice();
        private double _totalCartPrice = 0;
        private int _currentUserId; // שדה פנימי שישמור את ה-ID של המשתמש המחובר
        private List<CartUserControl> _loadedControls = new List<CartUserControl>();

        // תיקון: הבנאי מקבל כעת את ה-ID של המשתמש שצפה בעגלה
        public CartPage(int loggedInUserId)
        {
            InitializeComponent();

            // שמירת ה-ID שנשלח לעמוד
            this._currentUserId = loggedInUserId;

            // טעינת פריטי העגלה עבור המשתמש הזה
            LoadCartItemsAsync();
        }

        private async void LoadCartItemsAsync()
        {
            try
            {
                // שימוש ב-ID שהועבר בצורה מאובטחת דרך הבנאי - עובד דינמית לכל משתמש!
                List<Cart> allCarts = await _apiService.GetAllCarts();
                Cart userCart = allCarts.LastOrDefault(c => c.IdReader.Id == _currentUserId);

                // יצירת רשימת הספרים שתשמש אותנו להמשך ה-UI
                List<Book> cartBooks = new List<Book>();

                // הגנה: נמשיך רק אם באמת קיימת עגלה למשתמש הזה
                if (userCart != null)
                {
                    // 2. שליפת כל פרטי העגלות וסינון הפריטים ששייכים לעגלה שמצאנו
                    List<Cart_Detail> cartDetails = await _apiService.GetAllCartDetails();

                    // תיקון: השוואה לפי Id.Id כדי למנוע שגיאות טיפוסים
                    List<Cart_Detail> userCartDetails = cartDetails.Where(cd => cd.IdCart.Id == userCart.Id).ToList();

                    // 3. חילוץ הספרים מתוך פרטי העגלה
                    foreach (Cart_Detail detail in userCartDetails)
                    {
                        if (detail.IdBook != null)
                        {
                            cartBooks.Add(detail.IdBook);
                        }
                    }
                }

                LstCartItems.Items.Clear();
                _loadedControls.Clear();
                _totalCartPrice = 0;

                if (cartBooks != null && cartBooks.Count > 0)
                {
                    foreach (Book book in cartBooks)
                    {
                        CartUserControl bookControl = new CartUserControl();
                        bookControl.DataContext = book;

                        _loadedControls.Add(bookControl);
                        LstCartItems.Items.Add(bookControl);

                        _totalCartPrice += book.Price.GetValueOrDefault(); ;
                    }
                }

                TxtCartCount.Text = $" ({LstCartItems.Items.Count} items)";
                TxtSummaryBooksCount.Text = LstCartItems.Items.Count.ToString();
                UpdatePriceDisplay(_totalCartPrice);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cart items: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePriceDisplay(double price)
        {
            TxtFinalPrice.Text = price.ToString("C");
        }

        private void ChkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            SetAllItemsSelection(true);
        }

        private void ChkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetAllItemsSelection(false);
        }

        private void SetAllItemsSelection(bool isSelected)
        {
            foreach (CartUserControl bookControl in _loadedControls)
            {
                if (bookControl != null)
                {
                    var checkBox = bookControl.FindName("ItemCheckBox") as CheckBox;
                    if (checkBox != null)
                    {
                        checkBox.IsChecked = isSelected;
                    }
                }
            }
        }

        private async Task BtnApplyDiscount_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = TxtDiscountCode.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(enteredCode)) return;

            try
            {
                ListDiscountCodes codes = await _apiService.GetAllDiscountCodes();
                List<DiscountCodes> allCodes = codes.ToList();

                // 2. חיפוש הקופון שהמשתמש הקליד בתוך הרשימה (השוואה באותיות גדולות ליתר ביטחון)
                // בהנחה שבמודל DiscountCode יש שדות כמו Code (מחרוזת) ו-Percentage (מספר של אחוז ההנחה, למשל 20)
                DiscountCodes matchingCoupon = allCodes.FirstOrDefault(c => c.CodeText.ToUpper() == enteredCode);

                // 3. בדיקה האם הקופון נמצא והאם הוא בתוקף
                if (matchingCoupon != null)
                {
                    TxtInvalidCodeError.Visibility = Visibility.Collapsed; // העלמת הודעת השגיאה במידה והייתה

                    // חילוץ אחוז ההנחה מהקופון שנמצא (למשל, אם רשום 20, זה יהפוך ל-0.20)
                    double discountPercentage = matchingCoupon.Amount / 100.0;

                    // חישוב ההנחה והמחיר הסופי הדינמיים
                    double discount = _totalCartPrice * discountPercentage;
                    double finalPrice = _totalCartPrice - discount;

                    // הצגת המחיר המקורי עם קו מחיקה מעליו
                    TxtOriginalPrice.Visibility = Visibility.Visible;
                    TxtOriginalPrice.Text = _totalCartPrice.ToString("C");

                    // עדכון המחיר הסופי החדש על המסך לאחר ההנחה שחושבה
                    TxtFinalPrice.Text = finalPrice.ToString("C");

                    MessageBox.Show($"Coupon '{enteredCode}' applied successfully! You received a {matchingCoupon.Amount}% discount.",
                                    "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // אם הקוד לא נמצא בטבלה - מציגים את הודעת השגיאה באדום
                    TxtInvalidCodeError.Visibility = Visibility.Visible;
                    TxtOriginalPrice.Visibility = Visibility.Collapsed;
                    TxtFinalPrice.Text = _totalCartPrice.ToString("C"); // החזרת המחיר המקורי לתצוגה
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error validating discount code: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (LstCartItems.Items.Count == 0)
            {
                MessageBox.Show("Your cart is empty!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CheckOut checkoutPage = new CheckOut();
                this.NavigationService.Navigate(checkoutPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open checkout page: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

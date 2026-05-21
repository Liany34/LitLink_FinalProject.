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
        private List<Book> _chosenBooks = new List<Book>();

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
            foreach(CartUserControl bookControl in _loadedControls)
            {
                _chosenBooks.Add(bookControl.DataContext as Book);
            }
        }

        private void ChkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetAllItemsSelection(false);
            foreach (CartUserControl bookControl in _loadedControls)
            {
                _chosenBooks.Remove(bookControl.DataContext as Book);
            }
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

        private void BtnApplyDiscount_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = TxtDiscountCode.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(enteredCode)) return;

            // 2. הפעלת הקוד האסינכרוני בתוך Task בנפרד
            Task.Run(async () =>
            {
                try
                {
                    ListDiscountCodes codes = await _apiService.GetAllDiscountCodes();

                    // חיפוש הקופון (באמצעות האפשרות הראשונה שדיברנו עליה, בלי .ToList() )
                    DiscountCodes matchingCoupon = codes.FirstOrDefault(c => c.CodeText.ToUpper() == enteredCode);

                    // בגלל שאנחנו רצים ברקע, עדכון ה-UI (הטקסטים והמסך) חייב להתבצע דרך ה-Dispatcher
                    Dispatcher.Invoke(() =>
                    {
                        if (matchingCoupon != null)
                        {
                            TxtInvalidCodeError.Visibility = Visibility.Collapsed;

                            double discountPercentage = matchingCoupon.Amount / 100.0;
                            double discount = _totalCartPrice * discountPercentage;
                            double finalPrice = _totalCartPrice - discount;

                            TxtOriginalPrice.Visibility = Visibility.Visible;
                            TxtOriginalPrice.Text = _totalCartPrice.ToString("C");
                            TxtFinalPrice.Text = finalPrice.ToString("C");

                            MessageBox.Show($"Coupon '{enteredCode}' applied successfully! You received a {matchingCoupon.Amount}% discount.",
                                            "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            TxtInvalidCodeError.Visibility = Visibility.Visible;
                            TxtOriginalPrice.Visibility = Visibility.Collapsed;
                            TxtFinalPrice.Text = _totalCartPrice.ToString("C");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Error validating discount code: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });
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
                var allReaders = _apiService.GetAllReaders();
                Reader logedinUser = allReaders.Result.FirstOrDefault(r => r.Id == _currentUserId);

                foreach (CartUserControl bookControl in _loadedControls)
                {
                    var checkBox = bookControl.FindName("ItemCheckBox") as CheckBox;
                    if (checkBox != null && checkBox.IsChecked == true)
                    {
                        Book selectedBook = bookControl.DataContext as Book;
                        if (selectedBook != null)
                        {
                            _chosenBooks.Add(selectedBook);
                        }
                    }
                }


                Pages.CheckOut checkoutPage = new Pages.CheckOut();

                // 2. הפעלת הפונקציה והעברת הנתונים (הספרים שבעגלה, אימייל וטלפון של המשתמש הנוכחי)
                // שימי לב לשנות את השמות (MyCartList, App.CurrentUser) לשמות האמיתיים אצלך בפרויקט
                checkoutPage.SetupCheckout(_chosenBooks, logedinUser.Email, logedinUser.PhoneNumber, 0);

                // 3. ביצוע הניווט בתוך הפריים (MainFrame)
                this.NavigationService.Navigate(checkoutPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open checkout page: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

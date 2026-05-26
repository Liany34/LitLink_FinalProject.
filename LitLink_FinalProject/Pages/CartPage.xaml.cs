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
    public partial class CartPage : Page
    {
        private Apiservice apiService = new Apiservice();
        private double totalCartPrice = 0;
        private List<CartUserControl> loadedControls = new List<CartUserControl>();
        private List<Book> chosenBooks = new List<Book>();
        private int currentUserId;

        public CartPage(int loggedInUserId)
        {
            InitializeComponent();

            this.currentUserId = loggedInUserId;

            LoadCartItemsAsync();
        }

        private async void LoadCartItemsAsync()
        {
            try
            {
                List<Cart> allCarts = await apiService.GetAllCarts();
                Cart userCart = allCarts.LastOrDefault(c => c.IdReader.Id == currentUserId);

                List<Book> cartBooks = new List<Book>();

                if (userCart != null)
                {
                    List<Cart_Detail> cartDetails = await apiService.GetAllCartDetails();

                    List<Cart_Detail> userCartDetails = cartDetails.Where(cd => cd.IdCart.Id == userCart.Id).ToList();

                    foreach (Cart_Detail detail in userCartDetails)
                    {
                        if (detail.IdBook != null)
                        {
                            cartBooks.Add(detail.IdBook);
                        }
                    }
                }

                LstCartItems.Items.Clear();
                loadedControls.Clear();
                totalCartPrice = 0;

                if (cartBooks != null && cartBooks.Count > 0)
                {
                    foreach (Book book in cartBooks)
                    {
                        CartUserControl bookControl = new CartUserControl();
                        bookControl.DataContext = book;

                        loadedControls.Add(bookControl);
                        LstCartItems.Items.Add(bookControl);

                        totalCartPrice += book.Price.GetValueOrDefault(); ;
                    }
                }

                TxtCartCount.Text = $" ({LstCartItems.Items.Count} items)";
                TxtSummaryBooksCount.Text = LstCartItems.Items.Count.ToString();
                UpdatePriceDisplay(totalCartPrice);
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
            foreach(CartUserControl bookControl in loadedControls)
            {
                chosenBooks.Add(bookControl.DataContext as Book);
            }
        }

        private void ChkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            SetAllItemsSelection(false);
            foreach (CartUserControl bookControl in loadedControls)
            {
                chosenBooks.Remove(bookControl.DataContext as Book);
            }
        }

        private void SetAllItemsSelection(bool isSelected)
        {
            foreach (CartUserControl bookControl in loadedControls)
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

            Task.Run(async () =>
            {
                try
                {
                    ListDiscountCodes codes = await apiService.GetAllDiscountCodes();

                    DiscountCodes matchingCoupon = codes.FirstOrDefault(c => c.CodeText.ToUpper() == enteredCode);

                    Dispatcher.Invoke(() =>
                    {
                        if (matchingCoupon != null)
                        {
                            TxtInvalidCodeError.Visibility = Visibility.Collapsed;

                            double discountPercentage = matchingCoupon.Amount / 100.0;
                            double discount = totalCartPrice * discountPercentage;
                            double finalPrice = totalCartPrice - discount;

                            TxtOriginalPrice.Visibility = Visibility.Visible;
                            TxtOriginalPrice.Text = totalCartPrice.ToString("C");
                            TxtFinalPrice.Text = finalPrice.ToString("C");

                            MessageBox.Show($"Coupon '{enteredCode}' applied successfully! You received a {matchingCoupon.Amount}% discount.",
                                            "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            TxtInvalidCodeError.Visibility = Visibility.Visible;
                            TxtOriginalPrice.Visibility = Visibility.Collapsed;
                            TxtFinalPrice.Text = totalCartPrice.ToString("C");
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
                var allReaders = apiService.GetAllReaders();
                Reader logedinUser = allReaders.Result.FirstOrDefault(r => r.Id == currentUserId);

                foreach (CartUserControl bookControl in loadedControls)
                {
                    var checkBox = bookControl.FindName("ItemCheckBox") as CheckBox;
                    if (checkBox != null && checkBox.IsChecked == true)
                    {
                        Book selectedBook = bookControl.DataContext as Book;
                        if (selectedBook != null)
                        {
                            chosenBooks.Add(selectedBook);
                        }
                    }
                }


                Pages.CheckOut checkoutPage = new Pages.CheckOut();

                checkoutPage.SetupCheckout(chosenBooks, logedinUser.Email, logedinUser.PhoneNumber, 0);

                this.NavigationService.Navigate(checkoutPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open checkout page: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

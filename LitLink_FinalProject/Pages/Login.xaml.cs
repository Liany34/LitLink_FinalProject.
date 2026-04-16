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
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private const string DefaultEmail = "litlink@gmail.com";
        public Login()
        {
            InitializeComponent();
        }

        private void EmailInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailInput.Text == DefaultEmail)
            {
                EmailInput.Text = "";
                EmailInput.FontStyle = FontStyles.Normal;
                EmailInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                EmailInput.FontSize = 14;
            }
        }

        private void EmailInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailInput.Text))
            {
                EmailInput.Text = "Please enter your email.";
                EmailInput.Foreground = Brushes.DarkRed;
                EmailInput.FontStyle = FontStyles.Italic;
                EmailInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                EmailInput.FontSize = 14;
            }
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e) // שמתי תיבת טקסט מעל הסיסמה כדי שזה יחזיק
        {                                                                            // את הטקס שאני רוצה, ברגע שאני מקלידה משהו
            PasswordPlaceholder.Visibility = PasswordInput.Password.Length > 0       // זה מחביא את התיבת טקסט ושם את הסיסמה
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TogglePasswordButton_PreviewMouseDown(object sender, MouseButtonEventArgs e) 
        {                                                                           // כאשר אני לוחצת על העין זה מסתיר את הסיסמה 
            VisiblePasswordInput.Text = PasswordInput.Password;                     // כנקודות ומראה את הטקסט עצמו (שורה 62) וכך
            PasswordInput.Visibility = Visibility.Collapsed;                        // המשתמש יכול לראות את הסיסמה שלו
            VisiblePasswordInput.Visibility = Visibility.Visible;
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void TogglePasswordButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {                                                                           // כאשר אני משחררת את העין זה מחזיר את הסיסמה
            VisiblePasswordInput.Visibility = Visibility.Collapsed;                 //  למצב של נקודות ומסתיר את הטקסט עצמו
            PasswordInput.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(PasswordInput.Password)) PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            Apiservice buyerService = new Apiservice();
            var users = await buyerService.GetAllUsers();
            bool wentIn = false;
            foreach (var user in users)
            {
                if (user.Email == EmailInput.Text && user.Pass == PasswordInput.Password)
                {
                    this.NavigationService.Navigate(new HomePage());
                    return;
                }
            }
            if (!wentIn)
            {
                MessageBox.Show("Invalid email or password. Please try again.");
            }
        }

        private void Navigate_SignUp(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignUp());
        }

        private void Navigate_ResetPass(object sender, RoutedEventArgs e)
        {
            if (EmailInput.Text == DefaultEmail || string.IsNullOrWhiteSpace(EmailInput.Text))
            {
                MessageBox.Show("Please enter your email to reset your password.");
                EmailInput.Focus();
                return;
            }
            else
            {
                this.NavigationService.Navigate(new ResetPass(EmailInput.Text));
            }
        }
    }
}
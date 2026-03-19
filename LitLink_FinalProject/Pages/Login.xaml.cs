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
                EmailInput.Foreground = Brushes.Black;
                EmailInput.FontStyle = FontStyles.Normal;
            }
        }
        private void EmailInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailInput.Text))
            {
                EmailInput.Text = DefaultEmail;
                EmailInput.Foreground = Brushes.Gray;
                EmailInput.FontStyle = FontStyles.Italic;
            }
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = PasswordInput.Password.Length > 0
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TogglePasswordButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VisiblePasswordInput.Text = PasswordInput.Password;
            PasswordInput.Visibility = Visibility.Collapsed;
            VisiblePasswordInput.Visibility = Visibility.Visible;
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }
        private void TogglePasswordButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            VisiblePasswordInput.Visibility = Visibility.Collapsed;
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
                    var homepagePage = new HomePage();
                    Window.GetWindow(this).Content = homepagePage;
                    wentIn = true;
                    return;
                }
            }
            if (!wentIn)
            {
                MessageBox.Show("Invalid email or password. Please try again.");
            }
        }

        private void Navigate_SignUp(object sender, RequestNavigateEventArgs e)
        {
            NavigationService.Navigate(new Uri("SignUp.xaml", UriKind.Relative));
        }

        private void Navigate_ResetPass(object sender, RequestNavigateEventArgs e)
        {
            NavigationService.Navigate(new Uri("ResetPass.xaml", UriKind.Relative));
        }
    }
}

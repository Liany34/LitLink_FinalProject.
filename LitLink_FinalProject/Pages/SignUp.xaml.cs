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
using Model;
using Service;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace LitLink_FinalProject.Pages
{
    /// <summary>
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Page
    {
        private const string DefaultFirstName = "First Name";
        private const string DefaultLastName = "Last Name";
        private const string DefaultPhone = "000-0000000";
        private const string DefaultEmail = "litlink@gmail.com";
        private const string DefaultUsername= "Username";

        public SignUp()
        {
            InitializeComponent();
            BirthDatePicker.DisplayDateEnd = DateTime.Today;
            BirthDatePicker.DisplayDateStart = DateTime.Today.AddYears(-150);
        }

        private void FirstNameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FirstNameInput.Text == DefaultFirstName)
            {
                FirstNameInput.Text = "";
                FirstNameInput.FontStyle = FontStyles.Normal;
                FirstNameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                FirstNameInput.FontSize = 14;
            }
        }

        private void FirstNameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameInput.Text))
            {
                FirstNameInput.Text = "Please enter your first name.";
                FirstNameInput.Foreground = Brushes.DarkRed;
                FirstNameInput.FontStyle = FontStyles.Italic;
                FirstNameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                FirstNameInput.FontSize = 14;
            }
        }

        private void LastNameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LastNameInput.Text == DefaultLastName)
            {
                LastNameInput.Text = "";
                LastNameInput.FontStyle = FontStyles.Normal;
                LastNameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                LastNameInput.FontSize = 14;
            }
        }

        private void LastNameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameInput.Text))
            {
                LastNameInput.Text = "Please enter your last name.";
                LastNameInput.Foreground = Brushes.DarkRed;
                LastNameInput.FontStyle = FontStyles.Italic;
                LastNameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                LastNameInput.FontSize = 14;
            }
        }

        private void PhoneInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PhoneInput.Text == DefaultPhone)
            {
                PhoneInput.Text = "";
                PhoneInput.FontStyle = FontStyles.Normal;
                PhoneInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                PhoneInput.FontSize = 14;
            }
        }

        private void PhoneInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PhoneInput.Text))
            {
                PhoneInput.Text = "Please enter your phone.";
                PhoneInput.Foreground = Brushes.DarkRed;
                PhoneInput.FontStyle = FontStyles.Italic;
                PhoneInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                PhoneInput.FontSize = 14;
            }
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

        private void ReenterInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ReenterPlaceholder.Visibility = ReenterInput.Password.Length > 0
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ToggleReenterButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VisibleReenterInput.Text = ReenterInput.Password;
            ReenterInput.Visibility = Visibility.Collapsed;
            VisibleReenterInput.Visibility = Visibility.Visible;
            ReenterPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void ToggleReenterButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            VisibleReenterInput.Visibility = Visibility.Collapsed;
            ReenterInput.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(ReenterInput.Password)) PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void UsernameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameInput.Text == DefaultUsername)
            {
                UsernameInput.Text = "";
                UsernameInput.FontStyle = FontStyles.Normal;
                UsernameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                UsernameInput.FontSize = 14;
            }
        }

        private void UsernameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameInput.Text))
            {
                UsernameInput.Text = "Please enter your username.";
                UsernameInput.Foreground = Brushes.DarkRed;
                UsernameInput.FontStyle = FontStyles.Italic;
                UsernameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                UsernameInput.FontSize = 14;
            }
        }

        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            bool everythingOkay = true;
            if(PhoneInput.Text != @"^05\d-\d{7}$")
            {
                InvaildPhone.Visibility = Visibility.Visible;
                PhoneInput.Focus();
                everythingOkay = false;
            }
            if(EmailInput.Text != @"^[a-zA-Z0-9]+@[a-zA-Z0-9]+\.[a-zA-Z0-9]+[a-zA-Z0-9]+$")
            {
                InvalidEmail.Visibility = Visibility.Visible;
                EmailInput.Focus();
                everythingOkay = false;
            }
            if (PasswordInput.Password != ReenterInput.Password)
            {
                DiffPass.Visibility = Visibility.Visible;
                ReenterInput.Focus();
                everythingOkay = false;
            }
            if(BirthDatePicker.SelectedDate == null)
            {
                InvalidBirthDate.Visibility = Visibility.Visible;
                BirthDatePicker.Focus();
            }
            else
            {

                InvaildPhone.Visibility = Visibility.Collapsed;
                Apiservice buyerService = new Apiservice();
                if (everythingOkay && FirstNameInput.Text != DefaultFirstName && LastNameInput.Text != DefaultLastName && PhoneInput.Text != DefaultPhone && EmailInput.Text != DefaultEmail && UsernameInput.Text != DefaultUsername)
                {
                    User newUser = new User
                    {
                        FirstName = FirstNameInput.Text,
                        LastName = LastNameInput.Text,
                        PhoneNumber = PhoneInput.Text,
                        Email = EmailInput.Text,
                        Username = UsernameInput.Text,
                        Pass = PasswordInput.Password,
                        Birthdate = DateTime.Parse(BirthDatePicker.SelectedDate.ToString())
                    };
                    var homepagePage = new HomePage();
                    Window.GetWindow(this).Content = homepagePage;
                }
                else
                {
                    MessageBox.Show("Please fill in all the required fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

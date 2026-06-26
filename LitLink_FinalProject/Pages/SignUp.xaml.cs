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
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace LitLink_FinalProject.Pages
{
    public partial class SignUp : Page
    {
        private const string DefaultFirstName = "Please enter your First Name";
        private const string DefaultLastName = "Please enter your LastName";
        private const string DefaultEmail = "Please enter your Email";
        private const string DefaultPhone = "Please enter your Phone Number";
        private const string DefaultUsername = "Please enter your Username";
        private const string DefaultNickname = "Please enter your Nickname";

        private const string DefaultReaderPicture = "UserPicture1.png";

        private readonly Apiservice apiService = new Apiservice();

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
                FirstNameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6c3134"));
                FirstNameInput.FontStyle = FontStyles.Normal;
                FirstNameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                FirstNameInput.FontSize = 14;
            }
        }

        private void FirstNameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameInput.Text))
            {
                FirstNameInput.Text = DefaultFirstName;
                FirstNameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa3954"));
                FirstNameInput.FontStyle = FontStyles.Normal;
                FirstNameInput.FontFamily = new FontFamily("/Fonts/Alex Brush;component/#Alex Brush");
                FirstNameInput.FontSize = 15;
            }
        }

        private void LastNameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LastNameInput.Text == DefaultLastName)
            {
                LastNameInput.Text = "";
                LastNameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6c3134"));
                LastNameInput.FontStyle = FontStyles.Normal;
                LastNameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                LastNameInput.FontSize = 14;
            }
        }

        private void LastNameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameInput.Text))
            {
                LastNameInput.Text = DefaultLastName;
                LastNameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa3954"));
                LastNameInput.FontStyle = FontStyles.Normal;
                LastNameInput.FontFamily = new FontFamily("/Fonts/Alex Brush;component/#Alex Brush");
                LastNameInput.FontSize = 15;
            }
        }

        private void EmailInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailInput.Text == DefaultEmail)
            {
                EmailInput.Text = "";
                EmailInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6c3134"));
                EmailInput.FontStyle = FontStyles.Normal;
                EmailInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                EmailInput.FontSize = 14;
            }
        }

        private void EmailInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InvalidEmail == null || ExistingEmail == null)
                return;

            InvalidEmail.Visibility = Visibility.Collapsed;
            ExistingEmail.Visibility = Visibility.Collapsed;
        }

        private void EmailInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailInput.Text))
            {
                EmailInput.Text = DefaultEmail;
                EmailInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa3954"));
                EmailInput.FontStyle = FontStyles.Normal;
                EmailInput.FontFamily = new FontFamily("/Fonts/Alex Brush;component/#Alex Brush");
                EmailInput.FontSize = 15;
            }
        }

        private void PhoneInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PhoneInput.Text == DefaultPhone)
            {
                PhoneInput.Text = "";
                PhoneInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6c3134"));
                PhoneInput.FontStyle = FontStyles.Normal;
                PhoneInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                PhoneInput.FontSize = 14;
            }
        }

        private void PhoneInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InvaildPhone == null || ExistingPhone == null)
                return;

            InvaildPhone.Visibility = Visibility.Collapsed;
            ExistingPhone.Visibility = Visibility.Collapsed;
        }

        private void PhoneInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PhoneInput.Text))
            {
                PhoneInput.Text = DefaultPhone;
                PhoneInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa3954"));
                PhoneInput.FontStyle = FontStyles.Normal;
                PhoneInput.FontFamily = new FontFamily("/Fonts/Alex Brush;component/#Alex Brush");
                PhoneInput.FontSize = 15;
            }
        }

        private void UsernameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameInput.Text == DefaultUsername)
            {
                UsernameInput.Text = "";
                UsernameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6c3134"));
                UsernameInput.FontStyle = FontStyles.Normal;
                UsernameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                UsernameInput.FontSize = 14;
            }
        }

        private void UsernameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InvalidUsername == null)
                return;

            InvalidUsername.Visibility = Visibility.Collapsed;
        }

        private void UsernameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameInput.Text))
            {
                UsernameInput.Text = DefaultUsername;
                UsernameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa3954"));
                UsernameInput.FontStyle = FontStyles.Normal;
                UsernameInput.FontFamily = new FontFamily("/Fonts/Alex Brush;component/#Alex Brush");
                UsernameInput.FontSize = 15;
            }
        }

        private void NicknameInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NicknameInput.Text == DefaultNickname)
            {
                NicknameInput.Text = "";
                NicknameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6c3134"));
                NicknameInput.FontStyle = FontStyles.Normal;
                NicknameInput.FontFamily = new FontFamily("/Fonts/Roboto Slab;component/#Roboto Slab");
                NicknameInput.FontSize = 14;
            }
        }

        private void NicknameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NicknameInput.Text))
            {
                NicknameInput.Text = DefaultNickname;
                NicknameInput.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa3954"));
                NicknameInput.FontStyle = FontStyles.Normal;
                NicknameInput.FontFamily = new FontFamily("/Fonts/Alex Brush;component/#Alex Brush");
                NicknameInput.FontSize = 15;
            }
        }

        // ------------------------------------------------------------------ //
        //  Password show / hide
        // ------------------------------------------------------------------ //
        private void PasswordInput_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordInput.Password))
                PasswordPlaceholder.Visibility = Visibility.Visible;
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
            if (string.IsNullOrEmpty(PasswordInput.Password))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void ReenterInput_GotFocus(object sender, RoutedEventArgs e)
        {
            ReenterPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void ReenterInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ReenterInput.Password))
                ReenterPlaceholder.Visibility = Visibility.Visible;
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
            if (string.IsNullOrEmpty(ReenterInput.Password))
                ReenterPlaceholder.Visibility = Visibility.Visible;
        }

        // ------------------------------------------------------------------ //
        //  Sign Up — validation, duplicate checks, insert, navigate
        // ------------------------------------------------------------------ //
        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            SignUpButton.IsEnabled = false;

            try
            {
                List<User> existingUsers = (await apiService.GetAllUsers())?.ToList() ?? new List<User>();

                // Reset all error indicators before re-validating.
                ExistingNameWarning.Visibility = Visibility.Collapsed;
                InvaildPhone.Visibility = Visibility.Collapsed;
                ExistingPhone.Visibility = Visibility.Collapsed;
                InvalidEmail.Visibility = Visibility.Collapsed;
                ExistingEmail.Visibility = Visibility.Collapsed;
                InvalidUsername.Visibility = Visibility.Collapsed;
                WeakPassword.Visibility = Visibility.Collapsed;
                DiffPass.Visibility = Visibility.Collapsed;
                InvalidBirthDate.Visibility = Visibility.Collapsed;

                bool isValid = true;
                Control firstInvalidControl = null;

                string firstName = FirstNameInput.Text.Trim();
                string lastName = LastNameInput.Text.Trim();
                string email = EmailInput.Text.Trim();
                string phone = PhoneInput.Text.Trim();
                string username = UsernameInput.Text.Trim();
                string nickname = NicknameInput.Text.Trim();
                string password = PasswordInput.Password;
                string reenter = ReenterInput.Password;

                // ---- Required text fields ----
                if (string.IsNullOrWhiteSpace(firstName) || firstName == DefaultFirstName)
                {
                    ExistingNameWarning.Text = "First name is required";
                    ExistingNameWarning.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= FirstNameInput;
                }

                if (string.IsNullOrWhiteSpace(lastName) || lastName == DefaultLastName)
                {
                    isValid = false;
                    firstInvalidControl ??= LastNameInput;
                }

                // ---- Email format + duplicate ----
                if (string.IsNullOrWhiteSpace(email) || email == DefaultEmail ||
                    !Regex.IsMatch(email, @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9.-]+$"))
                {
                    InvalidEmail.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= EmailInput;
                }
                else if (existingUsers.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
                {
                    ExistingEmail.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= EmailInput;
                }

                // ---- Phone format + duplicate ----
                if (string.IsNullOrWhiteSpace(phone) || phone == DefaultPhone ||
                    !Regex.IsMatch(phone, @"^0(5[0-9])\d{7}$"))
                {
                    InvaildPhone.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= PhoneInput;
                }
                else if (existingUsers.Any(u => u.PhoneNumber == phone))
                {
                    ExistingPhone.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= PhoneInput;
                }

                // ---- Password ----
                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    WeakPassword.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= PasswordInput;
                }
                else if (password != reenter)
                {
                    DiffPass.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= ReenterInput;
                }

                // ---- Birth date ----
                if (BirthDatePicker.SelectedDate == null)
                {
                    InvalidBirthDate.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= BirthDatePicker;
                }

                // ---- Username required + duplicate ----
                if (string.IsNullOrWhiteSpace(username) || username == DefaultUsername)
                {
                    isValid = false;
                    firstInvalidControl ??= UsernameInput;
                }
                else if (existingUsers.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
                {
                    InvalidUsername.Visibility = Visibility.Visible;
                    isValid = false;
                    firstInvalidControl ??= UsernameInput;
                }

                // ---- Nickname required ----
                if (string.IsNullOrWhiteSpace(nickname) || nickname == DefaultNickname)
                {
                    isValid = false;
                    firstInvalidControl ??= NicknameInput;
                }

                if (!isValid)
                {
                    firstInvalidControl?.Focus();
                    MessageBox.Show(
                        "Please correct the highlighted errors before proceeding.",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // ---- Build and insert the new Reader (default picture, no picker) ----
                Reader newReader = new Reader
                {
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phone,
                    Email = email,
                    Username = username,
                    Pass = password,
                    Birthdate = BirthDatePicker.SelectedDate.Value,
                    Picture = DefaultReaderPicture,
                    PicturePath = DefaultReaderPicture,
                    Nickname = nickname
                };

                int result = await apiService.InsertReader(newReader);

                if (result <= 0)
                {
                    MessageBox.Show(
                        "Sign up failed. Please try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                // ---- Re-fetch to get the generated Id, then navigate home ----
                List<User> refreshedUsers = (await apiService.GetAllUsers())?.ToList() ?? new List<User>();
                User createdUser = refreshedUsers
                    .FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

                Reader readerToUse = newReader;
                if (createdUser != null)
                {
                    newReader.Id = createdUser.Id;
                }

                MainWindow.AppFrame.Navigate(new HomePage(readerToUse));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while signing up:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                SignUpButton.IsEnabled = true;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AppFrame.Navigate(new Login());
        }
    }
}
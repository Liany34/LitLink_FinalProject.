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
            try
            {
                Apiservice buyerService = new Apiservice();

                // טעינת כל סוגי המשתמשים מהשרת
                var users = await buyerService.GetAllUsers();
                var readers = await buyerService.GetAllReaders();
                var authors = await buyerService.GetAllAuthors();
                var admins = await buyerService.GetAllAdmins(); // ✨ הוספנו טעינת אדמינים

                bool wentIn = false;

                foreach (var u in users)
                {
                    if (u.Email == EmailInput.Text && u.Pass == PasswordInput.Password)
                    {
                        var loggedReader = readers.FirstOrDefault(r => r.Id == u.Id);
                        if (loggedReader != null)
                        {
                            HomePage homePage = new HomePage();
                            homePage.DataContext = loggedReader;
                            Window.GetWindow(this).Content = homePage;
                            wentIn = true;
                            return;
                        }

                        var loggedAuthor = authors.FirstOrDefault(a => a.Id == u.Id);
                        if (loggedAuthor != null)
                        {
                            var authorProfile = new AuthorProfile(loggedAuthor); 
                            Window.GetWindow(this).Content = authorProfile;
                            wentIn = true;
                            return;
                        }

                        var loggedAdmin = admins.FirstOrDefault(a => a.Id == u.Id);
                        if (loggedAdmin != null)
                        {
                            var adminProfile = new AdminProfile();
                            adminProfile.DataContext = loggedAdmin; 
                            Window.GetWindow(this).Content = adminProfile;
                            wentIn = true;
                            return;
                        }
                    }
                }

                if (!wentIn)
                {
                    MessageBox.Show("Invalid email or password. Please try again.", "LitLink");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"שגיאה בהתחברות: {ex.Message}", "LitLink Error");
            }
        }

        private void Navigate_SignUp(object sender, RoutedEventArgs e)
        {
            var signUp = new SignUp();
            Window.GetWindow(this).Content = signUp;
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
                var resetPass = new ResetPass(EmailInput.Text);
                Window.GetWindow(this).Content = resetPass;
            }
        }
    }
}
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
            Apiservice buyerService = new Apiservice();
            var readers = await buyerService.GetAllReaders();
            var authors = await buyerService.GetAllAuthors();
            var users = await buyerService.GetAllUsers();
            bool wentIn = false;
            foreach (var u in users)
            {
                if (u.Email == EmailInput.Text && u.Pass == PasswordInput.Password)
                {
                    if(readers.Any(r => r.Id == u.Id))
                    {
                        // בהנחה ש-loggedUser הוא האובייקט מסוג User שחזר מה-API אחרי התחברות מוצלחת:
                        User loggedUser = readers.First(r => r.Id == u.Id); // או פשוט u אם הוא כבר מכיל את כל המידע הדרוש

                        // 1. יצירת מופע חדש של עמוד הבית
                        HomePage homePage = new HomePage();

                        // 2. השמת המשתמש בתוך ה-DataContext של עמוד הבית החדש
                        homePage.DataContext = loggedUser;

                        // 3. ניווט למופע הקיים (ולא ל-Uri)
                        this.NavigationService?.Navigate(homePage);
                        wentIn = true;
                        return;
                    }
                    else
                    {
                        if (authors.Any(a => a.Id == u.Id))
                        {
                            this.NavigationService.Navigate(new AuthorProfile());
                            wentIn = true;
                            return;
                        }
                        else
                        {
                            this.NavigationService.Navigate(new AdminProfile());
                            wentIn = true;
                        }
                    }
                   
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
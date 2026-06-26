using LitLink_FinalProject;
using LitLink_FinalProject.Pages;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace LitLink_FinalProject.Pages
{
    public partial class Login : Page
    {
        private const string DefaultEmail = "litlink@gmail.com";
        private Apiservice apiservice = new Apiservice();

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

        //private async void Login_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        Apiservice buyerService = new Apiservice();
        //        var users = await buyerService.GetAllUsers();
        //        var readers = await buyerService.GetAllReaders();
        //        var authors = await buyerService.GetAllAuthors();
        //        var admins = await buyerService.GetAllAdmins();

        //        bool wentIn = false;

        //        foreach (var u in users)
        //        {
        //            if (u.Email == EmailInput.Text && u.Pass == PasswordInput.Password)
        //            {
        //                var loggedReader = readers.FirstOrDefault(r => r.Id == u.Id);
        //                if (loggedReader != null)
        //                {
        //                    MainWindow.AppFrame.Navigate(new HomePage(loggedReader));
        //                    wentIn = true;
        //                    return;
        //                }
        //                else
        //                {
        //                    var loggedAuthor = authors.FirstOrDefault(a => a.Id == u.Id);
        //                    if (loggedAuthor != null)
        //                    {
        //                        MainWindow.AppFrame.Navigate(new AuthorProfile(loggedAuthor));
        //                        wentIn = true;
        //                        return;
        //                    }
        //                    else
        //                    {
        //                        var loggedAdmin = admins.FirstOrDefault(a => a.Id == u.Id);
        //                        if (loggedAdmin != null)
        //                        {
        //                            MainWindow.AppFrame.Navigate(new AdminProfile(loggedAdmin));
        //                            wentIn = true;
        //                            return;
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        if (!wentIn)
        //            MessageBox.Show("Invalid email or password. Please try again.", "LitLink");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error while logging in: {ex.Message}", "LitLink Error");
        //    }
        //}

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                void Log(string label)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN-TIMING] {label}: {sw.ElapsedMilliseconds} ms");
                }

                Apiservice service = new Apiservice();

                Log("Start");

                var users = await service.GetAllUsers();
                Log("After GetAllUsers");

                var readers = await service.GetAllReaders();
                Log("After GetAllReaders");

                var authors = await service.GetAllAuthors();
                Log("After GetAllAuthors");

                var admins = await service.GetAllAdmins();
                Log("After GetAllAdmins");

                string email = EmailInput.Text.Trim();
                string password = PasswordInput.Password.Trim();

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter email and password.", "LitLink");
                    return;
                }

                User loggedUser = users.FirstOrDefault(u =>
                    u.Email != null &&
                    u.Pass != null &&
                    u.Email.Trim().Equals(email, StringComparison.OrdinalIgnoreCase) &&
                    u.Pass.Trim() == password);

                Log("After matching user");

                if (loggedUser == null)
                {
                    MessageBox.Show("Invalid email or password. Please try again.", "LitLink");
                    return;
                }

                Reader loggedReader = readers.FirstOrDefault(r => r.Id == loggedUser.Id);
                if (loggedReader != null)
                {
                    Log("Navigating as Reader");
                    System.Diagnostics.Debug.WriteLine($"[LOGIN-NAVIGATE] Using AppFrame HashCode={MainWindow.AppFrame.GetHashCode()}, IsLoaded={MainWindow.AppFrame.IsLoaded}");
                    MainWindow.AppFrame.Navigate(new HomePage(loggedReader));
                    return;
                }

                Author loggedAuthor = authors.FirstOrDefault(a => a.Id == loggedUser.Id);
                if (loggedAuthor != null)
                {
                    Log("Navigating as Author");
                    MainWindow.AppFrame.Navigate(new AuthorProfile(loggedAuthor));
                    return;
                }

                Admin loggedAdmin = admins.FirstOrDefault(a => a.Id == loggedUser.Id);
                if (loggedAdmin != null)
                {
                    Log("Navigating as Admin");
                    MainWindow.AppFrame.Navigate(new AdminProfile(loggedAdmin));
                    return;
                }

                MessageBox.Show("User exists, but no matching Reader / Author / Admin record was found.", "LitLink");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while logging in: {ex.Message}", "LitLink Error");
            }
        }

        //private async void Login_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        Apiservice service = new Apiservice();

        //        var users = await service.GetAllUsers();
        //        var readers = await service.GetAllReaders();
        //        var authors = await service.GetAllAuthors();
        //        var admins = await service.GetAllAdmins();

        //        string email = EmailInput.Text.Trim();
        //        string password = PasswordInput.Password.Trim();

        //        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        //        {
        //            MessageBox.Show("Please enter email and password.", "LitLink");
        //            return;
        //        }

        //        User loggedUser = users.FirstOrDefault(u =>
        //            u.Email != null &&
        //            u.Pass != null &&
        //            u.Email.Trim().Equals(email, StringComparison.OrdinalIgnoreCase) &&
        //            u.Pass.Trim() == password);

        //        if (loggedUser == null)
        //        {
        //            MessageBox.Show("Invalid email or password. Please try again.", "LitLink");
        //            return;
        //        }

        //        Reader loggedReader = readers.FirstOrDefault(r => r.Id == loggedUser.Id);
        //        if (loggedReader != null)
        //        {
        //            MainWindow.AppFrame.Navigate(new HomePage(loggedReader));
        //            return;
        //        }

        //        Author loggedAuthor = authors.FirstOrDefault(a => a.Id == loggedUser.Id);
        //        if (loggedAuthor != null)
        //        {
        //            MainWindow.AppFrame.Navigate(new AuthorProfile(loggedAuthor));
        //            return;
        //        }

        //        Admin loggedAdmin = admins.FirstOrDefault(a => a.Id == loggedUser.Id);
        //        if (loggedAdmin != null)
        //        {
        //            MainWindow.AppFrame.Navigate(new AdminProfile(loggedAdmin));
        //            return;
        //        }

        //        MessageBox.Show("User exists, but no matching Reader / Author / Admin record was found.", "LitLink");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error while logging in: {ex.Message}", "LitLink Error");
        //    }
        //}

        private async void Navigate_SignUp(object sender, RoutedEventArgs e) 
        {
            MainWindow.AppFrame.Navigate(new SignUp());
        }

        private async void Navigate_ResetPass(object sender, RoutedEventArgs e)
        {
            if (EmailInput.Text == DefaultEmail || string.IsNullOrWhiteSpace(EmailInput.Text))
            {
                MessageBox.Show("Please enter your email to reset your password.");
                EmailInput.Focus();
                return;
            }
            List<User> users = await apiservice.GetAllUsers();
            if (users.Any(u => u.Email == EmailInput.Text))
            {
                MainWindow.AppFrame.Navigate(new ResetPass(EmailInput.Text));
                return;
            }
            else
            {
                MessageBox.Show("No account found with that email. Please check and try again.");
                EmailInput.Focus();
                return;
            }
        }
    }
}
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
using Model;

namespace LitLink_FinalProject.Pages
{
    public partial class ResetPass : Page
    {
        private string email;
        public ResetPass()
        {
            InitializeComponent();
        }
        public ResetPass(string email) : this() 
        {
            this.email = email;
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
            if (string.IsNullOrEmpty(ReenterInput.Password)) ReenterPlaceholder.Visibility = Visibility.Visible;
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

        private async void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            DiffPass.Visibility = Visibility.Collapsed;

            if (PasswordInput.Password != ReenterInput.Password)
            {
                DiffPass.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordInput.Password))
            {
                MessageBox.Show("Please enter a valid password.");
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Email is missing. Please go back and enter your email again.");
                return;
            }

            try
            {
                Apiservice buyerService = new Apiservice();

                List<User> users = await buyerService.GetAllUsers();

                if (users == null)
                {
                    MessageBox.Show("Error retrieving user list. Please try again.");
                    return;
                }

                User user = users.Find(u =>
                    u != null &&
                    !string.IsNullOrEmpty(u.Email) &&
                    u.Email.Trim().ToLower() == email.Trim().ToLower());

                if (user == null)
                {
                    MessageBox.Show("User not found in the system.");
                    return;
                }

                UserUpdateDto dto = new UserUpdateDto
                {
                    Id = user.Id,

                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Username = user.Username,

                    // הסיסמה החדשה
                    Pass = PasswordInput.Password,

                    Birthdate = user.Birthdate,

                    // שומרים את התמונה הקיימת, לא מעלים תמונה חדשה
                    PicturePath = user.PicturePath,

                    FileName = null,
                    Base64Image = null
                };

                bool success = await buyerService.UpdateUser(dto);

                if (success)
                {
                    MessageBox.Show("Password reset successful! Please log in with your new password.");

                    var loginPage = new Login();
                    Window.GetWindow(this).Content = loginPage;
                }
                else
                {
                    MessageBox.Show("Password reset failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while resetting the password: {ex.Message}");
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var loginPage = new Login();
            Window.GetWindow(this).Content = loginPage;
        }
    }
}

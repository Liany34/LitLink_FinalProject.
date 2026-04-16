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
    /// Interaction logic for ResetPass.xaml
    /// </summary>
    public partial class ResetPass : Page
    {
        private string _email;
        public ResetPass()
        {
            InitializeComponent();
        }
        public ResetPass(string email) : this() // משתמש בפעולה הבונה שלמעלה וגם בזאתי
        {
            _email = email;
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
            if(PasswordInput.Password != ReenterInput.Password)
            {
                DiffPass.Visibility = Visibility.Visible;
            }
            else
            {
                Apiservice buyerService = new Apiservice();
                var users = await buyerService.GetAllUsers();
                var user = users.Find(u => u.Email == _email);
                var saveUser = user;
                user.FirstName = saveUser.FirstName; 
                user.LastName = saveUser.LastName;
                user.PhoneNumber = saveUser.PhoneNumber;
                user.Email = saveUser.Email;
                user.Pass = PasswordInput.Password;
                user.Username = saveUser.Username;
                await buyerService.UpdateAUser(user);
                MessageBox.Show("Password reset successful! Please log in with your new password.");
                var loginPage = new Login();
                Window.GetWindow(this).Content = loginPage;
            }
        }
    }
}

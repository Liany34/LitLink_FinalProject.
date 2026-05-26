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
    public partial class BecomeAuthorPage : Page
    {
        private Apiservice apiService = new Apiservice();
        private User currentUser;

        public BecomeAuthorPage()
        {
            InitializeComponent();
            this.Loaded += BecomeAuthorPage_Loaded;
            currentUser = this.DataContext as User; 
        }

        private void BecomeAuthorPage_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUserPermissionsAndLoadGenres();
        }

        private async void CheckUserPermissionsAndLoadGenres()
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please log in to access this page.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.Navigate(new Uri("Pages/LoginPage.xaml", UriKind.Relative));
                return;
            }

            List<Admin> admins = await apiService.GetAllAdmins();
            List<Author> authors = await apiService.GetAllAuthors();
            if (admins.Contains(currentUser) || authors.Contains(currentUser))
            {
                MessageBox.Show("Authors and Administrators cannot create a new author profile.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Stop);

                if (this.NavigationService.CanGoBack)
                    this.NavigationService.GoBack();
                else
                    this.NavigationService?.Navigate(new Uri("Pages/HomePage.xaml", UriKind.Relative));
                return;
            }

            try
            {
                List<Genre> availableGenres = await apiService.GetAllGenres();
                CmbGenres.ItemsSource = availableGenres;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dynamic site genres: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string penName = TxtPenName.Text.Trim();
            string bio = TxtBio.Text.Trim();
            Genre selectedGenre = CmbGenres.SelectedItem as Genre;

            if (string.IsNullOrEmpty(penName) || string.IsNullOrEmpty(bio) || selectedGenre == null)
            {
                MessageBox.Show("Please fill out all fields and select a primary genre 🌸", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Author newAuthor = new Author
                {
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    PhoneNumber = currentUser.PhoneNumber,
                    Email = currentUser.Email,
                    Username = currentUser.Username,
                    Birthdate = currentUser.Birthdate,
                    Picture = currentUser.Picture,
                    Pass = currentUser.Pass,
                    PenName = penName,
                    InformationAboutAuthor = bio,
                    Genre = selectedGenre
                };

                await apiService.InsertAuthor(newAuthor);
                bool success;
                List<Author> authors = await apiService.GetAllAuthors();
                if(authors.Contains(newAuthor))
                {
                    success = true;
                }
                else
                {
                    success = false;
                }

                if (success)
                {
                    MessageBox.Show($"Congratulations ✨\nYou are now officially a registered LitLink Author! Welcome, {penName}.",
                        "LitLink Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await apiService.DeleteReader(currentUser.Id);
                    this.NavigationService?.Navigate(new Uri("Pages/AuthorProfile.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show("Registration failed. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving author registration: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack) this.NavigationService.GoBack();
        }
    }
}

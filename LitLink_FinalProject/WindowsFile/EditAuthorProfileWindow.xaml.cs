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
using System.Windows.Shapes;

namespace LitLink_FinalProject.WindowsFile
{
    /// <summary>
    /// Interaction logic for EditAuthorProfileWindow.xaml
    /// </summary>
    public partial class EditAuthorProfileWindow : Window
    {
        private Apiservice _apiService = new Apiservice();
        private Author _authorToEdit;
        private Author currentAuthor;

        public EditAuthorProfileWindow(Author currentAuthor)
        {
            InitializeComponent();
            _authorToEdit = currentAuthor;
            LoadAuthorAndUserData();
            currentAuthor = this.DataContext as Author;
        }

        private void LoadAuthorAndUserData()
        {
            // 1. טעינת נתוני הסופר לשדות
            TxtEditPenName.Text = _authorToEdit.PenName;
            TxtEditBio.Text = _authorToEdit.InformationAboutAuthor;

            // 2. טעינת נתוני המשתמש הכלליים מתוך ה-Session
            if (currentAuthor != null)
            {
                TxtEditUsername.Text = currentAuthor.Username;
                TxtEditNickname.Text = currentAuthor.Username;
                TxtEditFirstName.Text = currentAuthor.FirstName;
                TxtEditLastName.Text = currentAuthor.LastName;
                TxtEditPhone.Text = currentAuthor.PhoneNumber;
                TxtEditEmail.Text = currentAuthor.Email;
                TxtEditImgUrl.Text = currentAuthor.Picture;
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEditPenName.Text) || string.IsNullOrWhiteSpace(TxtEditUsername.Text) || string.IsNullOrWhiteSpace(TxtEditEmail.Text))
            {
                MessageBox.Show("Pen Name, Username and Email are required fields.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. עדכון אובייקט הסופר ושמירתו בטבלת Authors
                _authorToEdit.PenName = TxtEditPenName.Text.Trim();
                _authorToEdit.InformationAboutAuthor = TxtEditBio.Text.Trim();

                // 2. עדכון אובייקט המשתמש הגלובלי ושמירתו בטבלת Users
                currentAuthor.Username = TxtEditUsername.Text.Trim();
                currentAuthor.Username = TxtEditNickname.Text.Trim();
                currentAuthor.FirstName = TxtEditFirstName.Text.Trim();
                currentAuthor.LastName = TxtEditLastName.Text.Trim();
                currentAuthor.PhoneNumber = TxtEditPhone.Text.Trim();
                currentAuthor.Email = TxtEditEmail.Text.Trim();
                currentAuthor.Picture = TxtEditImgUrl.Text.Trim();
                await _apiService.UpdateAuthor(_authorToEdit);
                List<Author> authors = await _apiService.GetAllAuthors();
                bool authorSuccess = authors.Any(a => a.Id == _authorToEdit.Id && a.Username == _authorToEdit.Username && a.PenName == _authorToEdit.PenName && a.InformationAboutAuthor == _authorToEdit.InformationAboutAuthor && a.FirstName == _authorToEdit.FirstName && a.LastName == _authorToEdit.LastName && a.Email == _authorToEdit.Email && a.PhoneNumber == _authorToEdit.PhoneNumber && a.Picture == _authorToEdit.Picture);

                if (authorSuccess)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save some of the updates in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating author profile: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
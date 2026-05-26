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
    public partial class EditAuthorProfileWindow : Window
    {
        private Apiservice apiService = new Apiservice();
        private Author authorToEdit;
        private Author currentAuthor;

        public EditAuthorProfileWindow(Author currentAuthor)
        {
            InitializeComponent();
            authorToEdit = currentAuthor;
            LoadAuthorAndUserData();
            currentAuthor = this.DataContext as Author;
        }

        private void LoadAuthorAndUserData()
        {
            TxtEditPenName.Text = authorToEdit.PenName;
            TxtEditBio.Text = authorToEdit.InformationAboutAuthor;

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
                authorToEdit.PenName = TxtEditPenName.Text.Trim();
                authorToEdit.InformationAboutAuthor = TxtEditBio.Text.Trim();

                currentAuthor.Username = TxtEditUsername.Text.Trim();
                currentAuthor.Username = TxtEditNickname.Text.Trim();
                currentAuthor.FirstName = TxtEditFirstName.Text.Trim();
                currentAuthor.LastName = TxtEditLastName.Text.Trim();
                currentAuthor.PhoneNumber = TxtEditPhone.Text.Trim();
                currentAuthor.Email = TxtEditEmail.Text.Trim();
                currentAuthor.Picture = TxtEditImgUrl.Text.Trim();
                await apiService.UpdateAuthor(authorToEdit);
                List<Author> authors = await apiService.GetAllAuthors();
                bool authorSuccess = authors.Any(a => a.Id == authorToEdit.Id && a.Username == authorToEdit.Username && a.PenName == authorToEdit.PenName && a.InformationAboutAuthor == authorToEdit.InformationAboutAuthor && a.FirstName == authorToEdit.FirstName && a.LastName == authorToEdit.LastName && a.Email == authorToEdit.Email && a.PhoneNumber == authorToEdit.PhoneNumber && a.Picture == authorToEdit.Picture);

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
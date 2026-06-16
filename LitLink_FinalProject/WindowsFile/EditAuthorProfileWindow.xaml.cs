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
using Microsoft.Win32;
using System.IO;

namespace LitLink_FinalProject.WindowsFile
{
    public partial class EditAuthorProfileWindow : Window
    {
        private Apiservice apiService = new Apiservice();
        private Author authorToEdit;
        private Author currentAuthor;
        private string selectedAuthorImagePath = null;

        public EditAuthorProfileWindow(Author currentAuthor)
        {
            InitializeComponent();

            this.authorToEdit = currentAuthor;
            this.currentAuthor = currentAuthor;

            LoadAuthorAndUserData();
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
            if (!string.IsNullOrEmpty(authorToEdit.Picture))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(authorToEdit.Picture);
                    ImgAuthorPreview.Source = ByteImageConverter.ByteToImage(imageBytes);
                }
                catch
                {
                }
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEditPenName.Text) ||
                string.IsNullOrWhiteSpace(TxtEditUsername.Text) ||
                string.IsNullOrWhiteSpace(TxtEditEmail.Text))
            {
                MessageBox.Show("Pen Name, Username and Email are required fields.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                AuthorUpdateDto dto = new AuthorUpdateDto
                {
                    Id = currentAuthor.Id,

                    FirstName = TxtEditFirstName.Text.Trim(),
                    LastName = TxtEditLastName.Text.Trim(),
                    PhoneNumber = TxtEditPhone.Text.Trim(),
                    Email = TxtEditEmail.Text.Trim(),
                    Username = TxtEditUsername.Text.Trim(),
                    Pass = currentAuthor.Pass,
                    Birthdate = currentAuthor.Birthdate,

                    PicturePath = currentAuthor.PicturePath,

                    PenName = TxtEditPenName.Text.Trim(),
                    InformationAboutAuthor = TxtEditBio.Text.Trim(),
                    IdGenre = currentAuthor.Genre.Id
                };

                if (!string.IsNullOrEmpty(selectedAuthorImagePath))
                {
                    dto.FileName = System.IO.Path.GetFileName(selectedAuthorImagePath);

                    byte[] imageBytes = File.ReadAllBytes(selectedAuthorImagePath);
                    dto.Base64Image = Convert.ToBase64String(imageBytes);
                }

                bool success = await apiService.UpdateAuthor(dto);

                if (success)
                {
                    MessageBox.Show("Author profile updated successfully.", "LitLink");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Author profile was not updated.", "LitLink");
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
        private void BrowseAuthorImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedAuthorImagePath = openFileDialog.FileName;

                TxtEditImgUrl.Text = System.IO.Path.GetFileName(selectedAuthorImagePath);

                byte[] imageBytes = File.ReadAllBytes(selectedAuthorImagePath);
                ImgAuthorPreview.Source = ByteImageConverter.ByteToImage(imageBytes);
            }
        }
    }
}
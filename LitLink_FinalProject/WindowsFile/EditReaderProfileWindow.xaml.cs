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
using Model;
using Microsoft.Win32;
using System.IO;

namespace LitLink_FinalProject.WindowsFile
{
    public partial class EditReaderProfileWindow : Window
    {
        private Apiservice apiService = new Apiservice();
        private Reader currentReader;
        private string selectedReaderImagePath = null;
        public EditReaderProfileWindow(Reader reader)
        {
            InitializeComponent();
            this.currentReader = reader;
            LoadCurrentUserData();
        }

        private void LoadCurrentUserData()
        {
            if (currentReader != null)
            {
                TxtEditUsername.Text = currentReader.Username;
                TxtEditNickname.Text = currentReader.Nickname; 
                TxtEditFirstName.Text = currentReader.FirstName;
                TxtEditLastName.Text = currentReader.LastName;
                TxtEditPhone.Text = currentReader.PhoneNumber;
                TxtEditEmail.Text = currentReader.Email;
                TxtEditImgUrl.Text = currentReader.Picture;
            }
            if (!string.IsNullOrEmpty(currentReader.Picture))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(currentReader.Picture);
                    ImgReaderPreview.Source = ByteImageConverter.ByteToImage(imageBytes);
                }
                catch
                {
                }
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtEditUsername.Text) ||
                string.IsNullOrWhiteSpace(TxtEditEmail.Text))
            {
                MessageBox.Show("Username and Email are required Fields 🌸", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ReaderUpdateDto dto = new ReaderUpdateDto
                {
                    Id = currentReader.Id,

                    FirstName = TxtEditFirstName.Text.Trim(),
                    LastName = TxtEditLastName.Text.Trim(),
                    PhoneNumber = TxtEditPhone.Text.Trim(),
                    Email = TxtEditEmail.Text.Trim(),
                    Username = TxtEditUsername.Text.Trim(),
                    Pass = currentReader.Pass,
                    Birthdate = currentReader.Birthdate,

                    PicturePath = currentReader.PicturePath,

                    Nickname = TxtEditNickname.Text.Trim(),
                    IsFlaged = currentReader.IsFlaged
                };

                if (!string.IsNullOrEmpty(selectedReaderImagePath))
                {
                    dto.FileName = System.IO.Path.GetFileName(selectedReaderImagePath);

                    byte[] imageBytes = File.ReadAllBytes(selectedReaderImagePath);
                    dto.Base64Image = Convert.ToBase64String(imageBytes);
                }

                bool success = await apiService.UpdateReader(dto);

                if (success)
                {
                    MessageBox.Show("Reader profile updated successfully.", "LitLink");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Reader profile was not updated.", "LitLink");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating reader profile: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void BrowseReaderImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedReaderImagePath = openFileDialog.FileName;

                TxtEditImgUrl.Text = System.IO.Path.GetFileName(selectedReaderImagePath);

                byte[] imageBytes = File.ReadAllBytes(selectedReaderImagePath);
                ImgReaderPreview.Source = ByteImageConverter.ByteToImage(imageBytes);
            }
        }
    }
}
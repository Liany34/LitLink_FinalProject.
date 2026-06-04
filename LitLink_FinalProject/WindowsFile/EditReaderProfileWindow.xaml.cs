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
        private string selectedReaderImageBase64 = null;
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
            if (string.IsNullOrWhiteSpace(TxtEditUsername.Text) || string.IsNullOrWhiteSpace(TxtEditEmail.Text))
            {
                MessageBox.Show("Username and Email are required Fields 🌸", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                currentReader.Username = TxtEditUsername.Text.Trim();
                currentReader.Nickname = TxtEditNickname.Text.Trim();
                currentReader.FirstName = TxtEditFirstName.Text.Trim();
                currentReader.LastName = TxtEditLastName.Text.Trim();
                currentReader.PhoneNumber = TxtEditPhone.Text.Trim();
                currentReader.Email = TxtEditEmail.Text.Trim();
                if (!string.IsNullOrEmpty(selectedReaderImageBase64))
                {
                    currentReader.Picture = selectedReaderImageBase64;
                }
                else
                {
                    currentReader.Picture = TxtEditImgUrl.Text.Trim();
                }

                await apiService.UpdateUser(currentReader);
                List<Reader> updatedReaders = await apiService.GetAllReaders();
                bool isSuccess = updatedReaders.Any(r => r.Id == currentReader.Id && r.Username == currentReader.Username && r.Email == currentReader.Email && r.Nickname == currentReader.Nickname && r.FirstName == currentReader.FirstName && r.LastName == currentReader.LastName && r.PhoneNumber == currentReader.PhoneNumber && r.Picture == currentReader.Picture);
                if (isSuccess)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to update profile details in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating details: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);

                selectedReaderImageBase64 = Convert.ToBase64String(imageBytes);

                TxtEditImgUrl.Text = openFileDialog.FileName;

                ImgReaderPreview.Source = ByteImageConverter.ByteToImage(imageBytes);
            }
        }
    }
}
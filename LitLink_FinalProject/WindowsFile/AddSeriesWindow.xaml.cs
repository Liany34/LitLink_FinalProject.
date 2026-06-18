using Model;
using Service;
using System.Windows;

namespace LitLink_FinalProject.WindowsFile
{
    public partial class AddSeriesWindow : Window
    {
        private readonly Apiservice _apiService = new Apiservice();
        private readonly int _ownerId;

        public AddSeriesWindow(Reader reader)
        {
            InitializeComponent();
            _ownerId = reader.Id;
        }

        public AddSeriesWindow(Author author)
        {
            InitializeComponent();
            _ownerId = author.Id;
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtSeriesName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a list name.", "LitLink");
                return;
            }

            try
            {
                var dto = new BookSeriesInsertDto
                {
                    NameSeries = name,
                    IdUser = _ownerId
                };

                int result = await _apiService.InsertBookSeries(dto);

                if (result > 0)
                    DialogResult = true;
                else
                    MessageBox.Show("Failed to create list. Please try again.", "LitLink");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error creating list: " + ex.Message, "LitLink");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
using Model;
using Service;
using System.Windows;

namespace LitLink_FinalProject.WindowsFile
{
    public partial class AddSeriesWindow : Window
    {
        private Apiservice apiService = new Apiservice();
        private int ownerId;

        public AddSeriesWindow(Reader reader)
        {
            InitializeComponent();
            this.ownerId = reader.Id;
        }

        public AddSeriesWindow(Author author)
        {
            InitializeComponent();
            this.ownerId = author.Id;
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
                List<User> users = await apiService.GetAllUsers();
                User owner = users.Find(u => u.Id == ownerId);
                Book_Series newSeries = new Book_Series
                {
                    NameSeries = name,
                    IdUser = owner
                };
                await apiService.InsertBookSeries(newSeries);
                DialogResult = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error creating list: " + ex.Message, "LitLink");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}

using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LitLink_FinalProject.WindowsFile
{
    public partial class AddBookToSeriesWindow : Window
    {
        private Apiservice apiService = new Apiservice();
        private int ownerId;
        private List<Book> availableBooks;

        public AddBookToSeriesWindow(int ownerId, List<Book_Series> series, List<Book> books)
        {
            InitializeComponent();
            this.ownerId = ownerId;
            this.availableBooks = books;
            CmbSeries.ItemsSource = series;
            if (series.Count > 0) CmbSeries.SelectedIndex = 0;
            CmbBooks.ItemsSource = books;
            if (books.Count > 0) CmbBooks.SelectedIndex = 0;
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (CmbSeries.SelectedItem == null || CmbBooks.SelectedItem == null)
            {
                MessageBox.Show("Please select both a list and a book.", "LitLink");
                return;
            }

            Book_Series selectedSeries = (Book_Series)CmbSeries.SelectedItem;
            Book selectedBook = (Book)CmbBooks.SelectedItem;
            int number = int.TryParse(TxtNumber.Text, out int n) ? n : 1;

            try
            {
                List<Series_Detail> allDetails = await apiService.GetAllSeriesDetails();
                bool alreadyIn = allDetails.Any(d =>
                    d.IdSeries != null && d.IdSeries.Id == selectedSeries.Id &&
                    d.IdBook != null && d.IdBook.Id == selectedBook.Id);

                if (alreadyIn)
                {
                    MessageBox.Show("This book is already in that list.", "LitLink");
                    return;
                }

                Series_Detail detail = new Series_Detail
                {
                    IdSeries = selectedSeries,
                    IdBook = selectedBook,
                    Number = number
                };

                await apiService.InsertSeriesDetail(detail);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding book to list: " + ex.Message, "LitLink");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
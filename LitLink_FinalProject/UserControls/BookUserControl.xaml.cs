using LitLink_FinalProject.Pages;
using LitLink_FinalProject.WindowsFile;
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

namespace LitLink_FinalProject.UserControls
{
    public partial class BookUserControl : UserControl
    {
        private bool isAdmin;
        private bool isAuthor;
        private Apiservice apiService = new Apiservice();

        public BookUserControl(Book bookData, bool userOwnsBook, bool isAdmin, bool isAuthor, Reader currentUser = null)
        {
            InitializeComponent();
            this.DataContext = bookData;
            this.isAdmin = isAdmin;
            this.isAuthor = isAuthor;

            SetActionButtons(userOwnsBook);
            SetupPermissions();

            this.Loaded += async (s, e) => {
                if (DescriptionTextBlock.ActualHeight < 120)
                    ReadMoreBtn.Visibility = Visibility.Collapsed;

                Book currentBook = this.DataContext as Book;
                if (currentBook == null) return;

                try
                {
                    ReviewsStackPanel.Children.Clear();
                    TotalReviewsTextBlock.Text = "(0)"; 

                    List<Reviews> allReviews = await apiService.GetAllReviews();

                    if (allReviews != null)
                    {
                        var bookReviews = allReviews
                            .Where(r => r != null && r.IdBook != null && r.IdBook.Id == currentBook.Id)
                            .ToList();

                        TotalReviewsTextBlock.Text = $"({bookReviews.Count})";

                        if (bookReviews.Any())
                        {
                            double average = bookReviews.Average(r => r.Stars);
                            StarsTextBlock.Text = $"{average:0.0} ★";

                            foreach (var reviewItem in bookReviews)
                            {
                                if (reviewItem == null) continue;

                                ReviewsUserControl reviewControl = new ReviewsUserControl(reviewItem, 0, isAdmin);
                                ReviewsStackPanel.Children.Add(reviewControl);
                            }
                        }
                        else
                        {
                            StarsTextBlock.Text = "No reviews yet";
                            StarsTextBlock.FontSize = 14;
                            StarsTextBlock.Foreground = Brushes.Gray;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading reviews: " + ex.Message);
                    StarsTextBlock.Text = "— ★";
                }

                try
                {
                    string st = await apiService.GetBookCoverByBookIDByte64(currentBook.Id);

                    if (!string.IsNullOrEmpty(st))
                    {
                        byte[] imgStr = Convert.FromBase64String(st);
                        this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WPF Image Load Error]: {ex.Message}");
                }

                //            try
                //            {
                //                string st = await apiService.GetBookCoverByBookIDByte64(currentBook.Id);

                //                if (!string.IsNullOrEmpty(st))
                //                {
                //                    byte[] imgStr = Convert.FromBase64String(st);
                //                    this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
                //                }
                //                else
                //                {
                //                    this.BookCoverImage.Source = new BitmapImage(new Uri(
                //"pack://application:,,,/Covers/To_be_revealed.png", UriKind.Absolute));
                //                }
                //            }
                //            catch (Exception ex)
                //            {
                //                System.Diagnostics.Debug.WriteLine("Error loading cover image: " + ex.Message);
                //                try
                //                {
                //                    this.BookCoverImage.Source = new BitmapImage(new Uri(
                // "pack://application:,,,/Covers/To_be_revealed.png", UriKind.Absolute));
                //                }
                //                catch
                //                {
                //                    this.BookCoverImage.Source = new BitmapImage(new Uri(
                // "pack://application:,,,/Covers/To_be_revealed.png", UriKind.Absolute));
                //                }
                //            }
            };
        }

        private void SetActionButtons(bool ownsBook)
        {
            if (isAdmin)
            {
                BuyBtn.Visibility = Visibility.Collapsed;
                AddToListBtn.Visibility = Visibility.Collapsed;
            }
            else if (isAuthor)
            {
                BuyBtn.Visibility = Visibility.Collapsed;
                AddToListBtn.Visibility = Visibility.Visible;
            }
            else
            {
                AddToListBtn.Visibility = Visibility.Visible;
                BuyBtn.Visibility = ownsBook ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void SetupPermissions()
        {
            Visibility editVis = (isAdmin || isAuthor) ? Visibility.Visible : Visibility.Collapsed;
            Visibility reportVis = (isAdmin || isAuthor) ? Visibility.Collapsed : Visibility.Visible;

            EditCoverItem.Visibility = editVis;
            EditNameItem.Visibility = editVis;
            EditDescItem.Visibility = editVis;
            EditPriceItem.Visibility = editVis;
            EditDateItem.Visibility = editVis;
            DeleteItem.Visibility = editVis;
            AdminSeparator.Visibility = editVis;
            ReportItem.Visibility = reportVis;
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            BookContextMenu.PlacementTarget = sender as Button;
            BookContextMenu.IsOpen = true;
        }

        private void ReadMoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DescriptionTextBlock.MaxHeight == 120)
            {
                DescriptionTextBlock.MaxHeight = double.PositiveInfinity;
                ReadMoreBtn.Content = "Read Less";
            }
            else
            {
                DescriptionTextBlock.MaxHeight = 120;
                ReadMoreBtn.Content = "Read More";
            }
        }

        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            try
            {
                MessageBox.Show($"'{currentBook.BookName}' has been added to your purchases successfully!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);
                BuyBtn.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to complete purchase: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            try
            {
                MessageBox.Show($"'{currentBook.BookName}' has been successfully added to your Reading List!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding book to list: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            if (MessageBox.Show($"Are you sure you want to permanently delete '{currentBook.BookName}' from LitLink?",
                                "Delete Book", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    apiService.DeleteBook(currentBook.Id);
                    MessageBox.Show("The book has been successfully deleted.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete book: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ReportBook_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            if (MessageBox.Show("Report this book for inappropriate content?", "Report Book", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    currentBook.IsFlaged = true;
                    apiService.UpdateBook(currentBook);

                    MessageBox.Show("Thank you. This book has been flagged and will be reviewed by an administrator.", "Report Sent");
                    ReportItem.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to send report: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            try
            {
                EditBookWindow editWindow = new EditBookWindow(currentBook);

                if (editWindow.ShowDialog() == true)
                {
                    this.DataContext = null;
                    this.DataContext = currentBook;

                    if (!string.IsNullOrEmpty(currentBook.Cover))
                    {
                        byte[] imgStr = Convert.FromBase64String(currentBook.Cover);
                        this.BookCoverImage.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open edit window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
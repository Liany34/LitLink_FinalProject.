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
using Model;
using Service;

namespace LitLink_FinalProject.UserControls
{
    public partial class ReviewsUserControl : UserControl
    {
        private Reviews review;
        private Apiservice apiService = new Apiservice();

        public ReviewsUserControl(Reviews reviewData, int currentUserId, bool isAdmin)
        {
            InitializeComponent();
            this.review = reviewData;
            this.DataContext = reviewData;

            ConfigureMenuPermissions(currentUserId, isAdmin);

            this.Loaded += (s, e) => {
                if (CommentTextBlock.ActualHeight < 100)
                    ReadMoreBtn.Visibility = Visibility.Collapsed;

                if (review != null && review.IdReader != null && !string.IsNullOrEmpty(review.IdReader.Picture))
                {
                    try
                    {
                        byte[] imgStr = Convert.FromBase64String(review.IdReader.Picture);
                        this.ReaderProfileImage.Source = ByteImageConverter.ByteToImage(imgStr);
                    }
                    catch (Exception)
                    {
                        this.ReaderProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/PRP/DefultUser.png", UriKind.RelativeOrAbsolute));
                    }
                }
                else
                {
                    this.ReaderProfileImage.Source = new BitmapImage(new Uri("pack://application:,,,/PRP/DefultUser.png", UriKind.RelativeOrAbsolute));
                }
            };
        }

        private void ConfigureMenuPermissions(int currentUserId, bool isAdmin)
        {
            bool isMyComment = (review.IdReader.Id == currentUserId);

            DeleteOwnItem.Visibility = isMyComment ? Visibility.Visible : Visibility.Collapsed;

            ReportItem.Visibility = (!isMyComment && !isAdmin) ? Visibility.Visible : Visibility.Collapsed;

            AdminDeleteItem.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            RemoveReportItem.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CommentMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            CommentContextMenu.PlacementTarget = sender as Button;
            CommentContextMenu.IsOpen = true;
        }

        private void ReadMoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CommentTextBlock.MaxHeight == 100)
            {
                CommentTextBlock.MaxHeight = double.PositiveInfinity;
                ReadMoreBtn.Content = "Read Less";
            }
            else
            {
                CommentTextBlock.MaxHeight = 100;
                ReadMoreBtn.Content = "Read More";
            }
        }
        private void DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this comment?", "LitLink", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    apiService.DeleteReview(review.Id);

                    MessageBox.Show("Comment deleted successfully.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete comment: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ReportComment_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to report this comment?", "Report", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Reviews updatedReview = new Reviews();
                updatedReview.Id = review.Id;
                updatedReview.IdReader = review.IdReader;
                updatedReview.IdBook = review.IdBook;
                updatedReview.Text = review.Text;
                updatedReview.Stars = review.Stars;
                updatedReview.IsFlaged = true;

                try
                {
                    apiService.UpdateReview(updatedReview);

                    review.IsFlaged = true;
                    MessageBox.Show("Thank you. The comment has been flagged for review.", "Report Sent");
                    this.Opacity = 0.6;
                    ReportItem.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to report comment: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveReport_Click(object sender, RoutedEventArgs e)
        {
            Reviews updatedReview = new Reviews();
            updatedReview.Id = review.Id;
            updatedReview.IdReader = review.IdReader;
            updatedReview.IdBook = review.IdBook;
            updatedReview.Text = review.Text;
            updatedReview.Stars = review.Stars;
            updatedReview.IsFlaged = false;

            try
            {
                apiService.UpdateReview(updatedReview);

                review.IsFlaged = false;
                MessageBox.Show("The flag has been successfully removed.", "Admin Control");
                this.Opacity = 1.0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove flag: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
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
    /// <summary>
    /// Interaction logic for ReviewsUserControl.xaml
    /// </summary>
    public partial class ReviewsUserControl : UserControl
    {
        private Reviews _review;
        private Apiservice apiService = new Apiservice(); // ריכוז הגדרת ה-Service למעלה

        public ReviewsUserControl(Reviews reviewData, int currentUserId, bool isAdmin)
        {
            InitializeComponent();
            this._review = reviewData;
            this.DataContext = reviewData;

            // בדיקת הרשאות לתפריט שלוש הנקודות
            ConfigureMenuPermissions(currentUserId, isAdmin);

            // הסתרת "Read More" אם הטקסט לא ארוך מספיק (פחות מ-5 שורות)
            this.Loaded += (s, e) => {
                if (CommentTextBlock.ActualHeight < 100)
                    ReadMoreBtn.Visibility = Visibility.Collapsed;
            };
        }

        private void ConfigureMenuPermissions(int currentUserId, bool isAdmin)
        {
            // האם אני כתבתי את התגובה הזו?
            bool isMyComment = (_review.IdReader.Id == currentUserId);

            // 1. הגדרות לבעל התגובה
            DeleteOwnItem.Visibility = isMyComment ? Visibility.Visible : Visibility.Collapsed;

            // 2. הגדרות דיווח (מוצג רק אם זה לא המנהל וזו לא התגובה שלו)
            ReportItem.Visibility = (!isMyComment && !isAdmin) ? Visibility.Visible : Visibility.Collapsed;

            // 3. הגדרות מנהל
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

        // ==================== לוגיקת עריכה, מחיקה ודיווח ====================

        // 2. לחיצה על כפתור Save (שומר את הטקסט החדש לאקסס ומעדכן את המסך)
        private void SaveComment_Click(object sender, RoutedEventArgs e)
        {
            string updatedText = EditCommentTextBox.Text.Trim();

            if (string.IsNullOrEmpty(updatedText))
            {
                MessageBox.Show("Comment cannot be empty!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Reviews updatedReview = new Reviews();
            updatedReview.Id = _review.Id;
            updatedReview.IdReader = _review.IdReader;
            updatedReview.IdBook = _review.IdBook;
            updatedReview.Text = updatedText;
            updatedReview.Stars = _review.Stars;
            updatedReview.IsFlaged = _review.IsFlaged;

            try
            {
                // קריאה ישירה לפונקציה (בגלל שהיא void ולא מחזירה ערך)
                apiService.UpdateReview(updatedReview);

                // אם הגענו לשורות האלו - סימן שלא נזרקה שגיאה והכל הצליח!
                _review.Text = updatedText;
                CommentTextBlock.Text = updatedText;

                SwitchToDisplayMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save changes: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 3. לחיצה על כפתור Cancel
        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            SwitchToDisplayMode();
        }

        // פונקציית עזר פרטית להחזרת ה-UI למצב קריאה רגיל
        private void SwitchToDisplayMode()
        {
            DisplayArea.Visibility = Visibility.Visible;
            EditArea.Visibility = Visibility.Collapsed;
            CommentMenuBtn.Visibility = Visibility.Visible; // החזרת כפתור שלוש הנקודות
        }

        // 4. מחיקת תגובה
        private void DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this comment?", "LitLink", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // קריאה ישירה לפונקציה (מכיוון שהיא void)
                    apiService.DeleteReview(_review.Id);

                    // אם הגענו לכאן - המחיקה באקסס הצליחה! נציג הודעה ונעלים את הרכיב מהמסך
                    MessageBox.Show("Comment deleted successfully.", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);

                    // העלמת ה-UserControl מהרשימה כדי שהמשתמש יראה מיד שהתגובה נעלמה
                    this.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    // במקרה של שגיאה במסד הנתונים (למשל בעיית התחברות)
                    MessageBox.Show("Failed to delete comment: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 5. דיווח על תגובה
        private void ReportComment_Click(object sender, RoutedEventArgs e) 
        {
            if (MessageBox.Show("Are you sure you want to report this comment?", "Report", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Reviews updatedReview = new Reviews();
                updatedReview.Id = _review.Id;
                updatedReview.IdReader = _review.IdReader;
                updatedReview.IdBook = _review.IdBook;
                updatedReview.Text = _review.Text;
                updatedReview.Stars = _review.Stars;
                updatedReview.IsFlaged = true;

                try
                {
                    // קריאה ישירה לפונקציה
                    apiService.UpdateReview(updatedReview);

                    _review.IsFlaged = true;
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

        // 6. ביטול דיווח על תגובה (Admin)
        private void RemoveReport_Click(object sender, RoutedEventArgs e) 
        {
            Reviews updatedReview = new Reviews();
            updatedReview.Id = _review.Id;
            updatedReview.IdReader = _review.IdReader;
            updatedReview.IdBook = _review.IdBook;
            updatedReview.Text = _review.Text;
            updatedReview.Stars = _review.Stars;
            updatedReview.IsFlaged = false;

            try
            {
                // קריאה ישירה לפונקציה
                apiService.UpdateReview(updatedReview);

                _review.IsFlaged = false;
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

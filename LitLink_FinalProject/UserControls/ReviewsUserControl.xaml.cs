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
            EditItem.Visibility = isMyComment ? Visibility.Visible : Visibility.Collapsed;
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

        // מקום ללוגיקה של ה-Access שלך
        private void EditComment_Click(object sender, RoutedEventArgs e) { /* פתחי חלון עריכה */ }
        private void DeleteComment_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this comment?", "LitLink", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // קוד למחיקה מהמסד נתונים
            }
        }
        private void ReportComment_Click(object sender, RoutedEventArgs e) 
        {
            /* עדכון IsReported ל-True */
        }
        private void RemoveReport_Click(object sender, RoutedEventArgs e) 
        { 
            /* עדכון IsReported ל-False */ 
        }
    }
}

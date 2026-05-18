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
    /// <summary>
    /// Interaction logic for BookUserControl.xaml
    /// </summary>
    public partial class BookUserControl : UserControl
    {
        private bool _isAdmin;
        private bool _isAuthor;
        private Apiservice apiService = new Apiservice();

        // constructor מקבל עכשיו מידע על המשתמש
        public BookUserControl(Book bookData, bool userOwnsBook, bool isAdmin, bool isAuthor)
        {
            InitializeComponent();
            this.DataContext = bookData;
            this._isAdmin = isAdmin;
            this._isAuthor = isAuthor;

            // 1. הגדרת כפתורי פעולה (Buy/AddToList)
            SetActionButtons(userOwnsBook);

            // 2. הגדרת תפריט שלוש נקודות (Permissions)
            SetupPermissions();

            // 3. בדיקת אורך טקסט
            this.Loaded += (s, e) => {
                if (DescriptionTextBlock.ActualHeight < 120)
                    ReadMoreBtn.Visibility = Visibility.Collapsed;
            };
        }

        private void SetActionButtons(bool ownsBook)
        {
            if (_isAdmin)
            {
                // מנהל: לא רואה אף כפתור פעולה
                BuyBtn.Visibility = Visibility.Collapsed;
                AddToListBtn.Visibility = Visibility.Collapsed;
            }
            else if (_isAuthor)
            {
                // סופר של הספר: רואה רק "Add to List"
                BuyBtn.Visibility = Visibility.Collapsed;
                AddToListBtn.Visibility = Visibility.Visible;
            }
            else
            {
                // קורא רגיל: רואה Add to List ותלוי אם קנה את הספר או לא
                AddToListBtn.Visibility = Visibility.Visible;

                if (ownsBook)
                {
                    // אם הספר כבר בבעלותו, נעלים את כפתור הרכישה
                    BuyBtn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BuyBtn.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetupPermissions()
        {
            // מנהל או סופר רואים אפשרויות עריכה
            Visibility editVis = (_isAdmin || _isAuthor) ? Visibility.Visible : Visibility.Collapsed;
            // רק קורא (שאינו מנהל/סופר הספר) רואה דיווח
            Visibility reportVis = (_isAdmin || _isAuthor) ? Visibility.Collapsed : Visibility.Visible;

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

        // ==================== לוגיקת פעולות ספר ====================

        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            Book currentBook = this.DataContext as Book;
            if (currentBook == null) return;

            try
            {
                // זמני לבגרות: מציגים הודעה חיובית ומעלימים את כפתור הרכישה מהמסך
                MessageBox.Show($"'{currentBook.BookName}' has been added to your purchases successfully!", "LitLink", MessageBoxButton.OK, MessageBoxImage.Information);

                // עדכון ויזואלי מהיר: העלמת כפתור הרכישה לאחר שנקנה
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
                // במקום לפתוח חלון שלא קיים, נציג הודעה חלקה שהספר נוסף בהצלחה לרשימת הקריאה
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
                    // רענון ה-DataContext כדי שהעדכונים שנעשו בטופס יופיעו מיד על המסך
                    this.DataContext = null;
                    this.DataContext = currentBook;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open edit window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

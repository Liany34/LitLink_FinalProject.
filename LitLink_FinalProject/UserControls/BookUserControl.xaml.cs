using LitLink_FinalProject.Pages;
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

        // constructor מקבל עכשיו מידע על המשתמש
        public BookUserControl(Book bookData, bool userOwnsBook, bool isAdmin, bool isAuthor)
        {
            InitializeComponent();
            this.DataContext = bookData;
            this._isAdmin = isAdmin;
            this._isAuthor = isAuthor;

            // 1. הגדרת כפתורי פעולה (Buy/Read/AddToList)
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
                ReadBtn.Visibility = Visibility.Collapsed;
                AddToListBtn.Visibility = Visibility.Collapsed;
            }
            else if (_isAuthor)
            {
                // סופר של הספר: רואה רק "Add to List"
                BuyBtn.Visibility = Visibility.Collapsed;
                ReadBtn.Visibility = Visibility.Collapsed;
                AddToListBtn.Visibility = Visibility.Visible;
            }
            else
            {
                // קורא רגיל: רואה Add to List ותלוי אם קנה את הספר או לא
                AddToListBtn.Visibility = Visibility.Visible;

                if (ownsBook)
                {
                    BuyBtn.Visibility = Visibility.Collapsed;
                    ReadBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    BuyBtn.Visibility = Visibility.Visible;
                    ReadBtn.Visibility = Visibility.Collapsed;
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

        Apiservice apiService = new Apiservice();

        // פונקציות ריקות לפעולות שלך
        private void Buy_Click(object sender, RoutedEventArgs e) 
        {
            
        }
        private void Read_Click(object sender, RoutedEventArgs e) 
        {
           
        }
        private void AddToList_Click(object sender, RoutedEventArgs e) 
        {

        }
        private void DeleteBook_Click(object sender, RoutedEventArgs e) 
        { 

        }
        private void ReportBook_Click(object sender, RoutedEventArgs e) 
        { 

        }
        private void EditBook_Click(object sender, RoutedEventArgs e) 
        {

        }
    }
}

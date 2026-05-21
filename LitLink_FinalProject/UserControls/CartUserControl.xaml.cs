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
    /// Interaction logic for CartUserControl.xaml
    /// </summary>
    public partial class CartUserControl : UserControl
    {
        public event EventHandler MoveToWishListRequested;

        // אירוע שיופעל כאשר המשתמש מסמן או מוריד את הסימון (Checkbox) של הספר לצורך קנייה
        public event EventHandler IsSelectedChanged;

        public CartUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// מאפיין שמחזיר האם הספר הנוכחי מסומן ב-Checkbox לקנייה
        /// </summary>
        public bool IsBookSelected
        {
            get => Convert.ToBoolean(CartCheckBox.IsChecked);
            set => CartCheckBox.IsChecked = value;
        }

        // לחיצה על כפתור MOVE TO WISH LIST
        private void MoveToWishList_Click(object sender, RoutedEventArgs e)
        {
            // בדיקה אם מישהו (למשל דף העגלה הראשי) נרשם לאירוע הזה
            MoveToWishListRequested?.Invoke(this, EventArgs.Empty);

            // דוגמה להודעה זמנית לבדיקה (תוכלי למחוק אותה אחרי שהכל מחובר)
            MessageBox.Show("The book has been moved to your Wish List!", "Wish List", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // הפעלת אירוע כאשר ה-Checkbox משנה את מצבו
        private void CartCheckBox_CheckedChange(object sender, RoutedEventArgs e)
        {
            IsSelectedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
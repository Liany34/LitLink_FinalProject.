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
    public partial class CatalogUserControl : UserControl
    {
        public CatalogUserControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public static readonly DependencyProperty BookNameProperty =
            DependencyProperty.Register("BookName", typeof(string), typeof(CatalogUserControl), new PropertyMetadata(string.Empty));

        public string BookName
        {
            get { return (string)GetValue(BookNameProperty); }
            set { SetValue(BookNameProperty, value); }
        }
        public static readonly DependencyProperty PriceProperty =
            DependencyProperty.Register("Price", typeof(decimal), typeof(CatalogUserControl), new PropertyMetadata(0.00m));

        public decimal Price
        {
            get { return (decimal)GetValue(PriceProperty); }
            set { SetValue(PriceProperty, value); }
        }

        public static readonly DependencyProperty CoverProperty =
            DependencyProperty.Register("Cover", typeof(ImageSource), typeof(CatalogUserControl), new PropertyMetadata(null));

        public ImageSource Cover
        {
            get { return (ImageSource)GetValue(CoverProperty); }
            set { SetValue(CoverProperty, value); }
        }
    }
}

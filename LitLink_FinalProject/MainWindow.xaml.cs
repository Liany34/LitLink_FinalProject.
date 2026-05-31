using LitLink_FinalProject.Pages;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LitLink_FinalProject
{
    public partial class MainWindow : Window
    {
        public static Frame AppFrame { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            AppFrame = MainFrame;
            MainFrame.Navigate(new LogoPage());
        }
    }
}
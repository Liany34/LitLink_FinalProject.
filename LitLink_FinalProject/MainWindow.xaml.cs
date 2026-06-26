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

            MainFrame.Navigating += (s, e) =>
                System.Diagnostics.Debug.WriteLine($"[FRAME-NAVIGATING] To: {e.Content}");

            MainFrame.Navigated += (s, e) =>
                System.Diagnostics.Debug.WriteLine($"[FRAME-NAVIGATED] Content: {e.Content}");

            MainFrame.NavigationFailed += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[FRAME-NAVIGATION-FAILED] Exception: {e.Exception}");
                e.Handled = true;
            };

            MainFrame.Navigate(new LogoPage());
        }
    }
}
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
using System.Windows.Threading;

namespace LitLink_FinalProject.Pages
{
    /// <summary>
    /// Interaction logic for LogoPage.xaml
    /// </summary>
    public partial class LogoPage : Page
    {
        private DispatcherTimer timer;
        public LogoPage()
        {
            InitializeComponent();
            StartTimer();
        }
        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick; // ברגע שעוברות 5 שניות נשלח לפעולה שדואגת שהטיימר יפסיק
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            NavigationService.Navigate(new HomePage());
        }
    }
}

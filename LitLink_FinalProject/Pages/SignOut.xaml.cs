using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LitLink_FinalProject.Pages
{
    public partial class SignOut : Page
    {
        private DispatcherTimer timer;

        public SignOut()
        {
            InitializeComponent();
            StartTimer();
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            MainWindow.AppFrame.Navigate(new Login());
        }
    }
}
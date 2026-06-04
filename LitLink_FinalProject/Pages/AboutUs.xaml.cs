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
using System.Xml.Linq;
using Model;
using Service;

namespace LitLink_FinalProject.Pages
{
    public partial class AboutUs : Page
    {
        private Reader loggedUser;

        public AboutUs(Reader currentReadr)
        {
            InitializeComponent();
            this.loggedUser = currentReadr;
        }
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AppFrame.Navigate(new HomePage(loggedUser));
        }
    }
}

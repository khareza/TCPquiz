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
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace TCPquiz_Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string ipaddr = @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$";

            Match match = Regex.Match(IPAddr.Text, ipaddr);

            if (String.IsNullOrWhiteSpace(UserName.Text))
            {
                MessageBox.Show("Nazwa gracza nie może być pusta");
            }
            else
            {

                 if (match.Success)
                 {
                     MainWindow MainWindow = new MainWindow();
                     MainWindow.Name = UserName.Text;
                     this.Close();
                     try
                     {
                         MainWindow?.Show();
                     }
                     catch (Exception)
                     {

                     }
                 }
                 else
                 {
                     MessageBox.Show("Błędny adres IP");
                 }
            }

        }
    }
}

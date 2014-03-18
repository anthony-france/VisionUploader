using System;
using System.Collections.Generic;
using System.Configuration;
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
using VisionUploader.Classes;

namespace VisionUploader
{
    /// <summary>
    /// Interaction logic for AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        public AccountWindow()
        {
            InitializeComponent();

            string username = Properties.Settings.Default.username.ToString();
            string password = Properties.Settings.Default.password.ToString();

            txtUsername.Text = username;
            txtPassword.Password = CredentialsHelper.ToInsecureString( CredentialsHelper.DecryptString(password) );

        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.username = txtUsername.Text;
            Properties.Settings.Default.password = CredentialsHelper.EncryptString(CredentialsHelper.ToSecureString(txtPassword.Password));
            Properties.Settings.Default.Save();
            Close();
        }
    }
}

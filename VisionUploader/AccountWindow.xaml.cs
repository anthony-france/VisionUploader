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
            
            txtServer.Text = Properties.Settings.Default.server;
            txtPort.Text = Properties.Settings.Default.port;
            txtUsername.Text = Properties.Settings.Default.username;
            txtPassword.Password = CredentialsHelper.ToInsecureString(CredentialsHelper.DecryptString(Properties.Settings.Default.password));
            txtKeyFile.Text = Properties.Settings.Default.public_key_file;

            txtServer.IsEnabled = Properties.Settings.Default.edit_server_field;
            txtPort.IsEnabled = Properties.Settings.Default.edit_port_field;
            txtUsername.IsEnabled = Properties.Settings.Default.edit_username_field;
            txtPassword.IsEnabled = Properties.Settings.Default.edit_password_field;
            txtKeyFile.IsEnabled = Properties.Settings.Default.edit_publickeyfile_field;
            btnLocateKeyFile.IsEnabled = Properties.Settings.Default.edit_publickeyfile_field;
        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.edit_server_field)
            {
                Properties.Settings.Default.server = txtServer.Text;
            }

            if (Properties.Settings.Default.edit_port_field)
            {
                Properties.Settings.Default.port = txtPort.Text;
            }

            if (Properties.Settings.Default.edit_username_field)
            {
                Properties.Settings.Default.username = txtUsername.Text;
            }

            if (Properties.Settings.Default.edit_password_field)
            {
                Properties.Settings.Default.password = CredentialsHelper.EncryptString(CredentialsHelper.ToSecureString(txtPassword.Password));
            }

            if (Properties.Settings.Default.edit_publickeyfile_field)
            {
                Properties.Settings.Default.public_key_file = txtKeyFile.Text;
            }

            Properties.Settings.Default.Save();
            Close();
        }

        private void btnLocateKeyFile_Click(object sender, RoutedEventArgs e)
        {
                var dialog = new System.Windows.Forms.OpenFileDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txtKeyFile.Text = dialog.FileName;
                }
        }
    }
}

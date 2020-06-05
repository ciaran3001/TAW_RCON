using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TAWRcon_HLL_WPF.Models;
using TAWRcon_HLL_WPF.Properties;

namespace TAWRcon_HLL_WPF.Forms
{
    /// <summary>
    /// Interaction logic for connectdialog.xaml
    /// </summary>
    public partial class ConnectDialog : Window, IComponentConnector
    {
        internal Grid connectionDialogGrid;
        internal Label lblAddress;
        internal TextBox txtAddress;
        internal Label lblPort;
        internal TextBox txtPort;
        internal Label lblPassword;
        internal PasswordBox txtPassword;
        internal TextBlock txtblockError;
        internal Button btnDialogOk;
        private bool _contentLoaded;

        public ServerConnectionDetails ConnectionDetails { get; private set; }

        private string ServerAddress
        {
            get
            {
                return this.txtAddress.Text;
            }
        }

        private int ServerPort
        {
            get
            {
                int result = -1;
                if (!int.TryParse(this.txtPort.Text, out result))
                    return -1;
                return result;
            }
        }

        private string ServerPassword
        {
            get
            {
                return this.txtPassword.Password;
            }
        }

        public ConnectDialog(ServerConnectionDetails connectionDetails = default(ServerConnectionDetails))
        {
            this.InitializeComponent();
            if (connectionDetails.Equals((object)new ServerConnectionDetails()))
            {
                this.txtAddress.Text = Settings.Default.LastUsedServerAddress;
                this.txtPort.Text = Settings.Default.LastUsedServerPort.ToString();
            }
            else
            {
                this.txtAddress.Text = connectionDetails.ServerAddress;
                this.txtPort.Text = connectionDetails.ServerPort.ToString();
                this.txtPassword.Password = connectionDetails.ServerPassword;
            }
            this.txtblockError.Text = this.VersionString();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.txtblockError.Text = "";
            bool flag = true;
            if (string.IsNullOrWhiteSpace(this.ServerAddress))
            {
                this.txtblockError.Text += "A server address is needed!\n";
                flag = false;
            }
            if (this.ServerPort < 0 || this.ServerPort > (int)ushort.MaxValue)
            {
                this.txtblockError.Text += "Please provide a valid portnumber!\n";
                flag = false;
            }
            if (string.IsNullOrWhiteSpace(this.ServerPassword))
            {
                this.txtblockError.Text += "No rcon password provided!\n";
                flag = false;
            }
            if (!flag)
                return;
            this.ConnectionDetails = new ServerConnectionDetails(this.ServerAddress.Trim(), this.ServerPort, this.ServerPassword);
            Settings.Default.LastUsedServerAddress = this.ServerAddress;
            Settings.Default.LastUsedServerPort = this.ServerPort;
            Settings.Default.Save();
            this.DialogResult = new bool?(true);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtAddress.Text))
            {
                this.txtAddress.Focus();
            }
            else
            {
                this.txtPassword.Focus();
                this.txtPassword.SelectAll();
            }
        }

        protected string VersionString()
        {
            string[] strArray = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
            string str = "$Change: 642594 $";
            return "v" + string.Join(".", strArray[0], strArray[1], str.Substring(9).TrimEnd('$'));
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/RconClient;component/connectdialog.xaml", UriKind.Relative));
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    ((Window)target).ContentRendered += new EventHandler(this.Window_ContentRendered);
                    break;
                case 2:
                    this.connectionDialogGrid = (Grid)target;
                    break;
                case 3:
                    this.lblAddress = (Label)target;
                    break;
                case 4:
                    this.txtAddress = (TextBox)target;
                    break;
                case 5:
                    this.lblPort = (Label)target;
                    break;
                case 6:
                    this.txtPort = (TextBox)target;
                    break;
                case 7:
                    this.lblPassword = (Label)target;
                    break;
                case 8:
                    this.txtPassword = (PasswordBox)target;
                    break;
                case 9:
                    this.txtblockError = (TextBlock)target;
                    break;
                case 10:
                    this.btnDialogOk = (Button)target;
                    this.btnDialogOk.Click += new RoutedEventHandler(this.btnDialogOk_Click);
                    break;
                default:
                    this._contentLoaded = true;
                    break;
            }
        }
    }
}

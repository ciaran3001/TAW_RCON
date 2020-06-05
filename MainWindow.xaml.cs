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
using TAWRcon_HLL_WPF.Forms;
using TAWRcon_HLL_WPF.Helpers;
using TAWRcon_HLL_WPF.Models;

namespace TAWRcon_HLL_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IComponentConnector
    {
        internal DockPanel MainWindowContainer;
        private bool _contentLoaded;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = this.Title + " v" + this.VersionString();
            RconStaticLibrary.UpdateAvailableCommandsAndGetters();
            ConnectDialog connectDialog = new ConnectDialog(new ServerConnectionDetails());
            bool? nullable = connectDialog.ShowDialog();
            bool flag = false;
            if ((nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                this.Close();
            Task.Run((Action)(() => this.StartNewSessionAndCreateUI(connectDialog)));
        }

        private void StartNewSessionAndCreateUI(ConnectDialog connectDialog)
        {
            ServerSession newSession = new ServerSession(connectDialog.ConnectionDetails);
            this.Dispatcher.Invoke((Action)(() => this.MainWindowContainer.Children.Add((UIElement)new ServerControl(newSession))));
        }

        public void OnNewSessionStarted(ConnectDialog connectDialog)
        {
            this.MainWindowContainer.Children.Clear();
            this.StartNewSessionAndCreateUI(connectDialog);
        }

        protected string VersionString()
        {
            string[] strArray = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.');
            string str = "$Change: 602602 $";
            return string.Join(".", strArray[0], strArray[1], str.Substring(9).TrimEnd('$'));
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/RconClient;component/mainwindow.xaml", UriKind.Relative));
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            if (connectionId == 1)
                this.MainWindowContainer = (DockPanel)target;
            else
                this._contentLoaded = true;
        }
    }
}

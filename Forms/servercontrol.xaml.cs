using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Timers;
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
using TAWRcon_HLL_WPF.Helpers;
using TAWRcon_HLL_WPF.Models;

namespace TAWRcon_HLL_WPF.Forms
{
    public class ServerControl : UserControl, IComponentConnector
    {
        private static int s_getterInterval = 10000;
        private Dictionary<RconGetter, string> m_getterToData = new Dictionary<RconGetter, string>();
        private Timer m_getterTimer;
        private ServerSession m_serverSession;
        internal WrapPanel wrapPanelCommands;
        internal Rectangle rectangleFeedback;
        internal TextBox textBlockFeedback;
        internal ListBox listBoxServerInfo;
        private bool _contentLoaded;

        public ServerControl(ServerSession serverSession)
        {
            this.InitializeComponent();
            this.m_serverSession = serverSession;
            this.DataContext = (object)serverSession;
            this.CreateReconnectButton();
            this.CreateCommandButtons();
            this.CreateServerInfoView();
            this.StartGetterTimer();
        }

        ~ServerControl()
        {
            this.m_getterTimer.Enabled = false;
        }

        private void CreateCommandButtons()
        {
            Binding binding = new Binding("Authenticated");
            binding.Source = (object)this.m_serverSession;
            foreach (RconCommand availableCommand in RconStaticLibrary.AvailableCommands)
            {
                Button button1 = new Button();
                button1.Content = (object)availableCommand.m_name;
                button1.Margin = new Thickness(0.0, 5.0, 0.0, 5.0);
                button1.Padding = new Thickness(4.0);
                button1.DataContext = (object)availableCommand;
                Button button2 = button1;
                button2.Click += new RoutedEventHandler(this.OnCommandButtonClicked);
                button2.SetBinding(UIElement.IsEnabledProperty, (BindingBase)binding);
                this.wrapPanelCommands.Children.Add((UIElement)button2);
            }
        }

        private void CreateReconnectButton()
        {
            Button button1 = new Button();
            button1.Content = (object)"Connect to...";
            button1.Margin = new Thickness(0.0, 5.0, 0.0, 5.0);
            button1.Padding = new Thickness(4.0);
            Button button2 = button1;
            button2.Click += new RoutedEventHandler(this.OnReconnectClicked);
            this.wrapPanelCommands.Children.Add((UIElement)button2);
        }

        private void OnReconnectClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ConnectDialog connectDialog = new ConnectDialog(this.m_serverSession.ConnectionDetails);
            bool? nullable = connectDialog.ShowDialog();
            bool flag = true;
            if ((nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                return;
            (Application.Current.MainWindow as MainWindow).OnNewSessionStarted(connectDialog);
        }

        private void CreateServerInfoView()
        {
            this.listBoxServerInfo.DataContext = (object)this.m_serverSession;
        }

        private void StartGetterTimer()
        {
            this.m_getterTimer = new Timer((double)ServerControl.s_getterInterval)
            {
                AutoReset = true,
                Enabled = true
            };
            this.m_getterTimer.Elapsed += new ElapsedEventHandler(this.OnGetterIntervalElapsed);
        }

        private void OnCommandButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ((sender as FrameworkElement).DataContext as RconCommand).StartExecuting(this.m_serverSession);
        }

        private void OnGetterIntervalElapsed(object source, ElapsedEventArgs e)
        {
            this.m_serverSession.UpdateServerInfo();
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/RconClient;component/servercontrol.xaml", UriKind.Relative));
        }

        [DebuggerNonUserCode]
        [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:
                    this.wrapPanelCommands = (WrapPanel)target;
                    break;
                case 2:
                    this.rectangleFeedback = (Rectangle)target;
                    break;
                case 3:
                    this.textBlockFeedback = (TextBox)target;
                    break;
                case 4:
                    this.listBoxServerInfo = (ListBox)target;
                    break;
                default:
                    this._contentLoaded = true;
                    break;
            }
        }
    }
}

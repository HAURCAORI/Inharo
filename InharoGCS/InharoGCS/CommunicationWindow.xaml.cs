using ControlzEx.Theming;
using InharoGCS.NetworkProperty;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace InharoGCS
{
    /// <summary>
    /// CommunicationWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommunicationWindow : Window
    {
        private ScrollViewer? _scrollViewer;

        public CommunicationWindow()
        {
            InitializeComponent();

            MainWindow.ThemeChanged += ChangeTheme;
            Connection.GetInstance.MainConnectedEvent += UpdateMainStatus;
            Connection.GetInstance.ProxyConnectedEvent += UpdateProxyStatus;

            SendDataTimer.Tick += new EventHandler(sendTelemetry);
        }

        private void ChangeTheme(object sender, ThemeEventArgs e)
        {
            if (e.IsDarkMode)
            {
                ThemeManager.Current.ChangeTheme(this, ((App)Application.Current).themeDark);
            }
            else
            {
                ThemeManager.Current.ChangeTheme(this, ((App)Application.Current).themeLight);
            }
        }

        #region 패킷 리스트 뷰 관련
        private void tbtPause_Click(object sender, RoutedEventArgs e)
        {
            if (tbtPause.IsChecked == true)
            {
                lsvDataLog.Pause = true;
            }
            else
            {
                lsvDataLog.Pause = false;
            }
        }

        private void lsvDataLog_LayoutUpdated(object sender, EventArgs e)
        {
            if (_scrollViewer == null)
            {
                _scrollViewer = SearchChild.GetChildOfType<ScrollViewer>(lsvDataLog);
                if (_scrollViewer != null)
                    _scrollViewer.ScrollChanged += Scroller_ScrollChanged;
            }
        }

        private void Scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(_scrollViewer == null) { return; }
            if (_scrollViewer.ScrollableHeight - e.VerticalOffset < 3)
            {
                lsvDataLog.AutoScroll = true;
            }
            else
            {
                lsvDataLog.AutoScroll = false;
            }
        }
        #endregion

        public void UpdateMainSetting()
        {
            UpdateMainStatus(this, new EventArgs());
        }

        public void UpdateProxySetting()
        {
            UpdateProxyStatus(this, new EventArgs());
        }

        #region Status
        public async void UpdateMainStatus(object sender, EventArgs e)
        {
            prStatus.IsActive = true;
            btRefresh.IsEnabled = false;
            tbName.Text = "";
            tbSerialNumber.Text = "";
            tbAddress64.Text = "";
            tbAddress16.Text = "";
            tbPort.Text = "";
            tbPANID.Text = "";
            tbChannel.Text = "";
            var status = await Connection.GetInstance.GetInfo();
            tbName.Text = status.Name;
            tbSerialNumber.Text = status.SerialNumber;
            tbAddress64.Text = status.Address64;
            tbAddress16.Text = status.Address16;
            tbPort.Text = status.Port;
            tbPANID.Text = status.PanID;
            tbChannel.Text = status.Channel;
            btRefresh.IsEnabled = true;
            prStatus.IsActive = false;
        }

        public async void UpdateProxyStatus(object sender, EventArgs e)
        {
            prStatusProxy.IsActive = true;
            btRefreshProxy.IsEnabled = false;
            tbNameProxy.Text = "";
            tbAddress64Proxy.Text = "";
            tbAddress16Proxy.Text = "";
            tbPortProxy.Text = "";
            tbPANIDProxy.Text = "";
            tbChannelProxy.Text = "";
            var status = await Connection.GetInstance.GetInfoProxy();
            tbNameProxy.Text = status.Name;
            tbAddress64Proxy.Text = status.Address64;
            tbAddress16Proxy.Text = status.Address16;
            tbPortProxy.Text = status.Port;
            tbPANIDProxy.Text = status.PanID;
            tbChannelProxy.Text = status.Channel;
            btRefreshProxy.IsEnabled = true;
            prStatusProxy.IsActive = false;
        }

        private void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateMainStatus(this, new EventArgs());
        }

        private void btRefreshProxy_Click(object sender, RoutedEventArgs e)
        {
            UpdateProxyStatus(this, new EventArgs());
        }
        #endregion

        #region Key Limit
        private void tbAddress64Set_KeyDown(object sender, KeyEventArgs e)
        {
            if (SysKeyCheck(e.Key)) { }
            else if (((TextBox)sender).Text.Replace(" ", "").Length >= 16) { e.Handled = true; }
            else if (HexKeyCheck(e.Key)) { }
            else e.Handled = true;
        }

        private void tbAddress16Set_KeyDown(object sender, KeyEventArgs e)
        {
            if (SysKeyCheck(e.Key)) { }
            else if (((TextBox)sender).Text.Replace(" ", "").Length >= 4) { e.Handled = true; }
            else if (HexKeyCheck(e.Key)) { }
            else e.Handled = true;
        }

        private static bool SysKeyCheck(Key k)
        {
            if (k == Key.Escape || k == Key.Tab || k == Key.CapsLock || k == Key.LeftShift || k == Key.LeftCtrl ||
                k == Key.LWin || k == Key.LeftAlt || k == Key.RightAlt || k == Key.RightCtrl || k == Key.RightShift ||
                k == Key.Left || k == Key.Up || k == Key.Down || k == Key.Right || k == Key.Return || k == Key.Delete ||
                k == Key.System) { return true; }
            return false;
        }

        private static bool HexKeyCheck(Key k)
        {
            if (k >= Key.D0 && k <= Key.D9) { return true; }
            else if (k >= Key.NumPad0 && k <= Key.NumPad9) { return true; }
            else if (k == Key.A || k == Key.B || k == Key.C || k == Key.D || k == Key.E || k == Key.F) { return true; }
            return false;
        }
        #endregion

        #region Main Connection Settings
        private async void btNameSet_Click(object sender, RoutedEventArgs e)
        {
            if (tbNameSet.Text.Trim().Length == 0) { return; }

            btNameSet.IsEnabled = false;
            if (!await Connection.GetInstance.SetName(tbNameSet.Text))
            {
                Logger.LogE("Fail to set name");
                Logger.MessageBoxShow("Fail to set name", "Setting Error");
            }
            else
            {
                Logger.LogS("Set Main Name           -> " + tbNameSet.Text);
                Logger.MessageBoxShow("Success to set name.", "Setting Success", true);
            }
            btNameSet.IsEnabled = true;
            UpdateMainStatus(this, new EventArgs());
        }

        private async void btAddress64Set_Click(object sender, RoutedEventArgs e)
        {
            if (tbAddress64Set.Text.Trim().Length == 0) { return; }
            if (!ExecuteCheck()) { return; }

            btAddress64Set.IsEnabled = false;
            byte[] source = ConvertHelper.StringToByteArray(tbAddress64Set.Text).Reverse().ToArray();
            byte[] bytes = new byte[8];

            Array.Copy(source, bytes, source.Length);

            ulong value = BitConverter.ToUInt64(bytes);
            if (!await Connection.GetInstance.SetAddress64(value))
            {
                Logger.LogE("Fail to set address(64-bit)");
                Logger.MessageBoxShow("Fail to set address(64-bit)", "Setting Error");
            }
            else
            {
                Logger.LogS("Set Main Address(64-bit)-> " + tbAddress64Set.Text);
                Logger.MessageBoxShow("Success to set address(64-bit)", "Setting Success", true);
            }
            btAddress64Set.IsEnabled = true;
            UpdateMainStatus(this, new EventArgs());
        }

        private async void btAddress16Set_Click(object sender, RoutedEventArgs e)
        {
            if (tbAddress16Set.Text.Trim().Length == 0) { return; }
            if (!ExecuteCheck()) { return; }

            btAddress16Set.IsEnabled = false;
            byte[] source = ConvertHelper.StringToByteArray(tbAddress16Set.Text).Reverse().ToArray();
            byte[] bytes = new byte[4];

            Array.Copy(source, bytes, source.Length);

            ushort value = BitConverter.ToUInt16(bytes);
            if (!await Connection.GetInstance.SetAddress16(value))
            {
                Logger.LogE("Fail to set address(16-bit)");
                Logger.MessageBoxShow("Fail to set address(16-bit)", "Setting Error");
            }
            else
            {
                Logger.LogS("Set Main Address(16-bit)-> " + tbAddress16Set.Text);
                Logger.MessageBoxShow("Success to set address(16-bit)", "Setting Success", true);
            }
            btAddress16Set.IsEnabled = true;
            UpdateMainStatus(this, new EventArgs());
        }

        private async void btChannelIDSet_Click(object sender, RoutedEventArgs e)
        {
            if (nudChannel.Value == null) { return; }
            if (nudPanID.Value == null) { return; }
            if (!ExecuteCheck()) { return; }

            btChannelIDSet.IsEnabled = false;

            byte channel = ((byte)nudChannel.Value);
            ushort panid = ((ushort)nudPanID.Value);
            if (!await Connection.GetInstance.SetChannelPANID(channel, panid))
            {
                Logger.LogE("Fail to set Channel/PAN ID");
                Logger.MessageBoxShow("Fail to set Channel/PAN ID", "Setting Error");
            }
            else
            {
                Logger.LogS("Set Main Channel/PANID  -> " + nudChannel.Value + "/" + nudPanID.Value);
                Logger.MessageBoxShow("Success to set Channel/PAN ID", "Setting Success", true);
            }
            btChannelIDSet.IsEnabled = true;
            UpdateMainStatus(this, new EventArgs());
        }

        private bool ExecuteCheck()
        {
            var ret = MessageBox.Show("Network should be reset. Are you sure?", "Setting", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            return (ret == MessageBoxResult.OK);
        }

        private async void btWrite_Click(object sender, RoutedEventArgs e)
        {
            btWrite.IsEnabled = false;

            if (!await Connection.GetInstance.WriteChanges())
            {
                Logger.LogE("Fail to write");
                Logger.MessageBoxShow("Fail to write", "Setting Error");
            }
            else
            {
                Logger.LogS("Write Done");
                Logger.MessageBoxShow("Write Done", "Setting Success", true);
            }

            btWrite.IsEnabled = true;
        }

        private async void btSoftReset_Click(object sender, RoutedEventArgs e)
        {
            btSoftReset.IsEnabled = false;

            if (!await Connection.GetInstance.SoftReset())
            {
                Logger.LogE("Fail to Soft Reset");
                Logger.MessageBoxShow("Fail to Soft Reset", "Setting Error");
            }
            else
            {
                Logger.LogS("SoftReset Done");
                Logger.MessageBoxShow("SoftReset Done", "Setting Success", true);
            }

            btSoftReset.IsEnabled = true;
        }
        #endregion

        #region Main Connection Network
        private async void btNetworkRefresh_Click(object sender, RoutedEventArgs e)
        {
            prNetwork.IsActive = true;
            btNetworkRefresh.IsEnabled = false;
            await Connection.GetInstance.NodeDiscovery();
            btNetworkRefresh.IsEnabled = true;
            prNetwork.IsActive = false;
        }

        private void copyAddress64_Click(object sender, RoutedEventArgs e)
        {
            if (lsvNetwork.SelectedIndex == -1) { return; }
            var item = (XBeeNodeInfo)lsvNetwork.SelectedItem;
            Clipboard.SetText(ConvertHelper.ToHexString(item.Address64));
        }

        private void copyAddress16_Click(object sender, RoutedEventArgs e)
        {
            if (lsvNetwork.SelectedIndex == -1) { return; }
            var item = (XBeeNodeInfo)lsvNetwork.SelectedItem;
            Clipboard.SetText(ConvertHelper.ToHexString(item.Address16));
        }

        private void lsvNetwork_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (lsvNetwork.SelectedIndex == -1)
            {
                copyAddress64.IsEnabled = false;
                copyAddress16.IsEnabled = false;
            }
            else
            {
                copyAddress64.IsEnabled = true;
                copyAddress16.IsEnabled = true;
            }
        }


        #endregion

        private void tbtClear_Click(object sender, RoutedEventArgs e)
        {
            lsvDataLog.Items.Clear();
        }

        private void viewDetail_Click(object sender, RoutedEventArgs e)
        {
            if(lsvDataLog.SelectedItem == null) { return; }
            ViewDetail vd = new ViewDetail(((Packet)lsvDataLog.SelectedItem).Data, ((Packet)lsvDataLog.SelectedItem).Number);
            vd.Topmost = true;
            vd.Show();
            vd.Focus();
             
        }

        private void lsvDataLog_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (lsvDataLog.SelectedIndex == -1)
            {
                viewDetail.IsEnabled = false;
            }
            else
            {
                viewDetail.IsEnabled = true;
            }
        }

        private static int _tleCount = 0;
        private static bool _tleAutoSend = false;
        private readonly DispatcherTimer SendDataTimer = new DispatcherTimer();
        private void sendTelemetry(object o, EventArgs e)
        {
            Telemetry tle;
            tle.TEAM_ID = 1234;
            tle.MISSION_TIME = DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second;
            tle.PACKET_COUNT = ++_tleCount;
            tle.MODE = (byte)((tleMode.IsChecked.HasValue && tleMode.IsChecked.Value) ? 0 : 255);
            tle.STATE = (TelemetryState)(tleState.SelectedItem ?? TelemetryState.IDLE);
            tle.ALTITUDE = (float)(tleAltitude.Value ?? 0);
            tle.AIR_SPEED = (float)(tleAirSpeed.Value ?? 0);
            tle.HS_DEPLOYED = (byte)((tleHS.IsChecked.HasValue && tleHS.IsChecked.Value) ? 0 : 255);
            tle.PC_DEPLOYED = (byte)((tlePC.IsChecked.HasValue && tlePC.IsChecked.Value) ? 0 : 255);
            tle.TEMPERATURE = (float)(tleTemperature.Value ?? 0);
            tle.VOLTAGE = (float)(tleVoltage.Value ?? 0);
            tle.PRESSURE = (float)(tlePressure.Value ?? 0);
            tle.GPS_TIME = tle.MISSION_TIME;
            tle.GPS_ALTITUDE = (float)(tleGPSAltitude.Value ?? 0);
            tle.GPS_LATITUDE = (float)(tleGPSLatitude.Value ?? 0);
            tle.GPS_LONGITUDE = (float)(tleGPSLongitude.Value ?? 0);
            tle.GPS_SATS = 5;
            tle.TILT_X = (float)(tleTilt_X.Value ?? 0);
            tle.TILT_Y = (float)(tleTilt_Y.Value ?? 0);
            tle.ROT_Z = (float)(tleRot_Z.Value ?? 0);
            tle.CMD_ECHO = TelemetryCommandEcho.IDLE;

            TelemetryHandler.Add(tle);
        }

        private void tleSend_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.RightButton == MouseButtonState.Pressed)
            {
                if(_tleAutoSend)
                {
                    tleSend.Content = "Send";
                    SendDataTimer.Stop();
                    _tleAutoSend = false;
                }
                else
                {
                    tleSend.Content = "Auto...";
                    SendDataTimer.Start();
                    _tleAutoSend = true;
                }
            }
        }

        private void tleSend_Click(object sender, RoutedEventArgs e)
        {
            sendTelemetry(this, new EventArgs());
        }
    }

    public class BindableListView : ListView
    {
        public bool AutoScroll = true;

        private readonly List<Packet> temp = new List<Packet>();
        private readonly Queue<Packet> removeQueue = new Queue<Packet>();
        public bool Pause {
            get { return (bool)GetValue(PauseProperty); }
            set { SetValue(PauseProperty, value); }
        }

        public static readonly DependencyProperty PauseProperty =
            DependencyProperty.Register("Pause", typeof(bool), typeof(BindableListView), new PropertyMetadata(OnPauseChanged));

        private static void OnPauseChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BindableListView lsv = (BindableListView)sender;
            if (!(bool)e.NewValue && lsv.temp.Count > 0)
            {
                foreach (var item in lsv.temp)
                {
                    lsv.Items.Add(item);
                }
                lsv.temp.Clear();
            }
        }

        public ObservableCollection<Packet> InlineList
        {
            get { return (ObservableCollection<Packet>)GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof(ObservableCollection<Packet>), typeof(BindableListView), new PropertyMetadata(OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BindableListView lsv = (BindableListView)sender;
            ObservableCollection<Packet> list = (ObservableCollection<Packet>)e.NewValue;
            foreach(var l in list)
                lsv.Items.Add(l);
            list.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(lsv.InlineCollectionChanged);
        }

        private void InlineCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems == null) { return; }
                int idx = e.NewItems.Count - 1;
                Packet? inline = e.NewItems[idx] as Packet;

                if(inline == null) { return; }

                if (Pause)
                    temp.Add(inline);
                else
                    this.Items.Add(inline);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems == null) { return; }
                foreach (var item in e.OldItems)
                {
                    temp.Remove((Packet)item);
                    if (Pause)
                        removeQueue.Enqueue((Packet)item);
                    else
                    {
                        while (removeQueue.Count > 0)
                        {
                            this.Items.Remove(removeQueue.Dequeue());
                        }
                        this.Items.Remove((Packet)item);
                    }
                }
            }

            if (this.Items.Count > 0)
            {
                if (AutoScroll)
                    this.ScrollIntoView(this.Items[this.Items.Count - 1]);
            }
        }
    }
}

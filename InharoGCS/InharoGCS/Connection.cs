using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using XBee.Frames.AtCommands;
using InharoGCS.NetworkProperty;
using System.Windows;
using XBee.Devices;
using XBee;
using System.Xml.Linq;
using System.Windows.Markup;

namespace InharoGCS
{

    public sealed class Connection : INotifyPropertyChanged
    {
        public static ObservableCollection<NetworkProperties> ConnectionList = new ObservableCollection<NetworkProperties>();

        private static readonly Connection _instance = new Connection();

        private XbeeSerialPort _serialDevice;
        private XbeeSerialPort _serialDeviceProxy;
        private XbeeController _xbeeController;
        private XbeeController _xbeeControllerProxy;
        private bool _suspend = false;
        private int _rssiTimeout = 50;
        private int _receiveCounter = 0;
        private string _latestRSSI = "-";
        private static int _maxPacketLine = 200;

        public ObservableCollection<XBeeNodeInfo> XBeeNodeList { get; set; }
        public ObservableCollection<Packet> PacketList { get; set; }

        public int ReceivedCount { get { return _receiveCounter; } }
        public string LatestRSSI { get { return _latestRSSI; } }

        // 이벤트
        public delegate void MainConnectedEventHandler(object sender, EventArgs e);
        public event MainConnectedEventHandler? MainConnectedEvent;
        public delegate void ProxyConnectedEventHandler(object sender, EventArgs e);
        public event ProxyConnectedEventHandler? ProxyConnectedEvent;
        public delegate void MainClosedEventHandler(object sender, EventArgs e);
        public event MainClosedEventHandler? MainClosedEvent;
        public delegate void ProxyClosedEventHandler(object sender, EventArgs e);
        public event ProxyClosedEventHandler? ProxyClosedEvent;

        static Connection()
        {

        }
        private Connection()
        {
            _serialDevice = new XbeeSerialPort();
            _serialDeviceProxy = new XbeeSerialPort();
            _xbeeController = new XbeeController(_serialDevice);
            _xbeeControllerProxy = new XbeeController(_serialDeviceProxy);
            _serialDevice.ConnectionChangeEvent += ConnectionMainChange;
            _serialDeviceProxy.ConnectionChangeEvent += ConnectionProxyChange;

            XBeeNodeList = new ObservableCollection<XBeeNodeInfo>();
            PacketList = new ObservableCollection<Packet>();
        }

        public static Connection GetInstance
        {
            get
            {
                return _instance;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));

            }
        }

        public bool IsMainClosed
        {
            get { return !_serialDevice.IsOpen(); }
        }

        public bool IsProxyClosed
        {
            get { return !_serialDeviceProxy.IsOpen(); }
        }

        public bool IsConnected()
        {
            return _serialDevice.IsOpen() || _serialDeviceProxy.IsOpen();
        }

        private void CreateControllerMain()
        {
            _xbeeController.Dispose();
            XBeeNodeList.Clear();
            _xbeeController = new XbeeController(_serialDevice);
            _xbeeController.NodeDiscovered += async (sender, args) =>
            {
                string name = await args.Node.GetNodeIdentifierAsync();
                XBeeNodeInfo nodeinfo = new(args.Node,name, args.Node.Address.LongAddress.Value, args.Node.Address.ShortAddress.Value);
                if (XBeeNodeList.Any(item => item == nodeinfo))
                {
                    Logger.LogI("Device Already Exists :" + name);
                }
                else
                {
                    XBeeNodeList.Add(nodeinfo);
                    Logger.LogI("Device Detected :" + name);
                }
            };
            _xbeeController.DataReceived += async (sender, args) =>
            {
                string rssi = "-";
                Packet packet = new Packet(_receiveCounter++, args.Address.ShortAddress.Value, rssi, args.Data.Length, args.Data);

                if (_xbeeController.Local != null)
                {
                    Task<byte> task = _xbeeController.Local.GetSignalStrengthAsync();
                    if (await Task.WhenAny(task, Task.Delay(_rssiTimeout)) == task)
                    {
                        rssi = "-" + (task.Result).ToString() + "dBm";
                    }
                    _latestRSSI = rssi;
                }

                packet.RSSI = rssi;

                if (PacketList.Count >= _maxPacketLine)
                {
                    await Application.Current.Dispatcher.BeginInvoke(() => PacketList.RemoveAt(0));
                }

                await Application.Current.Dispatcher.BeginInvoke(() => PacketList.Add(packet));

                OnPropertyChanged("ReceivedCount");
                OnPropertyChanged("LatestRSSI");
            };
        }

        private void CreateControllerProxy()
        {
            _xbeeControllerProxy.Dispose();
            _xbeeControllerProxy = new XbeeController(_serialDeviceProxy);
            _xbeeControllerProxy.DataReceived += (sender, args) =>
            {
                Logger.LogD("Data Received Proxy");
            };
        }

        public static async Task<NetworkProperties?> FindDevice(IEnumerable<NetworkProperties> networkPropertiesList)
        {
            Logger.LogI("     Search Start");
            foreach (NetworkProperties item in networkPropertiesList)
            {
                Logger.LogD("****Search for " + item.ToString());
                ConnectionType type = await XbeeController.FindAsync(item);
                if(type!= ConnectionType.NULL) {
                    var obj = item;
                    obj.Type = type;
                    if (type == ConnectionType.API)
                    {
                        Logger.LogS("*****API Detected");
                        obj.StateEnable();
                    }
                    else
                    {
                        Logger.LogS("*****AT Detected");
                        obj.StateDisable();

                    }
                    return obj;
                }
                else
                {
                    Logger.LogE("****Search failed");
                }
            }
            return null;
        }

        public async Task<bool> NodeDiscovery()
        {
            if (_xbeeController.Local == null) { return false; }
            try
            {
                Logger.LogD("***Node Discovery Start***");
                await _xbeeController.DiscoverNetworkAsync();
                Logger.LogD("***Node Discovery End  ***");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogE("Node Discovery Failed :" + e.ToString());
            }
            return false;
        }

        #region 데이터 UI 처리 관련 코드(Connect, Close, 연결 끊어짐)
        public async void ConnectMain(NetworkProperties network)
        {
            Logger.LogI("     Try to connect main   -" + network.PortName);
            var element = ConnectionList.FirstOrDefault(l => l.PortName == network.PortName);
            if (element == null) { return; }
            if (element.Equals(default(NetworkProperties))) { return; }

            foreach (var item in ConnectionList)
            {
                item.StateDisableMain();
            }

            element.ButtonType = ConnectionButtonType.Connecting;
            CreateControllerMain();

            if (await _xbeeController.OpenAsync(element))
            {
                Logger.LogS("  Main Open Success     -" + network.PortName);
                element.StateConnectedMain();
                OnPropertyChanged("IsMainClosed");
                MainConnectedEvent?.Invoke(this, new EventArgs());
                await NodeDiscovery();
            }
            else
            {
                Logger.LogE("****Fail to connect main  -" + network.PortName);
                foreach (var item in ConnectionList)
                {
                    item.StateEnableMain();
                }
                element.StateFailConnectMain();
            }
        }

        public async void ConnectProxy(NetworkProperties network)
        {
            Logger.LogI("     Try to connect proxy  -" + network.PortName);
            var element = ConnectionList.FirstOrDefault(l => l.PortName == network.PortName);
            if(element == null ) { return; }
            if (element.Equals(default(NetworkProperties))) { return; }

            foreach (var item in ConnectionList)
            {
                item.StateDisableProxy();
            }

            element.ButtonTypeProxy = ConnectionButtonType.Connecting;
            CreateControllerProxy();

            if (await _xbeeControllerProxy.OpenAsync(element))
            {
                Logger.LogS("  Proxy Open Success    -" + network.PortName);
                element.StateConnectedProxy();
                OnPropertyChanged("IsProxyClosed");
                ProxyConnectedEvent?.Invoke(this, new EventArgs());
            }
            else
            {
                Logger.LogE("****Fail to connect proxy -" + network.PortName);
                foreach (var item in ConnectionList)
                {
                    item.StateEnableProxy();
                }
                element.StateFailConnectProxy();
            }
        }

        public void CloseMain(NetworkProperties network)
        {
            Logger.LogI("     Closing Main...       -" + network.PortName);
            var element = ConnectionList.FirstOrDefault(l => l.PortName == network.PortName);
            if (element == null) { return; }
            if (element.Equals(default(NetworkProperties))) { return; }
            _xbeeController.Close();
            foreach (var item in ConnectionList)
            {
                item.StateEnableMain();
            }
            if(!_serialDeviceProxy.IsOpen())
            {
                element.StateEnable();
            }
            OnPropertyChanged("IsMainClosed");
            MainClosedEvent?.Invoke(this, new EventArgs());
            Logger.LogS("  Closed Main");
        }

        public void CloseProxy(NetworkProperties network)
        {
            Logger.LogI("     Closing Proxy...      -" + network.PortName);
            var element = ConnectionList.FirstOrDefault(l => l.PortName == network.PortName);
            if (element == null) { return; }
            if (element.Equals(default(NetworkProperties))) { return; }
            _xbeeControllerProxy.Close();
            foreach (var item in ConnectionList)
            {
                item.StateEnableProxy();
            }
            if (!_serialDevice.IsOpen())
            {
                element.StateEnable();
            }
            OnPropertyChanged("IsProxyClosed");
            ProxyClosedEvent?.Invoke(this, new EventArgs());
            Logger.LogS("  Closed Proxy");
        }

        private void ConnectionMainChange(object sender, ConnectionEventArgs e)
        {
            if(_suspend) { _suspend = false; return; }
            Logger.LogE("FATAL " +e.ToString());
            Task.Run(() => MessageBox.Show(e.ToString(), "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly));
               
            var network = ConnectionList.FirstOrDefault(l => l.PortName == e.Port);
            if(network != null)
                CloseMain(network);         
        }

        private void ConnectionProxyChange(object sender, ConnectionEventArgs e)
        {
            if (_suspend) { _suspend = false; return; }
            Logger.LogE("FATAL " + e.ToString());
            Task.Run(() => MessageBox.Show(e.ToString(), "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly));
            var network = ConnectionList.FirstOrDefault(l => l.PortName == e.Port);
            if (network != null)
                CloseProxy(network);
        }
        #endregion

        public async Task<NetworkStatus> GetInfo()
        {
            NetworkStatus st = new NetworkStatus();
            if (_xbeeController.Local == null) { return st; }
            try
            {
                var local = (XBeeSeries2) _xbeeController.Local;

                st.Port = _xbeeController.GetPort();
                st.Name = (await local.GetNodeIdentifierAsync()).Trim();
                st.SerialNumber = (await local.GetSerialNumberAsync()).ToString();
                string[] address = (await local.GetAddressAsync()).ToString().Split(',');
                st.Address64 = address[0].Trim();
                st.Address16 = address[1].Trim();
                st.PanID = (await local.GetPanIdAsync()).ToString();
                st.Channel = (await local.GetChannelAsync()).ToString();
            }
            catch (Exception e)
            {
                Logger.LogE("GetInfo Error :"+e.ToString());
            }
            return st;
        }

        public async Task<NetworkStatus> GetInfoProxy()
        {
            NetworkStatus st = new NetworkStatus();
            if (_xbeeControllerProxy.Local == null) { return st; }
            try
            {
                var local = (XBeeSeries2)_xbeeControllerProxy.Local;

                st.Port = _xbeeControllerProxy.GetPort();
                st.Name = (await local.GetNodeIdentifierAsync()).Trim();
                st.SerialNumber = (await local.GetSerialNumberAsync()).ToString();
                string[] address = (await local.GetAddressAsync()).ToString().Split(',');
                st.Address64 = address[0].Trim();
                st.Address16 = address[1].Trim();
                st.PanID = (await local.GetPanIdAsync()).ToString();
                st.Channel = (await local.GetChannelAsync()).ToString();
            }
            catch (Exception e)
            {
                Logger.LogE("GetInfoProxy Error :" + e.ToString());
            }
            return st;

        }

        public async Task<bool> SetName(string name)
        {
            if (_xbeeController.Local == null) { return false; }
            try
            {
                var local = (XBeeSeries2)_xbeeController.Local;
                await local.SetNodeIdentifierAsync(name);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogE("Set Name Error :" + e.ToString());
            }
            return false;
        }

        public async Task<bool> SetAddress64(ulong value)
        {
            if (_xbeeController.Local == null) { return false; }
            try
            {
                var local = (XBeeSeries2)_xbeeController.Local;
                await local.SetDestinationAddressAsync(new LongAddress(value));
                return true;
            }
            catch (Exception e)
            {
                Logger.LogE("Set Address(64-bit) Error :" + e.ToString());
            }
            return false;
        }

        public async Task<bool> SetAddress16(ushort value)
        {
            if (_xbeeController.Local == null) { return false; }
            try
            {
                var local = (XBeeSeries2)_xbeeController.Local;
                await local.SetSourceAddressAsync(new ShortAddress(value));
                return true;
            }
            catch (Exception e)
            {
                Logger.LogE("Set Address(16-bit) Error :" + e.ToString());
            }
            return false;
        }

        public async Task<bool> SetChannelPANID(byte channel, ushort panid)
        {
            if (_xbeeController.Local == null) { return false; }
            try
            {
                var local = (XBeeSeries2)_xbeeController.Local;
                await local.SetChannelAsync(channel);
                await local.SetPanIdAsync(panid);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogE("Set Channel/PANID Error :" + e.ToString());
            }
            return false;
        }

        public async Task<bool> SoftReset()
        {
            if (_xbeeController.Local == null) { return false; }
            try
            {
                var local = (XBeeSeries2)_xbeeController.Local;
                _suspend = true;
                await local.ResetAsync();
                return true;
            }
            catch (Exception e)
            {
                Logger.LogE("Write Changes Error :" + e.ToString());
            }
            return false;
        }

        public async Task<bool> WriteChanges()
        {
            if (_xbeeController.Local == null) { return false; }
            try
            {
                var local = (XBeeSeries2)_xbeeController.Local;
                _suspend = true;
                await local.WriteChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                Logger.LogE("Write Changes Error :" + e.ToString());
            }
            return false;
        }
    }
}

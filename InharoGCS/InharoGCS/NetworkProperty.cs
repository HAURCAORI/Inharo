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
using System.Globalization;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
using ScottPlot;
using System.Windows.Input;
using System.Configuration;
using System.Text.Json.Serialization;
using XBee;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;

namespace InharoGCS
{
    namespace NetworkProperty
    {
        public class XBeeNodeInfo
        {
            public XBeeNode Node { get; set; }
            public string Name { get; set; }
            public ulong Address64 { get; set; }
            public ushort Address16 { get; set; }
            public XBeeNodeInfo(XBeeNode node, string name, ulong address64, ushort address16)
                => (Node, Name, Address64, Address16) = (node, name, address64, address16);
            public static bool operator==(XBeeNodeInfo left, XBeeNodeInfo right)
            {
                if((object) left == null)
                    return (object) right == null;
                return left.Equals(right);
            }
            public static bool operator !=(XBeeNodeInfo left, XBeeNodeInfo right) => !(left == right);
            public override bool Equals(object? obj)
            {
                if (obj == null || GetType() != obj.GetType())
                    return false;
                var right = (XBeeNodeInfo) obj;
                return (Address64 == right.Address64 && Address16 == right.Address16);
            }
            public override int GetHashCode()
            {
                return Address64.GetHashCode() ^ Address16.GetHashCode();
            }
        }

        public class Packet
        {
            public int Number { get; }
            public DateTime DateTimeValue { get; }
            public string Time { get; }
            public ushort Address { get; }
            public string RSSI { get; set; }
            public int Length { get; }
            public byte[] Data { get; }
            
            public Packet(int number, ushort address, string rssi, int length, byte[] data)
            {
                Number = number;
                DateTimeValue = DateTime.Now;
                Time = DateTimeValue.ToString("hh:mm:ss.fff tt");
                Address = address;
                RSSI = rssi;
                Length = length;
                Data = data;
            }
        }
        

        public struct PortInfo
        {
            public string Port { get; set; }
            public string? Info { get; set; }
            public bool IsChecked { get; set; }
            public PortInfo(string port, string? info) => (Port, Info, IsChecked) = (port, info, false);
        }

        public struct NetworkCheckItem
        {
            public string Value { get; set; }
            public bool IsChecked { get; set; }
            public NetworkCheckItem(string value) => (Value, IsChecked) = (value, false);
            public NetworkCheckItem(string value, bool ischecked) => (Value, IsChecked) = (value, ischecked);
        }

        public struct NetworkStatus
        {
            public string Name;
            public string SerialNumber;
            public string Address64;
            public string Address16;
            public string Port;
            public string PanID;
            public string Channel;
        }

        #region 네트워크 연결 정보
        [Serializable]
        public class NetworkProperties : INotifyPropertyChanged
        {
            public string PortName { get; set; }
            public int BaudRate { get; set; }
            public int DataBits { get; set; }
            public int StopBits { get; set; }
            public int Parity { get; set; }
            public ConnectionType Type { get; set; }

            private bool _valid = false;
            private bool _validProxy = false;
            [JsonIgnore]
            public bool Valid {
                get { return _valid; }
                set { _valid = value; OnPropertyChanged("Valid"); }
            }
            [JsonIgnore]
            public bool ValidProxy
            {
                get { return _validProxy; }
                set { _validProxy = value; OnPropertyChanged("ValidProxy"); }
            }

            private ConnectionButtonType _buttonType = ConnectionButtonType.None;
            [JsonIgnore]
            public ConnectionButtonType ButtonType{
                get { return _buttonType; }
                set { _buttonType = value; OnPropertyChanged("ButtonType"); }
            }

            private ConnectionButtonType _buttonTypeProxy = ConnectionButtonType.None;
            [JsonIgnore]
            public ConnectionButtonType ButtonTypeProxy {
                get { return _buttonTypeProxy; }
                set { _buttonTypeProxy = value; OnPropertyChanged("ButtonTypeProxy"); }
            }

            private ConnectionStatus _status = ConnectionStatus.Unable;
            [JsonIgnore]
            public ConnectionStatus Status {
                get { return _status; }
                set { this._status = value; OnPropertyChanged("Status"); }
            }

            public NetworkProperties(string portname, int baudrate, int databits, int stopbits, int parity)
            {
                (PortName, BaudRate, DataBits, StopBits, Parity)
                            = (portname, baudrate, databits, stopbits, parity);
                Type = ConnectionType.NULL;
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler? handler = PropertyChanged;
                if(handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }

            public void StateDisable()
            {
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (false, false, ConnectionStatus.Unable, ConnectionButtonType.None, ConnectionButtonType.None);
            }

            public void StateEnable()
            {
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (true, true, ConnectionStatus.Disconnected, ConnectionButtonType.Connect, ConnectionButtonType.Connect);
            }

            public void StateConnectedMain()
            {
                if(Status == ConnectionStatus.Unable) { return; }
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (true, false, ConnectionStatus.Main, ConnectionButtonType.Close, ConnectionButtonType.None);
            }

            public void StateConnectedProxy()
            {
                if (Status == ConnectionStatus.Unable) { return; }
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (false, true, ConnectionStatus.Proxy, ConnectionButtonType.None, ConnectionButtonType.Close);
            }

            public void StateFailConnectMain()
            {
                if (Status == ConnectionStatus.Unable) { return; }
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (true, ValidProxy, ConnectionStatus.Fail, ConnectionButtonType.Connect, ButtonTypeProxy);
            }

            public void StateFailConnectProxy()
            {
                if (Status == ConnectionStatus.Unable) { return; }
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (Valid, true, ConnectionStatus.Fail, ButtonType, ConnectionButtonType.Connect);
            }

            public void StateDisableMain()
            {
                if (Status == ConnectionStatus.Unable) { return; }
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (false, ValidProxy, Status, ConnectionButtonType.None, ButtonTypeProxy);
            }

            public void StateDisableProxy()
            {
                if (Status == ConnectionStatus.Unable) { return; }
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (Valid, false, Status, ButtonType, ConnectionButtonType.None);
            }

            public void StateEnableMain()
            {
                if (Status == ConnectionStatus.Unable) { return; }
                if (Status == ConnectionStatus.Proxy) { return; }
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (true, ValidProxy, ConnectionStatus.Disconnected, ConnectionButtonType.Connect, ButtonTypeProxy);
            }

            public void StateEnableProxy()
            {
                if (Status == ConnectionStatus.Unable) { return; }
                if (Status == ConnectionStatus.Main) { return; }
                (Valid, ValidProxy, Status, ButtonType, ButtonTypeProxy)
                    = (Valid, true, ConnectionStatus.Disconnected, ButtonType, ConnectionButtonType.Connect);
            }

            public override string ToString()
            {
                return string.Join("",PortName, "/", BaudRate, "/", DataBits, "/", StopBits, "/", Parity);
            }
        }
        #endregion

        public enum ConnectionType
        {
            NULL,
            API,
            AT
        }

        public enum ConnectionButtonType
        {
            None,
            Connect,
            Connected,
            Connecting,
            Close,
        }

        public enum ConnectionStatus
        {
            Unable,
            Disconnected,
            Main,
            Proxy,
            Fail
        }

        #region Serial 관련
        public class Port
        {
            private Port() { }
            public static ObservableCollection<PortInfo> GetPorts
            {
                get
                {
                    IEnumerable<PortInfo> ports;
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                    {
                        var portnames = SerialPort.GetPortNames();
                        var portinfos = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());
                        ports = portnames.Zip(portinfos, (first, second) => new PortInfo(first, second![..second!.IndexOf('(')]));
                    }
                    return new ObservableCollection<PortInfo>(ports);
                }
            }
        }

        
        public class BaudRate
        {
            private BaudRate() { }

            private static readonly List<NetworkCheckItem> _baudRates = new()
            {
                new NetworkCheckItem("1200"),new NetworkCheckItem("2400"),new NetworkCheckItem("4800"),
                new NetworkCheckItem("9600",true),new NetworkCheckItem("19200"),new NetworkCheckItem("38400"),
                new NetworkCheckItem("57600"), new NetworkCheckItem("115200",true)
            };
            private static readonly Dictionary<String, int> _converter = new()
            {
                { "1200",1200 }, { "2400",2400 }, { "4800",4800 },
                { "9600",9600 }, { "19200",19200 }, { "38400",38400 },
                { "57600",57600 }, { "115200",115200 }
            };

            public static List<NetworkCheckItem> GetBaudRates
            {
                get
                {
                    return new List<NetworkCheckItem>(_baudRates);
                }
            }

            public static int Value(NetworkCheckItem item) { return _converter[item.Value]; }
        }

        public class DataBit
        {
            private DataBit() { }

            private static readonly List<NetworkCheckItem> _dataBits = new()
            {
                new NetworkCheckItem("7"),new NetworkCheckItem("8", true)
            };
            private static readonly Dictionary<String, int> _converter = new()
            {
                 {"7", 7},{"8", 8 }
            };

            public static List<NetworkCheckItem> GetDataBits
            {
                get
                {
                    return new List<NetworkCheckItem>(_dataBits);
                }
            }
            public static int Value(NetworkCheckItem item) { return _converter[item.Value]; }
        }

        public class StopBit
        {
            private StopBit() { }

            private static readonly List<NetworkCheckItem> _stopBits = new()
            {
                new NetworkCheckItem("1", true), new NetworkCheckItem("2")
            };
            private static readonly Dictionary<String, int> _converter = new()
            {
                 {"1", 1},{"2", 2 }
            };

            public static List<NetworkCheckItem> GetStopBits
            {
                get
                {
                    return new List<NetworkCheckItem>(_stopBits);
                }
            }
            public static int Value(NetworkCheckItem item) { return _converter[item.Value]; }
        }

        public class Parity
        {
            private Parity() { }

            private static readonly List<NetworkCheckItem> _parities = new()
            {
                new NetworkCheckItem("None", true),new NetworkCheckItem("Even"),
                new NetworkCheckItem("Mark"),new NetworkCheckItem("Odd"),
                new NetworkCheckItem("Space")
            };
            private static readonly Dictionary<String, int> _converter = new()
            {
                 {"None", 0},{"Even", 2 }, {"Mark", 3}, {"Odd", 1}, {"Space", 4}
            };

            public static List<NetworkCheckItem> GetParities
            {
                get
                {
                    return new List<NetworkCheckItem>(_parities);
                }
            }
            public static int Value(NetworkCheckItem item) { return _converter[item.Value]; }
        }
        #endregion
    }
}

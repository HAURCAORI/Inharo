using ControlzEx.Theming;
using InharoGCS.NetworkProperty;
using ScottPlot.Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InharoGCS
{
    /// <summary>
    /// ConnectionPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ConnectionPage : Window
    {
       
        public ConnectionPage()
        {
            InitializeComponent();
            PortUpdate();
            MainWindow.ThemeChanged += ChangeTheme;

            lsvResult.ItemsSource = Connection.ConnectionList;


            foreach (var item in NetworkProperty.BaudRate.GetBaudRates)
                lsbBaudRate.Items.Add(item);
            foreach (var item in NetworkProperty.DataBit.GetDataBits)
                lsbDataBits.Items.Add(item);
            foreach (var item in NetworkProperty.StopBit.GetStopBits)
                lsbStopBits.Items.Add(item);
            foreach (var item in NetworkProperty.Parity.GetParities)
                lsbParity.Items.Add(item);
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

        public void PortUpdate()
        {
            lsvPorts.Items.Clear();
            foreach (var item in NetworkProperty.Port.GetPorts)
                lsvPorts.Items.Add(item);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            PortUpdate();
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var portList = lsvPorts.Items.Cast<PortInfo>().Where(x => x.IsChecked == true).Select(x => x.Port);
            var baudrateList = lsbBaudRate.Items.Cast<NetworkCheckItem>().Where(x => x.IsChecked == true).Select(x => BaudRate.Value(x));
            var databitList = lsbDataBits.Items.Cast<NetworkCheckItem>().Where(x => x.IsChecked==true).Select(x => DataBit.Value(x));
            var stopbitList = lsbStopBits.Items.Cast<NetworkCheckItem>().Where(x => x.IsChecked == true).Select(x => StopBit.Value(x));
            var parityList = lsbParity.Items.Cast<NetworkCheckItem>().Where(x => x.IsChecked == true).Select(x => Parity.Value(x));

            if (portList.Count() == 0)
            {
                MessageBox.Show("Please select at least one port.", "Search", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Connection.GetInstance.IsConnected())
            {
                MessageBox.Show("Connection must be closed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            Connection.ConnectionList.Clear();
            btnSearch.IsEnabled = false;
            progressRing.IsActive = true;
            foreach (var port in portList)
            {
                IEnumerable<NetworkProperties> cartesian =
                    from p1 in baudrateList
                    from p2 in databitList
                    from p3 in stopbitList
                    from p4 in parityList
                    select new NetworkProperties(port, p1, p2, p3, p4);
                if (cartesian.Count() == 0)
                {
                    MessageBox.Show("Please select at least one option.", "Search", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                NetworkProperties? result = await Connection.FindDevice(cartesian);
                Debug.WriteLine("*****Port:"+ port);
                if(result != null)
                {
                    Connection.ConnectionList.Add((NetworkProperties) result);
                }
            }
            progressRing.IsActive = false;
            btnSearch.IsEnabled = true;
        }
        private void ConnectMain_Clicked(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button)sender;
            if (cmd.DataContext is NetworkProperties)
            {
                var el = (NetworkProperties)cmd.DataContext;
                if(el.Status == ConnectionStatus.Main)
                {
                    Connection.GetInstance.CloseMain(el);
                } 
                else
                {
                    Connection.GetInstance.ConnectMain(el);
                }
                
            }
        }

        private void ConnectProxy_Clicked(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button)sender;
            if (cmd.DataContext is NetworkProperties)
            {
                var el = (NetworkProperties)cmd.DataContext;
                if (el.Status == ConnectionStatus.Proxy)
                {
                    Connection.GetInstance.CloseProxy(el);
                }
                else
                {
                    Connection.GetInstance.ConnectProxy(el);
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

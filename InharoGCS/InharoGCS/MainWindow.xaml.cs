using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XBee.Frames.AtCommands;
using XBee;

using System.Diagnostics;
using static InharoGCS.XbeeSerialPort;
using XBee.Devices;
using System.Data;
using MahApps.Metro.Controls;
using ControlzEx.Theming;
using MahApps.Metro.Theming;
using System.Text.Json;
using InharoGCS.NetworkProperty;
using System.Windows.Threading;

namespace InharoGCS
{
    public class ThemeEventArgs : EventArgs
    {
        public bool IsDarkMode;
        public ThemeEventArgs(bool darkMode) => IsDarkMode=darkMode;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void ThemeChangeEvnetHandler(object sender, ThemeEventArgs e);
        public static event ThemeChangeEvnetHandler ThemeChanged;

        PerformanceCounter? cpuCounter;
        PerformanceCounter? ramCounter;


        public MainWindow()
        {
            InitializeComponent();
            Logger.LogI("Initializing...");
            ThemeManager.Current.ChangeTheme(this, ((App)Application.Current).themeDark);
            ThemeManager.Current.ChangeTheme(this, ((App)Application.Current).themeLight);

            DispatcherTimer UpdateTimer = new DispatcherTimer();
            UpdateTimer.Interval = TimeSpan.FromSeconds(1);
            UpdateTimer.Tick += timer_Tick;
            UpdateTimer.Start();
        }

        private void timer_Tick(object? sender, EventArgs e)
        {
            TileClock.Text = DateTime.Now.ToString("HH:mm:ss");
            if (cpuCounter == null)
                Task.Run(() => cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"));
            else
                TileCPU.Value = cpuCounter.NextValue();

            if (ramCounter == null)
                Task.Run(() => ramCounter = new PerformanceCounter("Memory", "Available MBytes"));
            else
            {
                TileMemory.Text = ramCounter.NextValue().ToString() + "Mb";
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connectionPage != null) { connectionPage.Close(); }
            if (communicationWindow != null) { communicationWindow.Close();}
            if (loggerWindow != null) {  loggerWindow.Close(); }
            Properties.Settings.Default.ConnectionList = JsonSerializer.Serialize(Connection.ConnectionList);
            Properties.Settings.Default.Save();
            Logger.LogI("Program Setting Saved");
            Logger.Flush();
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

            try
            {
                var connectionList = JsonSerializer.Deserialize<List<NetworkProperties>>(Properties.Settings.Default.ConnectionList);
                if (connectionList != null)
                {
                    foreach (var list in connectionList)
                    {
                        if(list.Type == ConnectionType.API)
                        {
                            list.StateEnable();
                        }
                        Connection.ConnectionList.Add(list);
                    }
                }
            } catch {
                Logger.LogE("Connection List Loading Error[Main]");
            }
            Logger.LogI("Initialized");
        }
        
        public void Error(object? sender, EventArgs args)
        {
            Logger.LogE(args.ToString() + "-No Connection");
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            e.Handled = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private ConnectionPage? connectionPage;
        private void OpenConnection_Click(object sender, RoutedEventArgs e)
        {
            if(connectionPage == null)
            {
                connectionPage = new ConnectionPage();
                connectionPage.Closed += (a, b) => connectionPage = null;
            }
            connectionPage.ShowInTaskbar = false;
           
            ThemeManager.Current.ChangeTheme(connectionPage, ((App)Application.Current).themeDark);
            ThemeManager.Current.ChangeTheme(connectionPage, ((App)Application.Current).themeLight);
            if (ThemeSelector.IsOn)
            {
                ThemeManager.Current.ChangeTheme(connectionPage, ((App)Application.Current).themeDark);
            }
            else
            {
                ThemeManager.Current.ChangeTheme(connectionPage, ((App)Application.Current).themeLight);
            }
            connectionPage.Show();
            connectionPage.Focus();
        }

        private LoggerWindow? loggerWindow;
        private void OpenLogger_Click(object sender, RoutedEventArgs e)
        {
            if (loggerWindow == null)
            {
                loggerWindow = new LoggerWindow();
                loggerWindow.Closed += (a, b) => loggerWindow = null;
            }
            loggerWindow.ShowInTaskbar = false;
            loggerWindow.Show();
            loggerWindow.Focus();
        }

        private CommunicationWindow? communicationWindow;
        private void OpenCommunication_Click(object sender, RoutedEventArgs e)
        {
            if (communicationWindow == null)
            {
                communicationWindow = new CommunicationWindow();
                communicationWindow.Closed += (a, b) => communicationWindow = null;
            }
            communicationWindow.ShowInTaskbar = false;

            ThemeManager.Current.ChangeTheme(communicationWindow, ((App)Application.Current).themeDark);
            ThemeManager.Current.ChangeTheme(communicationWindow, ((App)Application.Current).themeLight);
            if (ThemeSelector.IsOn)
            {
                ThemeManager.Current.ChangeTheme(communicationWindow, ((App)Application.Current).themeDark);
            }
            else
            {
                ThemeManager.Current.ChangeTheme(communicationWindow, ((App)Application.Current).themeLight);
            }
            communicationWindow.Show();
            communicationWindow.Focus();
        }

        private void ThemeSelector_Toggled(object sender, RoutedEventArgs e)
        {
            if (ThemeSelector.IsOn){ SetDarkMode(); } else { SetLightMode(); }
        }

        private void SetDarkMode()
        {
            ThemeManager.Current.ChangeTheme(this, ((App)Application.Current).themeDark);
            ThemeChanged(this, new ThemeEventArgs(true));
        }

        private void SetLightMode()
        {
            ThemeManager.Current.ChangeTheme(this, ((App)Application.Current).themeLight);
            ThemeChanged(this, new ThemeEventArgs(false));
        }
    }
}

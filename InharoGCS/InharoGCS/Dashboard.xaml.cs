using ControlzEx.Theming;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace InharoGCS
{
    /// <summary>
    /// Dashboard.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dashboard : Page
    {
        public Dashboard()
        {
            InitializeComponent();
            ThemeManager.Current.ChangeTheme(this, ((App)Application.Current).themeDark);
            ThemeManager.Current.ChangeTheme(this, ((App)Application.Current).themeLight);
            MainWindow.ThemeChanged += ChangeTheme;
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

        private void Grid_LayoutUpdated(object sender, EventArgs e)
        {
            pltTest.TelemetryList = TelemetryHandler.TelemetryList;
        }
    }
}

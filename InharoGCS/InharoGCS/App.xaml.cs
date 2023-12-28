using ControlzEx.Theming;
using MahApps.Metro.Theming;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace InharoGCS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Theme themeDark;
        public Theme themeLight;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            themeDark = ThemeManager.Current.AddLibraryTheme(new LibraryTheme(
                new Uri("pack://application:,,,/Component/Theme.Dark.xaml"),
                MahAppsLibraryThemeProvider.DefaultInstance));
            themeLight = ThemeManager.Current.AddLibraryTheme(new LibraryTheme(
                    new Uri("pack://application:,,,/Component/Theme.Light.xaml"),
                    MahAppsLibraryThemeProvider.DefaultInstance));
            ThemeManager.Current.ChangeTheme(this, themeDark);

            // Optionally enable App Mode theme switching
            //ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            //ThemeManager.Current.SyncTheme();
        }

    }
}

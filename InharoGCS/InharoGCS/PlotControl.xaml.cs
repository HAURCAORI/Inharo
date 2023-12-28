using InharoGCS.NetworkProperty;
using ScottPlot;
using ScottPlot.Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
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
    /// PlotControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PlotControl : UserControl
    {
        private string xUnit = "sec";
        private string yUnit = "m";
        readonly ScottPlot.Plottable.DataLogger pltLogger;

        public PlotControl()
        {
            InitializeComponent();
            MainWindow.ThemeChanged += ChangeTheme;
            pltLogger = pltControl.Plot.AddDataLogger();
            pltLogger.ViewFull();
            pltControl.Plot.AxisAuto();
            pltControl.Plot.XAxis.TickLabelFormat("HH:mm:ss", dateTimeFormat: true);
            pltControl.Render();
            //pltControl.Configuration.Pan = false;
            //pltControl.Configuration.Zoom = false;

        }


        public void ChangeTheme(object sender, ThemeEventArgs e)
        {
            if (e.IsDarkMode)
            {
                pltControl.Plot.Style(ScottPlot.Style.Gray2);
            }
            else
            {
                pltControl.Plot.Style(ScottPlot.Style.Seaborn);
            }
            pltControl.Render();
        }

        public ObservableCollection<Telemetry> TelemetryList
        {
            get { return (ObservableCollection<Telemetry>)GetValue(TelemetryListProperty); }
            set { SetValue(TelemetryListProperty, value); }
        }

        public static readonly DependencyProperty TelemetryListProperty =
            DependencyProperty.Register("TelemetryList", typeof(ObservableCollection<Telemetry>), typeof(PlotControl), new PropertyMetadata(OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PlotControl control = (PlotControl) sender;
            ObservableCollection<Telemetry> list = (ObservableCollection<Telemetry>)e.NewValue;
            control.pltLogger.AddRange(list.Select(x => new Coordinate(x.MISSION_TIME, x.PRESSURE)));
            list.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(control.CollectionChanged);
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems == null) { return; }
                int idx = e.NewItems.Count - 1;
                Telemetry item = (Telemetry)e.NewItems[idx]!;

                pltLogger.Add(TelemetryHandler.ToDateTime(item.MISSION_TIME).ToOADate(), item.PRESSURE);
                this.pltControl.Plot.AxisAuto();
                this.pltControl.Render();
                
            }
            
        }
    }
}

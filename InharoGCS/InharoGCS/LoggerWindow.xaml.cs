using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace InharoGCS
{
    /// <summary>
    /// LoggerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoggerWindow : Window
    {
        private bool autoScroll = true;

        public LoggerWindow()
        {
            InitializeComponent();
        }

        private void LogTextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (autoScroll)
            {
                Scroller.ScrollToBottom();
            }
        }

        private void Scroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {
                autoScroll = false;
            } else if (Scroller.ScrollableHeight - e.VerticalOffset < 1)
            {
                autoScroll = true;
            }

        }
    }

    public class BindableTextBlock : TextBlock
    {
        public ObservableCollection<Inline> InlineList
        {
            get { return (ObservableCollection<Inline>)GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof(ObservableCollection<Inline>), typeof(BindableTextBlock), new PropertyMetadata(OnPropertyChanged));
            
        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BindableTextBlock textBlock = (BindableTextBlock) sender;
            ObservableCollection<Inline> list = (ObservableCollection<Inline>) e.NewValue;
            textBlock.Inlines.AddRange(list);
            list.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(textBlock.InlineCollectionChanged);
        }

        private void InlineCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if(e.NewItems == null) { return; }
                int idx = e.NewItems.Count - 1;
                Inline? inline = e.NewItems[idx] as Inline;

                this.Inlines.Add(inline);
            }
            else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems == null) { return; }
                foreach(var item in e.OldItems)
                {
                    this.Inlines.Remove((Inline) item);
                }
            }

        }
    }
}

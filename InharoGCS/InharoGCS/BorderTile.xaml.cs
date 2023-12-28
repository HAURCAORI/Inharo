using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace InharoGCS
{
    /// <summary>
    /// BorderTile.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BorderTile : UserControl
    {
        public BorderTile()
        {
            InitializeComponent();
        }

        public enum TileStyle
        {
            String,
            Toggle,
            Percent
        }

        public TileStyle TileType
        {
            get { return (TileStyle)GetValue(TitleTypeProperty); }
            set { SetValue(TitleTypeProperty, value); }
        }

        public static readonly DependencyProperty TitleTypeProperty =
            DependencyProperty.Register("TitleType", typeof(TileStyle), typeof(BorderTile), new PropertyMetadata(TileStyle.String, OnTileTypeChanged));

        private static void OnTileTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control =  d as BorderTile;
            if(control == null) { return; }
            switch (e.NewValue)
            {
                case TileStyle.String:
                    control.ProgressEnable = false;

                    break;
                case TileStyle.Toggle:
                    control.ProgressEnable = false;

                    break;
                case TileStyle.Percent:
                    control.ProgressEnable = true;

                    break;
            }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(BorderTile), new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }
        public static readonly DependencyProperty ContentTextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(BorderTile), new PropertyMetadata(string.Empty));

        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }
        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register("TextColor", typeof(Brush), typeof(BorderTile), new PropertyMetadata(Brushes.LimeGreen));


        public bool ProgressEnable
        {
            get { return (bool)GetValue(ProgressEnableProperty); }
            set { SetValue(ProgressEnableProperty, value); }
        }
        public static readonly DependencyProperty ProgressEnableProperty =
            DependencyProperty.Register("ProgressEnable", typeof(bool), typeof(BorderTile), new PropertyMetadata(false));

        public float Value
        {
            get { return (float)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float), typeof(BorderTile), new PropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as BorderTile;
            if(control == null) { return;  }
            if(e.NewValue is float)
            {
                control.Text = String.Format("{0:0}%",e.NewValue);
                var anim = new DoubleAnimation((float)e.OldValue, (float)e.NewValue, new TimeSpan(0, 0, 0, 0, 500));
                control.Progress.BeginAnimation(ProgressBar.ValueProperty, anim, HandoffBehavior.Compose);
            }
            
        }
    }
}
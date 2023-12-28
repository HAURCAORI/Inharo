using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InharoGCS
{
    /// <summary>
    /// ViewDetail.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewDetail : Window, INotifyPropertyChanged
    {
        private BinaryReader binaryReader;

        public ViewDetail(byte[] data, int no = 0)
        {
            InitializeComponent();
            binaryReader = new BinaryReader(new MemoryStream());
            Reader = new BinaryReader(new MemoryStream(data));
            this.Title = "View Detail[" + no + "]";
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            //DataView.Text = ConvertHelper.ByteToHexStringWithSpace(_data);
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the source of the data to display using the <see cref="HexViewer"/> control.
        /// </summary>
        public BinaryReader Reader
        {
            get => binaryReader;

            set
            {
                binaryReader = value;

                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

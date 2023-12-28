using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static InharoGCS.XbeeSerialPort;

namespace InharoGCS
{
    public enum LogType {
        DEBUG,
        INFO,
        WARN,
        ERROR,
        SUCCESS
    }

    public readonly struct LogMessage
    {
        public readonly LogType Type;
        public readonly string Message;
        public readonly string Time;

        public LogMessage(string message)
        {
            Type = LogType.INFO;
            Message = message;
            Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt");
        }

        public LogMessage(string message, LogType type)
        {
            Type = type;
            Message = message;
            Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff tt");
        }

        public override string ToString()
        {
            return string.Join("", Time, "[", Type, "]:", Message);
        }
    }



    public sealed class Logger : INotifyPropertyChanged
    {
        private static string _fileName = "Log.txt";
        private static int _maxLine = 200;
        private static int _flushLimit = 10;
        private static readonly Logger _instance = new Logger();
        private ObservableCollection<Inline> _inline = new ObservableCollection<Inline>();
        private static Queue<string> _logQueue = new Queue<string>();
        private static readonly object _writeLock = new object();
        private static Dictionary<LogType, SolidColorBrush> _logColors = new Dictionary<LogType, SolidColorBrush>()
        {   
            {LogType.DEBUG, new SolidColorBrush(Colors.Gray)},
            {LogType.INFO, new SolidColorBrush(Colors.White)},
            {LogType.WARN, new SolidColorBrush(Colors.Yellow)},
            {LogType.ERROR, new SolidColorBrush(Colors.Red)},
            {LogType.SUCCESS, new SolidColorBrush(Colors.LawnGreen)},
        };


        static Logger() { }

        private Logger() {
            using(StreamWriter fw = new StreamWriter(_fileName))
            {
                fw.WriteLine("[InharoGCS Log]");
            }
        }

        public static Logger GetInstance
        {
            get
            {
                return _instance;
            }
        }

        public static void Log(string text) { Log(text,LogType.INFO); }
        public static void LogD(string text) { Log(text, LogType.DEBUG); }
        public static void LogI(string text) { Log(text, LogType.INFO); }
        public static void LogW(string text) { Log(text, LogType.WARN); }
        public static void LogE(string text) { Log(text, LogType.ERROR); }
        public static void LogS(string text) { Log(text, LogType.SUCCESS); }

        public static void Log(string text, LogType type)
        {
            LogMessage message = new LogMessage(text, type);
            Application.Current.Dispatcher.BeginInvoke((LogMessage m) => { Log(m); }, message);
        }

        private static void Log(LogMessage message)
        {
            lock (_writeLock)
            {
                string s = message.ToString();

                if (GetInstance._inline.Count >= _maxLine)
                {
                    GetInstance._inline.RemoveAt(0);
                }

                Run item = new Run(s + Environment.NewLine);
                item.Foreground = _logColors[message.Type];

                GetInstance._inline.Add(item);
                
                _logQueue.Enqueue(s);

                GetInstance.OnPropertyChanged("TextList");
                
            }
            if (_logQueue.Count > _flushLimit)
            {
                Flush();
            }
        }

        public static void Flush()
        {
            lock(_writeLock)
            {
                using (StreamWriter fw = new StreamWriter(_fileName,true))
                {
                    while(_logQueue.Count > 0)
                    {
                        fw.WriteLine(_logQueue.Dequeue());
                    }
                }

            }
        }

        public ObservableCollection<Inline> TextList
        {
            get { return _inline; }
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

        public static async void MessageBoxShow(string text, string title, bool success = false)
        {
            var image = (success) ? MessageBoxImage.None : MessageBoxImage.Error;
           await Task.Run(() => MessageBox.Show(text, title, MessageBoxButton.OK, image, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using XBee;
using System.IO;

using System.Diagnostics;
using System.Windows.Markup;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace InharoGCS
{
    public class ConnectionEventArgs : EventArgs
    {
        public string Port { get; }
        public string Text { get; }
        public ConnectionEventArgs(string port) => (Port, Text) = (port, string.Empty);
        public ConnectionEventArgs(string port, string text) => (Port, Text) = (port, text);
        public override string ToString() => "[" + Port + "] " + Text;
    }

    internal class XbeeSerialPort : ISerialDevice
    {
        // 멤버 변수
        private readonly SerialPort _serialPort;
        private readonly SerialDeviceStream _serialStream;

        // 이벤트 정의
        public delegate void NoConnectionEventHandler(object sender, ConnectionEventArgs e);
        public event NoConnectionEventHandler? NoConnectionEvent;
        public delegate void ConnectionClosedEventHandler(object sender, ConnectionEventArgs e);
        public event ConnectionClosedEventHandler? ConnectionClosedEvent;
        public delegate void ConnectionOpenEventHandler(object sender, ConnectionEventArgs e);
        public event ConnectionOpenEventHandler? ConnectionOpenEvent;
        public delegate void ConnectionChangeEventHandler(object sender, ConnectionEventArgs e);
        public event ConnectionChangeEventHandler? ConnectionChangeEvent;


        // 생성자
        public XbeeSerialPort()
        {
            _serialPort = new SerialPort();
            _serialPort.PinChanged += PinChangedEvent;

            _serialStream = new SerialDeviceStream(this);

            _serialPort.DataReceived += BufferCheck;
        }

        private void PinChangedEvent(object sender, SerialPinChangedEventArgs e)
        {
            bool valid = _serialPort.CDHolding || _serialPort.CtsHolding || _serialPort.DsrHolding;
            if (!valid)
                ConnectionChangeEvent?.Invoke(this, new ConnectionEventArgs(this.ToString(), "Connection Lost"));
                    //Application.Current.Dispatcher.BeginInvoke(new ConnectionChangeEventHandler(ConnectionChangeEvent), this, new ConnectionEventArgs(this.ToString(), "Connection Lost"));
        }

        private void BufferCheck(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort.BytesToRead + 128 > _serialPort.ReadBufferSize)
            {
                Task.Run(() => MessageBox.Show("Read Buffer Overflow. Please Restart The "+ this.ToString(), "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly));
                Logger.LogE("****" + "Read Buffer Overflow. Please Restart The " + this.ToString());
                _serialPort.DiscardInBuffer();
            }
            if (_serialPort.BytesToWrite + 128 > _serialPort.WriteBufferSize)
            {
                Task.Run(() => MessageBox.Show("Write Buffer Overflow. Please Restart The " + this.ToString(), "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly));
                Logger.LogE("****" + "Write Buffer Overflow. Please Restart The " + this.ToString());
                _serialPort.DiscardOutBuffer();
            }
        }

        public void Dispose()
        {
            Logger.LogI("*****Serial Port Dispose");
            _serialPort.Dispose();
        }

        // 메소드
        public void Open() {
            Logger.LogD("****Try to open           -" + _serialPort.PortName);
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                Logger.LogD("****Open success          -" + _serialPort.PortName);
                ConnectionOpenEvent?.Invoke(this, new ConnectionEventArgs(this.ToString()));
            }
            else
            {
                Logger.LogW("*****Already opened");
            }
        }
        public void Close()
        {
            Logger.LogD("****Try to close          -" + _serialPort.PortName);
            if (_serialPort.IsOpen)
            {
                try {
                    _serialPort.Close();
                    Logger.LogD("****Close success         -" + _serialPort.PortName);
                }
                catch
                {
                    Logger.LogE("****IO Error When Closing");
                }
                ConnectionClosedEvent?.Invoke(this, new ConnectionEventArgs(this.ToString()));
            }
            else
            {
                Logger.LogW("*****Already closed");
            }
        }

        public void Write(byte[] data)
        {
            if (!_serialPort.IsOpen) { NoConnectionEvent?.Invoke(this, new ConnectionEventArgs(this.ToString())); return; }
            _serialPort.BaseStream.Write(data, 0, data.Length);

#if DEBUG
            //Debug.WriteLine("write");
            //Debug.WriteLine(BitConverter.ToString(data).Replace("-", ""));
#endif
        }

        public async Task ReadAsync(byte[] buffer, int offset, int count)
        {
            if (!_serialPort.IsOpen) { NoConnectionEvent?.Invoke(this, new ConnectionEventArgs(this.ToString())); return; }

            var bytesToRead = count;
            var temp = new byte[count];
            int readBytes;
            while (bytesToRead > 0)
            {
                readBytes = await _serialPort.BaseStream.ReadAsync(temp.AsMemory(0, (int)bytesToRead));
                Array.Copy(temp, 0, buffer, offset + count - bytesToRead, readBytes);
                bytesToRead -= readBytes;
            }
#if DEBUG
            //Debug.WriteLine("read");
            //Debug.WriteLine(BitConverter.ToString(buffer).Replace("-", ""));
#endif
        }

        public async Task<byte[]> ReadAsync(uint count, CancellationToken cancellationToken)
        {
            var bufferStream = new MemoryStream();
            do
            {
                var data = new byte[count - (uint)bufferStream.Length];
                var read = await _serialPort.BaseStream.ReadAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                await bufferStream.WriteAsync(data, 0, read, cancellationToken);
                if (bufferStream.Length < count)
                {
                    await Task.Delay(100, cancellationToken);
                }
            } while (bufferStream.Length < count);
#if DEBUG
            //Debug.WriteLine("read");
            //Debug.WriteLine(BitConverter.ToString(bufferStream.ToArray()).Replace("-", ""));
#endif
            return bufferStream.ToArray();
        }

        public override string ToString() { return GetPortName(); }
        public Stream Stream() { return _serialPort.BaseStream; }
        // 속성
        public bool IsOpen() { return _serialPort.IsOpen; }

        public void SetBaudRate(int baudrate) { if (!IsOpen()) { _serialPort.BaudRate = baudrate; } }
        public void SetDataBits(int databits) { if (!IsOpen()) { _serialPort.DataBits = databits; } }
        public void SetParity(Parity parity) { if (!IsOpen()) { _serialPort.Parity = parity; } }
        public void SetPortName(string name) { if (!IsOpen()) { _serialPort.PortName = name; } }
        public void SetReadBufferSize(int size) { if (!IsOpen()) { _serialPort.ReadBufferSize = size; } }
        public void SetStopBits(StopBits stopbits) { if (!IsOpen()) { _serialPort.StopBits = stopbits; } }
        public void SetWriteBufferSize(int size) { if (!IsOpen()) { _serialPort.WriteBufferSize = size; } }

        public int GetBaudRate() => _serialPort.BaudRate;
        public int GetDataBits() { return _serialPort.DataBits; }
        public Parity GetParity() { return _serialPort.Parity; }
        public string GetPortName() { return _serialPort.PortName; }
        public int GetReadBufferSize() { return _serialPort.ReadBufferSize; }
        public StopBits GetStopBits() { return _serialPort.StopBits; }
        public int GetWriteBufferSize() { return _serialPort.WriteBufferSize; }
    }
    
}

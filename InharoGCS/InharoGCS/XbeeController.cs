using InharoGCS.NetworkProperty;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using XBee;
using XBee.Core;
using XBee.Frames.AtCommands;

namespace InharoGCS
{

    internal class XbeeController : XBeeControllerBase, IDisposable
    {
        private static int BufferSize = 65536;
        private XbeeSerialPort _xbeeSerialPort;
        public XbeeController() : this(new XbeeSerialPort()) {}

        public XbeeController(XbeeSerialPort xbeeSerialPort) : base(xbeeSerialPort) => _xbeeSerialPort = xbeeSerialPort;


        public override void Dispose()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ExceptionHandle(string text)
        {
            Task.Run(() => MessageBox.Show(text, "Connection Error", MessageBoxButton.OK,MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly));
            Logger.LogE("****" + text);
            _xbeeSerialPort.Close();
        }

        private void SetPortOptions(NetworkProperties networkProperties)
        {
            _xbeeSerialPort.SetPortName(networkProperties.PortName);
            _xbeeSerialPort.SetBaudRate(networkProperties.BaudRate);
            _xbeeSerialPort.SetDataBits(networkProperties.DataBits);
            System.IO.Ports.StopBits stop =  (networkProperties.StopBits < 0 || networkProperties.StopBits > 3) ? 0 : (System.IO.Ports.StopBits) networkProperties.StopBits;
            _xbeeSerialPort.SetStopBits(stop);
            System.IO.Ports.Parity parity = (networkProperties.Parity < 0 || networkProperties.Parity > 4) ? 0 : (System.IO.Ports.Parity)networkProperties.Parity;
            _xbeeSerialPort.SetParity(parity);

            _xbeeSerialPort.SetReadBufferSize(BufferSize);
            _xbeeSerialPort.SetReadBufferSize(BufferSize);
        }

        public string GetPort()
        {
            return _xbeeSerialPort.GetPortName();
        }

        public void Close()
        {
            _xbeeSerialPort.Close();
        }

        public async Task<bool> OpenAsync(NetworkProperties networkProperties)
        {
            SetPortOptions(networkProperties);
            try
            {
                _xbeeSerialPort.Open();
                await GetHardwareVersionAsync();
                return true;
            }
            catch (InvalidOperationException)
            {
                ExceptionHandle("[Port] Already Opened");
                // 이미 열려져 있는 경우
            }
            catch (UnauthorizedAccessException)
            {
                ExceptionHandle("[Port] Access Denied");
                // 포트 엑세스 거부
            }
            catch (ArgumentOutOfRangeException)
            {
                ExceptionHandle("[Port] Invalid Property");
                // 속성 문제
            }
            catch (ArgumentException)
            {
                ExceptionHandle("[Port] Name Error");
                // 포트 이륾이 COM으로 시작하지 않을 때 or 포트 형식 지원 안될때
            }
            catch (IOException)
            {
                ExceptionHandle("[Port] IO Error");
                 // 포트 상태 오류, 내부 포트 설정 오류
            }
            catch
            {
                ExceptionHandle("[Port] Timeout");
            }
            return false;
        }
        /*
        public Task GetDeviceInformation(NetworkProperties networkProperties)
        {
            GetHardwareVersionAsync();
            //return; 
        }*/

        //GetHardwareVersionAsync();

        public async Task<bool> APICheck(NetworkProperties networkProperties)
        {
            SetPortOptions(networkProperties);
            bool result = false;
            try
            {
                Logger.LogD("****API Check...");
                _xbeeSerialPort.Open();
                await GetHardwareVersionAsync();
                result = true;
            }
            catch
            {
                Logger.LogW("*****API Timeout");
                Logger.LogW("*****API Check Fail");
            }
            finally
            {
                _xbeeSerialPort.Close();
            }
            return result;
        }

        public async Task<bool> ATCheck(NetworkProperties networkProperties)
        {
            SetPortOptions(networkProperties);
            bool result = false;
            try
            {
                Logger.LogD("****AT Check...");
                byte[] command = new byte[] { 0x2B, 0x2B, 0x2B };
                byte[] buffer = new byte[2];
                _xbeeSerialPort.Open();
                _xbeeSerialPort.Write(command);
                CancellationTokenSource cts = new CancellationTokenSource();
                var task = _xbeeSerialPort.ReadAsync(2,cts.Token);
                if (await Task.WhenAny(task,Task.Delay(5000))==task)
                {
                    result = true;
                }
                else
                {
                    Logger.LogW("*****AT Timeout");
                }
                cts.Dispose();
            }
            catch
            {
                Logger.LogW("*****AT Check Fail");
            }
            finally
            {
                _xbeeSerialPort.Close();
            }
            return result;
        }

        public static async Task<ConnectionType> FindAsync(NetworkProperties networkProperties)
        {
            var controller = new XbeeController();

            if (await controller.APICheck(networkProperties))
            {
                return ConnectionType.API;
            }
            if (await controller.ATCheck(networkProperties))
            {
                return ConnectionType.AT;
            }
            
            controller.Dispose();
            return ConnectionType.NULL;
        }

        private static readonly TimeSpan AckTimeout = TimeSpan.FromSeconds(3);
        public async void Send()
        {
            Logger.LogD("send packet");
            Debug.WriteLine("sending");
            //var writer = new StreamWriter(_xbeeSerialPort.Stream(), Encoding.UTF8, 4096, true);
            //await writer.WriteAsync(StringToByteArray("7E 00 0A 01 01 11 11 00 48 65 6C 6C 6F E7").ToString());
            //await writer.FlushAsync();

            _xbeeSerialPort.Write(ConvertHelper.StringToByteArray("7E 00 0A 01 01 11 11 00 48 65 6C 6C 6F E7"));

            var start = DateTime.Now;
            CancellationTokenSource cts = new CancellationTokenSource();
            while (DateTime.Now - start < AckTimeout)
            {
                var data = new byte[1];
                var task = _xbeeSerialPort.Stream().ReadAsync(data, 0, data.Length);
                if (await Task.WhenAny(task, Task.Delay(500)) == task)
                {
                    Logger.LogW(BitConverter.ToString(data));
                }
                else
                {
                    Logger.LogW("*****timeout");
                }
            }
            cts.Dispose();
            Logger.LogD("Reading End");
        }

        
    }

    
}

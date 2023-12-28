using InharoGCS.NetworkProperty;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace InharoGCS
{
    internal class TelemetryHandler : INotifyPropertyChanged
    {
        private static readonly TelemetryHandler _instance = new TelemetryHandler();
        public static ObservableCollection<Telemetry> TelemetryList = new ObservableCollection<Telemetry>();


        static TelemetryHandler() { }
        private TelemetryHandler() { }
        public static TelemetryHandler GetInstance { get { return _instance; } }
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public static void Add(Telemetry telemetry)
        {
            TelemetryList.Add(telemetry);
        }

        public static DateTime ToDateTime(int value)
        {
            var now = DateTime.Now;
            var seconds = value % 60;
            var minutes = value / 60;
            var hours = minutes / 60;
            minutes = minutes % 60;

            return new DateTime(now.Year, now.Month, now.Day, hours, minutes, seconds);
        }

        // 타입 처리
        public static List<TelemetryState> TelemetryStateList = new List<TelemetryState> 
        {TelemetryState.IDLE, TelemetryState.LAUNCH_WAIT};

        public static List<string> PlottableTelemetryList = new List<string>
        {"abc","def"};
        public static object? GetTelemetryValue(Telemetry tle, TelemetryLabel target)
        {
            switch (target)
            {
                case TelemetryLabel.TEAM_ID:
                    return tle.TEAM_ID;
                case TelemetryLabel.MISSION_TIME:
                    return tle.MISSION_TIME;
                case TelemetryLabel.PACKET_COUNT:
                    return tle.PACKET_COUNT;
                case TelemetryLabel.MODE:
                    return tle.MODE;
                case TelemetryLabel.STATE:
                    return tle.STATE;
                case TelemetryLabel.ALTITUDE:
                    return tle.ALTITUDE;
                case TelemetryLabel.AIR_SPEED:
                    return tle.AIR_SPEED;
                case TelemetryLabel.HS_DEPLOYED:
                    return tle.HS_DEPLOYED;
                case TelemetryLabel.PC_DEPLOYED:
                    return tle.PC_DEPLOYED;
                case TelemetryLabel.TEMPERATURE:
                    return tle.TEMPERATURE;
                case TelemetryLabel.VOLTAGE:
                    return tle.VOLTAGE;
                case TelemetryLabel.PRESSURE:
                    return tle.PRESSURE;
                case TelemetryLabel.GPS_TIME:
                    return tle.GPS_TIME;
                case TelemetryLabel.GPS_ALTITUDE:
                    return tle.GPS_ALTITUDE;
                case TelemetryLabel.GPS_LATITUDE:
                    return tle.GPS_LATITUDE;
                case TelemetryLabel.GPS_LONGITUDE:
                    return tle.GPS_LONGITUDE;
                case TelemetryLabel.GPS_SATS:
                    return tle.GPS_SATS;
                case TelemetryLabel.TILT_X:
                    return tle.TILT_X;
                case TelemetryLabel.TILT_Y:
                    return tle.TILT_Y;
                case TelemetryLabel.ROT_Z:
                    return tle.ROT_Z;
                case TelemetryLabel.CMD_ECHO:
                    return tle.CMD_ECHO;
                default:
                    return null;
            }
        }
    }
}

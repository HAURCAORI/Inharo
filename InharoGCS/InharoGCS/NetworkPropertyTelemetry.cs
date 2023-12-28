using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InharoGCS
{
    namespace NetworkProperty
    {
        #region Telemetry 관련
        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct Telemetry
        {
            [FieldOffset(0)]
            public int TEAM_ID;
            [FieldOffset(4)]
            public int MISSION_TIME;
            [FieldOffset(8)]
            public int PACKET_COUNT;
            [FieldOffset(12)]
            public byte MODE;
            [FieldOffset(13)]
            public TelemetryState STATE;
            [FieldOffset(15)]
            public float ALTITUDE;
            [FieldOffset(19)]
            public float AIR_SPEED;
            [FieldOffset(23)]
            public byte HS_DEPLOYED;
            [FieldOffset(24)]
            public byte PC_DEPLOYED;
            [FieldOffset(25)]
            public float TEMPERATURE;
            [FieldOffset(29)]
            public float VOLTAGE;
            [FieldOffset(33)]
            public float PRESSURE;
            [FieldOffset(37)]
            public float GPS_TIME;
            [FieldOffset(41)]
            public float GPS_ALTITUDE;
            [FieldOffset(45)]
            public float GPS_LATITUDE;
            [FieldOffset(49)]
            public float GPS_LONGITUDE;
            [FieldOffset(53)]
            public int GPS_SATS;
            [FieldOffset(57)]
            public float TILT_X;
            [FieldOffset(61)]
            public float TILT_Y;
            [FieldOffset(65)]
            public float ROT_Z;
            [FieldOffset(69)]
            public TelemetryCommandEcho CMD_ECHO;
        }

        public enum TelemetryLabel
        {
            TEAM_ID, MISSION_TIME, PACKET_COUNT,
            MODE, STATE, ALTITUDE, AIR_SPEED,
            HS_DEPLOYED, PC_DEPLOYED, TEMPERATURE,
            VOLTAGE, PRESSURE, GPS_TIME,
            GPS_ALTITUDE, GPS_LATITUDE, GPS_LONGITUDE,
            GPS_SATS, TILT_X, TILT_Y, ROT_Z,
            CMD_ECHO
        }

        public enum TelemetryState : short
        {
            IDLE = 0,
            LAUNCH_WAIT = 1
        }

        public enum TelemetryCommandEcho : short
        {
            IDLE = 0
        }

        #endregion
    }
}

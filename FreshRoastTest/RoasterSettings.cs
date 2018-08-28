using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshRoastTest
{
    public class RoasterSettings
    {

        public RunningState RunningState { get; set; }
        public int FanSpeed { get; set; }
        public HeatLevel HeatLevel { get; set; }
        public float TimerMinutes { get; set; }

        public RoasterSettings()
        {
            this.RunningState = RunningState.IDLE;
            this.FanSpeed = 9;
            this.HeatLevel = HeatLevel.LOW;
            this.TimerMinutes = 1.5f;
        }

        public Packet ToPacket()
        {
            var packet = new Packet();
            packet.FanSpeed = new byte[] { (byte)this.FanSpeed };
            packet.HeatLevel = new byte[] { (byte)this.HeatLevel };
            packet.Timer = new byte[] { (byte)((int)(this.TimerMinutes * 10)) };
            switch(this.RunningState)
            {
                case RunningState.IDLE:
                    packet.Control = new byte[] { 0x02, 0x01 };
                    break;
                case RunningState.ROASTING:
                    packet.Control = new byte[] { 0x04, 0x02 };
                    break;
                case RunningState.COOLING:
                    packet.Control = new byte[] { 0x04, 0x04 };
                    break;
                case RunningState.STOPPED:
                    packet.Control = new byte[] { 0x08, 0x01 };
                    break;
            }
            return packet;
        }

        public static RoasterSettings FromPacket(Packet packet)
        {
            var settings = new RoasterSettings();
            settings.FanSpeed = (int)packet.FanSpeed[0];
            settings.TimerMinutes = ((int)packet.Timer[0] / 10f);
            
            var _tmpHeatLevel = (int)packet.HeatLevel[0];
            switch(_tmpHeatLevel)
            {
                case 0:
                    settings.HeatLevel = HeatLevel.OFF;
                    break;
                case 1:
                    settings.HeatLevel = HeatLevel.LOW;
                    break;
                case 2:
                    settings.HeatLevel = HeatLevel.MEDIUM;
                    break;
                case 3:
                    settings.HeatLevel = HeatLevel.HIGH;
                    break;
            }

            if (packet.Control[0] == 0x02 && packet.Control[1] == 0x01) settings.RunningState = RunningState.IDLE;
            if (packet.Control[0] == 0x04 && packet.Control[1] == 0x02) settings.RunningState = RunningState.ROASTING;
            if (packet.Control[0] == 0x04 && packet.Control[1] == 0x04) settings.RunningState = RunningState.COOLING;
            if (packet.Control[0] == 0x08 && packet.Control[1] == 0x01) settings.RunningState = RunningState.STOPPED;

            return settings;
        }
    }
    public enum RunningState
    {
        IDLE = 1,
        ROASTING = 2,
        COOLING = 3,
        STOPPED = 4
    }

    public enum HeatLevel
    {
        OFF = 0,
        LOW = 1,
        MEDIUM = 2,
        HIGH = 3
    }

}

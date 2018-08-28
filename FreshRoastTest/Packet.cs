using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshRoastTest
{
    public class Packet
    {
        public static byte[] INITPACKET = new byte[] { 0xAA, 0x55, 0x61, 0x74, 0x63, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA, 0xFA };

        public byte[] PacketHeader { get; private set; }
        public byte[] ID { get; private set; }
        public byte[] Flags { get; private set; }
        public byte[] Control { get; set; }
        public byte[] FanSpeed { get; set; }
        public byte[] Timer { get; set; }
        public byte[] HeatLevel { get; set; }
        public byte[] CurrentTemperature { get; private set; }
        public byte[] Footer { get; private set; }

        public Packet()
        {
            this.PacketHeader =             new byte[] { 0xAA, 0xAA };  //AAAA
            this.ID =                       new byte[] { 0x61, 0x74 };  //6174
            this.Flags =                    new byte[] { 0x63 };        //Sending Packet
            this.Control =                  new byte[] { 0x02, 0x01 };  //IDLE
            this.FanSpeed =                 new byte[] { 0x09 };        //9
            this.Timer =                    new byte[] { 0x15 };        //1.5 minutes
            this.HeatLevel =                new byte[] { 0x01 };        //1
            this.CurrentTemperature =       new byte[] { 0x00, 0x00 };  //0000
            this.Footer =                   new byte[] { 0xAA, 0xFA };  //AAFA
        }


        public static Packet FromByteArray(byte[] data)
        {
            if(data.Length == 14)
            {
                var packet = new Packet();
                packet.PacketHeader = data.Take(2).ToArray();
                packet.ID = data.Skip(2).Take(2).ToArray();
                packet.Flags = data.Skip(4).Take(1).ToArray();
                packet.Control = data.Skip(5).Take(2).ToArray();
                packet.FanSpeed = data.Skip(7).Take(1).ToArray(); ;
                packet.Timer = data.Skip(8).Take(1).ToArray(); ;
                packet.HeatLevel = data.Skip(9).Take(1).ToArray(); ;
                packet.CurrentTemperature = data.Skip(10).Take(2).ToArray();
                packet.Footer = data.Skip(12).Take(2).ToArray();

                return packet;
            }
            return null;
        }

        public byte[] ToByteArray()
        {
            byte[] rv = new byte[14];

            System.Buffer.BlockCopy(this.PacketHeader,          0, rv, 0,  2);
            System.Buffer.BlockCopy(this.ID,                    0, rv, 2,  2);
            System.Buffer.BlockCopy(this.Flags,                 0, rv, 4,  1);
            System.Buffer.BlockCopy(this.Control,               0, rv, 5,  2);
            System.Buffer.BlockCopy(this.FanSpeed,              0, rv, 7,  1);
            System.Buffer.BlockCopy(this.Timer,                 0, rv, 8,  1);
            System.Buffer.BlockCopy(this.HeatLevel,             0, rv, 9,  1);
            System.Buffer.BlockCopy(this.CurrentTemperature,    0, rv, 10, 2);
            System.Buffer.BlockCopy(this.Footer,                0, rv, 12, 2);

            return rv;
        }


        public override string ToString()
        {
            StringBuilder hex = new StringBuilder(14 * 2);
            foreach (byte b in this.ToByteArray())
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }

        public int TemperatureValue()
        {
            if (this.CurrentTemperature[0] == 0xFF && this.CurrentTemperature[1] == 0x00) return 150;


            StringBuilder hex = new StringBuilder(2 * 2);
            foreach (byte b in this.CurrentTemperature)
                hex.AppendFormat("{0:X2}", b);
            return int.Parse(hex.ToString(), System.Globalization.NumberStyles.HexNumber);
        }
    }
}

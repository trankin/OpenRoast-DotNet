using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreshRoastTest
{

    public class Roaster
    {
        public class DataReceivedEventArgs : EventArgs
        {
            public int Temperature { get; set; }
            public RoasterSettings Settings { get; set; }
        }


        private const string DEVICE_ID = "VID_1A86&PID_5523";

        private SerialPort _serialPort;
        private InternalState _state = InternalState.INITIALIZING;
        public RoasterSettings ActiveSettings { get; private set; }
        public RoasterSettings Settings { get; set; }
        public IList<RoasterSettings> CurrentRecipe { get; private set; }
        public int Temperature { get; private set; }
        private enum InternalState
        {
            INITIALIZING,
            RECEIVING_RECIPE,
            INITIALIZED
        }

        public Roaster()
        {
            this.CurrentRecipe = new List<RoasterSettings>();
            this.Settings = new RoasterSettings();
            
            var autoEvent = new AutoResetEvent(false);
            Timer timer = new Timer(Tick, autoEvent, 1000, 100);

            var usbDeviceList = USBDeviceManager.GetUSBDevices();
            if(usbDeviceList.Count > 0)
            {
                var usbDevice = usbDeviceList.FirstOrDefault(f => f.DeviceID.Contains(DEVICE_ID));
                if(usbDevice != null)
                {
                    _serialPort = new SerialPort(usbDevice.Port, 9600, Parity.None, 8, StopBits.One);
                    _serialPort.ReadTimeout = 500;
                    _serialPort.WriteTimeout = 500;
                    _serialPort.DataReceived += _serialPort_DataReceived;
                    _serialPort.ErrorReceived += _serialPort_ErrorReceived;
                    _serialPort.Open();
                    _serialPort.Write(Packet.INITPACKET, 0, Packet.INITPACKET.Length);
                }
                else
                {
                    //throw an error... can't find the port.
                    throw (new Exception());
                }
                
            }

        }

        protected virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            EventHandler<DataReceivedEventArgs> handler = DataReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        private void Tick(object state)
        {
            if(_serialPort.IsOpen && this._state == InternalState.INITIALIZED)
            {
                var packet = this.Settings.ToPacket();
                //Console.WriteLine(packet.ToString() + ": - Sending");
                _serialPort.Write(packet.ToByteArray(), 0, 14);
            }
        }

        private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!_serialPort.IsOpen) return;
            byte[] buffer = new byte[_serialPort.BytesToRead];
            _serialPort.Read(buffer, 0, buffer.Length);

            if(buffer.Length == 14)
            {
                var packet = Packet.FromByteArray(buffer);
                var settings = RoasterSettings.FromPacket(packet);
                if(this._state == InternalState.INITIALIZED)
                {
                    this.ActiveSettings = settings;
                    this.Temperature = packet.TemperatureValue();
                    OnDataReceived(new DataReceivedEventArgs { Temperature = this.Temperature, Settings = this.Settings });
                }

                if(this._state == InternalState.INITIALIZING)
                {
                    this.ActiveSettings = settings;
                    this._state = InternalState.RECEIVING_RECIPE;
                }

                if(this._state == InternalState.RECEIVING_RECIPE)
                {
                    this.CurrentRecipe.Add(settings);
                    if (buffer[4] == 0xAF)
                    {
                        this._state = InternalState.INITIALIZED;
                    }
                }

                //Console.WriteLine(packet.ToString() + ": -- packet");
            }
            string dat = BitConverter.ToString(buffer).Replace("-", "").ToUpper();

            //Console.WriteLine(dat + ":" + DateTime.Now.Ticks);
        }
    }




}

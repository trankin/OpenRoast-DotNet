using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Management;

namespace FreshRoastTest
{
    class Program
    {

        
        //private static byte[] ROASTPACKET = StringToByteArray("AAAA6174630402013B010000AAFA");
        //private static SerialPort _serialPort;

        static bool _continue;
        static Roaster roaster;
        static void Main(string[] args)
        {
            roaster = new Roaster();
            roaster.DataReceived += Roaster_DataReceived;
            roaster.Settings.FanSpeed = 3;
            Thread.Sleep(1000);
            roaster.Settings.HeatLevel = HeatLevel.HIGH;

            Thread.Sleep(1000);
            roaster.Settings.RunningState = RunningState.ROASTING;
            roaster.Settings.HeatLevel = HeatLevel.OFF;
            roaster.Settings.TimerMinutes = .2f;

            Thread.Sleep(4000);
            roaster.Settings.RunningState = RunningState.COOLING;
            roaster.Settings.FanSpeed = 9;


            Thread readThread = new Thread(Read);
            readThread.Start();
            Console.ReadLine();

        }

        private static void Roaster_DataReceived(object sender, Roaster.DataReceivedEventArgs e)
        {
            if(e.Temperature > 200)
            {
                roaster.Settings.RunningState = RunningState.COOLING;
            }
            else
            {
                roaster.Settings.RunningState = RunningState.ROASTING;
            }
            Console.WriteLine(e.Temperature);
        }

        private static void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    
                }
                catch (TimeoutException) { }
            }
        }





    }
}

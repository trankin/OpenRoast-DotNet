using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FreshRoastTest
{
    public class USBDeviceManager
    {

        public static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ConnectionOptions options = new ConnectionOptions();
            ManagementScope connectionScope = new ManagementScope(@"\root\CIMV2");
            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
            ManagementObjectSearcher comPortSearcher = new ManagementObjectSearcher(connectionScope, objectQuery);
            foreach (ManagementObject device in comPortSearcher.Get())
            {
                object captionObj = device["Caption"];

                if (captionObj != null)
                {
                    var caption = captionObj.ToString();
                    Regex r = new Regex(@"\((COM\d)\)", RegexOptions.IgnoreCase);
                    var match = r.Match(caption);
                    if (match.Success)
                    {
                        devices.Add(new USBDeviceInfo(
                        (string)device.GetPropertyValue("DeviceID"),
                        (string)device.GetPropertyValue("PNPDeviceID"),
                        (string)device.GetPropertyValue("Description"),
                        (string)device.GetPropertyValue("Name"),
                        match.Value.Replace("(", "").Replace(")", "")
                        ));
                    }
                }
            }
            return devices;
        }

        public class USBDeviceInfo
        {
            public USBDeviceInfo(string deviceID, string pnpDeviceID, string description, string name, string port)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Description = description;
                this.Name = name;
                this.Port = port;

            }
            public string DeviceID { get; private set; }
            public string PnpDeviceID { get; private set; }
            public string Description { get; private set; }
            public string Name { get; private set; }
            public string Port { get; private set; }

        }
    }
}

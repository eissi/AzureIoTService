using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace IoTDevices
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "juleedemo.azure-devices.net";
        //static string deviceKey = "rDSDirCgGmtZB0BSqW7fGUWaM2m3SRqBh81Csgc0leU=";

        // for add devices
        static RegistryManager registryManager;
        static string connectionString = "HostName=juleedemo.azure-devices.net;SharedAccessKeyName=registryReadWrite;SharedAccessKey=UyRlyfGxaAWHs5c8LFvzDSaYWB8liQaW4stcynxWo5s=";


        static void Main(string[] args)
        {
            string mode;
            Console.Write("Type this device ID:");
            //string deviceId=Console.ReadLine();
            string deviceId = ".net";

            //Add or get deviceKey
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            Task<string> task = AddmyDeviceAsync(deviceId);
            string deviceKey = task.Result;


            //Console.WriteLine("choose mode of devices: 1 to send telemetry 2 to get commands");
            //mode=Console.ReadLine();

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));
            //deviceClient = DeviceClient.CreateFromConnectionString("HostName=IoT-julee.azure-devices.net;DeviceId=javaclient;SharedAccessKey=dKwsHSWDJA3Gy4wZqG+TQ36dVt1cfAgvl4B/LCz+PG0=");
            //deviceClient.OpenAsync();
            //if (mode == "1")
            //{
            //    Console.WriteLine("Simulated device\n");
                
            //    SendDeviceToCloudMessagesAsync(deviceId);
            //}
            //else
            //{
                ReceiveC2dAsync();
            //}
            //Console.WriteLine("Program exit. Type Enter.");
            Console.ReadLine();
        }
        private static async void SendDeviceToCloudMessagesAsync(string deviceId)
        {
            double avgWindSpeed = 10; // m/s
            Random rand = new Random();

            var devPerf = new DevicePerformance();

            while (true)
            {
                try
                {
                    devPerf.set_cpuvalue();
                    devPerf.set_memvalue();

                    var telemetrydatapoint = new { deviceid = deviceId, CPUusage = devPerf.cpuusage, MEMusage = devPerf.memusage };
                    var messagestring = JsonConvert.SerializeObject(telemetrydatapoint);
                    var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messagestring));
                    await deviceClient.SendEventAsync(message);
                    Console.WriteLine("{0} > sending message: {1}", DateTime.Now, messagestring);
                    
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                Thread.Sleep(1000);

            }


        }
        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Microsoft.Azure.Devices.Client.Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) {
                    //ReceiveAsync가 timeout이 있는 듯 나중에 체크할 것
                    Console.WriteLine("{0}: no message",DateTime.Now);
                    continue;
                }
                
                

                string serviceMessage = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                Console.WriteLine("RECEIVED: {0}", DateTime.Now);

                try
                {
                    var messagestring = serviceMessage + "," + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");
                    //var telemetrydatapoint = new { StartTime = serviceMessage, DeviceTime = DateTime.Now.ToString() };
                    //var messagestring = JsonConvert.SerializeObject(telemetrydatapoint);
                    var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messagestring));
                    await deviceClient.SendEventAsync(message);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Message Sent: {0}", messagestring);
                    Console.ResetColor();

                    await deviceClient.CompleteAsync(receivedMessage);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }               
                
                
                                
            }
        }
        private async static Task<string> AddmyDeviceAsync(string deviceId)
        {
            //string deviceId = "myFirstDevice4";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            return device.Authentication.SymmetricKey.PrimaryKey;
            
        }
    }
}

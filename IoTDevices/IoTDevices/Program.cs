﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

using System.Runtime.Serialization;

namespace IoTDevices
{

    class Program
    {


        static DeviceClient deviceClient;
        static string iotHubUri = "julee1part.azure-devices.net";
        //static string deviceKey = "rDSDirCgGmtZB0BSqW7fGUWaM2m3SRqBh81Csgc0leU=";

        // for add devices
        static RegistryManager registryManager;
        static string connectionString = "HostName=julee1part.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=EY/1g6WzzOmtuUkqF4JcC7raC20pg+nNQ1wutFC6z6Y=";


        static void Main(string[] args)
        {

            Trace.Listeners.Add(new TextWriterTraceListener("device.log"));
            Trace.AutoFlush = true;

            string mode;
            Console.Write("Type this device ID:");
            string deviceId = Console.ReadLine();
            //string deviceId = ".net";

            //Add or get deviceKey
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            Task<string> task = AddmyDeviceAsync(deviceId);
            string deviceKey = task.Result;

            //string deviceId="";
            //Console.Write("Enter Device Connection String for this device:");
            //connectionString = Console.ReadLine();
            ////HostName = juleedemo.azure - devices.net; DeviceId = java; SharedAccessKey = jstNFhvprAs + Nr8HYbl9GV2YprEvyF7W2WyLKUdr5YQ =

            //var components = connectionString.Split(';');
            //var blocks = components.Select(component => component.Split('='));
            //foreach (var block in blocks)
            //{
            //    string key = block[0].Replace(" ", string.Empty);
            //    string value = block[1].Replace(" ", string.Empty);
            //    if (key == "DeviceId")
            //        deviceId = value;

            //}
            //string deviceId=Console.ReadLine();
            //string deviceId = ".net";

            //Add or get deviceKey
            //registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            //Task<string> task = AddmyDeviceAsync(deviceId);
            //string deviceKey = task.Result;


            //Console.WriteLine("choose mode of devices: 1 to send telemetry 2 to get commands");
            //mode=Console.ReadLine();

            //deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), (Microsoft.Azure.Devices.Client.TransportType)2);

            //deviceClient = DeviceClient.CreateFromConnectionString("HostName=IoT-julee.azure-devices.net;DeviceId=javaclient;SharedAccessKey=dKwsHSWDJA3Gy4wZqG+TQ36dVt1cfAgvl4B/LCz+PG0=");
            //deviceClient.OpenAsync();
            //if (mode == "1")
            //{
            //    Console.WriteLine("Simulated device\n");

                SendDeviceToCloudMessagesAsync(deviceId);
            //}
            //else
            //{
            //ReceiveC2dAsync(deviceId);
            //}
            //Console.WriteLine("Program exit. Type Enter.");
            Console.ReadLine();
        }



        private static async void ReceiveC2dAsync(string deviceid)
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Microsoft.Azure.Devices.Client.Message receivedMessage;
                try
                {
                    receivedMessage = await deviceClient.ReceiveAsync();
                }
                catch(Exception e)
                {
                    Console.WriteLine("{0}:{1}", DateTime.Now, e.Message);
                    Trace.TraceError("{0}:{1}", DateTime.Now, e.Message);
                    continue;
                }
                
                if (receivedMessage == null)
                {
                    //ReceiveAsync가 timeout이 있는 듯 나중에 체크할 것
                    Console.WriteLine("{0}: no message", DateTime.Now);
                    Trace.TraceInformation("{0}: no message", DateTime.Now);
                    continue;
                }



                string serviceMessage = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                Console.WriteLine("RECEIVED: {0}", DateTime.Now);
                Trace.TraceInformation("RECEIVED: {0}", DateTime.Now);

                try
                {
                    //var messagestring = serviceMessage + "," + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");
                    var telemetrydatapoint = new { DeviceID = deviceid, StartTime = serviceMessage, DeviceTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ") };
                    var messagestring = JsonConvert.SerializeObject(telemetrydatapoint);
                    var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messagestring));
                    await deviceClient.SendEventAsync(message);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Message Sent: {0}", messagestring);
                    Trace.TraceInformation("Message Sent: {0}", messagestring);
                    Console.ResetColor();

                    await deviceClient.CompleteAsync(receivedMessage);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Trace.TraceError("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }



            }
        }
        private static async void SendDeviceToCloudMessagesAsync(string deviceId)
        {
            //double avgWindSpeed = 10; // m/s
            //Random rand = new Random();

            //var devPerf = new DevicePerformance();

            while (true)
            {
                try
                {
                    //devPerf.set_cpuvalue();
                    //devPerf.set_memvalue();
                    var timer = new Stopwatch();
                    int no_events = 1000;
                    timer.Start();
                    for (int i = 0; i < no_events; i++)
                    {
                        var telemetrydatapoint = new { deviceid = deviceId, Time = DateTime.Now };
                        //var telemetrydatapoint = new { deviceid = deviceId, CPUusage = devPerf.cpuusage, MEMusage = devPerf.memusage };
                        var messagestring = JsonConvert.SerializeObject(telemetrydatapoint);
                        var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messagestring));
                        await deviceClient.SendEventAsync(message);
                        //Console.WriteLine("{0} > sending message: {1}", DateTime.Now, messagestring);
                    }
                    timer.Stop();
                    Console.WriteLine("{0} events/sec", no_events/timer.Elapsed.TotalSeconds);


                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                //Thread.Sleep(1000);

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

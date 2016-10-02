using System;
using System.IO;
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
        //static DeviceClient deviceClient;
        static string iotHubUri = "j2part.azure-devices.net";
        //static string deviceKey = "rDSDirCgGmtZB0BSqW7fGUWaM2m3SRqBh81Csgc0leU=";

        // for add devices
        static RegistryManager registryManager;
        static string connectionString;

        static DateTime instanceTime = DateTime.Now;
        static int no_event_device;

        static void Main(string[] args) //deviceid, sleeptime(ms), number_of_events_by_this_simulator_device, connectionstring
        {
            string tracefile = System.Diagnostics.Process.GetCurrentProcess().ProcessName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
            //Console.WriteLine(tracefile);

            Trace.Listeners.Add(new TextWriterTraceListener(tracefile));
            Trace.AutoFlush = true;

            //Add or get deviceKey
            registryManager = RegistryManager.CreateFromConnectionString(args[2]);
            Task<string> task = AddmyDeviceAsync(args[0]);
            string deviceKey = task.Result;

            
            //run thread for each device
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(args[0], deviceKey), (Microsoft.Azure.Devices.Client.TransportType)2);

            int sleeptime;
            Int32.TryParse(args[1], out sleeptime);
            Int32.TryParse(args[2], out no_event_device);

            SendDeviceToCloudMessagesAsync(args[0], sleeptime);

            Console.ReadLine();
        }




        private static async void SendDeviceToCloudMessagesAsync(string deviceId, int sleeptime)
        {
            int no_event_emit = 0;
            while (true)
            {
                try
                {
                    var timer = new Stopwatch();
                    int no_events = 100;
                    timer.Start();
                    for (int i = 0; i < no_events; i++)
                    {
                        var telemetrydatapoint = new { DeviceID = deviceId, StartTime = instanceTime.ToUniversalTime().ToString("O"), DeviceTime = DateTime.Now.ToUniversalTime().ToString("O") };
                        var messagestring = JsonConvert.SerializeObject(telemetrydatapoint);
                        var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messagestring));
                        await deviceClient.SendEventAsync(message);
                        //Console.ForegroundColor = ConsoleColor.Yellow;
                        //Console.WriteLine("Message Sent: {0}", messagestring);
                        //Trace.TraceInformation("Message Sent: {0}", messagestring);
                        //Console.ResetColor();
                        if (no_event_device < no_event_emit++) break;
                        Thread.Sleep(sleeptime);
                    }
                    timer.Stop();


                    if (no_event_device < no_event_emit) break;
                    Console.WriteLine("DeviceID: {0}, {1} events/sec", deviceId, no_events / timer.Elapsed.TotalSeconds);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                //Thread.Sleep(5000);

            }
            Console.WriteLine("Device emiited all: {0}", deviceId);

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

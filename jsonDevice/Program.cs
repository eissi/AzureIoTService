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


        //static DeviceClient deviceClient;
        static string iotHubUri = "j2part.azure-devices.net";
        //static string deviceKey = "rDSDirCgGmtZB0BSqW7fGUWaM2m3SRqBh81Csgc0leU=";

        // for add devices
        static RegistryManager registryManager;
        static string connectionString = "HostName=j2part.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=rwVuPRFKt18FJNMrCbnKnv91N0Xc2UCwPXR0Gmipzsw=";

        static DateTime instanceTime = DateTime.Now;

        
        static int no_device = 1;
        static int no_event_device=10000;
        static int no_event = no_device*no_event_device;
        static void Main(string[] args)
        {
            string tracefile = System.Diagnostics.Process.GetCurrentProcess().ProcessName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
            Console.WriteLine(tracefile);

            Trace.Listeners.Add(new TextWriterTraceListener(tracefile));
            Trace.AutoFlush = true;

            //get number of devices
            //Console.Write("Number of devices:");
            //string deviceno = Console.ReadLine();
            //string deviceId = ".net";
            
            Console.WriteLine("The number of devices to be simulated: {0}", no_device);
            IEnumerable<Device> devices = null;


            //Check if the file exists for device keys
            string device_json_file = "devices.json";
            if (File.Exists(device_json_file))
            {
                //if exist, load device information
                Console.WriteLine($"Find the file: {device_json_file}");
                string jsoncontent = File.ReadAllText(device_json_file);
                devices = JsonConvert.DeserializeObject<IEnumerable<Device>>(jsoncontent);

            }
            //no file or if no enough devices, create, load and update the file
            if (!File.Exists(device_json_file) || (devices == null) || (devices.ToList<Device>().Count < no_device))
            {
                devices = Enumerable.Range(0, no_device)
                .ToList().Select(x => x.ToString()).Select(x => new Device(x));
                //if not, get from IoT Hub and write to file to re-load

                //devices.ToList().ForEach(Console.WriteLine);

                registryManager = RegistryManager.CreateFromConnectionString(connectionString);

                //foreach (Device dev in devices)
                //{
                //    registryManager.RemoveDeviceAsync(dev);
                //}
                

                if (!AddmyDevicesAsync(devices).Result) //fails to add devices, in this case, some devices already registered
                {    //read from IoT Hub
                    devices = ReadmyDevicesAsync(no_device*2+5).Result;
                    //devices = registryManager.GetDevicesAsync(no_device).Result;
                    //Device dev = registryManager.GetDeviceAsync("device0").Result;
                    //for (int i = 1; i < 15; i++)
                    //{
                    //    devices = registryManager.GetDevicesAsync(i).Result;
                    //    Console.WriteLine(devices.Count());
                    //}
                    
                    //devices = registryManager.GetDevicesAsync(1).Result;
                    //devices = registryManager.GetDevicesAsync(2).Result;
                    //devices = registryManager.GetDevicesAsync(3).Result;
                    //devices = registryManager.GetDevicesAsync(4).Result;
                }

                //write file for re-run
                File.WriteAllText(device_json_file, JsonConvert.SerializeObject(devices, Formatting.Indented));

            }

            //run thread for each device

            for (int i = 0; i < no_device; i++)
            {
                SendDeviceToCloudMessagesAsync(devices.ElementAt(i));
                Thread.Sleep(100);
            }
            //devices.ToList().ForEach(SendDeviceToCloudMessagesAsync);

            //deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),(Microsoft.Azure.Devices.Client.TransportType)2);

            //deviceClient.OpenAsync();
            //if (mode == "1")
            //{
            //    Console.WriteLine("Simulated device\n");


            //}
            //else
            //{
            //    ReceiveC2dAsync(deviceId);
            //}
            //Console.WriteLine("Program exit. Type Enter.");
            Console.ReadLine();
        }



        //private static async void ReceiveC2dAsync(string deviceid)
        //{
        //    Console.WriteLine("\nReceiving cloud to device messages from service");
        //    while (true)
        //    {
        //        Microsoft.Azure.Devices.Client.Message receivedMessage;
        //        try
        //        {
        //            receivedMessage = await deviceClient.ReceiveAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine("{0}:{1}", DateTime.Now, e.Message);
        //            Trace.TraceError("{0}:{1}", DateTime.Now, e.Message);
        //            continue;
        //        }

        //        if (receivedMessage == null)
        //        {
        //            //ReceiveAsync가 timeout이 있는 듯 나중에 체크할 것
        //            Console.WriteLine("{0}: no message", DateTime.Now);
        //            Trace.TraceInformation("{0}: no message", DateTime.Now);
        //            continue;
        //        }



        //        string serviceMessage = Encoding.ASCII.GetString(receivedMessage.GetBytes());

        //        Console.WriteLine("RECEIVED: {0}", DateTime.Now);
        //        Trace.TraceInformation("RECEIVED: {0}", DateTime.Now);

        //        try
        //        {
        //            //var messagestring = serviceMessage + "," + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");
        //            var telemetrydatapoint = new { DeviceID = deviceid, StartTime = serviceMessage, DeviceTime = DateTime.Now.ToUniversalTime().ToString("O") };
        //            var messagestring = JsonConvert.SerializeObject(telemetrydatapoint);
        //            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messagestring));
        //            await deviceClient.SendEventAsync(message);

        //            Console.ForegroundColor = ConsoleColor.Yellow;
        //            Console.WriteLine("Message Sent: {0}", messagestring);
        //            Trace.TraceInformation("Message Sent: {0}", messagestring);
        //            Console.ResetColor();

        //        }
        //        catch (Exception exception)
        //        {
        //            Console.ForegroundColor = ConsoleColor.Red;
        //            Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
        //            Trace.TraceError("{0} > Exception: {1}", DateTime.Now, exception.Message);
        //            Console.ResetColor();
        //        }



        //    }
        //}
        private static async void SendDeviceToCloudMessagesAsync(Device device)
        {
            DeviceClient deviceClient;
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(device.Id, device.Authentication.SymmetricKey.PrimaryKey), (Microsoft.Azure.Devices.Client.TransportType)2);

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
                        var telemetrydatapoint = new { DeviceID = device.Id, StartTime = instanceTime.ToUniversalTime().ToString("O"), DeviceTime = DateTime.Now.ToUniversalTime().ToString("O") };
                        var messagestring = JsonConvert.SerializeObject(telemetrydatapoint);
                        var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messagestring));
                        await deviceClient.SendEventAsync(message);
                        //Console.ForegroundColor = ConsoleColor.Yellow;
                        //Console.WriteLine("Message Sent: {0}", messagestring);
                        //Trace.TraceInformation("Message Sent: {0}", messagestring);
                        //Console.ResetColor();
                        if (no_event_device < no_event_emit++) break;
                        Thread.Sleep(no_device*10);
                    }
                    timer.Stop();

                    
                    if (no_event_device < no_event_emit) break;
                    Console.WriteLine("DeviceID: {0}, {1} events/sec", device.Id, no_events / timer.Elapsed.TotalSeconds);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                //Thread.Sleep(5000);
                
            }
            Console.WriteLine("Device emiited all: {0}", device.Id);

        }

        private async static Task<bool> AddmyDevicesAsync(IEnumerable<Device> devices)
        {
            //string deviceId = "myFirstDevice4";
            var result = new BulkRegistryOperationResult();
            //var devices = new List<Device>();
            //Dictionary<string, string>.KeyCollection keys = devmap.Keys;
            //foreach (string key in keys)
            //{
            //    devices.Add(new Device(key));
            //}

            //var devices = Enumerable.Range(0, no_device)
            //    .ToList().Select(x => "device" + x.ToString()).Select(x=>new Device(x));

            //try
            //{
            result =  registryManager.AddDevices2Async(devices).Result;
            //    //await registryManager.AddDeviceAsync(new Device("test"));
            //}
            //catch (DeviceAlreadyExistsException)
            //{
            //    //device = await registryManager.GetDeviceAsync(deviceId);
            //}
            return result.IsSuccessful;
            //if (result.Errors.All(x=>x==false))
            //    return true;
            //else
            //    return false;
            //return device.Authentication.SymmetricKey.PrimaryKey;

        }
        private async static Task<IEnumerable<Device>> ReadmyDevicesAsync(int no_device)
        {
            return registryManager.GetDevicesAsync(no_device).Result;
            //return devices;
        }
    }
}

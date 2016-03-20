using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

using Microsoft.ServiceBus.Messaging;

using System.Threading;

using System.Data.SqlClient;

namespace IoTConsole
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=juleedemo.azure-devices.net;SharedAccessKeyName=registryReadWrite;SharedAccessKey=UyRlyfGxaAWHs5c8LFvzDSaYWB8liQaW4stcynxWo5s=";
        static ServiceClient serviceClient;
        static string serviceConnectionString = "HostName=juleedemo.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=3N3AWYm/f77RGIrGcAqkuw+2onRjHueUaop9S2QXGz0=";


        //receive용
        static string eventhubconnectionString = "HostName=juleedemo.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=3N3AWYm/f77RGIrGcAqkuw+2onRjHueUaop9S2QXGz0=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static string commandReceived="";
        static DateTime commandReceivedTime;
        static int timeout = 10; //seconds
        static void Main(string[] args)
        {
            DateTime instanceTime = DateTime.Now;

            var conn = new SqlConnection("Server=tcp:iotjulee.database.windows.net,1433;Database=IotHubDemo;User ID=julee@iotjulee;Password=Passw0rd;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            conn.Open();

            string dvcSDKver = ".Net 1.0.2";
            string svcSDKver = ".Net 1.0.3";
            
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\julee\iothubperf.txt", true);

            Console.WriteLine("Start to send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(serviceConnectionString);

            ReceiveFeedbackAsync();

            //receive용
            eventHubClient = EventHubClient.CreateFromConnectionString(eventhubconnectionString, iotHubD2cEndpoint);
            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            foreach (string partition in d2cPartitions)
            {
                ReceiveMessagesFromDeviceAsync(partition);
            }
            
            Console.Write("Enter the device ID:");
            string deviceId = Console.ReadLine();
            //string deviceId = "demo";
            Console.Write("Enter the device SDK version:");
            dvcSDKver = Console.ReadLine();
            Console.Write("Enter a description:");
            string desc = Console.ReadLine();

            while (true)
            {
                bool commandProcessed = false;
                DateTime commandTime = DateTime.Now;
                string command = commandTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");
                try { 
                    SendCloudToDeviceMessageAsync(deviceId, command).Wait();
                }
                catch(Exception e)
                {
                    continue;
                }
                Console.WriteLine("SENT: {0}", command);

                while (!commandProcessed)
                {
                    if (commandReceived != "")
                    {
                        Console.WriteLine("Enter: Processing command received! - {0}", commandReceived);
                        string[] timing = commandReceived.Split(',');
                        if (command == timing[0])
                        {
                            DateTime finishTime = DateTime.Now;
                            string finish = finishTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");
                            DateTime devTime;
                            string devTimeString = timing[1];
                            DateTime.TryParseExact(devTimeString, "yyyy-MM-dd HH:mm:ss:fffff", null, System.Globalization.DateTimeStyles.None, out devTime);
                            TimeSpan elapsedTime = finishTime - commandTime;
                            //file.WriteLine("{0},{1},{2}", commandReceived, finish, elapsedTime.TotalMilliseconds);
                            //file.FlushAsync();

                            var cmd = conn.CreateCommand();
                            cmd.CommandText = @"
                INSERT dbo.PerfLogs (DeviceID, ServiceSendTime, DeviceTime, IoTHubReceiveTime, ServiceReceiveTime, ElapsedTime, TimeOut, Success, ServiceSDKversion, DeviceSDKversion, InstanceStartTime, Description)
                VALUES (@DeviceID, @ServiceSendTime, @DeviceTime, @IoTHubReceiveTime, @ServiceReceiveTime, @ElapsedTime, @TimeOut, @Success, @ServiceSDKversion, @DeviceSDKversion, @InstanceStartTime, @Description)";

                            cmd.Parameters.AddWithValue("@DeviceID", deviceId);
                            cmd.Parameters.AddWithValue("@ServiceSendTime", commandTime.ToUniversalTime());
                            cmd.Parameters.AddWithValue("@ServiceReceiveTime", finishTime.ToUniversalTime());
                            cmd.Parameters.AddWithValue("@ElapsedTime", elapsedTime.TotalMilliseconds);
                            cmd.Parameters.AddWithValue("@TimeOut", 30000);
                            cmd.Parameters.AddWithValue("@Success", 1);
                            cmd.Parameters.AddWithValue("@ServiceSDKversion", svcSDKver);
                            cmd.Parameters.AddWithValue("@DeviceSDKversion", dvcSDKver);

                            cmd.Parameters.AddWithValue("@DeviceTime", devTime.ToUniversalTime());
                            cmd.Parameters.AddWithValue("@IoTHubReceiveTime", commandReceivedTime);

                            cmd.Parameters.AddWithValue("@InstanceStartTime", instanceTime.ToUniversalTime());

                            cmd.Parameters.AddWithValue("@Description", desc);


                            cmd.ExecuteScalar();

                           
                            Console.WriteLine("Received: {0},{1},{2}", commandReceived, finish, elapsedTime.TotalMilliseconds);
                            commandReceived = "";
                            commandProcessed = true;
                        }
                        commandReceived = "";



                    }

                    //no command received yet
                    //Console.WriteLine("Command Not Received");
                    Thread.Sleep(100);

                    //if timeout occurred
                    if(timeout < (DateTime.Now - commandTime).TotalSeconds)
                    {
                        Console.WriteLine("TIMEOUT occurred");

                        DateTime finishTime = DateTime.Now;
                        string finish = finishTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");
                        
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = @"
                INSERT dbo.PerfLogs (DeviceID, ServiceSendTime, ServiceReceiveTime, TimeOut, Success, ServiceSDKversion, DeviceSDKversion, InstanceStartTime, Description)
                VALUES (@DeviceID, @ServiceSendTime, @ServiceReceiveTime, @TimeOut, @Success, @ServiceSDKversion, @DeviceSDKversion, @InstanceStartTime, @Description)";

                        cmd.Parameters.AddWithValue("@DeviceID", deviceId);
                        cmd.Parameters.AddWithValue("@ServiceSendTime", commandTime.ToUniversalTime());
                        cmd.Parameters.AddWithValue("@ServiceReceiveTime", finishTime.ToUniversalTime());
                        //cmd.Parameters.AddWithValue("@ElapsedTime", elapsedTime.TotalMilliseconds);
                        cmd.Parameters.AddWithValue("@TimeOut", timeout);
                        cmd.Parameters.AddWithValue("@Success", 0);
                        cmd.Parameters.AddWithValue("@ServiceSDKversion", svcSDKver);
                        cmd.Parameters.AddWithValue("@DeviceSDKversion", dvcSDKver);

                        //cmd.Parameters.AddWithValue("@DeviceTime", devTime);
                        //cmd.Parameters.AddWithValue("@IoTHubReceiveTime", commandReceivedTime);

                        cmd.Parameters.AddWithValue("@InstanceStartTime", instanceTime.ToUniversalTime());

                        cmd.Parameters.AddWithValue("@Description", desc);


                        cmd.ExecuteScalar();

                        commandReceived = "";
                        commandProcessed = true;
                                                
                    }

                }

            }
            Console.WriteLine("Program exit. Type Enter.");
            Console.ReadLine();

            conn.Close();


        }
        private async static Task AddDeviceAsync(string deviceId)
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
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
        private async static Task SendCloudToDeviceMessageAsync(string deviceId, string command)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(command));
            //commandMessage.Ack = DeliveryAcknowledgement.Full;
            await serviceClient.SendAsync(deviceId, commandMessage);
        }
        private async static Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
            while (true)
            {
                EventData eventData=null;
                try
                {
                    eventData = await eventHubReceiver.ReceiveAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in reading events!");
                }
                
                if (eventData == null) continue;

                commandReceived = Encoding.UTF8.GetString(eventData.GetBytes());
                commandReceivedTime = eventData.EnqueuedTimeUtc;

                Console.WriteLine("RECEIVED: {0}", commandReceived);
                
            }
        }
        private async static void ReceiveFeedbackAsync()
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();

            Console.WriteLine("\nReceiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }
    }
}


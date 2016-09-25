﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

using Microsoft.ServiceBus.Messaging;

using System.Threading;

using System.Data.SqlClient;

using Newtonsoft.Json;

namespace IoTConsole
{
    class IoTHubMessage
    {
        public string message;
        public DateTime enqueuedTime;
    }
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=julee.azure-devices.net;SharedAccessKeyName=registryReadWrite;SharedAccessKey=/S/FvvslVBY506w2P/LVhWkFe5hBHpokLWacSelrVO4=";
        static ServiceClient serviceClient;
        static string serviceConnectionString = "HostName=julee.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=HGCjNhb8Wy6b4EYxiuz4poOU14ij8Bpf0UHZDmz/LpA=";


        //receive용 test
        static string eventhubconnectionString = serviceConnectionString;
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static int timeout = 10; //seconds

        static DateTime instanceTime;
        static SqlConnection conn;
        static string dvcSDKver;
        static string svcSDKver;
        static string deviceId;
        static DateTime startTime;
        static string startTimeString = "";
        static string desc;

        static bool isfirst = true;

        static int messageCount = 0;
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener("console.log"));
            Trace.AutoFlush = true;

            instanceTime = DateTime.Now;

            conn = new SqlConnection("Server=tcp:julee.database.windows.net,1433;Initial Catalog=iot;Persist Security Info=False;User ID=julee;Password=Passw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            conn.Open();

            dvcSDKver = ".Net 1.0.2";
            svcSDKver = ".Net 1.0.3";

            //System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\julee\iothubperf.txt", true);

            Console.WriteLine("Start to send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(serviceConnectionString);

            ReceiveFeedbackAsync();



            Console.Write("Enter the device ID:");
            deviceId = Console.ReadLine();
            //string deviceId = "demo";
            Console.Write("Enter the device SDK version:");
            dvcSDKver = Console.ReadLine();
            Console.Write("Enter a description:");
            desc = Console.ReadLine();

            //while (true)
            //{
            //receive용
            eventHubClient = EventHubClient.CreateFromConnectionString(eventhubconnectionString, iotHubD2cEndpoint);
            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            foreach (string partition in d2cPartitions)
            {
                ReceiveMessagesFromDeviceAsync(partition);
            }

            try
            {
                SendCloudToDeviceMessageAsync().Wait();
            }
            catch (Exception e)
            {

            }



            //    while (!commandProcessed)
            //    {
            //        if (commandReceived != "")
            //        {

            //            }
            //            commandReceived = "";



            //        }

            //        //no command received yet
            //        //Console.WriteLine("Command Not Received");
            //        Thread.Sleep(100);

            //        //if timeout occurred


            //    }

            //}
            Console.WriteLine("Program Running. Pless anykey to terminate!");
            Console.ReadLine();

            conn.Close();


        }

        private async static Task SendCloudToDeviceMessageAsync()
        {
            //Trace.TraceInformation("SENT ROUTINE STARTED.");
            //bool commandProcessed = false;
            startTime = DateTime.Now;
            startTimeString = startTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");
            var commandMessage = new Message(Encoding.ASCII.GetBytes(startTimeString));
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            //Trace.TraceInformation("Before while");
            while (true)
            {
                try
                {
                    //Trace.TraceInformation("Before await");
                    await serviceClient.SendAsync(deviceId, commandMessage);
                    //Trace.TraceInformation("After await");
                    break;
                }
                catch (Exception e)
                {
                    Trace.TraceInformation("Exception Entered");
                    Console.WriteLine("{0}: Exception - {1}", DateTime.Now, e.Message);
                    Trace.TraceError("{0}: Exception - {1}", DateTime.Now, e.Message);
                    Thread.Sleep(1000);
                }
            }

            Trace.TraceInformation("SENT:{1}: {0}", startTimeString, DateTime.Now);
            Console.WriteLine("SENT{1}: {0}", startTimeString, DateTime.Now);
            //sent = true;
        }
        private async static Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
            while (true)
            {
                EventData eventData = null;
                try
                {
                    eventData = await eventHubReceiver.ReceiveAsync(TimeSpan.FromSeconds(1));
                    Trace.TraceInformation("RAW RECEIVED:{1}: {0}", Encoding.UTF8.GetString(eventData.GetBytes()), DateTime.Now);
                    Console.WriteLine("RAW RECEIVED:{1}: {0}", Encoding.UTF8.GetString(eventData.GetBytes()), DateTime.Now);
                }
                catch (Exception e)
                {
                    if (isfirst)
                    {
                        isfirst = false;
                        if ((timeout < (DateTime.Now - startTime).TotalSeconds) && startTimeString != "")
                        {
                            Trace.TraceWarning("{0}:TIMEOUT occurred", DateTime.Now);
                            Console.WriteLine("{0}:TIMEOUT occurred", DateTime.Now);

                            //sent = false;

                            DateTime finishTime = DateTime.Now;
                            string finish = finishTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");

                            var cmd = conn.CreateCommand();
                            cmd.CommandText = @"
                INSERT dbo.PerfLogs (DeviceID, ServiceSendTime, ServiceReceiveTime, TimeOut, Success, ServiceSDKversion, DeviceSDKversion, InstanceStartTime, Description)
                VALUES (@DeviceID, @ServiceSendTime, @ServiceReceiveTime, @TimeOut, @Success, @ServiceSDKversion, @DeviceSDKversion, @InstanceStartTime, @Description)";

                            cmd.Parameters.AddWithValue("@DeviceID", deviceId);
                            cmd.Parameters.AddWithValue("@ServiceSendTime", startTime.ToUniversalTime());
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
                            startTimeString = "";
                            await SendCloudToDeviceMessageAsync();
                            isfirst = true;
                        }

                    }
                }

                if (eventData == null) continue;


                IoTHubMessage msg = new IoTHubMessage();
                msg.enqueuedTime = eventData.EnqueuedTimeUtc;
                msg.message = Encoding.UTF8.GetString(eventData.GetBytes());
                //Trace.TraceInformation("RECEIVED: {0}", msg.message);
                //Console.WriteLine("RECEIVED: {0}", msg.message);

                ProcessMessageAsync(msg);


            }
        }

        class jsonmessage { public string DeviceID; public string StartTime; public string DeviceTime; };

        static async void ProcessMessageAsync(IoTHubMessage msg)
        {
            //Console.WriteLine("Enter: Processing command received! - {0}", msg.message);
            //string[] timing = msg.message.Split(',');
            //string timing = msg.message;

            jsonmessage timing = JsonConvert.DeserializeObject<jsonmessage>(msg.message);
            if ((timing.DeviceID != deviceId) || (startTimeString != timing.StartTime))
            {

            }
            else //(startTimeString == timing[1])
            {
                //sent = false;

                DateTime finishTime = DateTime.Now;
                string finish = finishTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:fffff");
                DateTime devTime;
                string devTimeString = timing.DeviceTime;
                DateTime.TryParseExact(devTimeString, "yyyy-MM-dd HH:mm:ss:fffff", null, System.Globalization.DateTimeStyles.None, out devTime);
                TimeSpan elapsedTime = finishTime - startTime;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Elapsed Time:{1}:{0}", elapsedTime, ++messageCount);
                Console.ResetColor();
                //file.WriteLine("{0},{1},{2}", commandReceived, finish, elapsedTime.TotalMilliseconds);
                //file.FlushAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                INSERT dbo.PerfLogs (DeviceID, ServiceSendTime, DeviceTime, IoTHubReceiveTime, ServiceReceiveTime, ElapsedTime, TimeOut, Success, ServiceSDKversion, DeviceSDKversion, InstanceStartTime, Description)
                VALUES (@DeviceID, @ServiceSendTime, @DeviceTime, @IoTHubReceiveTime, @ServiceReceiveTime, @ElapsedTime, @TimeOut, @Success, @ServiceSDKversion, @DeviceSDKversion, @InstanceStartTime, @Description)";

                cmd.Parameters.AddWithValue("@DeviceID", deviceId);
                cmd.Parameters.AddWithValue("@ServiceSendTime", startTime.ToUniversalTime());
                cmd.Parameters.AddWithValue("@ServiceReceiveTime", finishTime.ToUniversalTime());
                cmd.Parameters.AddWithValue("@ElapsedTime", elapsedTime.TotalMilliseconds);
                cmd.Parameters.AddWithValue("@TimeOut", 30000);
                cmd.Parameters.AddWithValue("@Success", 1);
                cmd.Parameters.AddWithValue("@ServiceSDKversion", svcSDKver);
                cmd.Parameters.AddWithValue("@DeviceSDKversion", dvcSDKver);

                cmd.Parameters.AddWithValue("@DeviceTime", devTime.ToUniversalTime());
                cmd.Parameters.AddWithValue("@IoTHubReceiveTime", msg.enqueuedTime);

                cmd.Parameters.AddWithValue("@InstanceStartTime", instanceTime.ToUniversalTime());

                cmd.Parameters.AddWithValue("@Description", desc);

                try
                {
                    cmd.ExecuteScalar();
                }
                catch (Exception e)
                {
                    Trace.TraceError("{0}: SQL Exception - {1}", DateTime.Now, e.Message);
                    Console.WriteLine("{0}: SQL Exception - {1}", DateTime.Now, e.Message);
                }


                Trace.TraceInformation("Processed:{3}: {0},{1},{2}", msg.message, finish, elapsedTime.TotalMilliseconds, DateTime.Now);
                Console.WriteLine("Processed:{3}: {0},{1},{2}", msg.message, finish, elapsedTime.TotalMilliseconds, DateTime.Now);
                startTimeString = "";
                while (true)
                {
                    try
                    {
                        await SendCloudToDeviceMessageAsync();
                        break;
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("{0}: Exception - {1}", DateTime.Now, e.Message);
                        Console.WriteLine("{0}: Exception - {1}", DateTime.Now, e.Message);
                        Thread.Sleep(1000);
                    }

                }

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

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }
        private async static Task AddDeviceAsync()
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
    }
}


CREATE TABLE [dbo].[PerfLogs] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [DeviceID]           NVARCHAR (50) NULL,
    [ServiceSendTime]    DATETIME      NULL,
    [DeviceTime]         DATETIME      NULL,
    [IoTHubReceiveTime]  DATETIME      NULL,
    [ServiceReceiveTime] DATETIME      NULL,
    [ElapsedTime]        INT           NULL,
    [TimeOut]            INT           NULL,
    [Success]            BIT           NULL,
    [ServiceSDKversion]  NVARCHAR (50) NULL,
    [DeviceSDKversion]   NVARCHAR (50) NULL,
    [InstanceStartTime] DATETIME NULL, 
    [Description] NVARCHAR(100) NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


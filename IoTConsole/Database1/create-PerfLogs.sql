CREATE TABLE [dbo].[PerfLogs] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [DeviceID]           NVARCHAR (50)  NULL,
    [ServiceSendTime]    DATETIME2 (7)      NULL,
    [DeviceTime]         DATETIME2 (7)      NULL,
    [IoTHubReceiveTime]  DATETIME2 (7)      NULL,
    [ServiceReceiveTime] DATETIME2 (7)      NULL,
    [ElapsedTime]        INT            NULL,
    [TimeOut]            INT            NULL,
    [Success]            BIT            NULL,
    [ServiceSDKversion]  NVARCHAR (50)  NULL,
    [DeviceSDKversion]   NVARCHAR (50)  NULL,
    [InstanceStartTime]  DATETIME2 (7)  NULL,
    [Description]        NVARCHAR (100) NULL,
    [LogCreatedTime]     DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
	[E2ETime]			 INT	 NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [dbo].[IoTDemo]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [DeviceId] NVARCHAR(50) NULL, 
    [StartTime] DATETIME NULL, 
    [DeviceTime] DATETIME NULL, 
    [ElapsedTime] DATETIME NULL
)

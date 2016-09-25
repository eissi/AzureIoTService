CREATE PROCEDURE [dbo].[InsertProcedure]
	@deviceid nvarchar(50),
	@svcsendtime datetime2,
	@dvctime datetime2,
	@hubrcvtime datetime2,
	@svcrcvtime datetime2,
	@elapsedtime int,
	@timoue int,
	@success bit,
	@svcSDKver nvarchar(50),
	@dvcSDKver nvarchar(50),
	@insttime datetime2,
	@desc nvarchar(100),
	@dbtime datetime2
AS
	insert into PerfLogs
	values(@deviceid, @svcsendtime,	@dvctime,
	@hubrcvtime ,
	@svcrcvtime ,
	@elapsedtime ,
	@timoue ,
	@success ,
	@svcSDKver ,
	@dvcSDKver ,
	@insttime ,
	@desc ,
	@dbtime) 
GO

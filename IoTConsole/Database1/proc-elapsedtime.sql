CREATE PROCEDURE [dbo].[proc-elapsedtime]
AS
update PerfLogs
set ElapsedTime=datediff(ms,ServiceSendTime,LogCreatedTime)
where datediff(SECOND,LogCreatedTime,getdate()) < 6500
RETURN 0

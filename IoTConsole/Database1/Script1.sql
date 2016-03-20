select AVG(ElapsedTime) as [Average Time] From dbo.PerfLogs where DeviceSDKVersion='java 2016-03-11'


select InstanceStartTime, AVG(ElapsedTime) as [Average Time], Count(*) as Count, Description from dbo.PerfLogs where DeviceID='JavaDemo' group by InstanceStartTime, Description

select InstanceStartTime, AVG(ElapsedTime) as [Average Time], Count(*) as Count, Description from dbo.PerfLogs group by InstanceStartTime, Description order by InstanceStartTime DESC


select top 1 InstanceStartTime from dbo.PerfLogs group by InstanceStartTime order by InstanceStartTime desc

select * from dbo.PerfLogs where InstanceStartTime=(select top 1 InstanceStartTime from dbo.PerfLogs group by InstanceStartTime order by InstanceStartTime desc)

select datediff(ms,ServiceSendTime,DeviceTime) as S2DTime, datediff(ms,DeviceTime,IoTHubReceiveTime) as D2HTime, datediff(ms,IotHubReceiveTime,ServiceReceiveTime) as H2STime, ElapsedTime as [Total Elapsed Time] from dbo.PerfLogs where InstanceStartTime=(select top 1 InstanceStartTime from dbo.PerfLogs group by InstanceStartTime order by InstanceStartTime desc)

select success, count(*) from dbo.PerfLogs where InstanceStartTime=(select top 1 InstanceStartTime from dbo.PerfLogs group by InstanceStartTime order by InstanceStartTime desc) group by Success 

select replace(success,'1','SUCCESS') as [Success or Fail],count(*) as [Number of transactions] from dbo.PerfLogs where Description='with timeout enabled' group by Success 

select datediff(mi,min(servicesendtime),max(servicereceivetime)) as [Total Duration] from dbo.PerfLogs where InstanceStartTime='2016-03-19 10:54:22.693'

select top 10 * from dbo.PerfLogs order by ID DESC

select Description from dbo.PerfLogs where Description like '%develop%' group by Description

delete from dbo.PerfLogs where Description like '%develop%'

select AVG(ElapsedTime) as [Average Time] From dbo.PerfLogs where DeviceSDKVersion='java 2016-03-11'


select InstanceStartTime, AVG(ElapsedTime) as [Average Time], Count(*) as Count, Description from dbo.PerfLogs where DeviceID='JavaDemo' group by InstanceStartTime, Description

select InstanceStartTime, AVG(ElapsedTime) as [Average Time], Count(*) as Count, DeviceId, Description from dbo.PerfLogs group by InstanceStartTime,DeviceId,Description order by InstanceStartTime DESC


select top 1 InstanceStartTime from dbo.PerfLogs group by InstanceStartTime order by InstanceStartTime desc

select * from dbo.PerfLogs where InstanceStartTime=(select top 1 InstanceStartTime from dbo.PerfLogs group by InstanceStartTime order by InstanceStartTime desc)
select * from dbo.PerfLogs order by ElapsedTime DESC

select InstanceStartTime,Description,datediff(ms,ServiceSendTime,DeviceTime) as S2DTime, datediff(ms,DeviceTime,IoTHubReceiveTime) as D2HTime, datediff(ms,IotHubReceiveTime,ServiceReceiveTime) as H2STime, ElapsedTime as [Total Elapsed Time] from dbo.PerfLogs where InstanceStartTime=(select top 1 InstanceStartTime from dbo.PerfLogs group by InstanceStartTime order by InstanceStartTime desc) order by ElapsedTime DESC
select InstanceStartTime,Description,datediff(ms,ServiceSendTime,DeviceTime) as S2DTime, datediff(ms,DeviceTime,IoTHubReceiveTime) as D2HTime, datediff(ms,IotHubReceiveTime,ServiceReceiveTime) as H2STime, ElapsedTime as [Total Elapsed Time] from dbo.PerfLogs where Description != 'surface' order by ElapsedTime DESC
select InstanceStartTime,Description,avg(datediff(ms,ServiceSendTime,DeviceTime)) as S2DTime, avg(datediff(ms,DeviceTime,IoTHubReceiveTime)) as D2HTime, avg(datediff(ms,IotHubReceiveTime,ServiceReceiveTime)) as H2STime, avg(ElapsedTime) as [Total Elapsed Time] from dbo.PerfLogs where Description != 'surface' group by instancestarttime,Description 

select replace(success,'1','SUCCESS') as [Success or Fail], count(*) as [Count], avg(elapsedtime) as [Average Transaction Time(ms)] from dbo.PerfLogs where InstanceStartTime=(select top 1 InstanceStartTime from dbo.PerfLogs group by InstanceStartTime order by InstanceStartTime desc) group by Success
select instancestarttime,Description,replace(success,'1','SUCCESS') as [Success or Fail], count(*) as [Count], avg(elapsedtime) as [Average Transaction Time(ms)] from dbo.PerfLogs group by Success, Description,InstanceStartTime order by InstanceStartTime DESC

select replace(success,'1','SUCCESS') as [Success or Fail],count(*) as [Number of transactions] from dbo.PerfLogs where Description='single machine 3rd after device exception correction' group by Success
select * from dbo.PerfLogs where Description='single machine' and success='0' 

select datediff(mi,min(servicesendtime),max(servicereceivetime)) as [Total Duration] from dbo.PerfLogs where InstanceStartTime='2016-03-19 10:54:22.693'

select top 10 * from dbo.PerfLogs order by ID DESC

select Description from dbo.PerfLogs where Description like '%develop%' group by Description

--delete from dbo.PerfLogs where instancestarttime='2016-03-25 01:11:36.877'
--truncate table dbo.perflogs

select top 10 * from dbo.PerfLogs order by ElapsedTime DESC

select description, min(servicesendtime) as [First Message Time],max(serviceSendtime) as [Last Message Time] from dbo.PerfLogs group by description order by [First Message Time] DESC

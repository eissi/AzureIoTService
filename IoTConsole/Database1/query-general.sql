insert into PerfLogs
values('demo',getdate(),getdate(),getdate(),getdate(),datediff(ms,getdate(),sysdatetime()),'','','','',getdate(),'insert test',getdate())

select * from PerfLogs where datediff(SECOND,LogCreatedTime,getdate()) < 6500

update PerfLogs
set E2ETime=datediff(ms,ServiceSendTime,LogCreatedTime)
where deviceid='su1round3'
--where datediff(SECOND,LogCreatedTime,getdate()) < 6500

select iothubreceivetime,logcreatedtime,ElapsedTime,description from PerfLogs where deviceid='demo' and Description='single_no_wait_no_window_on_ASA'
select iothubreceivetime,logcreatedtime,ElapsedTime,description from PerfLogs where deviceid='demo' and Description='stream analytics'
select 
	description,
	count(distinct logcreatedtime) as [number of DB trans], 
	count(logcreatedtime) as [number of trans], 
	count(logcreatedtime)/datediff(second,min(logcreatedtime),max(logcreatedtime)) as [trans per sec], 
	Avg(E2ETime) as [average E2E time(ms)],
	min(E2ETime) as [min E2E time(ms)],
	max(E2ETime) as [max E2E time(ms)],
	avg(elapsedtime) as [average VM sercice time(ms)]
	from PerfLogs 
	where deviceid='su1round3' 
	group by Description

select datediff(second,min(logcreatedtime),max(logcreatedtime)) from PerfLogs where deviceid='demo3'
select count(logcreatedtime) from PerfLogs where deviceid='su1'
select count(logcreatedtime)/datediff(second,min(logcreatedtime),max(logcreatedtime)) as [trans per sec] from PerfLogs where deviceid='demo3'

--delete from PerfLogs where devicesdkversion='32part 100 dev P0 round2'
--delete from PerfLogs where datediff(day,devicetime,getdate())=0
--delete from PerfLogs where servicesendtime='2016-10-02 02:16:44.8240303'

select distinct description from PerfLogs

select * from PerfLogs where id>309579 order by id desc 
select * from PerfLogs where deviceid='device05'
select * from PerfLogs where datediff(HOUR,devicetime,getdate())<2


select * from PerfLogs where ServiceSendTime=(select top 1 servicesendtime from perflogs where  datediff(minute,devicetime,getdate())<60 order by servicesendtime desc)
select distinct servicesendtime from PerfLogs where datediff(minute,devicetime,getdate())<6
select servicesendtime from perflogs where  datediff(minute,devicetime,getdate())<60

select top 1 servicesendtime from perflogs where  datediff(minute,devicetime,getdate())<60 order by servicesendtime desc
-- Declare the variable to be used.
DECLARE @TestRunTime datetime2;
select @TestRunTime = '2016-10-02 07:19:03.8261687'
--select @TestRunTime = (select top 1 servicesendtime from perflogs where  datediff(minute,devicetime,getdate())<60 order by servicesendtime desc)

DECLARE @DevSDK nvarchar(50);
select @DevSDK = devicesdkversion from PerfLogs where servicesendtime=@testruntime and Description='VM'
--SET @DevSDK = '32part single 1 devices 2round';

--select * from PerfLogs where DeviceSDKversion= @DevSDK
--select description,count(*) from PerfLogs where DeviceSDKversion=@DevSDK group by Description

update PerfLogs
set DeviceSDKversion=@DevSDK
where ServiceSendTime=@TestRunTime

update PerfLogs
set E2ETime=datediff(ms,DeviceTime,LogCreatedTime)
where DeviceSDKversion=@DevSDK

select 
	DeviceSDKversion,
	description,
	count(distinct logcreatedtime) as [number of DB trans], 
	count(logcreatedtime) as [number of trans], 
	count(logcreatedtime)/datediff(second,min(logcreatedtime),max(logcreatedtime)) as [trans per sec], 
	Avg(E2ETime) as [average E2E time(ms)],
	min(E2ETime) as [min E2E time(ms)],
	max(E2ETime) as [max E2E time(ms)],
	avg(elapsedtime) as [average VM sercice time(ms)]
	from PerfLogs 
	where DeviceSDKversion=@DevSDK
	group by Description, DeviceSDKversion

	
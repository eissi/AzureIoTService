insert into PerfLogs
values('demo',getdate(),getdate(),getdate(),getdate(),datediff(ms,getdate(),sysdatetime()),'','','','',getdate(),'insert test',getdate())

select * from PerfLogs where datediff(SECOND,LogCreatedTime,getdate()) < 6500

update PerfLogs
set E2ETime=datediff(ms,ServiceSendTime,LogCreatedTime)
where deviceid='su1round3'
where datediff(SECOND,LogCreatedTime,getdate()) < 6500

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

delete from PerfLogs where description='insert test'

select distinct description from PerfLogs

select * from PerfLogs where deviceid='su1test2'
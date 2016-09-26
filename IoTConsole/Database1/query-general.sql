insert into PerfLogs
values('demo',getdate(),getdate(),getdate(),getdate(),datediff(ms,getdate(),sysdatetime()),'','','','',getdate(),'insert test',getdate())

select * from PerfLogs where datediff(SECOND,LogCreatedTime,getdate()) < 6500

update PerfLogs
set ElapsedTime=datediff(ms,ServiceSendTime,LogCreatedTime)
where datediff(SECOND,LogCreatedTime,getdate()) < 6500

select iothubreceivetime,logcreatedtime,ElapsedTime,description from PerfLogs where deviceid='demo' and Description='single_no_wait_no_window_on_ASA'
select iothubreceivetime,logcreatedtime,ElapsedTime,description from PerfLogs where deviceid='demo' and Description='stream analytics'
select 
	description,
	count(distinct logcreatedtime) as [number of DB trans], 
	count(logcreatedtime) as [number of trans], 
	count(logcreatedtime)/datediff(second,min(logcreatedtime),max(logcreatedtime)) as [trans per sec], 
	Avg(ElapsedTime) as [average elapsed time(ms)],
	min(ElapsedTime) as [min elapsed time(ms)],
	max(elapsedtime) as [max elapsed time(ms)]
	from PerfLogs 
	where deviceid='demo' 
	group by Description

select datediff(second,min(logcreatedtime),max(logcreatedtime)) from PerfLogs

delete from PerfLogs where description='insert test'

select distinct description from PerfLogs
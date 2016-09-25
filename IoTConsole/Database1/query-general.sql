insert into PerfLogs
values('demo',getdate(),getdate(),getdate(),getdate(),datediff(ms,getdate(),sysdatetime()),'','','','',getdate(),'insert test',getdate())

select * from PerfLogs where datediff(SECOND,LogCreatedTime,getdate()) < 6500

update PerfLogs
set ElapsedTime=datediff(ms,ServiceSendTime,LogCreatedTime)
where datediff(SECOND,LogCreatedTime,getdate()) < 6500

select iothubreceivetime,logcreatedtime,ElapsedTime,description from PerfLogs where deviceid='demo' and Description='single_no_wait_no_window_on_ASA'